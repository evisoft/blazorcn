#!/usr/bin/env node
// Responsive + a11y sweep: for every route, detects horizontal overflow at
// 320/375/768/1024/1440/1920, blank pages, console errors, and axe-core
// violations (serious+critical) at 1280. Writes audit/responsive-sweep.json.
import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const BASE = "http://127.0.0.1:53185";
const VIEWPORTS = [1920, 1440, 1024, 768, 375, 320];

async function loadPlaywright() {
  try { return await import("playwright"); }
  catch {
    const fb = path.join(repoRoot, "audit", "node_modules", "playwright", "index.mjs");
    return await import(`file:///${fb.replaceAll("\\", "/")}`);
  }
}
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const OVERFLOW_PROBE = `(() => {
  const iw = document.documentElement.clientWidth;
  const sw = Math.max(document.documentElement.scrollWidth, document.body ? document.body.scrollWidth : 0);
  // scrollWidth over-reports (it can include content clipped inside inner
  // overflow-x-auto containers) — ground truth is whether the WINDOW actually
  // scrolls horizontally.
  const y = window.scrollY;
  window.scrollTo(99999, y);
  const realOverflow = window.scrollX > 1;
  window.scrollTo(0, y);
  const out = { iw, sw, overflow: realOverflow, offenders: [] };
  if (out.overflow) {
    const seen = new Set();
    const els = document.querySelectorAll("body *");
    for (const el of els) {
      const r = el.getBoundingClientRect();
      if (r.width === 0 && r.height === 0) continue;
      const cs = getComputedStyle(el);
      if (cs.position === "fixed") continue; // off-screen sentinels/portals
      if (r.right > iw + 4 || r.left < -4) {
        // keep only outermost offenders: skip if an ancestor already recorded
        let anc = el.parentElement, skip = false;
        while (anc) { if (seen.has(anc)) { skip = true; break; } anc = anc.parentElement; }
        if (skip) continue;
        seen.add(el);
        const cls = (typeof el.className === "string" ? el.className : "").split(/\\s+/).slice(0, 6).join(".");
        const slot = el.getAttribute && el.getAttribute("data-slot");
        out.offenders.push({
          tag: el.tagName.toLowerCase() + (el.id ? "#" + el.id : "") + (cls ? "." + cls : ""),
          slot: slot || undefined,
          left: Math.round(r.left), right: Math.round(r.right), width: Math.round(r.width),
          scrollableAncestor: (() => { let a = el.parentElement; while (a) { const s = getComputedStyle(a); if (/(auto|scroll)/.test(s.overflowX)) return true; a = a.parentElement; } return false; })(),
        });
        if (out.offenders.length >= 8) break;
      }
    }
  }
  return out;
})()`;

async function main() {
  const { chromium } = await loadPlaywright();
  const routesFile = process.argv[2] || path.join(repoRoot, "audit", "all-routes-full.txt");
  const routes = (await readFile(routesFile, "utf8")).split("\n").map(s => s.trim()).filter(Boolean);
  const axeSource = await readFile(path.join(repoRoot, "audit", "node_modules", "axe-core", "axe.min.js"), "utf8");
  let browser = await chromium.launch();
  let context, page;
  const results = [];
  let done = 0;
  const consoleErrors = [];
  const freshPage = async () => {
    try { if (context) await context.close(); } catch {}
    context = await browser.newContext({ viewport: { width: 1920, height: 1080 }, colorScheme: "light" });
    page = await context.newPage();
    page.on("console", m => {
      if (m.type() !== "error") return;
      // axe-core's CSSOM preloader resolves the `@import "blazorcn-components.css"`
      // inside _content/BlazorCN/blazorcn.css against the PAGE url instead of the
      // stylesheet url, so it XHRs /blazorcn-components.css and gets a 404 on every
      // route. The browser itself resolves the import correctly (200 at
      // _content/BlazorCN/...). Measurement artifact of the scanner — not an app bug.
      if (/blazorcn-components\.css/.test(m.location()?.url || "")) return;
      consoleErrors.push(m.text().replace(/\s+/g, " ").slice(0, 300));
    });
    page.on("pageerror", e => consoleErrors.push("PAGEERROR: " + String(e).replace(/\s+/g, " ").slice(0, 300)));
  };
  await freshPage();

  for (const route of routes) {
    // WASM pages leak; recycle the context every 10 routes to avoid tab crashes
    if (done > 0 && done % 10 === 0) await freshPage();
    consoleErrors.length = 0;
    let rec = { route, errors: [], blank: false, overflows: {}, axe: [] };
    // A crashed tab means the route was never measured — that reads as "clean" in
    // the report. Retry once on a fresh context so a crash can't hide findings.
    for (let attempt = 0; attempt < 2; attempt++) {
      if (attempt > 0) {
        await freshPage();
        consoleErrors.length = 0;
        rec = { route, errors: [], blank: false, overflows: {}, axe: [], retried: true };
      }
      const failed = await measure(rec);
      if (!failed) break;
    }
    const counts = {};
    for (const e of consoleErrors) counts[e] = (counts[e] || 0) + 1;
    rec.errors.push(...Object.entries(counts).map(([m, n]) => (n > 1 ? `(x${n}) ${m}` : m)));
    results.push(rec);
    done++;
    const ovKeys = Object.keys(rec.overflows);
    const axeSerious = rec.axe.filter(a => a.impact === "serious" || a.impact === "critical");
    console.log(`[${String(done).padStart(3)}/${routes.length}] ${rec.blank ? "BLANK " : ""}${ovKeys.length ? "OVF@" + ovKeys.join(",") + " " : ""}${axeSerious.length ? "AXE:" + axeSerious.length + " " : ""}${rec.errors.length ? "ERR:" + rec.errors.length + " " : ""}${route}`);
  }

  // Measures one route into `rec`; returns true if it crashed and should be retried.
  async function measure(rec) {
    const route = rec.route;
    try {
      await page.setViewportSize({ width: 1920, height: 1080 });
      await page.goto(BASE + route, { waitUntil: "load", timeout: 60000 });
      // Wait for real TEXT, not just for <main> to have children: a heavy blocks page
      // (150+ blocks under a cold WASM start) can have the element but no painted
      // content for several seconds. A fixed sleep here reported /docs/blocks/about as
      // blank on a slow run while it actually renders 149KB of text.
      try { await page.waitForFunction(`(() => { const m = document.querySelector("main"); return !!m && m.innerText.trim().length > 40; })()`, null, { timeout: 45000, polling: 250 }); }
      catch { rec.blank = true; }
      await sleep(400);
      rec.blank = rec.blank && await page.evaluate(`(() => { const m = document.querySelector("main"); return !m || m.innerText.trim().length < 10; })()`);
      for (const w of VIEWPORTS) {
        await page.setViewportSize({ width: w, height: w < 500 ? 800 : 1080 });
        await sleep(280);
        const probe = await page.evaluate(OVERFLOW_PROBE);
        if (probe.overflow) rec.overflows[w] = { scrollWidth: probe.sw, offenders: probe.offenders };
      }
      // axe at 1280
      await page.setViewportSize({ width: 1280, height: 1080 });
      await sleep(250);
      await page.evaluate(axeSource);
      const axeRes = await page.evaluate(`axe.run(document, { resultTypes: ["violations"], runOnly: { type: "tag", values: ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "best-practice"] } }).then(r => r.violations.map(v => ({ id: v.id, impact: v.impact, help: v.help, nodes: v.nodes.slice(0, 5).map(n => n.target.join(" ")) , count: v.nodes.length })))`);
      rec.axe = axeRes;
      return false;
    } catch (e) {
      rec.errors.push("NAV-FAIL: " + String(e).slice(0, 150));
      const crashed = /crashed|closed/i.test(String(e));
      if (crashed) { try { await freshPage(); } catch {} }
      return crashed;
    }
  }
  await browser.close();
  await writeFile(path.join(repoRoot, "audit", "responsive-sweep.json"), JSON.stringify(results, null, 2));
  const bad = results.filter(r => r.blank || r.errors.length || Object.keys(r.overflows).length || r.axe.some(a => a.impact === "serious" || a.impact === "critical"));
  console.log(`\n${results.length} routes; ${bad.length} flagged. -> audit/responsive-sweep.json`);
}
main().catch(e => { console.error(e); process.exit(1); });

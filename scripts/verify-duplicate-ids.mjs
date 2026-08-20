#!/usr/bin/env node
// A duplicated `id` is invalid HTML, but the damage is concrete rather than pedantic:
// `getElementById` and `label.control` both resolve to the FIRST match, so on a page that
// renders many blocks together, the "Email" label of the 50th block focuses the 1st block's
// input. Screen-reader users get the wrong name announced; sighted users get focus teleported.
//
// The DOM is the only ground truth here — a static scan over-reports, because two blocks
// sharing an id may never render on the same page.
//
//   node scripts/verify-duplicate-ids.mjs [routes.txt]
import { chromium } from "./../audit/node_modules/playwright/index.mjs";
import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";

const BASE = "http://127.0.0.1:53185";
const repoRoot = path.resolve(import.meta.dirname, "..");
const routesFile = process.argv.slice(2).find(a => !a.startsWith("--"))
  ?? path.join(repoRoot, "scripts/routes-blocks.txt");
const routes = (await readFile(routesFile, "utf8")).split(/\r?\n/).map(s => s.trim()).filter(Boolean);

const browser = await chromium.launch();
const page = await (await browser.newContext({ viewport: { width: 1280, height: 1000 } })).newPage();
page.on("console", () => {});

let totalDup = 0, totalMisrouted = 0;
const perRoute = [];

for (const route of routes) {
  try {
    await page.goto(BASE + route, { waitUntil: "load", timeout: 120000 });
    // NOTE the `null`: with a STRING predicate, waitForFunction's 2nd positional is `arg`,
    // not `options` — passing options there silently keeps the 30s default.
    await page.waitForFunction(
      `document.querySelector("main") && document.querySelector("main").innerText.trim().length > 40`,
      null, { timeout: 180000, polling: 250 });
    await page.waitForTimeout(600);

    const r = await page.evaluate(() => {
      const counts = new Map();
      for (const el of document.querySelectorAll("[id]")) counts.set(el.id, (counts.get(el.id) || 0) + 1);
      const dups = [...counts].filter(([, n]) => n > 1).sort((a, b) => b[1] - a[1]);
      // Behavioural consequence: label resolves to a control outside the label's own block.
      const blockOf = (el) => el.closest("section, article, [data-block]") || el.parentElement;
      let misrouted = 0;
      for (const l of document.querySelectorAll("label[for]")) {
        const t = document.getElementById(l.htmlFor);
        if (!t || l.control !== t) continue;
        if ((counts.get(l.htmlFor) || 0) > 1 && !blockOf(l)?.contains(t)) misrouted++;
      }
      return { dupIds: dups.length, misrouted, worst: dups.slice(0, 6), allDupIds: dups.map(([id]) => id) };
    });

    totalDup += r.dupIds; totalMisrouted += r.misrouted;
    if (r.dupIds) perRoute.push({ route, ...r });
    console.log(`${r.dupIds ? "DUP " : "ok  "} ${route.padEnd(34)} dup-ids ${String(r.dupIds).padStart(3)}  misrouted-labels ${String(r.misrouted).padStart(4)}` +
      (r.worst.length ? `   worst: ${r.worst.map(([k, n]) => `${k}×${n}`).join(", ")}` : ""));
  } catch (e) {
    console.log(`ERR  ${route}  ${String(e).slice(0, 80)}`);
  }
}
await browser.close();

console.log(`\nroutes: ${routes.length}`);
console.log(`duplicate ids (DOM):    ${totalDup}`);
console.log(`labels driving the wrong control: ${totalMisrouted}`);
if (process.argv.includes("--json"))
  await writeFile(path.join(repoRoot, "scripts/.dup-ids.json"), JSON.stringify(perRoute, null, 2));
process.exit(totalDup ? 1 : 0);

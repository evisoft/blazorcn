#!/usr/bin/env node
// Every other check in this suite runs in LIGHT mode, but dark mode is not a filter — it is a
// second, independent set of token values (`.dark { --background: … }` in blazorcn.css). A
// contrast pair that passes on white can fail on near-black and nothing else would notice.
//
// This runs axe's colour-contrast rule twice per route (light, then dark) and reports the
// DELTA, so pre-existing light-mode findings do not drown out dark-only regressions. It also
// checks that dark mode is actually in effect, because a silent failure to apply `.dark`
// would make everything look clean.
//
//   node scripts/verify-dark-mode.mjs [routes.txt]
import { chromium } from "./../audit/node_modules/playwright/index.mjs";
import { readFile } from "node:fs/promises";
import path from "node:path";

const BASE = "http://127.0.0.1:53185";
const repoRoot = path.resolve(import.meta.dirname, "..");
const AXE = path.join(repoRoot, "audit/node_modules/axe-core/axe.min.js");

const DEFAULT_ROUTES = [
  "/", "/themes", "/docs/components/button", "/docs/components/badge", "/docs/components/alert",
  "/docs/components/card", "/docs/components/table", "/docs/components/tabs",
  "/docs/components/select", "/docs/components/input", "/docs/components/dialog",
  "/docs/components/chart", "/docs/components/calendar", "/docs/components/sidebar",
  "/examples/dashboard", "/examples/tasks", "/examples/cards",
  "/docs/blocks/pricing", "/docs/blocks/stats", "/docs/blocks/login",
];

const routesFile = process.argv[2];
const routes = routesFile
  ? (await readFile(routesFile, "utf8")).split(/\r?\n/).map(s => s.trim()).filter(Boolean)
  : DEFAULT_ROUTES;

const axeSrc = await readFile(AXE, "utf8");
const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1280, height: 1000 } });
const page = await ctx.newPage();

// axe resolves the @import in blazorcn.css against the page URL and 404s on it; that request
// is an artefact of the scanner, not a missing file (the real path returns 200).
page.on("console", () => {});

const contrast = async () => {
  await page.evaluate(axeSrc);
  return page.evaluate(async () => {
    const r = await window.axe.run(document, { runOnly: { type: "rule", values: ["color-contrast"] } });
    const nodes = r.violations.flatMap(v => v.nodes);
    return {
      count: nodes.length,
      // Key each finding by its target so light and dark can be diffed element-wise.
      keys: nodes.map(n => (n.target || []).join(" ")),
      worst: nodes.map(n => {
        const m = /contrast of ([\d.]+)/.exec(n.any?.[0]?.message || "");
        return m ? parseFloat(m[1]) : null;
      }).filter(Boolean).sort((a, b) => a - b).slice(0, 3),
    };
  });
};

let lightTotal = 0, darkTotal = 0, darkOnlyTotal = 0;
const darkOnlyByRoute = [];

for (const route of routes) {
  try {
    await page.goto(BASE + route, { waitUntil: "load", timeout: 90000 });
    // NOTE the `null`: for a STRING predicate, waitForFunction's second positional argument is
    // `arg`, not `options`. Passing the options object there silently leaves the 30s default in
    // place, which made every WASM-heavy route report a spurious timeout.
    await page.waitForFunction(
      `document.querySelector("main") && document.querySelector("main").innerText.trim().length > 40`,
      null, { timeout: 120000, polling: 250 });

    await page.evaluate(() => document.documentElement.classList.remove("dark"));
    await page.waitForTimeout(150);
    const light = await contrast();

    await page.evaluate(() => document.documentElement.classList.add("dark"));
    await page.waitForTimeout(250);
    // Confirm dark mode really applied — otherwise a clean result means nothing.
    const applied = await page.evaluate(() => {
      const bg = getComputedStyle(document.body).backgroundColor;
      const m = /rgba?\(([\d.]+),\s*([\d.]+),\s*([\d.]+)/.exec(bg);
      if (!m) return { bg, dark: false };
      const lum = (+m[1] * 0.299 + +m[2] * 0.587 + +m[3] * 0.114) / 255;
      return { bg, dark: lum < 0.5 };
    });
    const dark = await contrast();

    const lightSet = new Set(light.keys);
    const darkOnly = dark.keys.filter(k => !lightSet.has(k));
    lightTotal += light.count; darkTotal += dark.count; darkOnlyTotal += darkOnly.length;
    if (darkOnly.length) darkOnlyByRoute.push({ route, n: darkOnly.length, sample: darkOnly.slice(0, 3), worst: dark.worst });

    const flag = !applied.dark ? "DARK-NOT-APPLIED" : darkOnly.length ? "DARK-ONLY" : "ok";
    console.log(`${flag.padEnd(17)} ${route}  light ${String(light.count).padStart(4)} | dark ${String(dark.count).padStart(4)} | dark-only ${darkOnly.length}` +
      (applied.dark ? "" : `  body=${applied.bg}`));
  } catch (e) {
    console.log(`ERR               ${route}  ${String(e).slice(0, 70)}`);
  }
}
await browser.close();

console.log(`\nroutes: ${routes.length}`);
console.log(`colour-contrast nodes — light: ${lightTotal}, dark: ${darkTotal}`);
console.log(`nodes failing ONLY in dark mode: ${darkOnlyTotal}`);
if (darkOnlyByRoute.length) {
  console.log(`\ndark-only findings by route:`);
  for (const d of darkOnlyByRoute.sort((a, b) => b.n - a.n)) {
    console.log(`  ${String(d.n).padStart(4)}  ${d.route}   lowest ratios: ${d.worst.join(", ")}`);
    for (const s of d.sample) console.log(`          ${s.slice(0, 100)}`);
  }
}
process.exit(darkOnlyTotal ? 1 : 0);

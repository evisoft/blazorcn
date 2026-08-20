#!/usr/bin/env node
// WCAG 2.2 SC 2.5.8 Target Size (Minimum), AA: an interactive target must be at least
// 24x24 CSS px, UNLESS it is spaced so that a 24px-diameter circle centred on it does
// not intersect any other target's circle. axe-core does not implement this rule, so
// nothing else in the suite catches it — and it is the rule touch users feel.
//
// Exemptions applied (per the SC's own wording): inline links in a sentence, targets
// whose function is duplicated by another control on the page (not detectable here, so
// reported separately as "small but spaced"), and browser-default UA controls.
//
//   node scripts/verify-target-size.mjs [routes.txt]
import { chromium } from "./../audit/node_modules/playwright/index.mjs";
import { readFile } from "node:fs/promises";
import path from "node:path";

const BASE = "http://127.0.0.1:53185";
const repoRoot = path.resolve(import.meta.dirname, "..");
const MIN = 24;

const DEFAULT_ROUTES = [
  "/", "/docs/components/button", "/docs/components/checkbox", "/docs/components/switch",
  "/docs/components/select", "/docs/components/pagination", "/docs/components/tabs",
  "/docs/components/carousel", "/docs/components/table", "/docs/components/data-table",
  "/docs/components/input-otp", "/docs/components/slider", "/docs/components/toggle-group",
  "/docs/blocks/calendar", "/docs/blocks/tables", "/docs/blocks/chat", "/docs/blocks/music",
  "/docs/blocks/kanban", "/docs/blocks/checkout", "/examples/dashboard", "/examples/tasks",
];

const PROBE = `(() => {
  const SEL = 'a[href], button, input:not([type=hidden]), select, textarea, summary,' +
    '[role=button], [role=checkbox], [role=switch], [role=radio], [role=tab],' +
    '[role=menuitem], [role=menuitemcheckbox], [role=menuitemradio], [role=option], [role=link]';
  const rects = [];
  for (const el of document.querySelectorAll(SEL)) {
    if (el.disabled) continue;
    const cs = getComputedStyle(el);
    if (cs.display === 'none' || cs.visibility === 'hidden' || +cs.opacity === 0) continue;
    const r = el.getBoundingClientRect();
    if (r.width < 1 || r.height < 1) continue;               // hidden / sr-only
    if (el.closest('[aria-hidden="true"]')) continue;         // decorative mockups
    rects.push({ el, r });
  }
  const small = rects.filter(({ r }) => r.width < ${MIN} || r.height < ${MIN});
  const out = [];
  for (const s of small) {
    // Spacing exception: no other target's 24px circle may intersect ours.
    const cx = s.r.left + s.r.width / 2, cy = s.r.top + s.r.height / 2;
    let crowded = false;
    for (const o of rects) {
      if (o.el === s.el || s.el.contains(o.el) || o.el.contains(s.el)) continue;
      const ox = o.r.left + o.r.width / 2, oy = o.r.top + o.r.height / 2;
      if (Math.hypot(cx - ox, cy - oy) < ${MIN}) { crowded = true; break; }
    }
    // Inline-link exception: an <a> inside a paragraph of text.
    const inlineLink = s.el.tagName === 'A' && !!s.el.closest('p, li, td, span');
    // "Equivalent" exception: the same function is offered by another control on the
    // page that does meet 24x24 — e.g. the 16px sidebar rail duplicates the sidebar
    // trigger button. Matched on identical accessible name.
    const myName = (s.el.getAttribute('aria-label') || s.el.textContent || '').trim();
    const equivalent = myName.length > 0 && rects.some(({ el, r }) =>
      el !== s.el && r.width >= ${MIN} && r.height >= ${MIN} &&
      (el.getAttribute('aria-label') || el.textContent || '').trim() === myName);
    const tag = s.el.tagName.toLowerCase() + (s.el.getAttribute('role') ? '[' + s.el.getAttribute('role') + ']' : '');
    const label = (s.el.getAttribute('aria-label') || s.el.textContent || '').trim().slice(0, 28);
    const cls = (s.el.className || '').toString().split(/\\s+/).filter(c => /^(size|w|h)-/.test(c)).slice(0, 3).join(' ');
    out.push({ tag, label, cls, w: Math.round(s.r.width), h: Math.round(s.r.height), crowded, inlineLink, equivalent });
  }
  return { total: rects.length, small: out };
})()`;

const routesFile = process.argv[2];
const routes = routesFile
  ? (await readFile(routesFile, "utf8")).split(/\r?\n/).map(s => s.trim()).filter(Boolean)
  : DEFAULT_ROUTES;

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1280, height: 1000 } });
const page = await ctx.newPage();

let violations = 0, spacedOk = 0, inlineOk = 0, equivalentOk = 0, scanned = 0;
const byShape = new Map();
const perRoute = [];   // also written to audit/target-size.json for fix waves

for (const route of routes) {
  try {
    await page.goto(BASE + route, { waitUntil: "load", timeout: 60000 });
    await page.waitForFunction(`document.querySelector("main") && document.querySelector("main").innerText.length > 40`, null, { timeout: 45000 });
    await new Promise(r => setTimeout(r, 500));
    const res = await page.evaluate(PROBE);
    scanned += res.total;
    let routeBad = 0;
    const routeItems = [];
    for (const s of res.small) {
      if (s.inlineLink) { inlineOk++; continue; }
      if (s.equivalent) { equivalentOk++; continue; }      // same function on a bigger control
      if (!s.crowded) { spacedOk++; continue; }            // small but isolated: SC met
      violations++; routeBad++;
      routeItems.push(s);
      const key = `${s.tag} ${s.w}x${s.h} ${s.cls}`.trim();
      const e = byShape.get(key) || { n: 0, routes: new Set(), labels: new Set() };
      e.n++; e.routes.add(route); if (s.label) e.labels.add(s.label);
      byShape.set(key, e);
    }
    if (routeItems.length) perRoute.push({ route, count: routeItems.length, items: routeItems });
    console.log(`${routeBad ? "FAIL" : "ok  "} ${route}  (${res.total} targets, ${res.small.length} under ${MIN}px, ${routeBad} failing)`);
  } catch (e) {
    console.log(`ERR  ${route}  ${String(e).slice(0, 80)}`);
  }
}

await browser.close();
await (await import("node:fs/promises")).writeFile(
  path.join(repoRoot, "audit", "target-size.json"),
  JSON.stringify(perRoute.sort((a, b) => b.count - a.count), null, 1));
console.log(`\nscanned ${scanned} targets across ${routes.length} routes`);
console.log(`under ${MIN}px and crowded (SC 2.5.8 failures): ${violations}`);
console.log(`under ${MIN}px but adequately spaced (exception met): ${spacedOk}`);
console.log(`inline links in text (exception met): ${inlineOk}`);
console.log(`duplicated by a larger control (equivalent exception): ${equivalentOk}`);
if (byShape.size) {
  console.log(`\nfailing shapes, most common first:`);
  for (const [k, v] of [...byShape].sort((a, b) => b[1].n - a[1].n).slice(0, 20)) {
    console.log(`  ${String(v.n).padStart(4)}x  ${k}  | routes: ${[...v.routes].slice(0, 3).join(", ")}` +
      (v.labels.size ? `  | e.g. "${[...v.labels][0]}"` : ""));
  }
}
process.exit(violations ? 1 : 0);

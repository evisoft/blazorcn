#!/usr/bin/env node
// WCAG 2.4.7 Focus Visible (AA): every keyboard-focusable control must show a visible
// change when focused. The library sets `outline-none` on most components and relies on
// `focus-visible:ring-*`, so a component that lost its ring classes would leave keyboard
// users with no indicator at all — and no axe rule checks this.
//
// Method: for each focusable element, snapshot the computed styles that can carry a focus
// indicator, focus it via the keyboard path (element.focus() sets :focus-visible for
// keyboard-focusable elements in Chromium when the last input was a key), then diff.
//
//   node scripts/verify-focus-visible.mjs [routes.txt]
import { chromium } from "./../audit/node_modules/playwright/index.mjs";
import { readFile } from "node:fs/promises";

const BASE = "http://127.0.0.1:53185";
const DEFAULT_ROUTES = [
  "/docs/components/button", "/docs/components/input", "/docs/components/checkbox",
  "/docs/components/switch", "/docs/components/select", "/docs/components/textarea",
  "/docs/components/tabs", "/docs/components/accordion", "/docs/components/radio-group",
  "/docs/components/slider", "/docs/components/toggle-group", "/docs/components/pagination",
  "/docs/components/combobox", "/docs/components/input-otp", "/docs/components/native-select",
  "/docs/components/breadcrumb", "/docs/components/card", "/docs/components/table",
  "/docs/components/sidebar", "/docs/components/menubar", "/docs/components/scroll-area",
  "/", "/themes", "/examples/dashboard",
];

const PROBE = `(() => {
  const SEL = 'a[href], button:not([disabled]), input:not([type=hidden]):not([disabled]),' +
    'select:not([disabled]), textarea:not([disabled]), summary, [tabindex="0"]';
  const props = (el) => {
    const cs = getComputedStyle(el);
    return [cs.outlineStyle, cs.outlineWidth, cs.outlineColor, cs.outlineOffset,
            cs.boxShadow, cs.borderColor, cs.borderWidth, cs.backgroundColor,
            cs.color, cs.textDecorationLine].join('|');
  };
  const bad = [];
  let checked = 0;
  const els = [...document.querySelectorAll(SEL)].filter(el => {
    const r = el.getBoundingClientRect();
    const cs = getComputedStyle(el);
    return r.width > 1 && r.height > 1 && cs.visibility !== 'hidden' && +cs.opacity > 0
      && !el.closest('[aria-hidden="true"]');
  }).slice(0, 220);
  const prevActive = document.activeElement;
  for (const el of els) {
    const before = props(el);
    el.focus({ preventScroll: true });
    if (document.activeElement !== el) continue;   // not actually focusable
    checked++;
    const after = props(el);
    if (before === after) {
      const name = (el.getAttribute('aria-label') || el.textContent || el.getAttribute('placeholder') || '').trim().slice(0, 30);
      const slot = el.getAttribute('data-slot') || el.closest('[data-slot]')?.getAttribute('data-slot') || '';
      bad.push({ tag: el.tagName.toLowerCase(), slot, name,
                 cls: (el.className || '').toString().split(/\\s+/).filter(c => /focus|outline|ring/.test(c)).slice(0, 4).join(' ') });
    }
    el.blur();
  }
  if (prevActive && prevActive.focus) prevActive.focus({ preventScroll: true });
  return { checked, bad };
})()`;

const routesFile = process.argv[2];
const routes = routesFile
  ? (await readFile(routesFile, "utf8")).split(/\r?\n/).map(s => s.trim()).filter(Boolean)
  : DEFAULT_ROUTES;

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1280, height: 1000 } });
const page = await ctx.newPage();

let totalChecked = 0, totalBad = 0;
const byKind = new Map();
for (const route of routes) {
  try {
    await page.goto(BASE + route, { waitUntil: "load", timeout: 60000 });
    await page.waitForFunction(`document.querySelector("main") && document.querySelector("main").innerText.length > 40`, null, { timeout: 45000, polling: 250 });
    // make :focus-visible apply — Chromium needs a keyboard interaction first
    await page.keyboard.press("Tab");
    await new Promise(r => setTimeout(r, 250));
    const res = await page.evaluate(PROBE);
    totalChecked += res.checked; totalBad += res.bad.length;
    for (const b of res.bad) {
      const key = `${b.tag}${b.slot ? "[" + b.slot + "]" : ""}`;
      const e = byKind.get(key) || { n: 0, routes: new Set(), names: new Set(), cls: b.cls };
      e.n++; e.routes.add(route); if (b.name) e.names.add(b.name);
      byKind.set(key, e);
    }
    console.log(`${res.bad.length ? "FAIL" : "ok  "} ${route}  (${res.checked} focusable, ${res.bad.length} with no visible focus change)`);
  } catch (e) {
    console.log(`ERR  ${route}  ${String(e).slice(0, 80)}`);
  }
}
await browser.close();

console.log(`\nfocused ${totalChecked} controls across ${routes.length} routes`);
console.log(`no visible focus indicator: ${totalBad}`);
if (byKind.size) {
  console.log(`\nby element kind:`);
  for (const [k, v] of [...byKind].sort((a, b) => b[1].n - a[1].n).slice(0, 20)) {
    console.log(`  ${String(v.n).padStart(4)}x  ${k}  | routes: ${[...v.routes].slice(0, 3).join(", ")}` +
      (v.names.size ? `  | e.g. "${[...v.names][0]}"` : "") + (v.cls ? `  | focus classes: ${v.cls}` : "  | NO focus classes"));
  }
}
process.exit(totalBad ? 1 : 0);

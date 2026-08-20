#!/usr/bin/env node
// Post-fix interactive a11y verification: dialog trap, menu roles+close,
// select enter-close, radiogroup roving+arrows, menubar arrows, header overflow,
// mobile nav reachability, favicon. Prints PASS/FAIL per check.
import path from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const BASE = "http://127.0.0.1:53185";

async function loadPlaywright() {
  try { return await import("playwright"); }
  catch {
    const fb = path.join(repoRoot, "audit", "node_modules", "playwright", "index.mjs");
    return await import(`file:///${fb.replaceAll("\\", "/")}`);
  }
}
const sleep = ms => new Promise(r => setTimeout(r, ms));
let pass = 0, fail = 0;
const check = (name, ok, detail = "") => { console.log(`${ok ? "PASS" : "FAIL"}  ${name}${ok ? "" : "  — " + detail}`); ok ? pass++ : fail++; };

async function ready(page) {
  await page.waitForFunction(`(() => { const m = document.querySelector("main"); return m && m.innerText.length > 50; })()`, null, { timeout: 60000 });
  await sleep(500);
}

async function main() {
  const { chromium } = await loadPlaywright();
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
  const http404 = [];
  page.on("response", r => { if (r.status() === 404) http404.push(r.url()); });

  // ---- DIALOG ----
  await page.goto(BASE + "/docs/components/dialog"); await ready(page);
  {
    const t = page.locator('[data-slot="dialog-trigger"]').first();
    await t.scrollIntoViewIfNeeded(); await t.focus(); await page.keyboard.press("Enter"); await sleep(500);
    const s = await page.evaluate(`(() => { const c = document.querySelector('[data-slot="dialog-content"]'); return { open: !!c, modal: c?.getAttribute("aria-modal"), lbl: !!c?.getAttribute("aria-labelledby"), desc: c?.getAttribute("aria-describedby"), focusIn: c ? c.contains(document.activeElement) : false }; })()`);
    check("dialog: opens with aria-modal+labelledby, focus inside", s.open && s.modal === "true" && s.lbl && s.focusIn, JSON.stringify(s));
    check("dialog: aria-describedby wired", !!s.desc, "describedby=" + s.desc);
    await page.keyboard.press("Escape"); await sleep(400);
    const s2 = await page.evaluate(`(() => ({ closed: !document.querySelector('[data-slot="dialog-content"]'), restored: document.activeElement?.getAttribute("data-slot") === "dialog-trigger" }))()`);
    check("dialog: Escape closes + focus restored", s2.closed && s2.restored, JSON.stringify(s2));
  }

  // ---- DROPDOWN MENU ----
  await page.goto(BASE + "/docs/components/dropdown-menu"); await ready(page);
  {
    const t = page.locator('[data-slot="dropdown-menu-trigger"]').first();
    await t.scrollIntoViewIfNeeded(); await t.click(); await sleep(500);
    const s = await page.evaluate(`(() => { const c = document.querySelector('[data-slot="dropdown-menu-content"]'); return { role: c?.getAttribute("role"), controls: !!document.querySelector('[data-slot="dropdown-menu-trigger"][aria-controls]') }; })()`);
    check("dropdown: content role=menu", s.role === "menu", "role=" + s.role);
    check("dropdown: trigger aria-controls set when open", s.controls);
    // click an item -> menu closes
    await page.locator('[data-slot="dropdown-menu-item"]').first().click(); await sleep(500);
    const closed = await page.evaluate(`!document.querySelector('[data-slot="dropdown-menu-content"]')`);
    check("dropdown: item click closes menu", closed);
    // ArrowDown on closed trigger opens
    await t.focus(); await page.keyboard.press("ArrowDown"); await sleep(500);
    const reopened = await page.evaluate(`!!document.querySelector('[data-slot="dropdown-menu-content"]')`);
    check("dropdown: ArrowDown on trigger opens", reopened);
    await page.keyboard.press("Escape"); await sleep(300);
  }

  // ---- SELECT ----
  await page.goto(BASE + "/docs/components/select"); await ready(page);
  {
    const t = page.locator('[data-slot="select-trigger"]').first();
    await t.scrollIntoViewIfNeeded();
    const roleInfo = await t.evaluate(el => ({ role: el.getAttribute("role"), controls: el.getAttribute("aria-controls") }));
    check("select: trigger role=combobox", roleInfo.role === "combobox", JSON.stringify(roleInfo));
    await t.focus(); await page.keyboard.press("Enter"); await sleep(500);
    await page.keyboard.press("ArrowDown"); await sleep(150);
    await page.keyboard.press("Enter"); await sleep(600);
    // content stays mounted (item registration) and closes via hidden/data-state
    const closed = await page.evaluate(`(() => { const c = document.querySelector('[data-slot="select-content"]'); return !c || c.hidden || c.getAttribute("data-state") === "closed"; })()`);
    check("select: Enter-select closes listbox", closed);
  }

  // ---- RADIO GROUP ----
  await page.goto(BASE + "/docs/components/radio-group"); await ready(page);
  {
    const info = await page.evaluate(`(() => { const g = document.querySelector('[data-slot="radio-group"]'); const items = [...g.querySelectorAll('[data-slot="radio-group-item"]')]; const tis = items.map(i => i.getAttribute("tabindex")); return { tis, oneTabbable: tis.filter(t => t === "0").length === 1 }; })()`);
    check("radiogroup: roving tabindex (exactly one 0)", info.oneTabbable, JSON.stringify(info.tis));
    const first = page.locator('[data-slot="radio-group-item"][tabindex="0"]').first();
    await first.scrollIntoViewIfNeeded(); await first.focus();
    await page.keyboard.press("ArrowDown"); await sleep(300);
    const after = await page.evaluate(`(() => { const ae = document.activeElement; return { slot: ae?.getAttribute("data-slot"), checked: ae?.getAttribute("aria-checked") }; })()`);
    check("radiogroup: ArrowDown moves focus AND selects", after.slot === "radio-group-item" && after.checked === "true", JSON.stringify(after));
  }

  // ---- MENUBAR ----
  await page.goto(BASE + "/docs/components/menubar"); await ready(page);
  {
    const first = page.locator('[data-slot="menubar-trigger"]').first();
    await first.scrollIntoViewIfNeeded(); await first.focus();
    const t1 = await page.evaluate(`document.activeElement?.innerText`);
    await page.keyboard.press("ArrowRight"); await sleep(250);
    const t2 = await page.evaluate(`document.activeElement?.innerText`);
    check("menubar: ArrowRight moves between triggers", t1 !== t2 && !!t2, `${t1} -> ${t2}`);
    const menuRole = await page.evaluate(`(() => { const t = document.querySelector('[data-slot="menubar-trigger"]'); t.click(); return new Promise(res => setTimeout(() => { const c = document.querySelector('[data-slot="menubar-content"]'); res(c?.getAttribute("role")); t.click(); }, 400)); })()`);
    check("menubar: content role=menu", menuRole === "menu", "role=" + menuRole);
  }

  // ---- TOOLTIP semantics ----
  await page.goto(BASE + "/docs/components/tooltip"); await ready(page);
  {
    const t = page.locator('[data-slot="tooltip-trigger"]').first();
    await t.scrollIntoViewIfNeeded(); await t.hover(); await sleep(700);
    const s = await page.evaluate(`(() => { const c = document.querySelector('[data-slot="tooltip-content"]'); const trig = document.querySelector('[data-slot="tooltip-trigger"]'); return { role: c?.getAttribute("role"), described: !!(trig?.getAttribute("aria-describedby")), pe: c ? getComputedStyle(c).pointerEvents : null }; })()`);
    check("tooltip: role=tooltip + trigger describedby", s.role === "tooltip" && s.described, JSON.stringify(s));
    check("tooltip: pointer-events stays none (flicker guard)", s.pe === "none", "pe=" + s.pe);
  }

  // ---- HEADER overflow at 768 ----
  await page.setViewportSize({ width: 768, height: 900 }); await sleep(400);
  {
    const sw = await page.evaluate(`Math.max(document.documentElement.scrollWidth, document.body.scrollWidth)`);
    check("header: no horizontal overflow at 768px", sw <= 769, "scrollWidth=" + sw);
  }
  // ---- 375 mobile: docs nav reachable via sheet ----
  await page.setViewportSize({ width: 375, height: 800 }); await sleep(400);
  {
    const sw375 = await page.evaluate(`Math.max(document.documentElement.scrollWidth, document.body.scrollWidth)`);
    check("mobile 375: no horizontal overflow", sw375 <= 376, "scrollWidth=" + sw375);
    const burger = page.locator('header button[aria-label="Menu"]');
    if (await burger.count()) {
      await burger.click(); await sleep(600);
      const links = await page.evaluate(`document.querySelectorAll('[data-slot="sheet-content"] a').length`);
      check("mobile: sheet menu contains docs navigation (>20 links)", links > 20, links + " links");
      // navigate closes sheet
      const link = page.locator('[data-slot="sheet-content"] a').nth(8);
      await link.click(); await sleep(800);
      const sheetGone = await page.evaluate(`!document.querySelector('[data-slot="sheet-content"]')`);
      check("mobile: sheet closes on navigation", sheetGone);
    } else check("mobile: hamburger present", false, "no button[aria-label=Menu]");
    const search = await page.evaluate(`!!document.querySelector('header [aria-label*="Search" i], header [aria-label*="search" i]')`);
    check("mobile: search reachable in header", search);
  }

  check("no favicon/resource 404s", http404.length === 0, http404.slice(0, 3).join(", "));
  await browser.close();
  console.log(`\n${pass} passed, ${fail} failed`);
  process.exit(fail ? 1 : 0);
}
main().catch(e => { console.error(e); process.exit(2); });

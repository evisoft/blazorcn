#!/usr/bin/env node
// Verifies the app under OS-level accessibility settings, which the plain sweep
// cannot see: prefers-reduced-motion, forced-colors (Windows High Contrast) and
// 200% text zoom (WCAG 1.4.4). Run with the dev server already up.
//   node scripts/verify-a11y-settings.mjs
import { chromium } from "./../audit/node_modules/playwright/index.mjs";

const BASE = "http://127.0.0.1:53185";
const results = [];
const check = (name, ok, detail = "") => {
  results.push({ name, ok, detail });
  console.log(`${ok ? "PASS" : "FAIL"}  ${name}${detail ? "  — " + detail : ""}`);
};

const ready = (page) =>
  page.waitForFunction(() => {
    const m = document.querySelector("main");
    return m && m.innerText.trim().length > 40;
  }, { timeout: 90000 });

const browser = await chromium.launch();

// ---------------------------------------------------------------- reduced motion
{
  const ctx = await browser.newContext({ reducedMotion: "reduce" });
  const page = await ctx.newPage();
  await page.goto(`${BASE}/docs/components/dialog`, { waitUntil: "load" });
  await ready(page);

  // Open a dialog: its enter animation must be effectively instant.
  const trigger = page.locator('[data-slot="dialog-trigger"]').first();
  await trigger.click();
  await page.waitForSelector('[data-slot="dialog-content"]', { timeout: 15000 });
  const dlg = await page.evaluate(() => {
    const el = document.querySelector('[data-slot="dialog-content"]');
    const cs = getComputedStyle(el);
    return { animationDuration: cs.animationDuration, transitionDuration: cs.transitionDuration };
  });
  const ms = (v) => Math.max(...String(v).split(",").map((s) => (s.trim().endsWith("ms") ? parseFloat(s) : parseFloat(s) * 1000)));
  check("reduced-motion: dialog enter animation suppressed", ms(dlg.animationDuration) <= 1,
    `animation-duration=${dlg.animationDuration}`);
  await page.keyboard.press("Escape");

  // Status animations must survive: spinner keeps spinning.
  await page.goto(`${BASE}/docs/components/spinner`, { waitUntil: "load" });
  await ready(page);
  const spin = await page.evaluate(() => {
    const el = document.querySelector(".animate-spin");
    if (!el) return null;
    return getComputedStyle(el).animationDuration;
  });
  check("reduced-motion: spinner still animates (status, not decoration)",
    spin !== null && parseFloat(spin) >= 0.5, `animation-duration=${spin}`);

  // Smooth scrolling must be off.
  const sb = await page.evaluate(() => getComputedStyle(document.documentElement).scrollBehavior);
  check("reduced-motion: scroll-behavior is auto", sb === "auto", `scroll-behavior=${sb}`);

  // Suppressing animation must not break components whose REVEAL is animated —
  // they must still end up at their final geometry, not stuck at frame zero.
  for (const [name, route, trigger, content, assert] of [
    ["accordion expands", "/docs/components/accordion", "accordion-trigger", "accordion-content", (r) => r.height > 5],
    ["collapsible expands", "/docs/components/collapsible", "collapsible-trigger", "collapsible-content", (r) => r.height > 5],
    ["sheet reaches final position", "/docs/components/sheet", "sheet-trigger", "sheet-content",
      (r, vw) => r.left < vw && r.right > 0 && r.width > 50],
  ]) {
    await page.goto(BASE + route, { waitUntil: "load" });
    await ready(page);
    await page.locator(`[data-slot="${trigger}"]`).first().click();
    await page.waitForTimeout(300);
    const box = await page.evaluate((slot) => {
      const el = document.querySelector(`[data-slot="${slot}"]`);
      if (!el) return null;
      const r = el.getBoundingClientRect();
      return { height: Math.round(r.height), left: Math.round(r.left), right: Math.round(r.right),
               width: Math.round(r.width), vw: window.innerWidth };
    }, content);
    check(`reduced-motion: ${name}`, !!box && assert(box, box.vw),
      box ? `h=${box.height} x=[${box.left},${box.right}]` : "content not found");
    if (trigger === "sheet-trigger") await page.keyboard.press("Escape");
  }
  await ctx.close();
}

// ----------------------------------------------------------------- forced colors
{
  const ctx = await browser.newContext({ forcedColors: "active" });
  const page = await ctx.newPage();
  await page.goto(`${BASE}/docs/components/button`, { waitUntil: "load" });
  await ready(page);

  const focusRing = await page.evaluate(() => {
    const btn = document.querySelector('[data-slot="button"], button');
    btn.focus();
    const cs = getComputedStyle(btn);
    return { outlineWidth: cs.outlineWidth, outlineStyle: cs.outlineStyle };
  });
  check("forced-colors: focus ring visible on button",
    parseFloat(focusRing.outlineWidth) >= 1 && focusRing.outlineStyle !== "none",
    `outline=${focusRing.outlineWidth} ${focusRing.outlineStyle}`);

  await page.goto(`${BASE}/docs/components/switch`, { waitUntil: "load" });
  await ready(page);
  const sw = await page.evaluate(() => {
    const on = document.querySelector('[data-slot="switch"][data-state="checked"]');
    const off = document.querySelector('[data-slot="switch"][data-state="unchecked"]');
    if (!on || !off) return null;
    return { on: getComputedStyle(on).backgroundColor, off: getComputedStyle(off).backgroundColor };
  });
  check("forced-colors: checked switch distinguishable from unchecked",
    sw !== null && sw.on !== sw.off, sw ? `${sw.on} vs ${sw.off}` : "no switch pair found");
  await ctx.close();
}

// ------------------------------------------------------------------- 200 % text
{
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });
  const page = await ctx.newPage();
  const routes = ["/", "/docs/components/button", "/docs/components/dialog", "/docs/blocks/pricing", "/examples/dashboard"];
  const bad = [];
  for (const r of routes) {
    await page.goto(BASE + r, { waitUntil: "load" });
    await ready(page);
    await page.addStyleTag({ content: "html{font-size:200% !important}" });
    await page.waitForTimeout(250);
    // Ground truth for overflow: can the document actually be scrolled sideways?
    const overflow = await page.evaluate(() => {
      window.scrollTo(99999, 0);
      const x = window.scrollX;
      window.scrollTo(0, 0);
      return x;
    });
    if (overflow > 1) bad.push(`${r} (scrollX=${overflow})`);
  }
  check("200% text zoom: no horizontal document overflow", bad.length === 0, bad.join("; "));
  await ctx.close();
}

// ------------------------------------------------------------- touch (no hover)
{
  // A touch device has no hover, so anything revealed only by :hover is invisible to
  // it. Tapping focuses the trigger, and both components open on focus — verify that
  // holds, and that a popup opened on a 390px phone stays inside the viewport.
  const ctx = await browser.newContext({
    hasTouch: true, isMobile: true, viewport: { width: 390, height: 844 },
    userAgent: "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 " +
               "(KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
  });
  const page = await ctx.newPage();

  await page.goto(`${BASE}/docs/components/tooltip`, { waitUntil: "load" });
  await ready(page);
  await page.locator('[data-slot="tooltip-trigger"]').first().tap();
  await page.waitForTimeout(700);
  const tip = await page.evaluate(() => {
    const t = document.querySelector('[role="tooltip"]');
    return { visible: !!t && t.getBoundingClientRect().height > 4 };
  });
  check("touch: tooltip is reachable by tap (not hover-only)", tip.visible);

  await page.goto(`${BASE}/docs/components/dropdown-menu`, { waitUntil: "load" });
  await ready(page);
  await page.locator('[data-slot="dropdown-menu-trigger"]').first().tap();
  await page.waitForTimeout(500);
  const menu = await page.evaluate(() => {
    const m = document.querySelector('[role="menu"]');
    if (!m) return null;
    const r = m.getBoundingClientRect();
    return { visible: r.height > 4, inViewport: r.right <= window.innerWidth + 1 && r.left >= -1 };
  });
  check("touch: dropdown opens on tap and fits the phone viewport",
    !!menu && menu.visible && menu.inViewport, menu ? JSON.stringify(menu) : "no menu");
  await ctx.close();
}

// -------------------------------------------------------------------- skip link
{
  const ctx = await browser.newContext();
  const page = await ctx.newPage();
  await page.goto(`${BASE}/docs/components/button`, { waitUntil: "load" });
  await ready(page);
  // Blazor's <FocusOnNavigate Selector="h1"> parks focus on the page heading after
  // routing, so Tab from there walks forward into the content. Reset to the document
  // start first — that is the state a user lands in on a fresh load / after Ctrl+Home.
  await page.evaluate(() => {
    document.activeElement?.blur();
    document.body.setAttribute("tabindex", "-1");
    document.body.focus();
    document.body.removeAttribute("tabindex");
  });
  await page.keyboard.press("Tab");
  const first = await page.evaluate(() => {
    const a = document.activeElement;
    return { tag: a?.tagName, text: a?.textContent?.trim(), href: a?.getAttribute("href"),
             visible: a ? a.getBoundingClientRect().width > 1 : false };
  });
  check("skip link is the first tab stop and becomes visible",
    first.tag === "A" && first.href === "#main-content" && first.visible,
    `first stop: <${first.tag}> "${first.text}" href=${first.href} visible=${first.visible}`);

  await page.keyboard.press("Enter");
  await page.waitForTimeout(300);
  // Either <main> itself takes focus, or FocusOnNavigate re-fires on the fragment
  // navigation and lands on the page <h1> inside it. Both put focus past the nav,
  // which is what 2.4.1 asks for — so assert containment, not a specific element.
  const landed = await page.evaluate(() => {
    const a = document.activeElement;
    const main = document.getElementById("main-content");
    return { inMain: !!(a && main && (a === main || main.contains(a))),
             where: a?.id ? "#" + a.id : a?.tagName };
  });
  check("skip link moves focus into main content", landed.inMain, `focus now on: ${landed.where}`);
  await ctx.close();
}

await browser.close();

const failed = results.filter((r) => !r.ok);
console.log(`\n${results.length - failed.length} passed, ${failed.length} failed`);
process.exit(failed.length ? 1 : 0);

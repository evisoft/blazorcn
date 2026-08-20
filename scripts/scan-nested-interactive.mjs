#!/usr/bin/env node
// Static detector for axe `nested-interactive`: an element whose ARIA role makes its
// descendants presentational (button, checkbox, radio, switch, tab, option, menuitem*,
// progressbar, slider, img) must not CONTAIN a focusable element. Nested <button> is also
// invalid HTML — the parser auto-closes the outer one under Static SSR, so the markup that
// ships is not the markup that was written.
//
// Why a real scanner and not grep: the wrapper and the nested control are usually many
// lines apart, so the pattern only shows up with depth-tracked tag matching. Razor
// components are resolved to the element they render (a table below), because
// `<CheckboxCn/>` IS a <button role="checkbox">.
//
//   node scripts/scan-nested-interactive.mjs            # human summary
//   node scripts/scan-nested-interactive.mjs --json     # audit/nested-interactive-static.json
import { readFile, writeFile, readdir } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const SCAN_DIRS = ["docs/BlazorCN.Demo", "src/BlazorCN"];

// BlazorCN components that render a focusable element (verified against src/).
const FOCUSABLE_COMPONENTS = [
  "ButtonCn", "CheckboxCn", "SwitchCn", "InputCn", "TextareaCn", "SliderCn",
  "RadioGroupItemCn", "ToggleCn", "ToggleGroupItemCn", "NativeSelectCn", "InputOtpCn",
  "PaginationLinkCn", "PaginationNextCn", "PaginationPreviousCn", "CommandInputCn",
  "SidebarMenuButtonCn", "SidebarMenuActionCn", "SidebarTriggerCn", "CarouselPreviousCn",
  "CarouselNextCn", "BreadcrumbLinkCn", "NavigationMenuLinkCn", "ComboboxCn", "SelectCn",
  "DropdownMenuCn", "PopoverCn", "DialogCn", "SheetCn", "DrawerCn", "AlertDialogCn",
  "MenubarCn", "TabsCn", "AccordionCn", "CollapsibleCn", "CommandCn", "CalendarCn",
];
// Components whose ROOT element makes descendants presentational.
const PRESENTATIONAL_ROOT_COMPONENTS = ["CheckboxCn", "SwitchCn", "RadioGroupItemCn", "ToggleCn", "TabsTriggerCn"];
const PRESENTATIONAL_ROLES = new Set([
  "button", "checkbox", "radio", "switch", "tab", "option", "menuitem",
  "menuitemcheckbox", "menuitemradio", "progressbar", "slider", "img", "meter",
]);
const VOID = new Set(["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr"]);

const TAG = /<\/?([A-Za-z][\w.-]*)((?:"[^"]*"|'[^']*'|[^>"'])*?)(\/?)>/g;

function focusableInside(body) {
  const hits = [];
  if (/<button\b/i.test(body)) hits.push("<button>");
  if (/<a\s[^>]*href/i.test(body)) hits.push("<a href>");
  if (/<(input|select|textarea|summary)\b/i.test(body)) hits.push("<input/select/textarea>");
  if (/tabindex=""?"?0/i.test(body)) hits.push('tabindex="0"');
  if (/role=""?"?(button|link|checkbox|radio|switch|tab|menuitem)/i.test(body)) hits.push("role=widget");
  for (const c of FOCUSABLE_COMPONENTS) if (new RegExp(`<${c}\\b`).test(body)) hits.push(`<${c}>`);
  for (const c of [...FOCUSABLE_COMPONENTS].filter(x => /TriggerCn$/.test(x))) if (new RegExp(`<${c}\\b`).test(body)) hits.push(`<${c}>`);
  if (/<[A-Za-z]+TriggerCn\b/.test(body)) hits.push("<*TriggerCn>");
  return [...new Set(hits)];
}

// The wrapper's own attributes/tag decide whether its descendants are presentational.
function wrapperKind(tag, attrs) {
  const role = /role=""?"?([a-z]+)/i.exec(attrs)?.[1]?.toLowerCase();
  if (role && PRESENTATIONAL_ROLES.has(role)) return `role="${role}"`;
  if (role) return null;                              // an explicit non-widget role wins
  if (tag.toLowerCase() === "button") return "<button>";
  if (PRESENTATIONAL_ROOT_COMPONENTS.includes(tag)) return `<${tag}>`;
  return null;
}

async function* walk(dir) {
  for (const e of await readdir(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { if (!/^(bin|obj|node_modules)$/.test(e.name)) yield* walk(p); }
    else if (e.name.endsWith(".razor")) yield p;
  }
}

// The <Name>Code samples in *BlocksPage.razor are C# verbatim strings whose quotes are all
// DOUBLED. `class=""x""` defeats the quote tracking in TAG (it reads as an empty string, then
// bare text, then another empty string), so a `>` inside a doubled attribute value — e.g. the
// `=>` of an @onclick lambda — ends the tag early and invents a wrapper that isn't there.
// Skipping the samples costs no coverage: verify-code-samples.mjs already pins every sample to
// be identical to its live block, and the live blocks are scanned here directly.
const stripVerbatimStrings = (src) => {
  let out = "", i = 0;
  while (i < src.length) {
    const at = src.indexOf('@"', i);
    if (at === -1) { out += src.slice(i); break; }
    out += src.slice(i, at);
    let j = at + 2;
    while (j < src.length) {
      if (src[j] !== '"') { j++; continue; }
      if (src[j + 1] === '"') { j += 2; continue; }
      break;
    }
    out += src.slice(at, j + 1).replace(/[^\n]/g, "");   // keep line numbers accurate
    i = j + 1;
  }
  return out;
};

const findings = [];
for (const dir of SCAN_DIRS) {
  for await (const file of walk(path.join(repoRoot, dir))) {
    const src = stripVerbatimStrings(await readFile(file, "utf8"));
    const rel = path.relative(repoRoot, file).replace(/\\/g, "/");
    // Collect every tag once, then match opens to closes by depth per tag name.
    const tags = [];
    for (const m of src.matchAll(TAG)) {
      tags.push({ closing: m[0][1] === "/", name: m[1], attrs: m[2] || "",
                  self: m[3] === "/" || VOID.has(m[1].toLowerCase()), start: m.index, end: m.index + m[0].length });
    }
    for (let i = 0; i < tags.length; i++) {
      const open = tags[i];
      if (open.closing || open.self) continue;
      const kind = wrapperKind(open.name, open.attrs);
      if (!kind) continue;
      let depth = 1, close = null;
      for (let j = i + 1; j < tags.length; j++) {
        if (tags[j].name !== open.name || tags[j].self) continue;
        depth += tags[j].closing ? -1 : 1;
        if (depth === 0) { close = tags[j]; break; }
      }
      if (!close) continue;
      const body = src.slice(open.end, close.start);
      const inner = focusableInside(body);
      if (!inner.length) continue;
      findings.push({
        file: rel, line: src.slice(0, open.start).split("\n").length,
        wrapper: kind, wrapperTag: open.name, inner,
        library: rel.startsWith("src/"),
        sample: /BlocksPage\.razor$|Page\.razor$/.test(path.basename(rel)) && /""/.test(open.attrs + body.slice(0, 200)),
        group: rel.split("/").slice(0, 6).filter(s => !s.endsWith(".razor")).pop() || "-",
        excerpt: src.slice(open.start, open.start + 120).replace(/\s+/g, " "),
      });
    }
  }
}

if (process.argv.includes("--json")) {
  await writeFile(path.join(repoRoot, "audit", "nested-interactive-static.json"), JSON.stringify(findings, null, 1));
}
const by = (fn) => findings.reduce((m, f) => (m.set(fn(f), (m.get(fn(f)) || 0) + 1), m), new Map());
const show = (title, m, n = 25) => {
  console.log(`\n${title}`);
  for (const [k, v] of [...m].sort((a, b) => b[1] - a[1]).slice(0, n)) console.log(`  ${String(v).padStart(4)}  ${k}`);
};
console.log(`nested-interactive candidates: ${findings.length}`);
console.log(`  library (src/):  ${findings.filter(f => f.library).length}`);
console.log(`  demo live:       ${findings.filter(f => !f.library && !f.sample).length}`);
console.log(`  demo samples:    ${findings.filter(f => f.sample).length}`);
show("by wrapper kind:", by(f => f.wrapper));
show("by inner control:", by(f => f.inner.join(" + ")), 15);
show("by group:", by(f => f.group), 30);
show("worst files:", by(f => f.file), 20);

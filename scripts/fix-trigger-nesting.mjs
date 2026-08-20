#!/usr/bin/env node
// Un-nests <XTriggerCn><ButtonCn ...>content</ButtonCn></XTriggerCn> into
// <XTriggerCn Class="cn-button ...variant...size..." data-variant data-size>content</XTriggerCn>
// — removes invalid button-in-button nesting across the demo. Handles the
// doubled-quote variant inside @code verbatim strings. Dry-run by default; --write applies.
import { readFile, writeFile } from "node:fs/promises";
import { glob } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { findTagEnd } from "./_tagscan.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const WRITE = process.argv.includes("--write");
const TARGET = path.join(repoRoot, "docs", "BlazorCN.Demo", "Pages");

const TRIGGERS = [
  "DialogTriggerCn", "DialogCloseCn", "AlertDialogTriggerCn", "SheetTriggerCn", "SheetCloseCn",
  "DrawerTriggerCn", "DrawerCloseCn", "PopoverTriggerCn", "DropdownMenuTriggerCn",
  "CollapsibleTriggerCn", "TooltipTriggerCn", "HoverCardTriggerCn",
];

const BASE = "cn-button group/button inline-flex shrink-0 items-center justify-center whitespace-nowrap transition-all outline-none select-none disabled:pointer-events-none disabled:opacity-50 [&_svg]:pointer-events-none [&_svg]:shrink-0";
const VARIANT = { Default: "cn-button-variant-default", Destructive: "cn-button-variant-destructive", Outline: "cn-button-variant-outline", Secondary: "cn-button-variant-secondary", Ghost: "cn-button-variant-ghost", Link: "cn-button-variant-link" };
const SIZE = { Default: "cn-button-size-default", Xs: "cn-button-size-xs", Sm: "cn-button-size-sm", Lg: "cn-button-size-lg", Icon: "cn-button-size-icon", IconXs: "cn-button-size-icon-xs", IconSm: "cn-button-size-icon-sm", IconLg: "cn-button-size-icon-lg" };

const ICON_LABELS = { X: "Close", MoreHorizontal: "More options", MoreVertical: "More options", Ellipsis: "More options", EllipsisVertical: "More options", ChevronDown: "Open menu", ChevronsUpDown: "Toggle", Menu: "Menu", Settings: "Settings", Settings2: "Settings", Plus: "Add", Trash2: "Delete", Pencil: "Edit", Info: "Information", CircleHelp: "Help", Calendar: "Open calendar", Filter: "Filter", Funnel: "Filter", Share2: "Share", Bell: "Notifications", User: "Account" };
function labelForIcon(name) {
  if (ICON_LABELS[name]) return ICON_LABELS[name];
  const words = name.replace(/([a-z0-9])([A-Z])/g, "$1 $2").split(" ").filter(w => !/^\d+$/.test(w));
  const l = words.join(" ").toLowerCase();
  return l.charAt(0).toUpperCase() + l.slice(1);
}

// parse attributes from a tag body like ` Variant="ButtonVariant.Outline" Class="w-full"` (q='"' or '""')
function parseAttrs(body, q) {
  const attrs = {};
  const re = new RegExp(`([@:\\w-]+)=${q}([^"]*?)${q}`, "g");
  let m;
  while ((m = re.exec(body)) !== null) attrs[m[1]] = m[2];
  // flag-style attrs (rare)
  return attrs;
}

function processContent(src, q, fileRel, notes) {
  let changed = 0;
  for (const trig of TRIGGERS) {
    let idx = 0;
    for (;;) {
      const open = src.indexOf(`<${trig}`, idx);
      if (open === -1) break;
      const openEnd = findTagEnd(src, open); // quote-aware: attr values may hold '>'
      if (openEnd === -1) break;
      const trigTagBody = src.slice(open + trig.length + 1, openEnd);
      if (trigTagBody.endsWith("/")) { idx = openEnd; continue; } // self-closing trigger
      // next non-space must be <ButtonCn
      let p = openEnd + 1;
      while (p < src.length && /\s/.test(src[p])) p++;
      if (!src.startsWith("<ButtonCn", p)) { idx = openEnd; continue; }
      const btnOpenEnd = findTagEnd(src, p);
      if (btnOpenEnd === -1) break;
      let btnBody = src.slice(p + "<ButtonCn".length, btnOpenEnd);
      const selfClosed = btnBody.trimEnd().endsWith("/");
      let inner, afterBtn;
      if (selfClosed) { inner = ""; afterBtn = btnOpenEnd + 1; }
      else {
        const btnClose = src.indexOf("</ButtonCn>", btnOpenEnd);
        if (btnClose === -1) { idx = openEnd; continue; }
        // no nested ButtonCn inside
        inner = src.slice(btnOpenEnd + 1, btnClose);
        if (inner.includes("<ButtonCn")) { idx = openEnd; continue; }
        afterBtn = btnClose + "</ButtonCn>".length;
      }
      // after button: optional whitespace then </trig>
      let p2 = afterBtn;
      while (p2 < src.length && /\s/.test(src[p2])) p2++;
      if (!src.startsWith(`</${trig}>`, p2)) { idx = openEnd; continue; }

      // Quote-style evidence guard: only transform when the matched tags carry
      // at least one attribute in THIS pass's quote style. A bare
      // <XTriggerCn><ButtonCn>… has no evidence, and guessing wrong inside a
      // verbatim Code string terminates the C# string — skip those.
      const evidence = trigTagBody + " " + btnBody;
      // Razor expression attrs like aria-invalid="@(x ? "true" : null)" carry
      // nested quotes that our flat parseAttrs cannot round-trip — skip the match.
      if (/=("|"")@\(/.test(evidence)) {
        notes.push(`SKIP (expression attr): ${fileRel} @${trig}`);
        idx = openEnd + 1; continue;
      }
      const hasDoubled = /=""/.test(evidence);
      const hasSingle = /="[^"]/.test(evidence);
      if (q === '"' ? (!hasSingle || hasDoubled) : !hasDoubled) {
        if (evidence.trim()) notes.push(`SKIP (no ${q} quote evidence): ${fileRel} @${trig}`);
        idx = openEnd + 1; continue;
      }
      const attrs = parseAttrs(btnBody, q);
      // skip risky instances
      if (attrs["OnClick"] || attrs["@onclick"] || attrs["Href"] || attrs["Disabled"] || attrs["Type"]) {
        notes.push(`SKIP (handler/href/disabled): ${fileRel} @${trig}`);
        idx = openEnd + 1; continue;
      }
      const variantName = (attrs["Variant"] || "ButtonVariant.Default").replace("ButtonVariant.", "");
      const sizeName = (attrs["Size"] || "ButtonSize.Default").replace("ButtonSize.", "");
      const vClass = VARIANT[variantName], sClass = SIZE[sizeName];
      if (!vClass || !sClass) { notes.push(`SKIP (dynamic variant): ${fileRel} @${trig}`); idx = openEnd + 1; continue; }
      if ((attrs["Class"] || "").includes("@")) { notes.push(`SKIP (dynamic class): ${fileRel} @${trig}`); idx = openEnd + 1; continue; }

      const trigAttrs = parseAttrs(trigTagBody, q);
      // lowercase class= lands in the splat and would duplicate Class= (RZ10009) — merge it too
      const mergedClass = [BASE, vClass, sClass, attrs["Class"] || "", attrs["class"] || "", trigAttrs["Class"] || "", trigAttrs["class"] || ""].filter(Boolean).join(" ");
      if ((trigAttrs["class"] || attrs["class"] || "").includes("@")) { notes.push(`SKIP (dynamic class): ${fileRel} @${trig}`); idx = openEnd + 1; continue; }

      // accessible name for icon-only content
      const textOutsideTags = inner.replace(/<[^>]+>/g, " ").replace(/@[\w.()]+/g, " ").trim();
      let ariaLabel = attrs["aria-label"] || trigAttrs["aria-label"] || null;
      if (!ariaLabel && !textOutsideTags && !/sr-only/.test(inner)) {
        const im = inner.match(/<Lucide(?!IconCn)([A-Za-z0-9]+)Cn\b/) || inner.match(new RegExp(`<LucideIconCn[^>]*Name=${q}([a-z0-9-]+)${q}`));
        if (im) {
          let icon = im[1];
          if (icon.includes("-")) icon = icon.split("-").map(w => w[0].toUpperCase() + w.slice(1)).join("");
          ariaLabel = labelForIcon(icon);
        }
      }

      // rebuild trigger tag body: original attrs minus Class/class/aria-label, plus merged
      let newBody = trigTagBody
        .replace(new RegExp(`\\s*Class=${q}[^"]*?${q}`), "")
        .replace(new RegExp(`\\s*class=${q}[^"]*?${q}`), "")
        .replace(new RegExp(`\\s*aria-label=${q}[^"]*?${q}`), "");
      const extra = [
        `Class=${q}${mergedClass}${q}`,
        `data-variant=${q}${variantName.toLowerCase()}${q}`,
        `data-size=${q}${sizeName.toLowerCase()}${q}`,
      ];
      if (ariaLabel) extra.push(`aria-label=${q}${ariaLabel}${q}`);
      // transfer misc safe attrs from button (id, title, data-*, aria-*)
      for (const [k, v] of Object.entries(attrs)) {
        if (["Variant", "Size", "Class", "class", "aria-label"].includes(k)) continue;
        if (/^(id|title|tabindex|data-[\w-]+|aria-[\w-]+)$/.test(k)) extra.push(`${k}=${q}${v}${q}`);
      }
      const newOpen = `<${trig}${newBody.replace(/\s+$/, "")} ${extra.join(" ")}>`;
      const innerTrimmed = selfClosed ? "" : inner;
      const replacement = newOpen + innerTrimmed + `</${trig}>`;
      src = src.slice(0, open) + replacement + src.slice(p2 + `</${trig}>`.length);
      idx = open + replacement.length;
      changed++;
    }
  }
  return { src, changed };
}

async function main() {
  const files = [];
  for await (const f of glob(TARGET.replaceAll("\\", "/") + "/**/*.razor")) files.push(f);
  let total = 0, fileCount = 0;
  const notes = [];
  for (const f of files) {
    let src = await readFile(f, "utf8");
    const rel = path.relative(repoRoot, f);
    let r1 = processContent(src, '"', rel, notes);
    let r2 = processContent(r1.src, '""', rel, notes);
    const changed = r1.changed + r2.changed;
    if (changed) {
      fileCount++; total += changed;
      if (WRITE) await writeFile(f, r2.src);
    }
  }
  console.log(`${WRITE ? "APPLIED" : "DRY-RUN"}: ${total} un-nestings across ${fileCount} files`);
  for (const n of notes.slice(0, 30)) console.log("  " + n);
  if (notes.length > 30) console.log(`  ...and ${notes.length - 30} more notes`);
}
main().catch(e => { console.error(e); process.exit(1); });

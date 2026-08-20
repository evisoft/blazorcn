#!/usr/bin/env node
// A label whose `for` points at an id that does not exist is INERT — and silently so. There is
// no fallback to a wrapped control: per the HTML spec a label's `for` attribute, when present,
// is the only association, so `<label for="ghost"><input></label>` labels nothing and clicking
// it does nothing. That is how the /docs/blocks/account theme picker ended up completely dead
// (the radio was `sr-only`, so the label WAS the entire control surface).
//
// This scans statically for `For=`/`for=` targets with no matching id in the same file.
//
//   node scripts/scan-dangling-labels.mjs
import { readFile } from "node:fs/promises";
import { glob } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");

// Code samples are verbatim C# strings holding a copy of a block. They are already gated by
// verify-code-samples.mjs (sample must equal its block), so scanning them would just double
// every finding — and their doubled quotes would confuse the attribute regexes.
const stripVerbatimStrings = (src) => {
  let out = "", i = 0;
  while (i < src.length) {
    const at = src.indexOf('@"', i);
    if (at === -1) { out += src.slice(i); break; }
    out += src.slice(i, at);
    let j = at + 2;
    while (j < src.length) {
      if (src[j] !== '"') { j++; continue; }
      if (src[j + 1] === '"') { j += 2; continue; }  // "" is an escaped quote, not the end
      break;
    }
    // Preserve newlines so reported line numbers stay accurate.
    out += src.slice(at, j + 1).replace(/[^\n]/g, "");
    i = j + 1;
  }
  return out;
};

// `id="@(idx == 0 ? "code" : $"otp-{idx}")"` is dynamic but CAN yield the literal "code".
// Pull every quoted literal out of an expression so those don't read as dangling.
const literalsIn = (value) => {
  if (!value.includes("@")) return [value];
  return [...value.matchAll(/"([^"{}@]+)"/g)].map((m) => m[1]);
};

const LABEL_RE = /<(?:LabelCn|FieldLabelCn|FormLabelCn)\b[^>]*?\bFor="([^"]*)"|<label\b[^>]*?\bfor="([^"]*)"/gi;
const ID_RE = /\b(?:id|Id|HtmlFor|For)="([^"]*)"/g;

const files = [];
for await (const f of glob("**/*.razor", { cwd: repoRoot })) {
  if (/[\\/](bin|obj|original|oldblazor|node_modules)[\\/]/.test(f)) continue;
  files.push(f);
}

let checked = 0;
const dangling = [];

for (const rel of files.sort()) {
  const raw = await readFile(path.join(repoRoot, rel), "utf8");
  const src = stripVerbatimStrings(raw);

  const ids = new Set();
  for (const m of src.matchAll(ID_RE)) for (const lit of literalsIn(m[1])) ids.add(lit);

  for (const m of src.matchAll(LABEL_RE)) {
    const target = m[1] ?? m[2];
    // A fully dynamic target (`For="@ctx.Id"`) cannot be resolved statically; assume it is fine
    // rather than emitting noise the reader has to re-triage every run.
    if (!target || (target.includes("@") && literalsIn(target).length === 0)) continue;
    checked++;
    const hit = literalsIn(target).some((lit) => ids.has(lit));
    if (!hit) {
      const line = src.slice(0, m.index).split("\n").length;
      dangling.push({ rel, line, target });
    }
  }
}

console.log(`razor files scanned:        ${files.length}`);
console.log(`label targets checked:      ${checked}`);
console.log(`dangling label targets:     ${dangling.length}`);
for (const d of dangling) console.log(`  ${d.rel}:${d.line}  for="${d.target}"`);
process.exit(dangling.length ? 1 : 0);

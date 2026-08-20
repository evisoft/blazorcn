#!/usr/bin/env node
// Static detector for select triggers that get NO accessible name.
//
// `SelectTriggerCn` renders `<button role="combobox">`, and per the accname spec a
// `combobox` does NOT take its name from its contents. So a select whose trigger visibly
// shows "United States" still has an EMPTY accessible name: axe reports `button-name` and a
// screen reader announces just "combobox". A visible caption next to it does not help
// unless it is wired up with `aria-label`, `aria-labelledby`, or a `<label for>` pointing at
// the trigger's id (a <button> is a labelable element, so label/for genuinely works — I
// verified that in Chrome).
//
// `ComboboxTriggerCn` is deliberately NOT checked: it renders no role, so it is a plain
// <button> named by its own text content. Including it produced 200+ false positives.
//
// The association that the demo actually uses is indirect:
//     <FieldLabelCn For="country">      ->  renders <label for="country">
//     <SelectCn Id="country">           ->  SelectTriggerCn renders id="@(Select?.TriggerId)"
// so the id lives on the PARENT SelectCn, not on the trigger. Model that or you get ~150
// false positives from the Contact blocks, which are in fact all correctly labelled.
//
//   node scripts/scan-unnamed-controls.mjs [--json]
import { readFile, writeFile, readdir } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");

// Razor tags span many lines and their attribute values contain both quotes and `>`
// (e.g. `@(a > b)`), so a regex cannot reliably find a tag's closing bracket. Walk the text
// tracking quote state and paren depth instead.
function tagEnd(src, from) {
  let inQuote = false, depth = 0;
  for (let i = from; i < src.length; i++) {
    const c = src[i];
    if (c === '"') { if (src[i + 1] === '"') { i++; continue; } inQuote = !inQuote; continue; }
    if (c === "(") depth++;
    else if (c === ")") depth--;
    else if (c === ">" && !inQuote && depth <= 0) return i;
  }
  return -1;
}

const attr = (attrs, name) =>
  new RegExp(`\\b${name}=(?:""([^"]*)""|"([^"]*)")`, "i").exec(attrs)?.slice(1).find(Boolean) ?? null;

async function* walk(dir) {
  for (const e of await readdir(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { if (!/^(bin|obj|node_modules)$/.test(e.name)) yield* walk(p); }
    else if (e.name.endsWith(".razor")) yield p;
  }
}

const findings = [];
for await (const file of walk(path.join(repoRoot, "docs/BlazorCN.Demo"))) {
  const src = await readFile(file, "utf8");
  const rel = path.relative(repoRoot, file).replace(/\\/g, "/");

  // Every `for=`/`For=` value in the file — a trigger id present here is labelled.
  const forTargets = new Set([...src.matchAll(/\bfor=(?:""([^"]*)""|"([^"]*)")/gi)]
    .map(m => (m[1] ?? m[2] ?? "").trim()).filter(Boolean));

  for (const m of src.matchAll(/<SelectTriggerCn\b/g)) {
    const end = tagEnd(src, m.index + m[0].length);
    if (end < 0) continue;
    const attrs = src.slice(m.index + m[0].length, end);
    if (attr(attrs, "aria-label") || attr(attrs, "aria-labelledby")) continue;

    // The id may sit on the trigger, or be inherited from the enclosing SelectCn.
    let id = attr(attrs, "id");
    if (!id) {
      const open = src.lastIndexOf("<SelectCn", m.index);
      if (open !== -1) {
        const oEnd = tagEnd(src, open + "<SelectCn".length);
        if (oEnd > open && oEnd < m.index) id = attr(src.slice(open, oEnd), "id");
      }
    }
    if (id && forTargets.has(id)) continue;

    const line = src.slice(0, m.index).split("\n").length;
    const before = src.slice(Math.max(0, m.index - 700), m.index);
    findings.push({
      file: rel, line,
      sample: /=""/.test(src.slice(m.index, m.index + 300)),
      inheritedId: id,
      // The nearest preceding visible caption — usually what the fix should say.
      suggestedLabel: [...before.matchAll(/>([A-Z][^<>{@]{2,40}?)</g)].pop()?.[1]?.trim() ?? null,
      excerpt: src.slice(m.index, m.index + 90).replace(/\s+/g, " "),
    });
  }
}

if (process.argv.includes("--json")) {
  await writeFile(path.join(repoRoot, "audit", "unnamed-controls.json"), JSON.stringify(findings, null, 1));
}
const tally = (fn) => findings.reduce((m, f) => (m.set(fn(f), (m.get(fn(f)) || 0) + 1), m), new Map());
console.log(`SelectTriggerCn with no accessible name: ${findings.length}`);
console.log(`  live markup: ${findings.filter(f => !f.sample).length}   code samples: ${findings.filter(f => f.sample).length}`);
for (const [title, m] of [["by group:", tally(f => f.file.split("/")[5] || "-")], ["by file:", tally(f => f.file)]]) {
  console.log(`\n${title}`);
  for (const [k, v] of [...m].sort((a, b) => b[1] - a[1]).slice(0, 15)) console.log(`  ${String(v).padStart(4)}  ${k}`);
}
console.log(`\nnearest visible caption found for ${findings.filter(f => f.suggestedLabel).length}/${findings.length} sites`);

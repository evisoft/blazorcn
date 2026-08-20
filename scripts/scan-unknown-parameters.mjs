#!/usr/bin/env node
// Every BlazorCN component inherits ComponentBaseCn, which captures unmatched attributes into
// AdditionalAttributes and splats them onto the root element. That is what makes `id=`, `aria-*`
// and `data-*` work — and it is also why a MISTYPED parameter is silent:
//
//   <ButtonCn Varient="ButtonVariant.Outline">   ->   <button varient="...">  (default variant)
//
// No compiler error, no runtime error, just a setting that never applies. Same failure shape as
// the missing-icon bug: the page renders, it is simply wrong.
//
// Lowercase/kebab attributes are legitimately splatted, so only PascalCase ones are checked.
//
//   node scripts/scan-unknown-parameters.mjs
import { readFile, glob } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");

// ---- 1. Parameter names per component -------------------------------------------------------
const params = new Map();          // component name -> Set of parameter names
const addParam = (comp, name) => {
  if (!params.has(comp)) params.set(comp, new Set());
  params.get(comp).add(name);
};

const PARAM_RE = /\[(?:Parameter|CascadingParameter)[^\]]*\]\s*(?:public|internal)\s+[\w<>?\[\],\s]+?\s+(\w+)\s*\{/g;
const bases = new Map();           // component name -> base type name

const srcFiles = [];
for await (const f of glob("src/BlazorCN/**/*.{razor,cs}", { cwd: repoRoot }))
  srcFiles.push(f.split(path.sep).join("/"));

for (const rel of srcFiles) {
  if (/[\\/](bin|obj)[\\/]/.test(rel)) continue;
  const base = path.basename(rel).replace(/\.razor\.cs$/, "").replace(/\.(razor|cs)$/, "");
  const src = await readFile(path.join(repoRoot, rel), "utf8");
  for (const m of src.matchAll(PARAM_RE)) addParam(base, m[1]);
  // A generic component's type parameters are supplied as attributes (TValue="string").
  for (const m of src.matchAll(/@typeparam\s+(\w+)/g)) addParam(base, m[1]);
  // Parameters are inherited — LucideIconCn declares only Name and gets Size/StrokeWidth/… from
  // LucideIconBaseCn. Without following this, every icon usage reads as a typo.
  const inh = /@inherits\s+([\w.]+)/.exec(src) ?? new RegExp(`class\\s+${base}\\s*:\\s*([\\w.]+)`).exec(src);
  if (inh) bases.set(base, inh[1].split(".").pop().replace(/<.*/, ""));
}

// Flatten the inheritance chain.
for (const comp of [...params.keys(), ...bases.keys()]) {
  const seen = new Set();
  let cur = bases.get(comp);
  while (cur && !seen.has(cur)) {
    seen.add(cur);
    for (const p of params.get(cur) ?? []) addParam(comp, p);
    cur = bases.get(cur);
  }
}

// Inherited from ComponentBaseCn / ComponentBase — available on every component.
const UNIVERSAL = new Set(["Class", "Style", "ChildContent", "AdditionalAttributes"]);

// A PascalCase attribute that is ALSO a real HTML attribute still works: it misses the
// parameter match, lands in AdditionalAttributes and is splatted onto the root element
// lowercased (Id -> id, Rows -> rows, ColSpan -> colspan). Not a bug — just not a parameter.
const HTML_ATTRS = new Set(["Id", "Rows", "Cols", "ColSpan", "Colspan", "RowSpan", "Rowspan",
  "ReadOnly", "Readonly", "Required", "MaxLength", "Maxlength", "MinLength", "Minlength",
  "Min", "Max", "Step", "Name", "Type", "Value", "Placeholder", "Title", "Role", "Href",
  "Target", "Rel", "Src", "Alt", "Width", "Height", "Disabled", "Checked", "Selected",
  "Multiple", "AutoFocus", "Autofocus", "AutoComplete", "Autocomplete", "InputMode",
  "Pattern", "Accept", "For", "Form", "Tabindex", "TabIndex", "Lang", "Dir", "Loading"]);

// ---- 2. Component usages in the demo --------------------------------------------------------
const stripVerbatim = (s) => {
  let o = "", i = 0;
  while (i < s.length) {
    const a = s.indexOf('@"', i);
    if (a === -1) { o += s.slice(i); break; }
    o += s.slice(i, a);
    let j = a + 2;
    while (j < s.length) {
      if (s[j] !== '"') { j++; continue; }
      if (s[j + 1] === '"') { j += 2; continue; }
      break;
    }
    o += s.slice(a, j + 1).replace(/[^\n]/g, "");
    i = j + 1;
  }
  return o;
};

const demoFiles = [];
for await (const f of glob("docs/**/*.razor", { cwd: repoRoot })) {
  const rel = f.split(path.sep).join("/");
  if (/[\\/](bin|obj)[\\/]/.test(rel)) continue;
  demoFiles.push(rel);
}

let usages = 0;
const unknown = new Map();          // "Component.Attr" -> {count, sample}
const unknownComponents = new Set();

for (const rel of demoFiles.sort()) {
  const src = stripVerbatim(await readFile(path.join(repoRoot, rel), "utf8"));
  // Opening tags of Cn components. `[^>]*?` stops at the first '>', which can cut a tag short
  // when an attribute value contains '>' (e.g. a lambda) — that only loses attributes, it never
  // invents them, so the scan stays free of false positives.
  for (const m of src.matchAll(/<([A-Z]\w*Cn)\b([^>]*?)\/?>/g)) {
    const [, comp, attrs] = m;
    if (!params.has(comp)) { unknownComponents.add(comp); continue; }
    const known = params.get(comp);
    for (const a of attrs.matchAll(/(^|\s)([A-Za-z@][\w:@-]*)\s*=\s*"/g)) {
      const name = a[2];
      if (!/^[A-Z]/.test(name)) continue;                 // splatted html attribute — fine
      usages++;
      if (known.has(name) || UNIVERSAL.has(name)) continue;
      // Event wiring can never be salvaged by the attribute splat: `OnCheckedChange="@H"` puts a
      // delegate into AdditionalAttributes under a name no DOM event matches, so the handler is
      // simply never called. These are the ones that silently break a control.
      const isEventLike = /^(On[A-Z]|.*Changed$)/.test(name);
      const salvaged = HTML_ATTRS.has(name) && !isEventLike;
      const key = `${comp}.${name}`;
      if (!unknown.has(key)) unknown.set(key, { count: 0, salvaged, isEventLike, sites: [], sample: `${rel}:${src.slice(0, m.index).split("\n").length}` });
      const u = unknown.get(key);
      u.count++;
      u.sites.push(`${rel}:${src.slice(0, m.index).split("\n").length}`);
    }
  }
}

const rows = [...unknown].sort((a, b) => b[1].count - a[1].count);
const dead = rows.filter(([, v]) => !v.salvaged);
const splatted = rows.filter(([, v]) => v.salvaged);

console.log(`components with known parameters: ${params.size}`);
console.log(`PascalCase attributes checked:    ${usages}`);
console.log(`\nDEAD — no such parameter, cannot work as an HTML attribute: ${dead.length} distinct`);
for (const [key, v] of dead) {
  console.log(`  ${String(v.count).padStart(5)}  ${key.padEnd(44)}${v.isEventLike ? " [event wiring]" : ""}  e.g. ${v.sample}`);
  if (process.argv.includes("--sites"))
    for (const s of [...new Set(v.sites)]) console.log(`           ${s}`);
}
console.log(`\nsplatted as a plain HTML attribute (works, just not a parameter): ${splatted.length} distinct`);
for (const [key, v] of splatted.slice(0, 12)) console.log(`  ${String(v.count).padStart(5)}  ${key}`);
if (unknownComponents.size) console.log(`\nunresolved component names: ${[...unknownComponents].slice(0, 10).join(", ")}`);
process.exit(dead.length ? 1 : 0);

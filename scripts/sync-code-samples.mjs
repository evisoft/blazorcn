#!/usr/bin/env node
// Re-syncs a stale `<Name>Code` sample in a *BlocksPage.razor from its live `<Name>.razor`
// block, which is the source of truth (it is what actually renders and what axe measures).
//
// Why this is needed: several samples still show the ORIGINAL field order inside `@code`,
// where a computed `static readonly` sits before the array it reads. That order throws
// TypeInitializationException at class init and blanks the whole page — the blocks were
// fixed, the samples were not, so anyone copying the sample gets the crash. Others simply
// predate an accessibility fix and are missing an aria-label the block now has.
//
// Safety: this NEVER regexes across a verbatim string boundary. It locates the sample's
// exact span by walking the string (`""` = escaped quote, lone `"` = terminator), rebuilds
// the body from the block file, and re-doubles every quote.
//
//   node scripts/sync-code-samples.mjs                 # dry run: report what would change
//   node scripts/sync-code-samples.mjs --write         # apply
//   node scripts/sync-code-samples.mjs --write --only Dashboard,Billing
import { readFile, writeFile, readdir } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const write = process.argv.includes("--write");
const onlyArg = process.argv[process.argv.indexOf("--only") + 1];
const only = process.argv.includes("--only") ? onlyArg.split(",").map(s => s.trim()) : null;

function readVerbatim(src, start) {
  let out = "";
  for (let i = start; i < src.length; i++) {
    if (src[i] !== '"') { out += src[i]; continue; }
    if (src[i + 1] === '"') { out += '"'; i++; continue; }
    return { text: out, end: i };          // end = index of the terminating quote
  }
  return null;
}

async function* walk(dir) {
  for (const e of await readdir(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { if (!/^(bin|obj|node_modules)$/.test(e.name)) yield* walk(p); }
    else if (e.name.endsWith("BlocksPage.razor")) yield p;
  }
}

let checked = 0, stale = 0, synced = 0, skipped = [];
for await (const file of walk(path.join(repoRoot, "docs/BlazorCN.Demo/Pages/Docs/Blocks"))) {
  const group = path.basename(path.dirname(file));
  if (only && !only.includes(group)) continue;
  let src = await readFile(file, "utf8");
  const edits = [];                                    // collected, then applied back-to-front

  for (const m of [...src.matchAll(/string\s+(\w+)Code\s*=\s*@"/g)]) {
    const v = readVerbatim(src, m.index + m[0].length);
    if (!v) continue;
    const blockPath = path.join(path.dirname(file), m[1] + ".razor");
    let blockSrc;
    try { blockSrc = await readFile(blockPath, "utf8"); } catch { continue; }
    checked++;

    const norm = (t) => t.split(/\r?\n/).map(s => s.trim()).filter(s => s.length);
    const sampleN = norm(v.text), blockN = norm(blockSrc);
    const anchor = blockN.indexOf(sampleN[0]);
    if (anchor < 0) { skipped.push(`${group}/${m[1]}: sample's first line not in the block`); continue; }
    let same = true;
    for (let i = 0; i < sampleN.length && anchor + i < blockN.length; i++) {
      if (sampleN[i] !== blockN[anchor + i]) { same = false; break; }
    }
    if (same && sampleN.length >= blockN.length - anchor) continue;   // already in sync
    if (same) continue;                                              // genuine excerpt, leave it
    stale++;

    // Rebuild from the block: same starting point, through to the end of the block file.
    const blockRaw = blockSrc.split(/\r?\n/);
    const startLine = blockRaw.findIndex(l => l.trim() === sampleN[0]);
    if (startLine < 0) { skipped.push(`${group}/${m[1]}: could not locate start line verbatim`); continue; }
    const rebuilt = blockRaw.slice(startLine).join("\n").replace(/\s+$/, "\n");
    edits.push({ from: m.index + m[0].length, to: v.end, body: rebuilt.replace(/"/g, '""'), name: m[1], group });
  }

  if (!edits.length) continue;
  if (write) {
    for (const e of edits.sort((a, b) => b.from - a.from)) src = src.slice(0, e.from) + e.body + src.slice(e.to);
    await writeFile(file, src, "utf8");
  }
  synced += edits.length;
  console.log(`${write ? "synced" : "would sync"} ${String(edits.length).padStart(3)}  ${path.relative(repoRoot, file).replace(/\\/g, "/")}`);
  for (const e of edits.slice(0, 4)) console.log(`        ${e.name}Code`);
  if (edits.length > 4) console.log(`        … and ${edits.length - 4} more`);
}

console.log(`\nsamples checked: ${checked}, stale: ${stale}, ${write ? "synced" : "would sync"}: ${synced}`);
if (skipped.length) {
  console.log(`skipped ${skipped.length}:`);
  for (const s of skipped.slice(0, 10)) console.log(`  ${s}`);
}
if (!write) console.log(`\n(dry run — pass --write to apply)`);

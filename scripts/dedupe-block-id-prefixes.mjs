#!/usr/bin/env node
// Second pass of the duplicate-id fix, for ids BUILT IN C# rather than written as attributes:
//
//   var triggerId = $"faq-trigger-{item.Id}";
//   <button id="@triggerId" aria-controls="@panelId">
//
// dedupe-block-ids.mjs only rewrites attribute values, so these survived it — and 29 different
// FAQ blocks all rendering `faq-trigger-security` on one gallery page is the same bug.
//
// Renaming at the interpolated literal is the safe place to do it: the id and every reference
// to it come from that one variable, so they cannot drift apart.
//
//   node scripts/dedupe-block-id-prefixes.mjs [--write]
import { readFile, writeFile, glob } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const BLOCKS = "docs/BlazorCN.Demo/Pages/Docs/Blocks";
const write = process.argv.includes("--write");

const slugFor = (group, base) => {
  const stem = base.startsWith(group) ? base.slice(group.length) : base;
  return (stem.replace(/([a-z0-9])([A-Z])/g, "$1-$2").replace(/^-/, "").toLowerCase()) || group.toLowerCase();
};

// `$"some-prefix-{...}"` — capture the literal head of an interpolated id.
const PREFIX_RE = /\$"([a-z][a-z0-9]*(?:-[a-z0-9]+)*-)\{/g;

const byGroup = new Map();
for await (const f of glob(`${BLOCKS}/*/*.razor`, { cwd: repoRoot })) {
  const rel = f.split(path.sep).join("/");
  if (/BlocksPage\.razor$/.test(rel)) continue;
  const group = rel.split("/")[5];
  const src = await readFile(path.join(repoRoot, rel), "utf8");
  const prefixes = new Set([...src.matchAll(PREFIX_RE)].map(m => m[1]));
  if (!prefixes.size) continue;
  if (!byGroup.has(group)) byGroup.set(group, []);
  byGroup.get(group).push({ rel, src, prefixes });
}

let files = 0, renames = 0;
const report = [];

for (const [group, entries] of byGroup) {
  // A prefix used by more than one block of the group collides on that group's page.
  const owners = new Map();
  for (const e of entries)
    for (const p of e.prefixes) owners.set(p, (owners.get(p) ?? new Set()).add(e.rel));

  const colliding = new Set([...owners].filter(([, s]) => s.size > 1).map(([p]) => p));
  if (!colliding.size) continue;

  for (const e of entries) {
    const slug = slugFor(group, path.basename(e.rel, ".razor"));
    let src = e.src, n = 0;
    for (const p of e.prefixes) {
      if (!colliding.has(p)) continue;
      // Already carries this block's slug? leave it alone.
      if (p.startsWith(`${slug}-`)) continue;
      const re = new RegExp(`\\$"${p.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}\\{`, "g");
      const before = src;
      src = src.replace(re, `$"${slug}-${p}{`);
      if (src !== before) { n++; renames++; report.push(`  ${e.rel}  ${p} -> ${slug}-${p}`); }
    }
    if (n) {
      files++;
      if (write) await writeFile(path.join(repoRoot, e.rel), src, "utf8");
    }
  }
}

console.log(`${write ? "rewrote" : "would rewrite"} ${files} block files, ${renames} prefixes`);
for (const r of report.slice(0, 30)) console.log(r);
if (report.length > 30) console.log(`  … and ${report.length - 30} more`);
if (write) console.log(`\nnext: node scripts/sync-code-samples.mjs --write`);

#!/usr/bin/env node
// Fixes the duplicate-id collisions that verify-duplicate-ids.mjs reports.
//
// Blocks are authored standalone (each is a copy of an upstream shadcn .tsx), so they all reach
// for the obvious id: `email`, `name`, `message`. That is harmless in isolation and broken on
// the gallery page, where every block of a group renders at once.
//
// The rename is file-local by construction: a block's label and its control always live in the
// same .razor, so prefixing every colliding id in that file with the block's own slug cannot
// break a cross-file reference. Ids that do NOT collide are left alone — no gratuitous churn.
//
//   node scripts/dedupe-block-ids.mjs            # report
//   node scripts/dedupe-block-ids.mjs --write
import { readFile, writeFile, glob } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const BLOCKS = "docs/BlazorCN.Demo/Pages/Docs/Blocks";
const write = process.argv.includes("--write");

const dupJson = path.join(repoRoot, "scripts/.dup-ids.json");
const perRoute = JSON.parse(await readFile(dupJson, "utf8"));

// Route (/docs/blocks/contact) -> group directory (Contact), read from each group's @page.
const routeToDir = new Map();
for await (const f of glob(`${BLOCKS}/*/*BlocksPage.razor`, { cwd: repoRoot })) {
  const rel = f.split(path.sep).join("/");
  const src = await readFile(path.join(repoRoot, rel), "utf8");
  const m = /@page\s+"([^"]+)"/.exec(src);
  if (m) routeToDir.set(m[1], rel.split("/")[5]);
}

// Slug for a block: its file name minus the group prefix, kebab-cased.
//   Blocks/Contact/ContactAdoption.razor -> "adoption"
//   Blocks/Billing/BillingAddCard.razor  -> "add-card"
const slugFor = (group, base) => {
  const stem = base.startsWith(group) ? base.slice(group.length) : base;
  const kebab = stem.replace(/([a-z0-9])([A-Z])/g, "$1-$2").replace(/^-/, "").toLowerCase();
  return kebab || group.toLowerCase();
};

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

let filesChanged = 0, idsRenamed = 0, skippedDynamic = 0;
const notes = [];

for (const { route, worst } of perRoute) {
  const group = routeToDir.get(route);
  if (!group) { notes.push(`  ?? no group directory for ${route}`); continue; }

  const dupIds = new Set(worst.map(([id]) => id));
  // `worst` is only the top 6; re-derive the full set from the JSON if present.
  for (const id of (perRoute.find(p => p.route === route)?.allDupIds ?? [])) dupIds.add(id);

  const files = [];
  for await (const f of glob(`${BLOCKS}/${group}/*.razor`, { cwd: repoRoot })) {
    const rel = f.split(path.sep).join("/");
    if (/BlocksPage\.razor$/.test(rel)) continue;   // regenerated later by sync-code-samples
    files.push(rel);
  }

  for (const rel of files) {
    const abs = path.join(repoRoot, rel);
    let src = await readFile(abs, "utf8");
    const base = path.basename(rel, ".razor");
    const slug = slugFor(group, base);

    // Which of this file's ids actually collide on the page?
    const mine = new Set();
    for (const m of stripVerbatim(src).matchAll(/\b(?:id|Id)="([^"@]+)"/g))
      if (dupIds.has(m[1])) mine.add(m[1]);
    if (!mine.size) continue;

    let changed = 0;
    for (const id of mine) {
      const next = `${slug}-${id}`;
      const esc = id.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
      // Rewrite only ATTRIBUTE occurrences — never a bare string elsewhere in the file, which
      // could be a Value=, a dictionary key, or display text.
      const attrRe = new RegExp(`\\b(id|Id|for|For|aria-labelledby|aria-describedby|aria-controls|aria-owns)="${esc}"`, "g");
      // …and every way SVG/anchors point AT an id. Renaming `id="latencyGradient"` without also
      // rewriting `fill="url(#latencyGradient)"` would leave the chart referencing nothing —
      // turning a duplicate-id nit into a visibly broken gradient.
      const urlRe = new RegExp(`url\\(#${esc}\\)`, "g");
      const hashRe = new RegExp(`((?:xlink:)?href)="#${esc}"`, "g");

      const before = src;
      src = src.replace(attrRe, (_, attr) => `${attr}="${next}"`)
               .replace(urlRe, `url(#${next})`)
               .replace(hashRe, (_, attr) => `${attr}="#${next}"`);
      if (src !== before) { changed++; idsRenamed++; }

      // Flag (do not touch) the same literal used somewhere else — a JS-interop focus call or a
      // dynamic id expression would silently drift out of sync with the rename.
      if (new RegExp(`"${esc}"`).test(stripVerbatim(src).replace(attrRe, ""))) skippedDynamic++;
    }
    if (changed) {
      filesChanged++;
      if (write) await writeFile(abs, src, "utf8");
    }
  }
}

console.log(`${write ? "rewrote" : "would rewrite"} ${filesChanged} block files`);
console.log(`ids renamed: ${idsRenamed}`);
if (skippedDynamic) console.log(`same literal also appears outside an id/for attribute in ${skippedDynamic} case(s) — review`);
console.log(notes.join("\n"));
if (write) console.log(`\nnext: node scripts/sync-code-samples.mjs --write   (mirrors into *BlocksPage.razor)`);

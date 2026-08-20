#!/usr/bin/env node
// Fixes skipped heading levels INSIDE demo blocks (axe heading-order).
//
// Context: every block is rendered inside <ComponentPreview>, whose section title is
// an <h2>. So the first heading a block emits must be h3 at the deepest, and each
// subsequent heading may go one level deeper at a time. Ported blocks frequently
// jump (hero blocks: own h1 then h3 for features; footer blocks: h6 column titles),
// which axe reports as heading-order on 116 routes.
//
// The rewrite only changes the LEVEL DIGIT — size and weight come from Tailwind
// classes on the tag, so rendering is unchanged. Applied to the block file (what the
// page actually renders) and to the matching verbatim Code string in <Cat>BlocksPage
// (the sample shown to users), running the identical algorithm on both.
//
//   node scripts/fix-heading-order.mjs            # dry run
//   node scripts/fix-heading-order.mjs --apply
import { readFile, writeFile, readdir } from "node:fs/promises";
import path from "node:path";

const ROOT = "C:/Users/evisoft/source/repos/blazorcn/docs/BlazorCN.Demo/Pages/Docs/Blocks";
const APPLY = process.argv.includes("--apply");
const START_LEVEL = 2; // the ComponentPreview <h2> title precedes every block

// Rewrites heading levels in a markup string. `q` is the quote style so we can skip
// the @code section correctly in both plain files and doubled-quote code strings.
function fixHeadings(markup) {
  let prev = START_LEVEL;
  let out = "";
  let i = 0;
  let changes = 0;
  const re = /<h([1-6])([\s>])/g;
  let m;
  while ((m = re.exec(markup))) {
    const level = +m[1];
    let target = level;
    if (level > prev + 1) target = prev + 1;
    prev = target;
    if (target !== level) {
      // rewrite the open tag and its matching close tag
      const openStart = m.index;
      const closeTag = `</h${level}>`;
      const closeIdx = markup.indexOf(closeTag, openStart);
      out += markup.slice(i, openStart) + `<h${target}${m[2]}`;
      if (closeIdx === -1) {
        i = re.lastIndex; // unbalanced: leave the rest alone
      } else {
        out += markup.slice(re.lastIndex, closeIdx) + `</h${target}>`;
        i = closeIdx + closeTag.length;
        re.lastIndex = i;
      }
      changes++;
    }
  }
  out += markup.slice(i);
  return { text: out, changes };
}

let fileCount = 0, tagCount = 0, mirrorCount = 0;
for (const cat of await readdir(ROOT)) {
  const dir = path.join(ROOT, cat);
  let files;
  try { files = await readdir(dir); } catch { continue; }
  const pageFile = files.find((f) => f.endsWith("BlocksPage.razor"));

  for (const f of files) {
    if (!f.endsWith(".razor") || f === pageFile) continue;
    const p = path.join(dir, f);
    const src = await readFile(p, "utf8");
    const split = src.split(/^@code\s*\{/m);
    const { text, changes } = fixHeadings(split[0]);
    if (!changes) continue;
    fileCount++; tagCount += changes;
    console.log(`${cat}/${f}: ${changes} heading(s)`);
    if (APPLY) await writeFile(p, text + (split.length > 1 ? "@code {" + split.slice(1).join("@code {") : ""));
  }

  // Mirror into the displayed code samples so copy-paste stays correct.
  if (!pageFile) continue;
  const pp = path.join(dir, pageFile);
  const page = await readFile(pp, "utf8");
  let changedPage = page;
  const constRe = /(private const string \w+Code = @")([\s\S]*?)(";\r?\n)/g;
  let cm, rebuilt = "", cursor = 0, pageChanges = 0;
  while ((cm = constRe.exec(page))) {
    const body = cm[2];
    // un-double quotes -> fix -> re-double, so the algorithm sees real markup
    const real = body.replace(/""/g, '"');
    const { text, changes } = fixHeadings(real);
    if (!changes) continue;
    pageChanges += changes;
    rebuilt += page.slice(cursor, cm.index) + cm[1] + text.replace(/"/g, '""') + cm[3];
    cursor = cm.index + cm[0].length;
  }
  if (pageChanges) {
    rebuilt += page.slice(cursor);
    changedPage = rebuilt;
    mirrorCount += pageChanges;
    console.log(`${cat}/${pageFile}: ${pageChanges} heading(s) in code samples`);
    if (APPLY) await writeFile(pp, changedPage);
  }
}
console.log(`\n${APPLY ? "fixed" : "would fix"} ${tagCount} headings in ${fileCount} block files, plus ${mirrorCount} in code samples`);

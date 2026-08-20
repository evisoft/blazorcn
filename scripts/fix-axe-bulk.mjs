#!/usr/bin/env node
// Bulk axe-driven fixes across demo pages:
//  1. <ProgressCn ...> without aria-label -> aria-label="Progress"
//  2. <img ...> without alt -> alt="" (decorative default)
//  3. <AvatarImageCn ...> without Alt/alt -> Alt="" (decorative; name is adjacent text)
// Quote style is decided PER TAG from its existing attributes (=""  -> verbatim
// Code-string context, ="x" -> live markup); tags with no attribute evidence are
// skipped. Tag ends are found quote-aware (attr values may contain '>').
// Dry-run by default; --write applies.
import { readFile, writeFile } from "node:fs/promises";
import { glob } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { findTagEnd } from "./_tagscan.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const WRITE = process.argv.includes("--write");
const TARGET = path.join(repoRoot, "docs", "BlazorCN.Demo", "Pages");

function addAttr(src, tagName, markerRe, attrName, attrValue, stats, key) {
  let out = "";
  let pos = 0;
  const open = "<" + tagName;
  for (;;) {
    const start = src.indexOf(open, pos);
    if (start === -1) break;
    // must be a whole tag name (next char is whitespace, '>' or '/')
    const after = src[start + open.length];
    if (after && !/[\s/>]/.test(after)) { out += src.slice(pos, start + open.length); pos = start + open.length; continue; }
    const end = findTagEnd(src, start);
    if (end === -1) break;
    let body = src.slice(start + open.length, end);
    const selfClosing = body.trimEnd().endsWith("/");
    if (selfClosing) body = body.trimEnd().slice(0, -1);
    let replacement = null;
    if (!markerRe.test(body)) {
      const hasDoubled = /=""/.test(body);
      const hasSingle = /="[^"]/.test(body);
      let insert = null;
      if (hasSingle && !hasDoubled) insert = `${attrName}="${attrValue}"`;
      else if (hasDoubled && !hasSingle) insert = `${attrName}=""${attrValue}""`;
      if (insert) {
        stats[key]++;
        replacement = open + body.replace(/\s+$/, "") + " " + insert + (selfClosing ? " /" : "") + ">";
      } else {
        stats.skipped++;
      }
    }
    out += src.slice(pos, start) + (replacement ?? src.slice(start, end + 1));
    pos = end + 1;
  }
  out += src.slice(pos);
  return out;
}

async function main() {
  const files = [];
  for await (const f of glob(TARGET.replaceAll("\\", "/") + "/**/*.razor")) files.push(f);
  const stats = { progress: 0, img: 0, avatar: 0, files: 0, skipped: 0 };
  for (const f of files) {
    const src = await readFile(f, "utf8");
    let out = src;
    out = addAttr(out, "ProgressCn", /aria-label/i, "aria-label", "Progress", stats, "progress");
    out = addAttr(out, "img", /\balt\s*=/i, "alt", "", stats, "img");
    out = addAttr(out, "AvatarImageCn", /\balt\s*=/i, "Alt", "", stats, "avatar");
    if (out !== src) {
      stats.files++;
      if (WRITE) await writeFile(f, out);
    }
  }
  console.log(`${WRITE ? "APPLIED" : "DRY-RUN"}:`, stats);
}
main().catch(e => { console.error(e); process.exit(1); });

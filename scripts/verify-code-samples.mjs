#!/usr/bin/env node
// Every gallery page holds each block's markup TWICE: once as live Razor, and once inside a
// C# verbatim string (`private const string XxxCode = @"…";`) with every double quote
// doubled. The sample is what a consumer copies, so a divergence is a real (if quiet) defect
// — and it is invisible to the compiler, because a corrupted sample is still a valid string.
//
// A codemod earlier in this session corrupted samples in two distinct ways:
//   1. quote-count drift — `_email = "";` became `= """";` then `= """"";`
//   2. truncated markup — an inner tag was deleted but its attribute values were left cut
//      off mid-expression (`aria-invalid=""@(_submitted && _hasError ? ""` with nothing after)
// Shape 2 compiles fine, so only a sample-vs-block diff catches it.
//
// The trap that makes this hard: you cannot find a verbatim string's end with a lazy regex.
// `";` occurs INSIDE the samples (`_email = "";`), so `@"([\s\S]*?)";` stops early. The only
// correct way is to scan forward treating `""` as an escaped quote and a lone `"` as the
// terminator — which is what readVerbatim() below does.
//
//   node scripts/verify-code-samples.mjs [--verbose]
import { readFile, readdir } from "node:fs/promises";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const DEMO = path.join(repoRoot, "docs/BlazorCN.Demo");
const verbose = process.argv.includes("--verbose");

// `<Name>Code` samples mirror the sibling `<Name>.razor` block. Measured before the codemod
// waves; a change means a sample was added or lost, not merely edited.
const BASELINE_CODE_SAMPLES = 6170;

// Files that legitimately contain C# raw string literals (`"""…"""`), which trip the
// odd-quote-run heuristic without being corrupt.
const RAW_STRING_FILES = new Set([
  "InstallationPage.razor", "DialogSearchReplace.razor",
  "DialogTermsAccept.razor", "ChatDeveloperConsole.razor",
]);

function readVerbatim(src, start) {          // start = index just past the opening `@"`
  let out = "";
  for (let i = start; i < src.length; i++) {
    if (src[i] !== '"') { out += src[i]; continue; }
    if (src[i + 1] === '"') { out += '"'; i++; continue; }   // escaped quote
    return { text: out, end: i };                            // lone quote terminates
  }
  return null;                                               // unterminated
}

async function* walk(dir) {
  for (const e of await readdir(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) { if (!/^(bin|obj|node_modules)$/.test(e.name)) yield* walk(p); }
    else if (e.name.endsWith(".razor")) yield p;
  }
}

const problems = [];
let samples = 0, codeSamples = 0, truncated = 0, compared = 0, matched = 0, unterminated = 0;

for await (const file of walk(DEMO)) {
  const src = await readFile(file, "utf8");
  const rel = path.relative(repoRoot, file).replace(/\\/g, "/");
  const dir = path.dirname(file);

  for (const m of src.matchAll(/(?:private\s+)?(?:const|static\s+readonly)\s+string\s+(\w+)\s*=\s*@"/g)) {
    const v = readVerbatim(src, m.index + m[0].length);
    const line = src.slice(0, m.index).split("\n").length;
    if (!v) { unterminated++; problems.push({ rel, line, kind: "unterminated verbatim string", name: m[1] }); continue; }
    samples++;
    const text = v.text;

    // Shape 2 detector: a line whose LAST attribute value is left dangling — no closing
    // quote and ending on an operator. Restricted to line ends, because an attribute value
    // may legitimately contain quotes once the doubling is undone, so scanning within a
    // line produces only noise.
    for (const bad of text.matchAll(/\b[\w:-]+="[^"\n]*(?:\?|&&|\|\||=>)\s*$/gm)) {
      problems.push({ rel, line: line + text.slice(0, bad.index).split("\n").length - 1,
                      kind: "attribute value cut off mid-expression", detail: bad[0].trim().slice(0, 90), name: m[1] });
    }

    // Ground-truth diff: `XxxCode` mirrors the sibling block component `Xxx.razor`.
    const blockName = m[1].replace(/Code$/, "");
    if (blockName === m[1]) continue;
    codeSamples++;
    let blockSrc;
    try { blockSrc = await readFile(path.join(dir, blockName + ".razor"), "utf8"); } catch { continue; }
    compared++;
    // Indentation is normalised away: some samples were generated dedented by a space or two
    // relative to their block, which is cosmetic. Content drift is what matters here.
    const norm = (t) => t.split(/\r?\n/).map(s => s.trim()).filter(s => s.length);
    const sampleLines = norm(text);
    const blockLines = norm(blockSrc);
    // The sample usually omits the block's header comment, so align on its first line.
    const anchor = blockLines.indexOf(sampleLines[0]);
    if (anchor < 0) {
      problems.push({ rel, line, kind: "sample's first line not found in the block", name: m[1],
                      detail: sampleLines[0]?.slice(0, 80) });
      continue;
    }
    if (sampleLines.length < blockLines.length - anchor) truncated++;   // sample is an excerpt
    let diverged = null;
    for (let i = 0; i < sampleLines.length && anchor + i < blockLines.length; i++) {
      if (sampleLines[i] !== blockLines[anchor + i]) { diverged = { i, s: sampleLines[i], b: blockLines[anchor + i] }; break; }
    }
    if (diverged) {
      problems.push({ rel, line: line + diverged.i, kind: "sample diverges from its block component", name: m[1],
                      detail: `sample: ${diverged.s.slice(0, 70)}\n              block:  ${diverged.b.slice(0, 70)}` });
    } else matched++;
  }

  // Shape 1 detector, file-wide: an odd-length run of >= 3 quotes.
  if (!RAW_STRING_FILES.has(path.basename(file))) {
    for (const q of src.matchAll(/"{3,}/g)) {
      if (q[0].length % 2 === 0) continue;
      const ln = src.slice(0, q.index).split("\n").length;
      const lineText = src.split(/\r?\n/)[ln - 1] ?? "";
      // A verbatim string that ENDS with an escaped quote (`@"Backpack 15"""`) is legitimate.
      if (/"{3}\s*[,;)]/.test(lineText) && /@"/.test(lineText)) continue;
      problems.push({ rel, line: ln, kind: `odd run of ${q[0].length} quotes`, detail: lineText.trim().slice(0, 90) });
    }
  }
}

const byKind = new Map();
for (const p of problems) byKind.set(p.kind.replace(/\d+/, "N"), (byKind.get(p.kind.replace(/\d+/, "N")) || 0) + 1);

console.log(`verbatim strings scanned:      ${samples}`);
console.log(`  of which <Name>Code samples: ${codeSamples}   (baseline ${BASELINE_CODE_SAMPLES})`);
console.log(`compared against their block:  ${compared}, identical: ${matched}`);
console.log(`  excerpts (shorter than the block, expected): ${truncated}`);
console.log(`unterminated verbatim strings: ${unterminated}`);
if (compared !== BASELINE_CODE_SAMPLES) {
  console.log(`\n!! <Name>Code sample count moved from the baseline — one was added or lost, investigate`);
}
console.log(`\nproblems: ${problems.length}`);
for (const [k, v] of [...byKind].sort((a, b) => b[1] - a[1])) console.log(`  ${String(v).padStart(4)}  ${k}`);
if (problems.length && verbose) {
  console.log();
  for (const p of problems.slice(0, 80)) {
    console.log(`  ${p.rel}:${p.line}  [${p.kind}]${p.name ? " " + p.name : ""}`);
    if (p.detail) console.log(`              ${p.detail}`);
  }
  if (problems.length > 80) console.log(`  … and ${problems.length - 80} more`);
}
process.exit(problems.length ? 1 : 0);

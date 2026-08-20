#!/usr/bin/env node
// Runs the whole frontend verification suite in order and prints one summary table.
// Requires the dev server to already be up:
//   dotnet run --project docs/BlazorCN.Demo --no-build --urls http://127.0.0.1:53185
//
//   node scripts/verify-all.mjs              # everything
//   node scripts/verify-all.mjs --static     # only the checks that need no browser
//
// Exit code is non-zero if any step fails, so it works as a pre-release gate.
import { spawn } from "node:child_process";
import path from "node:path";

const repoRoot = path.resolve(import.meta.dirname, "..");
const staticOnly = process.argv.includes("--static");

const STEPS = [
  { name: "code samples match their blocks", script: "verify-code-samples.mjs", static: true },
  { name: "no nested interactive controls", script: "scan-nested-interactive.mjs", static: true,
    // This one is a reporter, not a gate: it exits 0 and prints a count.
    parse: (out) => {
      const n = /nested-interactive candidates: (\d+)/.exec(out)?.[1];
      return { ok: n === "0", detail: `${n ?? "?"} candidates` };
    } },
  { name: "every select trigger has a name", script: "scan-unnamed-controls.mjs", static: true,
    parse: (out) => {
      const n = /no accessible name: (\d+)/.exec(out)?.[1];
      return { ok: n === "0", detail: `${n ?? "?"} unnamed` };
    } },
  { name: "no label points at a missing id", script: "scan-dangling-labels.mjs", static: true,
    parse: (out) => {
      const n = /dangling label targets:\s+(\d+)/.exec(out)?.[1];
      return { ok: n === "0", detail: `${n ?? "?"} dangling` };
    } },
  { name: "no component gets a parameter that doesn't exist", script: "scan-unknown-parameters.mjs", static: true,
    parse: (out) => {
      const n = /cannot work as an HTML attribute: (\d+)/.exec(out)?.[1];
      return { ok: n === "0", detail: `${n ?? "?"} dead params` };
    } },
  { name: "no duplicate ids in the rendered DOM", script: "verify-duplicate-ids.mjs",
    parse: (out) => {
      const n = /duplicate ids \(DOM\):\s+(\d+)/.exec(out)?.[1];
      const m = /wrong control:\s+(\d+)/.exec(out)?.[1];
      return { ok: n === "0", detail: `${n ?? "?"} dup ids, ${m ?? "?"} misrouted labels` };
    } },
  { name: "interactive behaviour (dialog/menu/select/…)", script: "verify-a11y.mjs" },
  { name: "OS accessibility settings", script: "verify-a11y-settings.mjs" },
  { name: "WCAG 2.5.8 target size", script: "verify-target-size.mjs" },
  { name: "WCAG 2.4.7 focus visible", script: "verify-focus-visible.mjs" },
  { name: "responsive sweep (127 routes × 6 widths + axe)", script: "responsive-sweep.mjs", slow: true },
];

const run = (script) => new Promise((resolve) => {
  const p = spawn(process.execPath, [path.join(repoRoot, "scripts", script)], { cwd: repoRoot });
  let out = "";
  p.stdout.on("data", (d) => { out += d; process.stdout.write(d); });
  p.stderr.on("data", (d) => { out += d; });
  p.on("close", (code) => resolve({ code, out }));
});

const results = [];
for (const step of STEPS) {
  if (staticOnly && !step.static) { results.push({ ...step, skipped: true }); continue; }
  console.log(`\n${"=".repeat(78)}\n== ${step.name}${step.slow ? "  (this one takes a while)" : ""}\n${"=".repeat(78)}`);
  const { code, out } = await run(step.script);
  const parsed = step.parse ? step.parse(out) : { ok: code === 0, detail: `exit ${code}` };
  results.push({ ...step, ...parsed });
}

console.log(`\n${"=".repeat(78)}\nSUMMARY\n${"=".repeat(78)}`);
let failed = 0;
for (const r of results) {
  if (r.skipped) { console.log(`  SKIP  ${r.name}`); continue; }
  if (!r.ok) failed++;
  console.log(`  ${r.ok ? "PASS" : "FAIL"}  ${r.name}${r.detail ? `  — ${r.detail}` : ""}`);
}
console.log(`\n${results.filter(r => r.ok && !r.skipped).length} passed, ${failed} failed`);
process.exit(failed ? 1 : 0);

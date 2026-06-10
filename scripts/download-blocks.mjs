// Download all shadcn.io blocks (.tsx) into original/<file.path> using the
// account registry token. Reads the pre-fetched registry index, then fetches
// each block's /r/<name>.json for file content. Resumable via SKIP_EXISTING=1.
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(import.meta.dirname, '..');
const INDEX = path.join(ROOT, 'artifacts', 'shadcnio', 'registry.json');
const TOKEN = process.env.SHADCNIO_TOKEN;
const SKIP_EXISTING = process.env.SKIP_EXISTING === '1';
const CONCURRENCY = Number(process.env.CONCURRENCY || 8);
if (!TOKEN) { console.error('SHADCNIO_TOKEN env var required'); process.exit(1); }

const index = JSON.parse(fs.readFileSync(INDEX, 'utf8'));
let blocks = index.items.filter((i) => i.type === 'registry:block');
if (process.env.LIMIT) blocks = blocks.slice(0, Number(process.env.LIMIT));
console.error(`blocks in index: ${blocks.length} | skipExisting=${SKIP_EXISTING} | concurrency=${CONCURRENCY}`);

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
let written = 0, skipped = 0, done = 0, authErrors = 0;
const failures = [];

async function fetchJson(name) {
  const url = `https://www.shadcn.io/r/${encodeURIComponent(name)}.json?token=${TOKEN}`;
  for (let attempt = 1; attempt <= 4; attempt++) {
    try {
      const res = await fetch(url);
      if (res.status === 401 || res.status === 403) { authErrors++; throw new Error(`auth ${res.status}`); }
      if (res.status === 429 || res.status >= 500) { await sleep(500 * attempt); continue; }
      if (!res.ok) throw new Error(`http ${res.status}`);
      return await res.json();
    } catch (e) {
      if (attempt === 4) throw e;
      await sleep(400 * attempt);
    }
  }
}

async function handle(item) {
  // Fast path: if the single expected file already exists, skip the network call.
  if (SKIP_EXISTING && (item.files || []).every((f) => {
    const t = path.join(ROOT, 'original', f.path);
    return fs.existsSync(t) && fs.statSync(t).size > 0;
  })) { skipped++; done++; return; }

  let data;
  try { data = await fetchJson(item.name); }
  catch (e) { failures.push({ name: item.name, error: String(e.message || e) }); done++; return; }

  const baseDir = path.resolve(ROOT, 'original');
  for (const f of data.files || []) {
    if (!f.path) continue;
    // Guard against path traversal (zip-slip): f.path comes from a remote
    // registry — never let a "../" escape the original/ directory.
    const target = path.resolve(baseDir, f.path);
    if (target !== baseDir && !target.startsWith(baseDir + path.sep)) {
      failures.push({ name: item.name, error: `unsafe path skipped: ${f.path}` });
      continue;
    }
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.writeFileSync(target, f.content ?? '');
    written++;
  }
  done++;
}

// Simple worker pool.
let cursor = 0;
async function worker() {
  while (cursor < blocks.length) {
    if (authErrors >= 15) return; // token likely dead — bail
    const item = blocks[cursor++];
    await handle(item);
    if (done % 200 === 0) console.error(`progress: ${done}/${blocks.length} (written=${written} skipped=${skipped} failed=${failures.length})`);
  }
}

await Promise.all(Array.from({ length: CONCURRENCY }, worker));

const report = {
  totalBlocks: blocks.length,
  filesWritten: written,
  skipped,
  failed: failures.length,
  authErrors,
  failures: failures.slice(0, 200),
};
fs.writeFileSync(path.join(ROOT, 'artifacts', 'shadcnio', 'download-report.json'), JSON.stringify(report, null, 2));
console.error(`DONE: written=${written} skipped=${skipped} failed=${failures.length} authErrors=${authErrors}`);
if (authErrors >= 15) { console.error('ABORTED: too many auth errors — token expired, refresh and re-run with SKIP_EXISTING=1'); process.exit(2); }

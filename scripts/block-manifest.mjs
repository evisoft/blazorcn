// Build a work manifest for porting shadcn.io blocks (.tsx) into the BlazorCN
// demo (.razor). Reads the registry index + scans existing demo block folders to
// determine which blocks are already ported vs missing.
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(import.meta.dirname, '..');
const INDEX = path.join(ROOT, 'artifacts', 'shadcnio', 'registry.json');
const BLOCKS_DIR = path.join(ROOT, 'docs', 'BlazorCN.Demo', 'Pages', 'Docs', 'Blocks');
const OUT = path.join(ROOT, 'artifacts', 'shadcnio', 'work-manifest.json');

const cap = (s) => s ? s[0].toUpperCase() + s.slice(1) : s;
const pascal = (slug) => slug.split('-').map(cap).join('');
const titleCase = (s) => s.split('-').map(cap).join(' ');

const index = JSON.parse(fs.readFileSync(INDEX, 'utf8'));
const blocks = index.items.filter((i) => i.type === 'registry:block');

const byCat = {};
for (const item of blocks) {
  const f = (item.files || [])[0];
  if (!f || !f.path) continue;
  // path: blocks/<category>/<slug>.tsx — skip non-block items (e.g. charts/*.tsx)
  const parts = f.path.split('/');
  if (parts[0] !== 'blocks' || parts.length < 3) continue;
  const category = parts[1];
  const slug = item.name;
  const folder = pascal(category);
  const name = pascal(slug);
  const displaySuffix = slug.startsWith(category + '-') ? slug.slice(category.length + 1) : slug;
  const title = titleCase(displaySuffix || slug);
  const razorRel = path.join('docs/BlazorCN.Demo/Pages/Docs/Blocks', folder, name + '.razor');
  const exists = fs.existsSync(path.join(ROOT, razorRel));
  (byCat[category] ||= { category, folder, blocks: [] }).blocks.push({
    slug, name, title,
    description: item.description || '',
    deps: item.registryDependencies || [],
    tsxPath: 'original/' + f.path,
    razorRel,
    exists,
  });
}

const cats = Object.values(byCat).sort((a, b) => a.category.localeCompare(b.category));
let total = 0, missing = 0;
for (const c of cats) {
  c.blocks.sort((a, b) => a.slug.localeCompare(b.slug));
  c.total = c.blocks.length;
  c.missing = c.blocks.filter((b) => !b.exists).length;
  total += c.total; missing += c.missing;
}
fs.writeFileSync(OUT, JSON.stringify({ generatedTotal: total, generatedMissing: missing, categories: cats }, null, 2));

console.log(`categories: ${cats.length} | total blocks: ${total} | already ported: ${total - missing} | MISSING: ${missing}`);
console.log('per-category (folder: total / missing):');
for (const c of cats) console.log(`  ${c.category.padEnd(16)} -> ${c.folder.padEnd(14)} ${String(c.total).padStart(4)} / ${String(c.missing).padStart(4)} missing`);

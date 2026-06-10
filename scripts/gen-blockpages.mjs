// Deterministically (re)generate <Folder>BlocksPage.razor group pages and the
// NavData.Blocks menu from ported .razor files + registry metadata.
// Only blocks whose .razor exists on disk are included (partial categories show
// what's done). Existing page H1/description are preserved when present.
//
// Usage: node scripts/gen-blockpages.mjs <category|all>
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(import.meta.dirname, '..');
const MAN = JSON.parse(fs.readFileSync(path.join(ROOT, 'artifacts', 'shadcnio', 'work-manifest.json'), 'utf8'));
const BLOCKS_DIR = path.join(ROOT, 'docs', 'BlazorCN.Demo', 'Pages', 'Docs', 'Blocks');
const NAVDATA = path.join(ROOT, 'docs', 'BlazorCN.Demo', 'Data', 'NavData.cs');
const target = process.argv[2];
if (!target) { console.error('usage: gen-blockpages.mjs <category|all>'); process.exit(1); }

const SPECIAL = { ai: 'AI', crud: 'CRUD', nft: 'NFT', faq: 'FAQ', cta: 'CTA', web3: 'Web3', 'command-menu': 'Command Menu' };
const label = (cat) => SPECIAL[cat] || cat.split('-').map((s) => s[0].toUpperCase() + s.slice(1)).join(' ');
const stripHeader = (src) => src.replace(/^﻿?\s*@\*[\s\S]*?\*@\s*/, '');
const escVerbatim = (s) => s.replace(/"/g, '""');
// Title/Description attribute value. A literal "@" breaks component attributes
// (RZ9986), so emit a C# string expression form @("...") in that case — the
// same pattern the existing demo uses (e.g. Placeholder="@("a@b.com")").
const attr = (s) => {
  s = (s || '').replace(/\s+/g, ' ').trim();
  if (s.includes('@')) return `@("${s.replace(/\\/g, '\\\\').replace(/"/g, '\\"')}")`;
  return s.replace(/"/g, '&quot;');
};

function pagePreserve(file) {
  if (!fs.existsSync(file)) return null;
  const t = fs.readFileSync(file, 'utf8');
  const h1 = t.match(/<h1[^>]*>([\s\S]*?)<\/h1>/);
  const desc = t.match(/<p class="mt-2 text-lg text-muted-foreground">([\s\S]*?)<\/p>/);
  return { h1: h1 && h1[1].trim(), desc: desc && desc[1].trim() };
}

function genCategory(cat) {
  const c = MAN.categories.find((x) => x.category === cat);
  if (!c) { console.error('no category', cat); return false; }
  const folderDir = path.join(BLOCKS_DIR, c.folder);
  const included = c.blocks.filter((b) => fs.existsSync(path.join(folderDir, b.name + '.razor')));
  if (!included.length) { console.log(`  ${cat}: 0 ported, skipping page`); return false; }

  const pageFile = path.join(folderDir, c.folder + 'BlocksPage.razor');
  const prev = pagePreserve(pageFile);
  const h1 = (prev && prev.h1) || label(cat);
  const desc = (prev && prev.desc) || `${label(cat)} blocks for building application interfaces.`;

  const previews = [];
  const codes = [];
  for (const b of included) {
    const src = stripHeader(fs.readFileSync(path.join(folderDir, b.name + '.razor'), 'utf8')).trimEnd();
    previews.push(`    <ComponentPreview Title="${attr(b.title)}" Description="${attr(b.description)}" Code="@${b.name}Code">\n        <${b.name} />\n    </ComponentPreview>`);
    codes.push(`    private const string ${b.name}Code = @"${escVerbatim(src)}";`);
  }

  const page = `@page "/docs/blocks/${cat}"
@layout DocsLayout

<PageTitle>${h1} Blocks — BlazorCN</PageTitle>

<div class="space-y-6">
    <div>
        <h1 class="text-3xl font-bold tracking-tight">${h1}</h1>
        <p class="mt-2 text-lg text-muted-foreground">${desc}</p>
    </div>

    <SeparatorCn />

${previews.join('\n\n')}
</div>

@code {
${codes.join('\n\n')}
}
`;
  fs.writeFileSync(pageFile, page);
  console.log(`  ${cat}: wrote ${c.folder}BlocksPage.razor (${included.length} blocks)`);
  return true;
}

const cats = target === 'all' ? MAN.categories.map((c) => c.category) : [target];
for (const cat of cats) genCategory(cat);

// ---- Update NavData.Blocks from the group pages actually on disk ----
// (scan @page routes so pre-existing folders not in the registry — e.g.
//  product-card, nft — are preserved alongside newly generated categories.)
const routes = new Set();
for (const folder of fs.readdirSync(BLOCKS_DIR)) {
  const dir = path.join(BLOCKS_DIR, folder);
  if (!fs.statSync(dir).isDirectory()) continue;
  for (const f of fs.readdirSync(dir)) {
    if (!f.endsWith('BlocksPage.razor')) continue;
    const m = fs.readFileSync(path.join(dir, f), 'utf8').match(/@page\s+"\/docs\/blocks\/([^"]+)"/);
    if (m) routes.add(m[1]);
  }
}
const withPages = [...routes].sort((a, b) => label(a).localeCompare(label(b)));
const navItems = withPages.map((r) => `            new("${label(r)}", "/docs/blocks/${r}"),`).join('\n');
const navBlock = `    public static readonly NavSection[] Blocks =\n    [\n        new("Blocks", [\n${navItems}\n        ]),\n    ];`;
let nav = fs.readFileSync(NAVDATA, 'utf8');
nav = nav.replace(/    public static readonly NavSection\[\] Blocks =\s*\[[\s\S]*?\n    \];/, navBlock);
fs.writeFileSync(NAVDATA, nav);
console.log(`NavData.Blocks: ${withPages.length} categories`);

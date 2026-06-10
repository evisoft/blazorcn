// Emit a Workflow script that ports a batch of shadcn .tsx blocks -> BlazorCN
// .razor demo components (one agent per block). Workflow scripts have no fs
// access, so the batch (tsx path, target razor path, metadata) is embedded.
//
// Usage: node scripts/gen-port-workflow.mjs <category> [limit] [offset]
//   node scripts/gen-port-workflow.mjs banner 5 0
//   node scripts/gen-port-workflow.mjs all            (every missing block)
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(import.meta.dirname, '..');
const MAN = JSON.parse(fs.readFileSync(path.join(ROOT, 'artifacts', 'shadcnio', 'work-manifest.json'), 'utf8'));
const [, , catArg, limitArg, offsetArg] = process.argv;
if (!catArg) { console.error('usage: gen-port-workflow.mjs <category|all> [limit] [offset]'); process.exit(1); }
const limit = limitArg ? Number(limitArg) : Infinity;
const offset = offsetArg ? Number(offsetArg) : 0;

let cats = MAN.categories;
if (catArg !== 'all') cats = cats.filter((c) => c.category === catArg);
if (!cats.length) { console.error('no such category:', catArg); process.exit(1); }

let missing = [];
for (const c of cats) for (const b of c.blocks) if (!b.exists) missing.push({ ...b, category: c.category, folder: c.folder });
missing = missing.slice(offset, offset === 0 ? (limit === Infinity ? undefined : limit) : offset + (limit === Infinity ? missing.length : limit));

const batch = missing.map((b) => ({
  tsx: path.join(ROOT, b.tsxPath).replace(/\\/g, '/'),
  razor: path.join(ROOT, b.razorRel).replace(/\\/g, '/'),
  name: b.name, title: b.title, slug: b.slug, category: b.category, folder: b.folder, deps: b.deps,
  description: b.description,
}));

const PLAYBOOK = String.raw`You are porting ONE shadcn/ui React block (.tsx) into a BlazorCN Blazor demo component (.razor). Produce a faithful, COMPILING Razor port that renders the same UI. This is a presentational port — match layout, text, Tailwind classes, and structure exactly.

TSX SOURCE FILE: %%TSX%%
WRITE THE .razor TO: %%RAZOR%%
Block slug: %%SLUG%% | Component name: %%NAME%% | Category: %%CATEGORY%%

STEP 1 — READ the TSX with the Read tool. Also Read 1 existing ported block in the SAME demo folder (docs/BlazorCN.Demo/Pages/Docs/Blocks/%%FOLDER%%/) if any exist, OR a sibling like .../Blocks/About/AboutAccessibilityPledge.razor, to copy conventions EXACTLY.

STEP 2 — WRITE the .razor file (use the Write tool at the exact path above). Follow these conventions:

HEADER COMMENT (first lines, exactly this shape):
@* ============================================================
   Block: %%SLUG%%
   Title: %%NAME%%
   Description: %%DESC%%
   Source: %%TSXREL%%
   ============================================================ *@

MARKUP TRANSLATION (TSX/JSX -> Razor):
- className="..."  ->  class="..."   (keep ALL Tailwind classes verbatim, same order)
- className={cn("a", cond && "b")}  ->  class="@Cn.Merge("a", cond ? "b" : "")"
- {expr}  ->  @expr   ; {cond ? a : b}  ->  @(cond ? a : b)  ; {items.map(x => (...))}  ->  @foreach (var x in items) { ... }
- {cond && (<X/>)}  ->  @if (cond) { <X/> }
- onClick={() => foo(x)}  ->  @onclick="@(() => Foo(x))"   ; onClick={foo}  ->  @onclick="Foo"
- Self-close void elements as XML: <br />, <img ... />, <input ... />. Every tag MUST be balanced/closed (Razor is XML-strict).
- Boolean attrs: disabled  ->  disabled="disabled" (or Disabled="@x" on Cn components). aria-*/data-* unchanged.
- Inline style={{ width: x }} -> style="@($"width:{x}")" (only if used).
- JSX comments {/* c */} -> @* c *@. Curly text like a literal "{" in text -> "@("{")".

ICONS (lucide-react): import { CircleCheckBig, GiftIcon } from "lucide-react" then <CircleCheckBig className="size-5"/>:
- Use the GENERIC icon component: <LucideIconCn Name="circle-check-big" Size="20" Class="size-5 ..." />
- Name = the icon in kebab-case, WITHOUT any trailing "Icon" (GiftIcon -> "gift", CircleCheckBig -> "circle-check-big", ArrowRight -> "arrow-right").
- Map the React size: Tailwind size-4/h-4=16, size-5=20, size-6=24 (default). Put the original size-* class in Class too.
- LucideIconCn Size and StrokeWidth are INT. Use whole numbers only: Size="16", StrokeWidth="2" (round 1.5->2, 2.25->2). A decimal like StrokeWidth="1.5" will NOT compile.

SHADCN COMPONENTS -> BlazorCN (append "Cn"): Button->ButtonCn, Badge->BadgeCn, Card/CardHeader/CardTitle/CardDescription/CardContent/CardFooter->CardCn/CardHeaderCn/CardTitleCn/CardDescriptionCn/CardContentCn/CardFooterCn, Input->InputCn, Textarea->TextareaCn, Label->LabelCn, Separator->SeparatorCn, Avatar/AvatarImage/AvatarFallback->AvatarCn/AvatarImageCn/AvatarFallbackCn, Switch->SwitchCn, Checkbox->CheckboxCn, Tabs*->Tabs*Cn, Select*->Select*Cn, Progress->ProgressCn, Skeleton->SkeletonCn, Tooltip*->Tooltip*Cn, Slider->SliderCn, etc.
- Button variants: variant="outline"->Variant="ButtonVariant.Outline"; "ghost"->Ghost; "secondary"->Secondary; "destructive"->Destructive; "link"->Link; default omit. size="sm"->Size="ButtonSize.Sm"; "lg"->Lg; "icon"->Icon; default omit.
- Badge variants similarly: Variant="BadgeVariant.Secondary|Outline|Destructive".
- InputCn/TextareaCn already render the full bordered box (border, rounded, h-9, px-3, bg, text-sm, focus). DROP those redundant classes; keep additive ones (pl-9, font-mono) via Class. value/onChange -> @bind-Value if there is state; else just Placeholder=.
- If a referenced shadcn component is NOT obviously available, render the equivalent raw HTML with the same Tailwind classes (e.g. a plain <button class="..."> ) rather than inventing a component.

STATE & DATA (@code block at end):
- TS interfaces/types -> C# records: private record Foo(string Name, int Count, string[] Tags);
- const data arrays -> private static readonly Foo[] _data = [ new("...", 1, ["a","b"]), ... ];  (use collection-expression [] syntax)
- useState<T>(init) -> private T _field = init; mutate in handlers; methods like private void Foo(...) { ... }. NO StateHasChanged needed (auto after event).
- Keep field/method names PascalCase for methods, _camelCase for fields, matching the sibling reference file.
- Booleans in C#: true/false. String interpolation: $"{x}".  Ternaries fully parenthesised when inside @().
- Dates/random: do not call DateTime.Now in field initializers if it breaks determinism; a fixed value is fine.

HARD RULES (these are the most common compile failures — obey exactly):
- The file MUST be valid, COMPILING Razor. Balance every tag. Do not emit TypeScript syntax (no :type, no =>{} arrow types, no interface in markup).
- LITERAL "@" RULES (context-dependent):
  * In raw HTML element text or a raw HTML attribute -> write "@@" (e.g. <p>email me@@x.com</p>).
  * In a BlazorCN COMPONENT attribute (PascalCase attr on a *Cn tag, e.g. InputCn Placeholder, ComponentPreview Description) -> "@@" is INVALID; wrap the whole value as a C# string: Placeholder="@("you@example.com")".
  * To inject a C# value into an attribute, use ONE expression, never literal+expression mix: Id="@($"perm-{perm.Id}")" NOT Id="perm-@(perm.Id)" and NOT Id="perm-@perm.Id".
  * Inside the @code block (C# strings/data), there is NO Razor escaping at all: write a literal "@", "{", "}" directly. NEVER put @@ or @("{") inside an @code string — that breaks the C# literal.
- RESERVED IDENTIFIERS: do NOT name a variable (incl. @foreach loop vars) "section", "code", "functions", "inherits", "page", "model", or "namespace" — "@section.X" etc. is parsed as a Razor directive. Use "sec", "grp", "item", etc.
- CURLY BRACES in MARKUP text: a literal "{" or "}" in rendered text must be "@("{")" / "@("}")" — but ONLY in markup, NEVER inside @code.
- C# correctness: use .Count on List<T> (.Length is arrays/strings only). Prefer arrays (private static readonly T[]) for seed data. Do NOT use C# raw string literals (triple-quote """); use a verbatim @"..." or regular "..." string. Type any non-trivial event lambda: @onclick="@((MouseEventArgs e) => ...)".
- SliderCn Value and ValueChanged are DOUBLE: Value="@((double)_x)" ValueChanged="@((double v) => { _x = (int)v; })". A lambda body that is an assignment must be wrapped in { } (it returns void for EventCallback).
- Do NOT name ANY C# identifier (variable, parameter, ENUM MEMBER, property, field) with a reserved keyword: checked, class, event, lock, params, ref, fixed, base, default, in, out, object, string, new, etc. (e.g. an enum with members added/changed/fixed FAILS because "fixed" is reserved — use PascalCase Added/Changed/Fixed). When porting TSX string-literal unions to a C# enum, ALWAYS PascalCase the members and map the source strings to them.
- A lambda value in a component attribute must contain NO unescaped " and NO HTML entities (&quot;). If the body needs a string literal or ?? fallback, move it to a private helper method and call that: ValueChanged="@((string? v) => SetLang(v))".
- Razor intercepts the SVG <text> element (RZ1023: attributes not allowed). To emit an SVG <text> with attributes, build it as a string and render @((MarkupString)$"<text x=\"{x}\" ...>{label}</text>").
- @onclick:stopPropagation / :preventDefault modifiers do NOT work on a *Cn COMPONENT — put the handler+modifier on a raw HTML element (e.g. wrap in a <div @onclick:stopPropagation @onclick="...">).
- Every method, field, property, enum, or record you reference in markup MUST be defined in the @code block. Do NOT reference helpers like FormatDuration(...) unless you also define them. If the TSX had an inline helper, port it as a private method.
- Do NOT GUESS BlazorCN enum/type names. Names like TooltipSide, SeparatorOrientation, TooltipPlacement are NOT real and will not compile. If you are not 100% sure a component parameter/enum exists, render the raw HTML equivalent with the same Tailwind classes instead (e.g. a plain <hr> / <div role="separator"> instead of SeparatorCn Orientation=..., a plain styled div instead of a tooltip). Known-safe enums ONLY: ButtonVariant.{Default,Destructive,Outline,Secondary,Ghost,Link}, ButtonSize.{Default,Sm,Lg,Icon,IconSm,IconLg,Xs,IconXs}, BadgeVariant.{Default,Secondary,Destructive,Outline,Ghost,Link}, and for DropdownMenuContentCn/TooltipContentCn/PopoverContentCn alignment use Align="FloatingAlign.{Start,Center,End}" (NOT Side=/Orientation=/a bare string). For a vertical divider do NOT use SeparatorCn with an orientation enum — render <div class="w-px bg-border ..."></div>.
- Do NOT use "@{ ... }" code blocks in markup. To compute a per-iteration value use: @foreach (var x in items) { var y = ...; <div>@y</div> }  (the "var y" goes INSIDE the foreach braces, no "@"). Never write "@{" inside an attribute or expression.
- Inside @foreach, when a handler needs the loop variable, capture it: @foreach (var it in items) { var item = it; <button @onclick="@(() => Toggle(item.Id))"> }.
- Do not import anything; BlazorCN components + Cn + LucideIconCn are globally available via _Imports.
- Keep ALL visible text and Tailwind classes identical to the source.
- Output ONLY the file via Write. Then return the structured summary.

STEP 3 — Return: { name, written:true, notes (any component you substituted or risk), iconCount }.`;

const SCHEMA = {
  type: 'object', additionalProperties: false,
  properties: {
    name: { type: 'string' }, written: { type: 'boolean' },
    notes: { type: 'string' }, iconCount: { type: 'number' },
  },
  required: ['name', 'written'],
};

const out = `export const meta = {
  name: 'port-blocks-${catArg}',
  description: 'Port shadcn .tsx blocks to BlazorCN .razor (${batch.length} files)',
  phases: [{ title: 'Port', detail: 'one agent per block' }],
}

const SCHEMA = ${JSON.stringify(SCHEMA)}
const PLAYBOOK = ${JSON.stringify(PLAYBOOK)}
const BATCH = ${JSON.stringify(batch)}

function prompt(b) {
  const tsxRel = b.tsx.split('/blazorcn/').pop()
  return PLAYBOOK
    .replaceAll('%%TSX%%', b.tsx)
    .replaceAll('%%RAZOR%%', b.razor)
    .replaceAll('%%SLUG%%', b.slug)
    .replaceAll('%%NAME%%', b.name)
    .replaceAll('%%CATEGORY%%', b.category)
    .replaceAll('%%FOLDER%%', b.folder)
    .replaceAll('%%DESC%%', (b.description || '').replace(/\\s+/g, ' '))
    .replaceAll('%%TSXREL%%', tsxRel)
}

phase('Port')
log('Porting ' + BATCH.length + ' blocks')
const results = await parallel(BATCH.map(b => () => agent(prompt(b), { schema: SCHEMA, phase: 'Port', label: b.name, model: 'sonnet' })))
const ok = results.filter(Boolean)
const written = ok.filter(r => r.written)
log('Wrote ' + written.length + '/' + BATCH.length + ' (' + (BATCH.length - ok.length) + ' agent failures)')
return {
  requested: BATCH.length,
  written: written.length,
  failedAgents: BATCH.length - ok.length,
  notes: ok.filter(r => r.notes && r.notes.length > 4).map(r => ({ name: r.name, notes: r.notes })).slice(0, 60),
}
`;

const outPath = path.join(ROOT, 'scripts', '_port-wf.mjs');
fs.writeFileSync(outPath, out);
console.log(`wrote ${outPath}`);
console.log(`category=${catArg} batch=${batch.length} (offset=${offset} limit=${limit})`);
if (batch.length) console.log('first:', batch[0].name, '->', batch[0].razor);

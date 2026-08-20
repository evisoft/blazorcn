"""Fix component attributes that name a parameter which does not exist.

Every BlazorCN component splats unmatched attributes onto its root element, so a React prop
name carried over during the port (`OnCheckedChange`, `OnSelect`, `OnValueChange`) compiles
cleanly and then does nothing:

  * event wiring lands in AdditionalAttributes under a name no DOM event matches -> the handler
    is never called, and the control is dead while still looking interactive (`cursor-pointer`);
  * value props render as bogus HTML attributes (`tvalue="string"`, `aselement="a"`).

Detected by scripts/scan-unknown-parameters.mjs. Only LIVE blocks are rewritten here; the
mirrored <Name>Code samples are regenerated afterwards by scripts/sync-code-samples.mjs.

    python scripts/fix-dead-parameters.py [--write]
"""
import io
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BLOCKS = os.path.join(ROOT, "docs", "BlazorCN.Demo", "Pages", "Docs", "Blocks")
WRITE = "--write" in sys.argv

# component -> list of (old attribute, new attribute or None to delete)
RENAMES = {
    # Dead event wiring: the control looks interactive and nothing happens.
    "SwitchCn":            [("OnCheckedChange", "CheckedChanged"), ("ValueChanged", "CheckedChanged"), ("Value", "Checked")],
    "CheckboxCn":          [("OnCheckedChange", "CheckedChanged"), ("ValueChanged", "CheckedChanged"), ("Value", "Checked")],
    "TabsCn":              [("OnValueChange", "ValueChanged")],
    "DropdownMenuItemCn":  [("OnSelect", "OnClick")],
    # No OnClick parameter on these two — @onclick passes through the attribute splat instead,
    # which is how the other 2,600+ call sites in the demo already do it.
    "TableRowCn":          [("OnClick", "@onclick")],
    "BadgeCn":             [("OnClick", "@onclick")],
    "InputCn":             [("OnKeyDown", "@onkeydown")],
    # The current shadcn base variant renamed this prop `delay`; these blocks were ported from
    # the older radix variant, which called it `delayDuration`.
    "TooltipProviderCn":   [("DelayDuration", "Delay")],
    # Inert leftovers that render invalid HTML attributes.
    "SelectCn":            [("TValue", None), ("T", None)],
    "RadioGroupCn":        [("TValue", None)],
    "AccordionCn":         [("Collapsible", None)],
    "AccordionItemCn":     [("Value", None)],
    "ButtonCn":            [("AsElement", None), ("href", "Href")],
}
# ButtonCn href->Href only makes sense where AsElement was asking for an anchor; guard below.


def tag_spans(src, comp):
    """Yield (start, end) of each `<Comp ...>` opening tag, quote-aware.

    A naive `<Comp[^>]*>` truncates at the `>` inside a lambda (`@(() => X())`), which would
    silently skip any attribute written after it.
    """
    for m in re.finditer(r"<" + comp + r"(?=[\s/>])", src):
        i = m.end()
        quote = None
        while i < len(src):
            c = src[i]
            if quote:
                if c == quote:
                    quote = None
            elif c in "\"'":
                quote = c
            elif c == ">":
                yield m.start(), i + 1
                break
            i += 1


changed_files = 0
total = 0
report = []

for dirpath, _, filenames in os.walk(BLOCKS):
    for fn in filenames:
        if not fn.endswith(".razor") or fn.endswith("BlocksPage.razor"):
            continue
        path = os.path.join(dirpath, fn)
        src = io.open(path, encoding="utf-8", newline="").read()
        original = src

        for comp, pairs in RENAMES.items():
            spans = list(tag_spans(src, comp))
            for start, end in reversed(spans):          # reverse so earlier spans stay valid
                tag = src[start:end]
                new_tag = tag
                for old, new in pairs:
                    # ButtonCn's href is only wrong when it was paired with the dead AsElement.
                    if comp == "ButtonCn" and old == "href" and "AsElement" not in tag:
                        continue
                    pat = re.compile(r'(\s)' + re.escape(old) + r'\s*=\s*"([^"]*)"')
                    hit = pat.search(new_tag)
                    if not hit:
                        continue
                    # A Razor expression can nest quotes -- Value="@($"item-{i}")" -- and `[^"]*`
                    # stops at the inner one, so rewriting would splice the tag in half. Skip
                    # those and report them for a hand edit instead of corrupting the file.
                    if "@(" in hit.group(2) and hit.group(2).count("$\"") :
                        report.append("  SKIPPED (nested quotes, edit by hand): %s <%s %s=%s>" % (
                            os.path.relpath(path, ROOT).replace("\\", "/"), comp, old, hit.group(2)))
                        continue
                    if new is None:
                        new_tag = pat.sub("", new_tag)
                    else:
                        new_tag = pat.sub(lambda mm, n=new: '%s%s="%s"' % (mm.group(1), n, mm.group(2)), new_tag)
                    total += 1
                    report.append("  %s  <%s %s -> %s>" % (
                        os.path.relpath(path, ROOT).replace("\\", "/"), comp, old, new or "(removed)"))
                if new_tag != tag:
                    src = src[:start] + new_tag + src[end:]

        if src != original:
            changed_files += 1
            if WRITE:
                io.open(path, "w", encoding="utf-8", newline="").write(src)

print("%s %d attributes across %d block files" % ("rewrote" if WRITE else "would rewrite", total, changed_files))
for line in report[:80]:
    print(line)
if len(report) > 80:
    print("  ... and %d more" % (len(report) - 80))
if WRITE:
    print("\nnext: node scripts/sync-code-samples.mjs --write")

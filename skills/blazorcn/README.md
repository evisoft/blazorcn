# BlazorCN skill

An Agent Skill (Claude Code / Copilot / Codex / Gemini style) that teaches an AI
assistant to build correct **BlazorCN** UI — the Blazor 1:1 port of shadcn/ui.
Modeled on the official [`shadcn` skill](https://github.com/shadcn-ui/ui/tree/main/skills/shadcn),
adapted to Blazor/Razor and to BlazorCN's actual API (verified against source).

## Layout

```
blazorcn/
├── SKILL.md            # entry point: setup context, principles, critical rules, key patterns, workflow
├── setup.md            # install, AddBlazorCN, Tailwind config, render modes, AOT (the "cli.md" analog)
├── customization.md    # theming, CSS variables, dark mode, custom colors, cascade-layer caveat
├── components.md       # full catalog + quick API for high-frequency components + enums
├── rules/
│   ├── styling.md          # semantic colors, Class-for-layout, gap, size-*, Cn.Merge, z-index
│   ├── forms.md            # Field/FieldGroup, @bind-Value vs @bind-Checked, validation, EditForm
│   ├── composition.md      # overlays (@bind-Open/triggers/titles), optional groups, Card, Tabs, Avatar, toasts
│   ├── icons.md            # Lucide components, auto-sizing, no data-icon, AOT
│   └── blazor-vs-react.md  # porting shadcn React snippets to BlazorCN (the "base-vs-radix.md" analog)
├── evals/evals.json    # Blazor-specific evaluation prompts
└── agents/openai.yml   # display metadata
```

## Activating it

- **Claude Code (this repo):** copy or symlink this folder to `.claude/skills/blazorcn/`,
  or reference it from a plugin's `skills/` directory.
- **Project / personal scope:** place under `~/.claude/skills/blazorcn/` (personal)
  or your plugin marketplace.

`SKILL.md` has `user-invocable: false` — it activates automatically when the model
detects BlazorCN work (see its `description` triggers). The body links the detail
files via progressive disclosure, so only `SKILL.md` is loaded until a topic is
needed.

## Maintaining accuracy

The rules are grounded in `src/BlazorCN/Components/**` and `docs/BlazorCN.Demo`.
If the library's API changes (enum values, bind parameter names, trigger
behavior), update the affected rule/catalog entry and the `evals/`. The source
files are the single source of truth — the skill should never assert a parameter
or enum value that the code doesn't have.

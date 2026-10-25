# Weave UI documentation

Weave UI is a UI Toolkit package for Unity 6 runtime UI: utility-class styling on design tokens, a themed controls
library, art skins, screens bound to view models, an inventory grid with drag and drop, a fluent C# builder, and an
interaction system that hands each handler the values it asks for.

Start with [Getting started](getting-started.md), then read the pages for the parts you use. The
[samples](../Samples) show each feature in a complete screen.

## Building screens

| Page | Covers |
|---|---|
| [Getting started](getting-started.md) | Requirements, installing, and a first screen from view model to Play |
| [Screens](screens.md) | `ScreenHost`, screens and their lifecycle, the screen stack, transitions, modal dialogs |
| [Data binding](data-binding.md) | View models, bindable properties, binding from UXML and C#, per-item data sources |
| [Interaction](interaction.md) | Handler attributes, UXML handler names, injected parameters, custom injectable values |
| [Builder](builder.md) | Building UI in C# with the fluent builder |
| [Grid](grid.md) | `WeaveGrid` for inventories, hotbars and shops: cells, multi-slot items, drag and drop |

## Look and feel

| Page | Covers |
|---|---|
| [Styling](styling.md) | Utility classes, state and theme variants, design tokens, semantic colour variables |
| [Controls](controls.md) | The `weave:` controls, their variants and sizes, and tooltips |
| [Theming](theming.md) | Light and dark modes, palettes and skins at runtime through `ThemeService` |
| [Skins](skins.md) | `UISkin` assets, the skin inspector, and building skins from art packs |

## Tools and reference

| Page | Covers |
|---|---|
| [Tailwind pipeline](tailwind-pipeline.md) | The optional Tailwind CSS pipeline and importing a Tailwind theme as tokens |
| [Editor tools](editor-tools.md) | Editor windows, inspectors, settings and menus |
| [Known limitations](known-limitations.md) | What doesn't work yet, and the workarounds |

## Samples

| Sample | Shows |
|---|---|
| [Game HUD](../Samples/GameHud) | Utility-class layout, bound bars, a hotbar grid, toasts built in C# |
| [Settings menu](../Samples/SettingsMenu) | Tabs, two-way binding, Apply/Revert, key rebinding, a modal |
| [Shop + inventory](../Samples/ShopInventory) | A vendor grid and a multi-slot bag with drag to buy and sell |
| [Builder + themes](../Samples/BuilderThemes) | A C#-only screen in dark, light, a palette, and Kenney and Synty skins |

# Changelog

## [0.2.0-exp.1] - 2026-11-20

- **Controls:** `RadialProgress`, `Stepper`, `IntegerField`, `FloatField`, `ColorPicker`, `Typewriter`, `Toasts`,
  `InputPrompt` (with `InputGlyphs` device detection and `InputGlyphSet` assets) and `WeaveContextMenu`, with
  builder factories and `Bind` support.
- **Grid:** dragging near a scrolling grid's edge scrolls it (`AutoScrollEdge`, `AutoScrollSpeed`).
- **Tooltips:** also show when keyboard or gamepad focus moves onto an element (`ShowOnFocus`).
- **Builds:** methods named only in UXML (`click`, `change`, `drop`) are kept through code stripping by a generated
  `link.xml`; `[Preserve]` is no longer needed.
- **Skins:** a skin's *Sprites Folder* fills it from sprites named `<part>[-<state>]` and regenerates its USS when
  they change.
- **Right-to-left:**
  - `ThemeService.Direction` (`dir-ltr`/`dir-rtl`). `WeaveDirection.For(languageCode)` gives a language's direction.
  - The native utilities add logical classes (`ps-*`, `pe-*`, `ms-*`, `me-*`, `start-*`, `end-*`, `text-start`,
    `text-end`, `rounded-s/e/ss/se/es/ee-*`, `border-s/e-*`) that mirror under `dir-rtl`, where `flex-row` reverses.
  - The Tailwind pipeline mirrors logical properties the same way and supports `rtl:` and `ltr:` variants.
  - The controls mirror under `dir-rtl`. Arabic shaping needs the advanced text generator, which you turn on in
    Project Settings (see the README).
- **Localization** (with com.unity.localization 1.5+):
  - `.Localize(...)`, `Ui.LocalizedText`, `LocalizedTitle` and `LocalizedButton`.
  - `ScreenHost` follows the selected locale's direction (`FollowLocaleDirection`).
- **Changed:** `Ui.Row()` adds the `flex-row` class instead of an inline `flex-direction`, so it can mirror. It needs
  the utility classes, which the default theme includes.

## [0.1.0-exp.1] - 2026-10-16

First experimental release. Requires Unity 6000.3 or newer.

- **Styling:**
  - Design tokens from Tailwind v4's default theme, with native Tailwind-named utility classes.
  - Variants: `hover:`, `active:`, `focus:`, `disabled:` and `dark:`.
  - `grid`/`gap` utilities laid out at runtime.
- **Tailwind pipeline (optional):**
  - Runs your own Tailwind CSS v4 CLI (set in Project Settings; nothing is downloaded), plus a USS post-processor
    that keeps only what USS can take.
  - Breakpoints, `dark:` and `group-hover:` variants.
  - Live reload in play mode.
- **Skins:**
  - 9-slice sprite skins per control and state, generated as scoped USS.
  - Applied to the whole UI or to one control.
  - Presets for every control part.
- **Screens:**
  - `Screen<TViewModel>` and `ScreenManager` (push, pop, replace, back).
  - Modal screens, transitions (fade, slide, scale), and a zero-setup `ScreenHost`.
- **Interaction:** handlers declare the values they need (`[OnClick]`, `[OnCellClick]`, `[OnDrop]`, …). Values are
  resolved when the screen is built, and a click allocates nothing.
- **Controls:**
  - `weave:` Button, IconButton, Toggle, Slider, Dropdown, TextField, ProgressBar, ListView, TabView and Grid.
  - They take variants, sizes and handler names in UXML.
  - Value-change handlers (`change="..."`, `[OnValueChanged]`) fire for the player's changes, not when a data
    binding writes the model's value into the control.
  - Runtime tooltips.
- **Grid:** virtualised slots, drag and drop between grids, drop rules, multi-cell items (`OccupancyMap`), selection,
  and keyboard/gamepad navigation.
- **Developer experience:** fluent C# builder (`using static Nerdova.WeaveUI.Ui`), `ThemeService` (light/dark, skin,
  palette), and the *Window → Weave UI → Preview* window.
- **Samples:** Basic Menu (every feature in one small menu), Game HUD, Settings Menu, Shop + Inventory, and
  Builder + Themes.

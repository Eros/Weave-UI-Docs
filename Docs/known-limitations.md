# Known limitations

What USS and Weave UI can't do, what is only emulated, and what hasn't been verified. The development repository's
decision records (`docs/decisions/0001`-`0010`) have the evidence behind each point.

## Unity versions and platforms

- **Minimum is Unity 6000.3.** Only 6000.4.0f1 has been run (Windows, Mono editor). 6000.3 has the same stylesheet
  importer as 6000.4 and every API the package uses, but it hasn't been run either.
- **6000.0-6000.2 aren't supported.** This was found by reading Unity's C# source (UnityCsReference); no editor was
  run. The results:

  | Area | 6000.0-6000.2 | Effect on Weave UI |
  |---|---|---|
  | Escaped selectors (`.hover\:bg-red-500:hover`, `.w-1\/2`, `.p-0\.5`) | ❌ The importer turns each selector back into text and re-reads it with a regex that only accepts `[\w-]` in class names. 6000.3+ read the parser's class name directly | 3,760 of the 6,906 rules in `utilities.uss` can't match: every `hover:`, `focus:`, `active:` and `disabled:` variant, plus fractions and decimals. Each variant would also log `Unknown pseudo class "bg-red-500"` |
  | `aspect-ratio`, `filter` | ❌ Not USS properties until 6000.3 | Import warnings, no effect |
  | `_xHH` alias selectors (`dark:`, breakpoints, `group-hover:`) | ✅ Identifier-safe | Would work |
  | Unity 6 APIs: `TabView`/`Tab`, `[UxmlElement]`/`[UxmlAttribute]`, `DataBinding`/`[CreateProperty]` (incl. bindable `enabledSelf`), `canGrabFocus`, `FindObjectsByType(FindObjectsSortMode)` | ✅ Present | Would compile |
  | Selector specificity | Plain integer score; 6000.4 uses a per-type struct (6000.3 still uses the integer) | No difference for Weave's selectors (no ids, fewer than 10 classes) |

  Supporting 6000.0 would mean emitting `_xHH` aliases for these compound selectors too. They would then stop
  previewing in UI Builder (decision 0001's rejected option b). 6000.3 is an LTS release, so the minimum was raised
  instead.
- **IL2CPP:** the interaction system's invokers were verified on IL2CPP for Windows x64 only (zero allocations per
  click). Other AOT platforms are untested.
- **Runtime cursors:** USS cursor keywords (`cursor: link`, ...) only work in the editor. Players need texture cursors.

## Tailwind pipeline

- **You supply the Tailwind CLI.** Weave UI includes no executable and downloads nothing. Install Tailwind CSS v4
  yourself (npm or the standalone executable) and point *Project Settings → Weave UI → Tailwind CLI path* at it, or
  put `tailwindcss` on your PATH.
- **Tested with v4.3.3.** Other 4.x versions should work. The settings page warns about other major versions,
  because the generated input and the post-processor are v4-only.
- **The native utilities need nothing extra.** Without the pipeline you get the native utility classes, which are
  generated from your tokens inside Unity.

## USS can't do these (the classes are dropped, with a warning)

| Area | Not available |
|---|---|
| Effects | `box-shadow`, `backdrop-filter`, `outline`, `border-style` (borders are solid) |
| Text | `line-height`, `text-decoration`, `text-transform`; `font-weight` other than bold. `font-*` weights and `italic` share one USS property, so the last class wins |
| Layout | `z-index`, `order`, `align-items: baseline`, `align-content: space-between` |
| Selectors | `:first-child`, `:focus-visible`, attribute selectors, `::before`/`::after`, sibling combinators, `peer-*` variants |
| At-rules | `@media` other than min-width breakpoints, `dark` and `hover` (those are mapped; see below), `@supports`, `@container`, `@starting-style` and other at-rules around class rules |
| Composition | Several `filter`, `translate` or `scale` utilities on one element don't combine; the last one wins |

`tailwind-report.txt` (Tailwind pipeline) lists every dropped class and the reason. A console warning names each
dropped class you wrote yourself.

## Rewritten at build time (works, but not literally)

The post-processor converts these, because the raw CSS would lose the stylesheet or break the panel:

- `rem`/`em` become px (base 16, configurable).
- `calc()` is folded to a value.
- `oklch()`, `oklab()`, `hsl()` and `color-mix()` become `rgba()`/hex.
- `cubic-bezier()` becomes the nearest USS easing keyword.
- Percentages for `opacity` and `scale` become numbers.
- Logical properties (`padding-inline-start`, `text-align: start`, ...) become left/right, plus a mirrored rule under
  `dir-rtl`. So does `flex-direction: row`.
- `inset-*` is expanded.

If you write USS by hand, avoid the raw forms. Unknown functions in a value break every later style update on that
panel.

## Emulated at runtime

- **Breakpoints and dark mode:** `md:` and similar apply while an ancestor has `bp-md`; `dark:` applies under
  `theme-dark`. `ScreenHost` sets both on its screen container (`BreakpointController`, `ThemeService`). Outside a
  `ScreenHost`, add the classes yourself.
- **`gap-*`** is done with margins:
  - On a flex row or column, children after the first get a leading margin, which replaces their own margin on that
    side.
  - With `flex-wrap`, every child gets trailing margins and the container a negative one.
  - Grid layouts handle gap themselves.
- **`grid grid-cols-N grid-rows-N col-span-N row-span-N`** on plain elements:
  - Children are positioned absolutely (CSS sparse auto-placement).
  - Rows are as tall as their tallest single-row child; children aren't stretched (like `items-start`).
    `grid-rows-N` rows are equal (the tallest one's height, or a share of a taller fixed height), and
    `row-span-full` spans all of them from the top row.
  - The container gets a min-height that fits the rows, or its own `min-h-*` if that is taller (read once, when the
    grid layout attaches).
  - Not supported: named areas, `minmax()`, `auto-flow dense`, arbitrary templates (`grid-cols-[...]`).
- **Variant classes need their alias class.** `dark:`, `md:` and `group-hover:` match through an `_xHH` alias that
  screens add when they build their layout, and `Ui.Class()` adds too. `WeaveGrid` adds them to its cells. For
  anything else you instantiate later, such as ListView rows from a template, call `WeaveClassAliases.Apply(element)`
  (the sample's controls screen shows this).
- **Layout utilities are attached when a screen builds its layout.** For trees built later, or classes or children
  added later, call `UtilityLayout.Apply(element)` (it re-runs the layout). Children that grow, hide or leave are
  followed on their own. In Tailwind's report these show as *handled at runtime*.

## Controls

- **`change="Method"` handlers fire when the player changes a value,** or when code sets one directly. They don't
  fire when a data binding writes the model's value into the control (the first write when a screen appears, a
  revert, a load). Weave tells these apart by checking whether the control's bound `value` or `index` already equals
  the source. A binding with a converter, where the two differ by type, still fires.
- **Unity's own disabled styles** (for example a grey button background) can show through Weave's variants.
- **Dropdown menus** open at the panel root, outside your screen, so their style in `controls.uss` is global.
- **Tooltips** come from `ScreenHost`'s `TooltipController`. Elsewhere, create one yourself. The tooltip follows the
  pointer target or the element that keyboard or gamepad focus moved to (clicks and `Focus()` from code don't show
  one).
- **Long-press and value-changed handlers can't be bound to grids.**
- **ColorPicker** keeps the value's alpha but has no alpha control. Its gradients use vertex colours, so in a linear
  colour space project they are slightly off from true HSV.
- **Typewriter** hides the untyped rest with `<alpha=#00>`. A `<color>` tag with its own alpha inside the untyped part
  shows early, and with rich text off the line re-wraps as it types.
- **InputPrompt** detects pads only with the Input System package (1.1+), from any button press. Without it, only key
  and mouse presses switch the scheme; call `InputGlyphs.Report` for pads. Built-in text glyphs cover `submit`,
  `cancel` and `menu`; add a glyph set for anything else.
- **Context menus** have one level (no submenus).

## Grid

- **Dragging:** one pointer drag at a time. Near a scrolling grid's edge (`AutoScrollEdge`, 40 by default) the grid
  under the item scrolls, faster closer to the edge (`AutoScrollSpeed`).
- **Occupancy maps:** placed items aren't virtualised (slots are). Swapping an occupancy item with an item in a plain
  grid is rejected.
- **Stale cells:** cells refresh when the grid refreshes or the item notifies a property change. Changing what an
  item's properties return without notifying won't update its cell.
- **Selection** is stored by slot index: it stays on the slot when items move.

## Transitions and theming

- **Screen lifecycle doesn't wait for transitions.** `OnShow`, `OnHide` and `OnClosed` run immediately; only the
  visual hide or removal waits. A leaving screen stops taking pointer input. If no transition event arrives, the
  leave callback runs after 1 s at the latest.
- **Controls fade their colours over 0.12 s.** Code that reads `resolvedStyle` right after a change sees the old
  colour until the transition ends.
- **The light palette covers the controls' semantic variables.** Utility classes in your own markup need `dark:`
  pairs to switch with it.

## Right-to-left

- **Direction is set for a whole host.** `dir-rtl` sits on the screen host's container. USS has no `:not()`, so a
  `dir-ltr` class lower down doesn't undo it for its subtree. To keep a row left to right under `dir-rtl`, add
  `rtl:flex-row-reverse` (Tailwind pipeline) or set the direction inline.
- **Under `dir-rtl`, a logical class outranks a physical one on the same side.** For example, `ps-4` beats `pr-2` on
  one element, because the mirror rules have two classes' specificity.
- **Mirrors reset the other side to `initial`,** as CSS leaves it unset. `ps-4 pl-2` on one element loses the `pl-2`
  under `dir-rtl`.
- **Inline styles don't mirror** (`style.flexDirection = Row`, `style.paddingLeft`). `Ui.Row()` uses the `flex-row`
  class for this reason.
- **Not mirrored:** grid layouts (`grid-cols-*`), `WeaveGrid` slot order, slider fill direction, and dropdown menus
  (they open at the panel root, outside the host).
- **Text alignment:** plain labels keep Unity's left alignment; use `text-start`. Weave's field labels, text inputs,
  titles, dialogs and tooltips align right under `dir-rtl`.
- **Arabic shaping needs the advanced text generator,** a project setting (*Project Settings → UI Toolkit → Advanced
  Text Generator*) plus `-unity-text-generator: advanced` in your own USS. Weave UI can't turn it on, because the
  property logs an error while the setting is off. Without it, Arabic-script letters don't join and mixed-direction
  text keeps its stored order.
- **Locale direction** comes from a fixed list of right-to-left language codes, not `CultureInfo`, which IL2CPP
  builds with invariant globalization report as left to right.

## Localization

- **Needs com.unity.localization 1.5 or newer,** detected by a version define. Without it, `Localize`,
  `LocalizedText`/`LocalizedTitle`/`LocalizedButton` and locale-driven direction aren't compiled. Direction itself
  works without the package.
- **The builder uses separate names** (`LocalizedText`, not a `Text(LocalizedString)` overload). An overload would
  make every assembly that calls `Text("...")` reference Unity.Localization.
- **Dropdown choices and tab labels** aren't localized by Weave; bind them yourself.

## Skins

- **One background image per state:** USS draws a single background image, so layered art (fill + frame) has to be
  flattened into one sprite per state.
- **Skins out-rank utility classes** on the parts they skin, so a utility on a skinned button won't show through.

## Editor tools

- **Preview window:**
  - It runs in an editor panel, so Unity's base widget colours come from the editor theme rather than the runtime
    one. Weave's controls override almost all of them.
  - Hover, Active and Focus can't be forced (UI Toolkit has no public way to), so the Live column shows them as you
    hover, press or Tab to its controls.
- **Handler `link.xml`** covers methods named in UXML on screens, the view model types they declare, and `ViewModel`
  subclasses. Handlers on some other object you bind yourself still need `[Preserve]`.

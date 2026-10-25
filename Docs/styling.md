# Styling

Weave UI styles runtime UI with Tailwind-named utility classes generated from a design tokens asset, a set of semantic
colour variables that the controls read, and your own USS on top. This page lists the utility classes and their
variants, explains breakpoints and the runtime layout utilities, and shows how tokens, semantic variables, utilities
and your stylesheets fit together.

## How the styles fit together

`ScreenHost` gives its panel the Weave UI theme, which imports these stylesheets in order:

| Order | Stylesheet | Contains |
|---|---|---|
| 1 | Unity's default runtime theme | The base look of Unity's controls |
| 2 | `tokens.uss` | Design tokens as custom properties: `--color-slate-900`, `--radius-lg`, `--text-xl`, ... |
| 3 | `controls.uss` | Semantic variables (`--weave-primary`, ...) and the default look of the [controls](controls.md) |
| 4 | `transitions.uss` | Transition presets (`fade-in`, `hover-lift`, ...) |
| 5 | `utilities.uss` | The utility classes (`p-4`, `bg-red-500`, `hover:bg-red-400`, ...) |

`tokens.uss` and `utilities.uss` are generated from a `WeaveTokens` asset. Weave UI ships both, generated from Tailwind
v4's default theme, so the classes below work with no setup.

There are two ways to get utility classes:

- **Native utilities** (the default). The tokens asset generates a fixed set of classes, listed below. No external tools.
- **The Tailwind pipeline** (optional). The Tailwind CSS CLI scans your UXML and C# and generates exactly the classes you
  use, including breakpoint prefixes, `group-hover:`, arbitrary values and more variants. See
  [Tailwind pipeline](tailwind-pipeline.md).

Both use the same class names and the same token variables, so markup works with either.

## Utility classes

Put utility classes in a UXML `class` attribute, or add them in C# with `AddWeaveClass` or the builder's `Class`:

```xml
<ui:VisualElement class="flex-row items-center justify-between p-4 mb-2 bg-slate-800 rounded-lg">
    <ui:Label text="Gold" class="text-sm text-slate-400" />
    <ui:Label text="1,250" class="text-2xl font-bold text-amber-400" />
</ui:VisualElement>
```

```csharp
using static Nerdova.WeaveUI.Ui;

var row = Row().Class("items-center justify-between p-4 mb-2 bg-slate-800 rounded-lg").Children(
    Text("Gold").Class("text-sm text-slate-400"),
    Text("1,250").Class("text-2xl font-bold text-amber-400"));
```

### Native utility families

In the table, `{n}` is a step on the [spacing scale](#spacing-scale), `{f}` a [fraction](#fractions), `{colour}` a
[colour](#colours) and `{side}` one of `x`, `y`, `t`, `r`, `b`, `l`.

| Family | Classes | USS property |
|---|---|---|
| Display | `flex`, `hidden` | `display` |
| Position | `relative`, `absolute` | `position` |
| Visibility | `visible`, `invisible` | `visibility` |
| Overflow | `overflow-visible`, `overflow-hidden` | `overflow` |
| Flex direction | `flex-row`, `flex-row-reverse`, `flex-col`, `flex-col-reverse` | `flex-direction` |
| Flex wrap | `flex-wrap`, `flex-wrap-reverse`, `flex-nowrap` | `flex-wrap` |
| Flex | `flex-1`, `flex-auto`, `flex-initial`, `flex-none` | `flex-grow`, `flex-shrink`, `flex-basis` |
| Grow and shrink | `grow`, `grow-0`, `shrink`, `shrink-0` | `flex-grow`, `flex-shrink` |
| Flex basis | `basis-auto`, `basis-full`, `basis-{n}`, `basis-{f}` | `flex-basis` |
| Align items | `items-start`, `items-end`, `items-center`, `items-stretch` | `align-items` |
| Justify | `justify-start`, `justify-end`, `justify-center`, `justify-between`, `justify-around`, `justify-evenly` | `justify-content` |
| Align self | `self-auto`, `self-start`, `self-end`, `self-center`, `self-stretch` | `align-self` |
| Align content | `content-start`, `content-end`, `content-center`, `content-stretch` | `align-content` |
| Padding | `p-{n}`, `p{side}-{n}` (`px-4`, `pt-2`, ...) | `padding` |
| Margin | `m-{n}`, `m{side}-{n}`; negative `-m-{n}`, `-m{side}-{n}` (`-mt-2`) | `margin` |
| Width | `w-auto`, `w-full`, `w-{n}`, `w-{f}` | `width` |
| Height | `h-auto`, `h-full`, `h-{n}`, `h-{f}` | `height` |
| Size | `size-auto`, `size-full`, `size-{n}`, `size-{f}` | `width` and `height` |
| Min size | `min-w-full`, `min-w-{n}`, `min-h-full`, `min-h-{n}` | `min-width`, `min-height` |
| Max size | `max-w-full`, `max-w-none`, `max-w-{n}`, `max-h-full`, `max-h-none`, `max-h-{n}` | `max-width`, `max-height` |
| Inset | `inset-0`, `inset-x-0`, `inset-y-0` | `top`, `right`, `bottom`, `left` |
| Offsets | `top-`, `right-`, `bottom-`, `left-` + `auto`, `full` or `{n}`; negative `-top-{n}`, ... | `top`, `right`, `bottom`, `left` |
| Font size | `text-xs`, `text-sm`, `text-base`, `text-lg`, `text-xl`, `text-2xl` ... `text-9xl` | `font-size` |
| Font weight | `font-thin`, `font-extralight`, `font-light`, `font-normal`, `font-medium`, `font-semibold`, `font-bold`, `font-extrabold`, `font-black` | `-unity-font-style` |
| Font style | `italic`, `not-italic` | `-unity-font-style` |
| Text align | `text-left`, `text-center`, `text-right` | `-unity-text-align` (upper row) |
| Whitespace | `whitespace-normal`, `whitespace-nowrap`, `whitespace-pre-wrap` | `white-space` |
| Text overflow | `text-ellipsis`, `text-clip`, `truncate` | `text-overflow` (`truncate` also sets `overflow` and `white-space`) |
| Border width | `border`, `border-0`, `border-2`, `border-4`, `border-8`, `border-{side}`, `border-{side}-{0,2,4,8}` | `border-width` |
| Border radius | `rounded`, `rounded-{radius}`, `rounded-full`, and per side or corner: `rounded-{t,r,b,l,tl,tr,br,bl}-{radius}` | `border-radius` |
| Background colour | `bg-transparent`, `bg-{colour}` | `background-color` |
| Text colour | `text-transparent`, `text-{colour}` | `color` |
| Border colour | `border-transparent`, `border-{colour}` | `border-color` |
| Opacity | `opacity-0`, `opacity-5`, `opacity-10`, ... `opacity-100` (steps of 5) | `opacity` |
| Grid and gap | `grid`, `grid-cols-{count}`, `col-span-{count}`, `gap-{n}`, ... | Laid out at runtime: see [Layout utilities](#layout-utilities-grid-and-gap) |

Notes:

- **Font weights:** USS only has normal and bold, so weights of 600 and above (`font-semibold`, `font-bold`,
  `font-extrabold`, `font-black`) render bold and the rest render normal.
- **Font style:** `font-*`, `italic` and `not-italic` all set `-unity-font-style`, so they can't be combined. For bold
  italic, write `-unity-font-style: bold-and-italic;` in your own USS.
- **Text alignment** keeps Unity's default top alignment: `text-center` is `upper-center`. For vertical centring, write
  `-unity-text-align: middle-center;` in your own USS.
- **Radii:** `{radius}` is a radius token name: `none`, `xs`, `sm`, `md`, `lg`, `xl`, `2xl`, `3xl`, `4xl` with the
  default tokens. `rounded` alone uses the `DEFAULT` radius token. `rounded-full` is `9999px`.
- **Shorthands lose to longhands:** padding, margin, border and radius shorthands are written before their longhands,
  so `p-4 px-2` gives 16px top and bottom, 8px left and right, whatever the order in the `class` attribute.
- **Not available in USS:** shadows, `line-height`, `z-index`, `text-decoration` and more. See
  [Known limitations](known-limitations.md).

### Spacing scale

Spacing classes (`p-`, `m-`, `w-`, `h-`, `size-`, `min-`/`max-`, `basis-`, offsets and `gap-`) take a step from the
tokens' spacing scale. One step is the tokens' spacing unit (4px by default), so `p-4` is 16px.

| Step | Value |
|---|---|
| `px` | 1px |
| `0`, `0.5`, `1`, `1.5`, `2`, `2.5`, `3`, `3.5` | 0, 2, 4, 6, 8, 10, 12, 14px |
| `4` to `12` | 16 to 48px, every step |
| `14`, `16`, `20`, `24`, `28`, `32`, `36`, `40`, `44`, `48`, `52`, `56`, `60`, `64`, `72`, `80`, `96` | Step × 4px (56px to 384px) |

Spacing classes are literal pixel values, because USS has no `calc()`. Changing the spacing unit needs a
[regeneration](#using-your-own-tokens).

### Fractions

`w-`, `h-`, `size-` and `basis-` also take fractions: `n/d` for every `d` from 2 to 6 and `n` below `d` (`1/2`, `1/3`,
`2/3`, `1/4`, `3/4`, `2/5`, `5/6`, ...). `w-1/2` is `width: 50%`.

### Colours

`bg-`, `text-` and `border-` take any colour token. The default tokens are Tailwind v4's palette:

- **Hues:** `red`, `orange`, `amber`, `yellow`, `lime`, `green`, `emerald`, `teal`, `cyan`, `sky`, `blue`, `indigo`,
  `violet`, `purple`, `fuchsia`, `pink`, `rose`.
- **Greys:** `slate`, `gray`, `zinc`, `neutral`, `stone`, `mauve`, `olive`, `mist`, `taupe`.
- **Shades:** each of the above in `50`, `100`, `200`, ... `900`, `950` (`bg-slate-900`, `text-emerald-400`).
- **Plain:** `black`, `white`, and `transparent`.

Colour utilities reference the token variables (`.bg-red-500 { background-color: var(--color-red-500); }`), so
redefining a variable on a subtree recolours every utility that uses it. See [Design tokens](#design-tokens).

### Special characters in class names

Tailwind names contain `.`, `/` and `:`. Write them as they are in UXML and in C#:

```xml
<ui:VisualElement class="w-1/2 p-0.5 hover:bg-slate-700 dark:bg-slate-950" />
```

In hand-written USS, escape them with a backslash. A digit at the start of a class name is written as a hex escape
followed by a space:

| Class | Selector in USS |
|---|---|
| `p-0.5` | `.p-0\.5` |
| `w-1/2` | `.w-1\/2` |
| `hover:bg-red-500` | `.hover\:bg-red-500:hover` |
| `2xl:p-4` | `.\32 xl\:p-4` |

Escaped selectors only work as a single compound selector (one element, no combinator). Variants that depend on an
ancestor, such as `dark:`, breakpoints and `group-hover:`, need a descendant selector, so they target an
identifier-safe **alias** instead: every character outside `A-Z a-z 0-9 -` becomes `_xHH` (its hex code), as does a
digit at the start.

| Class | Alias | Generated selector |
|---|---|---|
| `dark:bg-slate-950` | `dark_x3Abg-slate-950` | `.theme-dark .dark_x3Abg-slate-950` |
| `md:p-4` | `md_x3Ap-4` | `.bp-md .md_x3Ap-4` (Tailwind pipeline) |
| `2xl:pr-3` | `_x32xl_x3Apr-3` | `.bp-2xl ._x32xl_x3Apr-3` (Tailwind pipeline) |

The element must carry the alias class as well as the real one. See [Class aliases](#class-aliases).
`WeaveClassNames` exposes the encodings if you generate selectors yourself:

| Member | Description |
|---|---|
| `WeaveClassNames.Escape(string className)` | The CSS-escaped form for a single compound selector: `p-0.5` → `p-0\.5` |
| `WeaveClassNames.Alias(string className)` | The identifier-safe alias: `md:p-4` → `md_x3Ap-4`. Returns the name unchanged when no alias is needed |
| `WeaveClassNames.NeedsAlias(string className)` | True if the class can't appear unescaped in a selector (it has a character outside `[A-Za-z0-9_-]`, or starts with a digit or with `-` and a digit) |

## State variants

The native utilities include four state variants for every **colour** (`bg-`, `text-`, `border-`) and **opacity**
utility. Each maps to a UI Toolkit pseudo-class on the same element:

| Prefix | Applies while the element... | Generated selector |
|---|---|---|
| `hover:` | has the pointer over it (`:hover`) | `.hover\:bg-slate-700:hover` |
| `active:` | is being pressed (`:active`) | `.active\:bg-slate-800:active` |
| `focus:` | has keyboard or gamepad focus (`:focus`); the element must be focusable | `.focus\:border-sky-400:focus` |
| `disabled:` | is disabled, through `SetEnabled(false)` or `enabled="false"` (`:disabled`) | `.disabled\:opacity-50:disabled` |

```xml
<ui:VisualElement focusable="true"
    class="p-4 rounded-lg border-2 border-transparent bg-slate-800 hover:bg-slate-700 active:bg-slate-900 focus:border-sky-400" />
```

A state variant has higher specificity than the plain utility and the controls' base rules, so
`bg-emerald-500 disabled:bg-slate-500` on a disabled button shows slate.

Other variants:

- **`dark:`** applies under an ancestor with the `theme-dark` class, which `ThemeService` sets for dark mode (the
  default). The native utilities include `dark:` for colour and opacity utilities. Pair it with the light value:
  `class="bg-slate-100 dark:bg-slate-950"`. See [Theming](theming.md).
- **The Tailwind pipeline** adds variants for every utility it can convert: `checked:`, `enabled:`, `focus-visible:`
  (mapped to `:focus`), `group-hover:` and the other `group-*` states (under an ancestor with class `group`),
  breakpoint prefixes, and stacks such as `dark:hover:`. See [Tailwind pipeline](tailwind-pipeline.md).
- **Not possible in USS:** `peer-*`, `first:`/`last:` and other structural variants, and `before:`/`after:`. See
  [Known limitations](known-limitations.md).

## Responsive breakpoints

USS has no media queries. Instead, a `BreakpointController` watches an element's width and toggles a class for each
breakpoint it has reached, with Tailwind's min-width semantics: at 1100 units wide, both `bp-md` and `bp-lg` are on.
`ScreenHost` keeps one on its screen container (`host.Breakpoints`), so every screen sits under the active `bp-*`
classes. Widths are panel units: with `ScreenHost`'s default panel settings (scale with screen size, 1920x1080
reference), they follow the reference resolution rather than the physical screen.

| Breakpoint | Class | Minimum width |
|---|---|---|
| `sm` | `bp-sm` | 640 |
| `md` | `bp-md` | 768 |
| `lg` | `bp-lg` | 1024 |
| `xl` | `bp-xl` | 1280 |
| `2xl` | `bp-2xl` | 1536 |

What responds to breakpoints:

- **Layout utilities** (`md:grid-cols-4`, `lg:gap-x-2`) work with the native utilities. See
  [Layout utilities](#layout-utilities-grid-and-gap).
- **Every other utility** (`md:p-8`, `lg:flex-row`) needs the [Tailwind pipeline](tailwind-pipeline.md), which writes
  `md:p-8` as `.bp-md .md_x3Ap-8`.
- **Your own USS** can use the classes directly:

```css
.inventory { width: 100%; }
.bp-lg .inventory { width: 960px; }
```

React to breakpoint changes in C#:

```csharp
host.Breakpoints.Changed += breakpoint => Debug.Log($"Breakpoint: {breakpoint ?? "below sm"}");
```

To use your own breakpoints, assign a tokens asset to the **Tokens** field on the `ScreenHost` component (its
breakpoint list replaces the defaults), or create a controller yourself:

```csharp
var breakpoints = new BreakpointController(panelRoot, new[] { ("tablet", 600f), ("desktop", 1200f) });
```

| Member | Description |
|---|---|
| `BreakpointController(VisualElement target, IEnumerable<(string name, float minWidth)> breakpoints = null, string prefix = "bp-")` | Watches `target`'s width and toggles `<prefix><name>` classes on it. Null breakpoints uses `Defaults` |
| `static Defaults` | Tailwind's breakpoints: `sm` 640, `md` 768, `lg` 1024, `xl` 1280, `2xl` 1536 |
| `static From(WeaveTokens tokens)` | The breakpoint list from a tokens asset |
| `const DefaultPrefix` | `"bp-"` |
| `Prefix` | The class prefix in use |
| `Current` | The widest active breakpoint name, or null below the smallest |
| `event Action<string> Changed` | Raised with the new `Current` when it changes |
| `Apply(float width)` | Sets the classes for a width (the controller calls it on every geometry change) |
| `Dispose()` | Stops watching the element |

The layout utilities, the Tailwind pipeline and the Preview window always use the `bp-` prefix, so keep the default
prefix unless you only use breakpoints in your own USS.

## Layout utilities: grid and gap

USS has no CSS grid and no `gap`. `UtilityLayout` reads these classes at runtime and lays elements out with
manipulators instead:

| Class | Effect |
|---|---|
| `grid`, `inline-grid` | Lays the children out in a grid (one column unless `grid-cols-*` is set) |
| `grid-cols-{count}` | Number of equal columns |
| `grid-rows-{count}` | Number of equal rows, kept even when empty |
| `col-span-{count}`, `col-span-full` | On a grid child: how many columns it covers |
| `row-span-{count}`, `row-span-full` | On a grid child: how many rows it covers (`full`: every `grid-rows-*` row, from the top) |
| `gap-{n}`, `gap-x-{n}`, `gap-y-{n}` | Space between children, in spacing steps (`gap-4` is 16px); also `gap-px` and `gap-[12px]` |

Each class can take one breakpoint prefix (`md:grid-cols-4`), which applies while an ancestor has the matching `bp-*`
class. Later breakpoints override earlier ones, and the unprefixed class is the base.

```xml
<ui:VisualElement class="grid grid-cols-2 md:grid-cols-4 gap-4">
    <ui:VisualElement class="stat-card col-span-2" />
    <ui:VisualElement class="stat-card" />
    <ui:VisualElement class="stat-card" />
</ui:VisualElement>

<ui:VisualElement class="flex-row mb-4 gap-3">
    <weave:Button text="Buy" variant="primary" />
    <weave:Button text="Sell" variant="secondary" />
</ui:VisualElement>
```

How they behave:

- **Grid:** children are positioned absolutely, row by row, using CSS's sparse auto-placement. Rows are as tall as
  their tallest single-row child, children keep their own height (like `items-start`), row-spanning children fill their
  rows, and the container gets a `min-height` that fits them.
- **Explicit rows:** with `grid-rows-N`, the first N rows are equal: as tall as the tallest of them, or, when the
  container is given more height (a fixed height, `flex-grow`), an equal share of it. Rows past N fit their content.
  `row-span-full` children are placed first, left to right along the top, and span all N rows (one row without
  `grid-rows-*`).
- **Gap on a flex row or column:** each child after the first gets a leading margin, which replaces its own margin on
  that side. With `flex-wrap`, every child gets trailing margins and the container a negative margin.
- **Spacing unit:** gap steps use the `--spacing` variable from the tokens (4px if it can't be read).

Screens apply the layout utilities when they build their layout, and `WeaveGrid` applies them to its cells. For
elements you create or classes you add later, call `UtilityLayout.Apply`:

```csharp
var cards = new VisualElement();
cards.AddWeaveClass("grid grid-cols-3 gap-4");
Root.Add(cards);
UtilityLayout.Apply(cards);
```

| Member | Description |
|---|---|
| `UtilityLayout.Apply(VisualElement root)` | Calls `Attach` on `root` and every descendant |
| `UtilityLayout.Attach(VisualElement element)` | Attaches the manipulators one element's classes need. Called again, it re-runs them and removes those its classes no longer need |
| `UtilityLayout.TryParse(string className, out string breakpoint, out LayoutUtility utility, out float value)` | Parses a layout class. `value` is the count (0 for `-full`) or, for gaps, spacing steps (negative means pixels) |
| `UtilityLayout.DefaultSpacing` | 4: the spacing unit used when `--spacing` can't be read |
| `LayoutUtility` | `Grid`, `Columns`, `Rows`, `ColumnSpan`, `RowSpan`, `Gap`, `GapX`, `GapY` |
| `GridLayoutManipulator.Layout()` | Lays the children out now (it also runs on every geometry change) |
| `GapManipulator.Apply()` | Re-applies the gap margins now (it also runs on every geometry change) |

Not supported: named grid areas, `minmax()`, dense auto-flow and arbitrary templates such as `grid-cols-[200px_1fr]`.

## Class aliases

Classes such as `dark:bg-slate-950` and `md:p-4` match through their [alias](#special-characters-in-class-names), so
the element needs both classes. Weave UI adds the aliases for you when:

- a screen builds its layout from UXML or C#,
- you add classes with the builder's `Class("...")` or with `AddWeaveClass`,
- `WeaveGrid` creates a cell.

For anything you instantiate later, such as `ListView` rows made from a template, call `WeaveClassAliases.Apply`:

```csharp
var rowTemplate = Resources.Load<VisualTreeAsset>("UI/SaveSlot");
Root.Q<WeaveListView>("saves").makeItem = () =>
{
    var row = rowTemplate.Instantiate();
    WeaveClassAliases.Apply(row);
    return row;
};
```

| Member | Description |
|---|---|
| `WeaveClassAliases.Apply(VisualElement root)` | Adds the alias next to every class that needs one, on `root` and all its descendants |
| `element.AddWeaveClass(string classNames)` | Adds a class, or a space-separated list, plus any aliases they need |

`AddToClassList` adds the class only. That is enough for plain utilities and for state variants such as
`hover:bg-red-500`, but not for `dark:`, breakpoint or `group-*` variants.

## Design tokens

A `WeaveTokens` asset is the single source of design values. The generators turn it into `tokens.uss` (custom
properties) and `utilities.uss` (utility classes). Create one with *Create → Weave UI → Design Tokens*. All lengths are
in px.

| Field | Type | Description |
|---|---|---|
| `spacingUnit` | `float` | Length of one spacing step (4 by default). `p-4` is 4 steps |
| `spacingSteps` | `List<float>` | The steps that get spacing classes (`p-*`, `m-*`, `w-*`, ...) |
| `colors` | `List<ColorToken>` | Named colours: `name` (`slate-900`) and `value` |
| `radii` | `List<LengthToken>` | Named radii: `name` and `px`. `DEFAULT` becomes `rounded` and `--radius`; `none` becomes `rounded-none` with no variable |
| `fontSizes` | `List<FontSizeToken>` | `name`, `px` and `lineHeightPx` (kept for the Tailwind theme export; USS has no line-height) |
| `fontWeights` | `List<FontWeightToken>` | `name` and CSS `weight` (100-900); 600 and above render bold |
| `breakpoints` | `List<LengthToken>` | Breakpoint names and minimum widths, for `ScreenHost`, `BreakpointController.From` and the Tailwind pipeline |
| `durations` | `List<DurationToken>` | Named durations: `name` and `milliseconds` |
| `easings` | `List<EasingToken>` | Named easings: `name` and a UI Toolkit `EasingMode` |
| `generatedFolder` | `string` | Where Generate USS writes, relative to the asset's folder (`Generated` by default), or a project path starting with `Assets/` or `Packages/` |
| `TryGetColor(string name, out Color color)` | method | Looks up a colour token by name |

### Token variables

`tokens.uss` defines one custom property per token on `:root, .weave-root`, using Tailwind's variable names so native
utilities and the Tailwind pipeline read the same values:

| Variable | From | Default examples |
|---|---|---|
| `--spacing` | `spacingUnit` | `4px` |
| `--color-<name>` | `colors` | `--color-slate-900: #0f172b`, `--color-emerald-500: #00bc7d`, `--color-white` |
| `--radius`, `--radius-<name>` | `radii` | `--radius: 4px`, `--radius-md: 6px`, `--radius-lg: 8px`, `--radius-2xl: 16px` |
| `--text-<name>` | `fontSizes` | `--text-sm: 14px`, `--text-base: 16px`, `--text-xl: 20px`, `--text-2xl: 24px` |
| `--breakpoint-<name>` | `breakpoints` | `--breakpoint-md: 768px` |
| `--duration-<name>` | `durations` | `--duration-150: 150ms`, `--duration-200: 200ms` |
| `--ease-<name>` | `easings` | `--ease-linear`, `--ease-in`, `--ease-out`, `--ease-in-out` |

Use them in your own USS so your styles follow the tokens:

```css
.gold__text { font-size: var(--text-2xl); color: var(--color-amber-400); }
.vendor-badge { border-radius: var(--radius-2xl); border-color: var(--color-amber-300); }
```

Colour, radius and font-size utilities reference these variables, so redefining one in a stylesheet on a subtree (for
example a [palette](theming.md)) changes the matching utilities there at runtime, with no regeneration. Spacing
utilities are literal pixels.

### Using your own tokens

1. Create a tokens asset with *Create → Weave UI → Design Tokens*, or fill one from a Tailwind v4 theme file with
   **Import Tailwind theme…** in its inspector (see [Tailwind pipeline](tailwind-pipeline.md)).
2. Edit the values, then press **Generate USS** in the inspector. It writes `tokens.uss` and `utilities.uss` into the
   asset's generated folder and imports them. Files that haven't changed are left alone.
3. Regenerate after every token change. Generation doesn't run automatically.

The generated files take effect once your panel's theme imports them:

- **With the Tailwind pipeline**, assign the asset in *Project Settings → Weave UI*. The pipeline writes a project theme
  that imports its `tokens.uss`, and `ScreenHost` uses that theme. See [Tailwind pipeline](tailwind-pipeline.md).
- **With native utilities**, make a theme style sheet (`.tss`) that imports the same files in the same order as the
  default theme, with your generated files in place of the defaults. Point the middle two imports at `controls.uss`
  and `transitions.uss` in the Weave UI package's `Styles/Controls` folder. Then use it for the host's panel: give
  the `UIDocument` panel settings that use it (`ScreenHost` only creates its own when the document has none), or set
  `host.Document.panelSettings.themeStyleSheet` after creating the host.

```css
/* MyTheme.tss */
@import url("unity-theme://default");
@import url("project://database/Assets/UI/Generated/tokens.uss");
@import url("<Weave UI package>/Styles/Controls/controls.uss");
@import url("<Weave UI package>/Styles/Controls/transitions.uss");
@import url("project://database/Assets/UI/Generated/utilities.uss");
```

To change only colours, you don't need new files: override the token or semantic variables in a palette stylesheet
(see [Semantic variables](#semantic-variables)).

The tokens inspector and the generated folders are covered in [Editor tools](editor-tools.md).

## Semantic variables

The controls never use token colours directly. `controls.uss` defines semantic variables that point at tokens, and
every control reads those. The dark values are the defaults (on `:root, .weave-root`); `ThemeService` adds
`theme-light` for light mode, which redefines them.

| Variable | Used for | Dark (default) | Light |
|---|---|---|---|
| `--weave-primary` | Primary buttons, checked toggles, slider handles, progress fill, selected tab underline, dropdown checkmark | `emerald-500` | `emerald-600` |
| `--weave-primary-hover` | Primary button and slider handle on hover | `emerald-400` | `emerald-500` |
| `--weave-primary-active` | Primary button while pressed | `emerald-600` | `emerald-700` |
| `--weave-on-primary` | Text, icons and the toggle tick on primary and danger colours | `white` | `white` |
| `--weave-danger` | Danger buttons, `progress--danger` fill, invalid grid cells | `red-500` | `red-600` |
| `--weave-danger-hover` | Danger button on hover | `red-400` | `red-500` |
| `--weave-danger-active` | Danger button while pressed | `red-600` | `red-700` |
| `--weave-surface` | Panels, dialogs, list background, empty grid cells, the slider handle ring | `slate-900` | `white` |
| `--weave-surface-raised` | Field inputs, the toggle box, the dropdown menu, hovered rows, grid cells | `slate-800` | `slate-100` |
| `--weave-control` | Default button, slider track, progress track, grid items | `slate-700` | `slate-200` |
| `--weave-control-hover` | Default button on hover, field borders on hover | `slate-600` | `slate-300` |
| `--weave-control-active` | Default button while pressed | `slate-800` | `slate-400` |
| `--weave-selection` | Selected list rows and grid cells, drop targets | `emerald-900` | `emerald-100` |
| `--weave-text` | Text in controls, labels, titles | `slate-100` | `slate-900` |
| `--weave-text-muted` | Ghost buttons, unselected tabs, dialog body text, the dropdown arrow | `slate-400` | `slate-500` |
| `--weave-border` | Field, panel, list, secondary-button and grid-cell borders | `slate-700` | `slate-300` |
| `--weave-focus` | Focus borders on buttons, fields, sliders and grid cursors | `sky-400` | `sky-500` |
| `--weave-tooltip` | Tooltip background | `slate-950` | `slate-900` |
| `--weave-tooltip-text` | Tooltip text | `slate-100` | `white` |
| `--weave-backdrop` | The backdrop behind modal screens | `rgba(2, 6, 24, 0.7)` | `rgba(15, 23, 43, 0.35)` |
| `--weave-radius` | Corner radius of buttons and lists | `--radius-lg` (8px) | Not redefined |

To restyle every control at once, override the variables in a palette stylesheet and set it with
`host.Theme.Palette`. This one is adapted from the [Builder + themes sample](../Samples/BuilderThemes):

```css
/* EmberPalette.uss */
.weave-root {
    --weave-primary: var(--color-orange-500);
    --weave-primary-hover: var(--color-orange-400);
    --weave-primary-active: var(--color-orange-600);
    --weave-on-primary: var(--color-stone-950);
    --weave-surface: rgb(33, 22, 18);
    --weave-surface-raised: rgb(46, 31, 25);
    --weave-text: var(--color-orange-50);
    --weave-focus: var(--color-amber-300);
}
```

```csharp
host.Theme.Palette = Resources.Load<StyleSheet>("UI/EmberPalette");
```

A palette sheet sits on the screen container, after the theme, so it wins at equal specificity. It applies in light
and dark mode alike; to vary a palette by mode, add a `.theme-light` rule after the `.weave-root` one. See
[Theming](theming.md).

Use the semantic variables in your own USS too, so custom elements follow palettes and light mode, as the
[Settings menu sample](../Samples/SettingsMenu) does:

```css
.setting-row { border-bottom-width: 1px; border-color: var(--weave-border); border-radius: 8px; }
.setting-row:hover { background-color: var(--weave-surface-raised); }
.setting-row__hint { font-size: 13px; color: var(--weave-text-muted); }
```

## How utilities, controls and your USS combine

UI Toolkit picks between conflicting rules by specificity first, then by order. In practice:

- **Utilities beat the controls' base styles.** `utilities.uss` comes after `controls.uss`, so at equal specificity a
  utility wins: `<weave:Button variant="primary" class="bg-violet-600 rounded-full" />` is violet and round.
- **Control state rules beat plain utilities.** `.btn--primary:hover` (class plus pseudo-class) outranks `.bg-violet-600`,
  so the button above turns `--weave-primary-hover` on hover. Add matching variants to keep your colour:
  `bg-violet-600 hover:bg-violet-500 active:bg-violet-700`.
- **Utilities on a control style its root element only.** Inner parts, such as a toggle's box or a slider's handle,
  are styled with descendant selectors (`.toggle .unity-toggle__checkmark`). Change them with the semantic variables,
  your own USS or a skin. [Controls](controls.md) lists each control's parts.
- **Your stylesheets beat the theme.** Stylesheets on elements (a UXML `<Style src="...">`, `styleSheets.Add`, a
  palette or a skin) come after the theme, so at equal specificity they override utilities and controls alike. A
  utility on the same element as a `.hud-card` rule from your sheet loses to it if both set the same property.
- **Variants that use an ancestor class have higher specificity.** `.theme-dark .dark_x3Abg-slate-950` (two classes)
  beats `.bg-slate-100` (one), which is what makes `bg-slate-100 dark:bg-slate-950` work.
- **Skins out-rank utilities** on the parts they skin. See [Skins](skins.md).

When a utility doesn't seem to apply, check for a more specific rule in *Window → UI Toolkit → Debugger*, or write the
style in your own USS with a selector that matches or exceeds it.

## Other ready-made classes

`controls.uss` and `transitions.uss` also style a few classes you can put on plain elements:

| Class | Effect |
|---|---|
| `panel` | A raised card: 24px padding, border, `--radius-2xl` corners, `--weave-surface` background. The usual skin target |
| `title` | A heading: `--text-2xl`, bold, `--weave-text` |
| `modal__panel`, `modal__title`, `modal__body`, `modal__actions` | Dialog parts for modal screens (see [Screens](screens.md)) |
| `hover-lift`, `hover-grow`, `press-shrink` | Motion on hover or press: move up 3px, grow to 104%, shrink to 96% |
| `fade-in`, `slide-up`, `slide-down`, `slide-left`, `slide-right`, `scale-in` | Enter and leave presets for `WeaveTransitions` (see [Screens](screens.md)) |

The motion and transition presets are timed by the `--duration-*` and `--ease-*` tokens.

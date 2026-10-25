# Tailwind CSS pipeline

The Tailwind pipeline is an optional build step. It runs the official Tailwind CSS CLI over your UXML and C#, then
converts the CSS it produces into USS that UI Toolkit can load. This page covers when to use it, how to set it up, what
it generates, how CSS is turned into USS, and how to import an existing Tailwind theme into your design tokens.

## When to use it

Weave UI works without Tailwind. Its built-in utility classes are generated from your design tokens with no external
tools (see [Styling](styling.md)). The pipeline adds what the built-in set can't cover:

| | Built-in utilities | Tailwind pipeline |
|---|---|---|
| Setup | None | Install the Tailwind CSS v4 CLI yourself, then turn the pipeline on in Project Settings |
| Class set | A fixed set generated from the tokens: layout, flex, spacing steps, sizing, position, type, borders, colours, opacity | Every class Tailwind generates for the names it finds in your files |
| State variants | `hover:`, `active:`, `focus:`, `disabled:` on colour and opacity classes | `hover:`, `active:`, `focus:`, `disabled:`, `enabled:`, `checked:` on any class |
| `dark:` | Colour and opacity classes | Any class |
| Breakpoints (`md:p-4`) | No | Yes, for the breakpoints in your tokens |
| `group-hover:` and other `group-*` variants | No | Yes |
| Arbitrary values (`p-[13px]`, `bg-[#ff0000]`, `w-[50%]`) | No | Yes |
| Opacity modifiers (`bg-red-500/50`) | No | Yes |
| Spacing | The steps listed in the tokens | Any multiple of the spacing unit (`p-13`) |
| Rebuilds | When you press **Generate USS** on the tokens | Automatically when you save a UXML or C# file |

Both modes read the same token values under the same names, so markup that only uses built-in classes looks the same
with the pipeline on or off. Turn the pipeline on when you want responsive variants, arbitrary values or the wider
class set; leave it off if you'd rather not install Tailwind and add a build step.

## Requirements

- Unity 6000.3 or newer.
- The Tailwind CSS **v4** CLI, which you install yourself. Weave UI doesn't include or download it. Either:
  - install it with npm: `npm install -g @tailwindcss/cli`, or
  - download Tailwind's standalone executable for your platform from the `tailwindlabs/tailwindcss` GitHub releases
    (no Node.js needed).

Weave UI runs the CLI set in **Tailwind CLI path** in *Project Settings > Weave UI*, absolute or
project-relative, or, when that is empty, `tailwindcss` found on your `PATH`. It is tested with v4.3.3. Other 4.x
versions should work; it warns about a CLI that isn't v4, or whose version it can't read.

The CLI isn't part of the project, so each machine (and each build agent) needs its own. A project-relative
**Tailwind CLI path** works for everyone only if the executable is at that path on every machine; otherwise leave the path empty
and put `tailwindcss` on each machine's `PATH`.

## Turning it on

1. Open **Edit > Project Settings > Weave UI**.
2. Set **Tailwind CLI path**, or click **Select the CLI…**. Leave it empty if `tailwindcss` is on your `PATH`.
3. Tick **Tailwind Enabled**. The pipeline runs straight away; if it finds no CLI, it offers **Select the CLI…**.
4. Check the Console for the summary, for example
   `Weave UI Tailwind: 128 rules from 139 classes; 0 of your classes unsupported (see Assets/WeaveUI/Generated/tailwind-report.txt)`.

From then on, `ScreenHost` loads the generated project theme instead of the package default, so your screens use the
Tailwind output.

Unticking **Tailwind Enabled** deletes the generated project theme, and `ScreenHost` falls back to the default theme
with the built-in utilities. `tailwind.uss` and the report stay where they are.

> **Your own Panel Settings.** `ScreenHost` picks the project theme only when it creates the panel settings itself (its
> `UIDocument` has none assigned). If you assign your own Panel Settings asset, set its **Theme Style Sheet** to
> `<Output Folder>/Resources/WeaveUI/WeaveProjectTheme.tss`. `ScreenHost.LoadTheme()` returns the project theme when
> it exists and the package default otherwise.

## Settings

The settings live in `ProjectSettings/WeaveUISettings.asset`, so commit that file to share them with your team. They
appear under **Project Settings > Weave UI**.

| Setting | Default | Description |
|---|---|---|
| Tailwind Enabled | Off | Generate USS with the Tailwind CLI. Its output replaces the built-in utility classes. Turning it on runs the pipeline; turning it off removes the project theme. |
| Tailwind CLI path | Empty | Your Tailwind CSS v4 CLI, absolute or project-relative: the standalone executable, or the `tailwindcss` (`tailwindcss.cmd` on Windows) that npm installs. Empty uses `tailwindcss` on your `PATH`. |
| Tokens | None | The `WeaveTokens` asset written into Tailwind's `@theme`. Empty uses the package's default tokens. |
| Sources | `Assets/**/*.uxml`, `Assets/**/*.cs` | Files Tailwind scans for class names, as project-relative globs. |
| Output Folder | `Assets/WeaveUI/Generated` | Where `tailwind.uss`, the report and the project theme are written. |
| Watch | On | Re-run the pipeline when a file matched by **Sources** changes. |
| Debounce Milliseconds | `300` | How long to wait after the last change before running. |
| Rem Px | `16` | Pixels per `rem` when converting Tailwind values to USS, and when **Import Tailwind theme…** reads a theme into tokens. |

`dark:` classes always require a `theme-dark` ancestor and breakpoint classes a `bp-*` one (`bp-md`), the classes
`ThemeService` and `BreakpointController` set, so neither is a setting.

Below the settings:

- A status line shows the CLI found and its version, or why none was found. A CLI that isn't v4 gets a warning.
- **Select the CLI…** opens a file picker and saves your choice to **Tailwind CLI path** (project-relative when the
  file is inside the project).
- **Run Tailwind now** runs the pipeline and logs a summary. It is available while the pipeline is on.
- **Open report** opens `tailwind-report.txt` in your default text editor.

### Running the pipeline from code

`TailwindPipeline.Run` runs the pipeline with the project settings. Pass `interactive: false` in build scripts and
batch mode: it never shows a dialog, so set **Tailwind CLI path** or put `tailwindcss` on the machine's `PATH` first.

```csharp
using UnityEditor;
using Nerdova.WeaveUI.Editor.Tailwind;

public static class StyleBuild
{
    // Unity.exe -batchmode -quit -projectPath . -executeMethod StyleBuild.GenerateTailwind
    public static void GenerateTailwind()
    {
        var result = TailwindPipeline.Run(interactive: false);
        if (!result.Success)
            throw new System.Exception(result.Message);
    }
}
```

`RunResult` has `Success`, `Message`, `Milliseconds` and `Output` (the post-processor's result, with per-class
reports).

## What a run does

1. **Writes the Tailwind input** to `Library/WeaveUI/tailwind/input.css`.
2. **Runs the CLI** on it, producing `Library/WeaveUI/tailwind/output.css`. The CLI has 60 seconds to finish.
3. **Converts the output to USS** (see [How CSS becomes USS](#how-css-becomes-uss)).
4. **Writes the generated files**, each only if its contents changed, so unchanged runs don't trigger reimports:

   | File | Contents |
   |---|---|
   | `<Output Folder>/tailwind.uss` | The utility classes as USS |
   | `<Output Folder>/tailwind-report.txt` | Every class that was dropped or partly dropped, with the reason |
   | `<Output Folder>/Resources/WeaveUI/WeaveProjectTheme.tss` | The project theme: Unity's default runtime theme, your tokens' `tokens.uss`, the controls' styles and transitions, then `tailwind.uss` last so utilities win over control styles |

   It also regenerates the tokens' `tokens.uss` and `utilities.uss` if `tokens.uss` is missing or out of date. Tokens
   inside a package, such as the defaults used when **Tokens** is empty, are never written there: if the package's
   `tokens.uss` is out of date, a current copy is written to `<Output Folder>/tokens.uss` and the theme uses that.
5. **Warns** in the Console about classes you wrote that produced no USS at all.
6. **Refreshes running panels** in play mode if anything changed.

Don't edit the generated files; your changes are overwritten on the next run.

### The Tailwind input

The input imports Tailwind's theme and utilities, without Tailwind's preflight (browser reset) styles. It turns off
Tailwind's automatic source detection, so only your **Sources** are scanned, and writes your tokens into an `@theme`
block. For the default tokens it looks like this (shortened):

```css
/* Generated by Weave UI. Do not edit: change Project Settings → Weave UI or the tokens. */
@import "tailwindcss/theme.css" layer(theme);
@import "tailwindcss/utilities.css" layer(utilities) source(none);
@source "../../../Assets/**/*.uxml";
@source "../../../Assets/**/*.cs";
@theme {
  --color-*: initial;
  --radius-*: initial;
  --text-*: initial;
  --font-weight-*: initial;
  --breakpoint-*: initial;
  --spacing: 4px;
  --color-red-500: #fb2c36;
  --text-xl: 20px;
  --text-xl--line-height: 28px;
  --font-weight-bold: 700;
  --breakpoint-md: 768px;
  --ease-out: ease-out;
}
```

The `initial` lines remove Tailwind's own colour, radius, text size, font weight and breakpoint scales, so your tokens
are the only source for them. A colour class such as `bg-brand-500` therefore exists only if your tokens have a
`brand-500` colour. The default tokens carry Tailwind's full default palette and scales. Namespaces the tokens don't
cover (shadows, letter spacing, blur and so on) keep Tailwind's defaults.

### What gets scanned

Tailwind reads each file in **Sources** as plain text and generates a class for every word that matches a utility. In
practice that means:

- UXML `class` attributes, including variants and arbitrary values.
- C# string literals, including fluent builder calls such as `.Class("flex-col gap-2")` and both branches of a
  conditional inside an interpolated string (`$"text-sm {(dim ? "opacity-75" : "opacity-100")}"`).

Write class names in full. A name assembled at runtime (`"bg-" + colour`) never appears in a file, so Tailwind doesn't
generate it.

Sources are relative to the project folder. To scan an embedded package, add globs such as
`Packages/com.mycompany.ui/**/*.uxml`. Tailwind skips files that your `.gitignore` ignores, even when a glob names them.

Because the scan is plain text, Tailwind also turns ordinary words in code (`static`, `filter`, `transform`) into
classes. These show up in the report but never in Console warnings. Warnings are limited to classes you clearly wrote:
those in UXML `class` attributes, and C# strings passed to `AddToClassList`, `RemoveFromClassList`,
`EnableInClassList`, `ToggleInClassList`, `AddWeaveClass` or the builder's `Class(...)`. For these warnings, a Sources
glob that starts with a wildcard (`**/*.uxml`) is only searched in `Assets` and `Packages`.

## Watch mode

With **Watch** on, saving, adding, moving or deleting a file that a **Sources** glob matches (outside the output
folder) schedules a run. In the globs, `*` matches within a folder, `**/` any number of folders and `?` one
character; matching ignores case. The run waits for **Debounce Milliseconds** after the last change and for any script
compilation or asset import to finish. A pending run survives the domain reload that a C# change causes.

Watch runs are quiet: they don't show dialogs, and only write to the Console on errors, when the CLI isn't v4, or when
one of your classes is newly unsupported. Later runs don't repeat a warning until the next script reload.

In play mode:

- When `tailwind.uss` changes, every panel using the project theme restyles without leaving play mode.
- When a screen's UXML file is reimported, open screens using that layout are rebuilt in place, keeping their position
  in the stack and their view model. `OnCreated`, and `OnShow` if the screen is visible, run again. This also works
  with the pipeline off.

From saving a UXML file to seeing the new styles on screen typically takes under a second.

## How CSS becomes USS

USS is a subset of CSS, and the ways it fails are severe:

- **Some syntax loses the whole stylesheet** at import, with no runtime signal: `rem` and `em` units, exponent numbers
  (`3.4e38px`), an empty `var(--x,)` fallback, CSS nesting, `:is()`, `:not()`, `:where()`, the `+` and `~`
  combinators, and ratios such as `16 / 9`.
- **An unknown function in a value breaks the panel.** `calc()`, `oklch()`, `color-mix()`, `cubic-bezier()` or
  `translateX()` import with only a warning, but then every style update on that panel throws, until a new panel is
  created.
- **Some values are silently wrong.** `opacity: 50%` is read as 50, `scale: 110%` as 110x, `rgb(255 0 0 / 50%)` drops
  its alpha, and `currentColor` renders transparent.

So the post-processor works from an allow-list. It keeps only properties and values USS accepts, rewrites what it can,
and drops everything else, recording the reason. Output values may only contain the functions `var`, `rgb`, `rgba`,
`url`, `resource`, `blur` and `grayscale`.

### Values

Tailwind's raw output is resolved in this order: custom properties set in the rule itself, then theme variables, then
the initial values Tailwind declares with `@property`.

- **Token references stay live.** A value that is exactly `var(--name)`, where your tokens' `tokens.uss` defines
  `--name`, is kept as a reference. That covers colours, spacing, radii, font sizes, breakpoints, durations and
  easings, so a palette stylesheet that overrides `--color-*` variables at runtime also restyles Tailwind classes (see
  [Theming](theming.md)).
- **Everything else is folded to a literal.** That includes Tailwind's `--tw-*` composition variables, so
  `translate-x-2` becomes `translate: 8px 0`.

| CSS from Tailwind | USS written |
|---|---|
| `rem` | px, at **Rem Px** (`1.5rem` → `24px`) |
| `em` in `letter-spacing` and `word-spacing` | px, using **Rem Px** as the font size (an approximation) |
| Other `em`, `vw`, `vh`, `ch` and similar units | Dropped |
| `calc()` | Folded to one value (`calc(1 / 2 * 100%)` → `50%`, `calc(infinity * 1px)` → `9999px`). Mixed units that need runtime `calc()` (`calc(100% - 10px)`) are dropped |
| `oklch()`, `oklab()`, `hsl()`, `color-mix()`, `rgb()` with space syntax | Hex, or `rgba()` when not opaque (`bg-red-500/50` → `rgba(251, 44, 54, 0.5)`) |
| `currentColor` | Dropped |
| `opacity` and `scale` as percentages | Numbers (`50%` → `0.5`) |
| `font-weight` | `-unity-font-style: bold` for 600 and above, `normal` below |
| `font-style: italic` | `-unity-font-style: italic` |
| `text-align: left/center/right` (and `start`/`end`) | `-unity-text-align: upper-left/upper-center/upper-right` |
| `display: block`, `inline`, `inline-block`, `inline-flex`, `flow-root` | `display: flex` |
| `position: static` / `fixed` | `relative` / `absolute` |
| `overflow` other than `visible` | `hidden` |
| `align-*` and `justify-content` `start`/`end` | `flex-start`/`flex-end` |
| `white-space: pre` | `pre-wrap` |
| `aspect-ratio: 16 / 9` | `1.7778` |
| `cursor: pointer`, `default`, `move`, `grab`, `ew-resize`, `ns-resize`, ... | USS cursor keywords (`link`, `arrow`, `move-arrow`, `pan`, `resize-horizontal`, `resize-vertical`) |
| `cubic-bezier()` easings | The nearest keyword (`ease-in`, `ease-out`, `ease-in-out`, `linear`) |
| `transition-property` | Only properties USS can animate; `transform` becomes `translate, scale, rotate`, custom properties are removed |
| Logical properties (`padding-inline`, `margin-inline-start`, `border-start-start-radius`, ...) | Physical sides, left-to-right |
| `inset-*` | `top`, `right`, `bottom`, `left` |
| `filter: blur()`, `filter: grayscale()` | Kept; other filter functions are dropped |
| `!important` | Removed |
| `-unity-*` properties | Kept as they are |

### Dropped properties

| CSS property | Reason |
|---|---|
| `line-height` | USS has no line height. Text size classes keep their font size and lose only this |
| `gap`, `row-gap`, `column-gap`, `display: grid`, `grid-*` | Not in USS. `gap-*`, `grid`, `grid-cols-*`, `col-span-*` and `row-span-*` are laid out at runtime instead (see below) |
| `z-index`, `order` | Order follows the element hierarchy |
| `box-shadow`, `backdrop-filter`, `outline*` | Not in USS |
| `border-style` | USS borders are always solid |
| `text-decoration*`, `text-transform` | Not in USS |
| `user-select`, `pointer-events` | Not in USS; use `VisualElement.pickingMode` instead of `pointer-events` |
| `clip-path` | Would break the panel |
| `transform` | Use the `translate`, `scale` and `rotate` classes |
| `background-image` | Gradients aren't supported |
| `animation` | USS has no keyframe animations; use transitions |
| `font-family` | Set fonts with `-unity-font-definition` |
| `box-sizing` | USS is always border-box |
| `overflow-x`, `overflow-y` | USS overflow can't be set per axis |
| `scroll-*`, vendor-prefixed (`-webkit-*`, `-moz-*`) and any other unknown property | Not in USS |

Because composition is folded at build time, several `translate`, `scale` or `filter` classes on one element don't
combine: the last one wins.

### Runtime layout

`gap-*` on flex containers, and `grid`, `grid-cols-N`, `col-span-N` and `row-span-N`, have no USS equivalent. Weave UI
lays these out at runtime instead (`UtilityLayout`), including breakpoint versions such as `md:grid-cols-3` when the
breakpoint is in your tokens. The report lists them under *Handled at runtime*. See [Styling](styling.md) for how the
emulation behaves.

### Selectors and variants

| Tailwind | USS |
|---|---|
| `p-4` | `.p-4` |
| `w-1/2`, `p-0.5`, `p-[13px]`, `2xl:p-4` | Escaped real name: `.w-1\/2`, `.p-0\.5`, `.p-\[13px\]`, `.\32 xl\:p-4` |
| `hover:`, `focus:`, `active:`, `disabled:`, `enabled:`, `checked:` | Pseudo-class on the escaped name: `.hover\:bg-red-600:hover` |
| `focus-visible:` | Approximated as `:focus` |
| Tailwind's `@media (hover: hover)` wrapper | Removed; the `:hover` rule inside is kept |
| `dark:` | `.theme-dark .dark_x3Abg-slate-950` |
| `md:` and other breakpoints | `.bp-md .md_x3Ap-4`. The name comes from the token breakpoint with the same width |
| `dark:` with a breakpoint | Three selectors, so it works with both classes on one element or nested either way round |
| `group-hover:`, `group-focus:`, `group-active:`, `group-disabled:`, `group-checked:` | `.group:hover .group-hover_x3Atext-white` |

Not supported, and listed in the report:

- `peer-*` variants (USS has no sibling combinators).
- Child and descendant variants: `space-x-*`, `divide-*`, `*:` and `[&_...]`.
- Pseudo-elements: `placeholder:`, `before:`, `after:` and so on.
- Other pseudo-classes such as `first:`, `last:` and `odd:`.
- Media queries other than min-width breakpoints, dark and hover: `max-*` breakpoints, `print:`, `motion-*`, and
  min-width queries that match no token breakpoint (`min-[900px]:`).
- Rules inside `@supports` blocks.

Container queries (`@container`), `@starting-style` and other at-rules around class rules aren't supported either;
their rules are left out of the output, and the report lists each class with the at-rule as the reason.

For example, this UXML from the [Settings menu sample](../Samples/SettingsMenu) switches background with the theme:

```xml
<ui:VisualElement class="flex-1 items-center justify-center bg-slate-100 dark:bg-slate-950" />
```

The pipeline writes:

```css
.bg-slate-100 { background-color: var(--color-slate-100); }
.theme-dark .dark_x3Abg-slate-950 { background-color: var(--color-slate-950); }
```

### Class-name escaping and aliases

Tailwind class names contain characters that USS selectors can't hold as they are (`:`, `/`, `.`, `[`, a leading
digit). Your markup always uses the real Tailwind names; the generated USS adapts to them in one of two ways:

- **Escapes.** A rule with a single compound selector (plain utilities, arbitrary values, fractions and state
  variants) uses standard CSS escapes, such as `.hover\:bg-red-500:hover`. USS reads these back as the real class name,
  so they match without any help and also preview in UI Builder.
- **Aliases.** USS loses escapes as soon as a selector has a combinator, which every ancestor-based variant needs
  (`dark:`, breakpoints, `group-*`). Those rules target an identifier-safe alias instead: every character outside
  `A-Z`, `a-z`, `0-9` and `-` becomes `_x` plus two uppercase hex digits (`_u` plus four above U+00FF), as does a
  leading digit. `md:p-4` becomes `md_x3Ap-4`, and `2xl:p-4` becomes `_x32xl_x3Ap-4`.

An alias works only if the element carries it next to the real class. Weave UI adds aliases for you:

- Screens add them after building their layout.
- The builder's `Class(...)` adds them.
- `WeaveGrid` adds them to its cells.

For elements you create or class later, such as `ListView` rows made from a template, add them yourself:

```csharp
using Nerdova.WeaveUI;

row.AddWeaveClass("md:p-4 dark:bg-slate-800");   // adds each class and its alias
WeaveClassAliases.Apply(row);                    // or alias every class already in a subtree
```

`WeaveClassNames.Escape(string)` and `WeaveClassNames.Alias(string)` return the two encodings if you write USS by
hand. UI Builder can't preview ancestor-based variants because its canvas has no `theme-dark` or `bp-*` ancestor; use
the [Preview window](editor-tools.md#preview-window) to check them.

### The report

`tailwind-report.txt` in the output folder starts with a summary line, followed by three sections:

```
139 classes, 128 USS rules; 6 unsupported, 10 partly supported.

Unsupported (no USS written):
  ring  [not in UXML/C# strings]: box-shadow: USS has no box-shadow
  transform  [not in UXML/C# strings]: transform: use translate/scale/rotate

Partly supported (some declarations dropped):
  border: border-style: USS borders are always solid
  text-xl: line-height: USS has no line-height

Handled at runtime (grid and gap layout, see UtilityLayout):
  gap-2
  grid
```

- **Unsupported** classes produced no USS. `[not in UXML/C# strings]` marks classes Tailwind generated from ordinary
  words in your code rather than from class attributes or class APIs. You can usually ignore those.
- **Partly supported** classes lost some declarations but still have an effect.
- **Handled at runtime** classes are laid out by Weave UI rather than by USS.

When an unsupported class is one you wrote, the Console also shows a warning such as
`Weave UI Tailwind: 'shadow-lg' has no USS equivalent and was dropped: box-shadow: USS has no box-shadow. (Report: Assets/WeaveUI/Generated/tailwind-report.txt)`

## Importing a Tailwind theme into tokens

If you already have a Tailwind theme, for example from a web project or a design system, you can load its values into
a `WeaveTokens` asset. Both the built-in utilities and the pipeline then use them.

1. Create a tokens asset with **Assets > Create > Weave UI > Design Tokens**, or select an existing one.
2. In its Inspector, click **Import Tailwind theme…** and choose a `.css` file.
3. Click **Generate USS** to regenerate `tokens.uss` and `utilities.uss`.
4. Assign the asset to **Tokens** in **Project Settings > Weave UI**, and to the **Tokens** field of your `ScreenHost`
   so its breakpoints match.

The importer reads every `--name: value;` declaration in the file, so you can give it an `@theme { }` block or the
output of `@import "tailwindcss/theme.css" theme(static);`. Values are read like this:

| Tailwind variable | Token list |
|---|---|
| `--color-*` (hex, `rgb()`, `oklch()`, `transparent`) | Colors |
| `--spacing` | Spacing Unit |
| `--radius-*` | Radii, plus `none` (0) and `DEFAULT` (4 px) |
| `--text-*`, with `--text-*--line-height` | Font Sizes. Ratio line heights such as `calc(1.75 / 1.25)` are multiplied by the size; without one, the line height is 1.5 times the size |
| `--font-weight-*` | Font Weights |
| `--breakpoint-*` | Breakpoints, sorted by width |
| `--ease-*` | Easings, plus `linear`. `in`, `out` and `in-out` become `EaseIn`, `EaseOut` and `EaseInOut`; any other name becomes `Ease` |

- Lengths must be in `px`, `rem` or a unitless `0`. `rem` is converted at **Rem Px** (16 by default).
- Variables USS can't use, such as fonts, shadows, animations, letter spacing and line-height multipliers, are skipped.
- A token list is replaced only when the file has values for it; other lists keep their contents.
- Empty **Spacing Steps** and **Durations** are filled with Tailwind's default steps.
- The import can be undone with **Edit > Undo**.

From an editor script, `WeaveStyleGenerator.ImportTailwindTheme(themeCssPath, assetPath, remPx)` creates a tokens
asset, or updates an existing one, from a theme file, and `TailwindThemeImporter.Import(css, tokens, remPx)` fills an
existing asset from CSS text. `remPx` is optional: `ImportTailwindTheme` defaults to **Rem Px**, `Import` to 16. Both are in the `Nerdova.WeaveUI.Editor` namespace.

## Troubleshooting

**"no Tailwind CLI found" or "the Tailwind CLI path in Project Settings → Weave UI doesn't exist".**
**Tailwind CLI path** is empty and `tailwindcss` isn't on the `PATH` Unity sees, or the path is wrong. Install the CLI
(see [Requirements](#requirements)), then set the path or click **Select the CLI…**. If you added it to your `PATH`
while Unity was open, restart Unity (or Unity Hub) so it sees the change.

**"the Tailwind CLI is v3..." or "couldn't read the Tailwind CLI's version".**
The input file and the post-processor are written for Tailwind v4. Install a v4 CLI and point **Tailwind CLI path** at
it.

**"Tailwind CLI failed: ..." or "timed out".**
The message includes the CLI's own output. The usual cause is a **Sources** entry Tailwind can't read. The CLI has 60
seconds per run.

**A class has no effect.**

1. Open the report. If the class is listed as unsupported, the reason says what USS lacks; use one of the
   alternatives in [Dropped properties](#dropped-properties).
2. If the class isn't in the report or the USS at all, Tailwind didn't find it. Check that the file is matched by
   **Sources**, isn't ignored by `.gitignore`, and spells the class in full.
3. For `dark:`, breakpoint and `group-*` classes, check that the element has the alias (see
   [Class-name escaping and aliases](#class-name-escaping-and-aliases)) and that an ancestor has `theme-dark`, the
   `bp-*` class or `group`.
4. For breakpoints, check that the width exists in your tokens' breakpoints. `ScreenHost` takes its breakpoints from
   its own **Tokens** field (Tailwind's defaults when empty), so give it the same tokens as the project settings.
5. A [skin](skins.md) out-ranks utility classes on the parts it styles.

**My screens still use the built-in utilities.**
Check that **Tailwind Enabled** is on and that `<Output Folder>/Resources/WeaveUI/WeaveProjectTheme.tss` exists. If
your `UIDocument` uses your own Panel Settings, point its **Theme Style Sheet** at that file.

**Saving a file doesn't re-run the pipeline.**
Check that **Watch** is on and that a **Sources** glob matches the file. Changes inside the output folder are
ignored. Click **Run Tailwind now** to run it by hand.

## See also

- [Styling](styling.md): utility classes, variants, design tokens and breakpoints.
- [Editor tools](editor-tools.md): the Preview window, inspectors and settings.
- [Known limitations](known-limitations.md): what USS can't do, and what is emulated.

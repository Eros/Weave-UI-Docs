# Editor tools

Weave UI adds a few tools to the Unity editor: a Preview window, inspectors for design tokens and skins, a Project
Settings page for the Tailwind pipeline, and live reload in play mode. This page lists each one, where to find it and
what it does.

## Menu items

| Menu path | What it does |
|---|---|
| **Window > Weave UI > Preview** | Opens the [Preview window](#preview-window) |
| **Assets > Create > Weave UI > Design Tokens** | Creates a `WeaveTokens` asset named `WeaveTokens` (see [Design tokens inspector](#design-tokens-inspector)) |
| **Assets > Create > Weave UI > Skin** | Creates a `UISkin` asset named `UISkin` (see [Skins](skins.md)) |
| **Edit > Project Settings > Weave UI** | The [project settings](#project-settings) page |
| **Tools > Weave UI Samples > Build Synty Fantasy Menus Skin** | Added by the Builder + Themes sample only (see [below](#build-synty-fantasy-menus-skin)) |

The two **Create** items also appear when you right-click in the Project window and choose **Create > Weave UI**.

## Preview window

The Preview window shows every Weave UI control normal, disabled, checked and live, styled with your project's stylesheets. Use it to check
a skin, a token change or light and dark mode without opening a scene or entering play mode.

Open it with **Window > Weave UI > Preview**.

### What it shows

Each row is a control; each column is a state.

| Rows | Columns |
|---|---|
| Button; Button primary, secondary, ghost and danger; Button sm / lg; Toggle; Slider; Dropdown; Text field; Progress bar; Grid cell; Grid cell (empty); Tooltip; Title / panel | Normal, Disabled, Live, Checked |

The **Checked** column shows the toggle switched on, a selected grid cell, and an empty cell as a drop target. The
Normal, Disabled and Checked elements ignore the mouse. The **Live** column stays interactive: hover over it, press it
or Tab to it to see the hover, active and focus states.

### Toolbar

| Control | Description |
|---|---|
| **Skin** | *Utility styling (no skin)*, or any `UISkin` asset in the project. The list is refreshed each time you open the menu |
| **Theme** | Dark or Light, as `ThemeService` sets them at runtime |
| **Breakpoint** | none, or a breakpoint from the tokens set in [project settings](#project-settings) (`sm`, `md`, `lg`, `xl` and `2xl` with the defaults). Choosing one turns on its `bp-*` class and every smaller one, as `BreakpointController` does, so responsive classes respond |
| **Reload styles** | Reloads the stylesheets and rebuilds the rows. Use it after generating USS or running the Tailwind pipeline |

### Which styles it uses

The window loads the same stylesheets the game does, in this order:

1. `tokens.uss` for the tokens set in [project settings](#project-settings) (the package defaults if none is set).
2. The controls' styles and transitions.
3. `tailwind.uss` when the Tailwind pipeline is on and has run; otherwise the built-in `utilities.uss` for those tokens.

`WeavePreviewWindow.ProjectSheets()` returns these stylesheets, so your own editor tools can style a subtree the same
way: add them to an element that has the `weave-root` class. `WeavePreviewWindow.Open()` opens the window from code.

### Limitations

- The window runs in an editor panel, so Unity's base widget colours come from the editor theme rather than the runtime
  theme. Weave UI's controls override almost all of them.
- UI Toolkit has no public way to force hover, active or focus, so those states only show in the **Live** column while
  you interact with it.

## Design tokens inspector

Selecting a `WeaveTokens` asset shows a short note naming the files it generates and where, two buttons, and then the
token lists.

| Button | What it does |
|---|---|
| **Generate USS** | Writes `tokens.uss` (the tokens as custom properties) and `utilities.uss` (the built-in utility classes) into the asset's **Generated Folder**, and logs the folder. Files whose contents haven't changed aren't rewritten |
| **Import Tailwind theme…** | Asks for a `.css` file and fills the asset from its Tailwind theme variables. See [Importing a Tailwind theme](tailwind-pipeline.md#importing-a-tailwind-theme-into-tokens) |

The token lists are:

| Field | Description |
|---|---|
| Spacing Unit | Length of one spacing step in px. `p-4` is 4 steps |
| Spacing Steps | Steps that get built-in spacing and sizing classes (`p-*`, `m-*`, `w-*`, ...) |
| Colors | Named colours (`red-500`, `brand`) |
| Radii | Border radius sizes |
| Font Sizes | Text sizes, each with a line height (kept for Tailwind; USS has no line height) |
| Font Weights | CSS weights (100-900). USS has only normal and bold: 600 and above render bold |
| Breakpoints | Minimum widths for `sm`, `md`, ... |
| Durations | Transition durations in milliseconds |
| Easings | Named USS easing keywords |
| Generated Folder | Where generated USS goes: relative to the asset's folder, or a project path starting with `Assets/` or `Packages/`. Default `Generated` |

See [Styling](styling.md) for how the classes and variables use these values.

### Regenerating styles

Generated USS files are never edited by hand. Regenerate them after changing tokens:

- **Built-in utilities:** select the tokens asset and click **Generate USS**.
- **Tailwind pipeline:** with **Watch** on, the pipeline re-runs when you save a file its **Sources** match. After changing tokens, click
  **Run Tailwind now** in project settings; the run also regenerates the tokens' `tokens.uss` if it is out of date.
- **Skins:** select the skin and click **Generate USS** in its inspector.

Then click **Reload styles** in the Preview window if it is open.

## Skin inspector

Selecting a `UISkin` asset shows:

- The skin's root class (`.skin-<name>`) and the path of the stylesheet it generates
  (`Generated/skin-<name>.uss` next to the asset).
- Warnings for problems: a skin name that isn't lower-case letters, digits and `-`, a part with no selector, a state
  listed twice, or a sprite with no 9-slice border.
- **Generate USS**: writes the skin's stylesheet and links it to the asset.
- **Add missing states**: adds an empty entry for every state a part doesn't have yet.
- **Add control**: a menu of ready-made parts, such as *Button / Primary*, *Toggle / Box* and *Grid / Slot*, each with
  its selector and an empty entry for each of its states.

[Skins](skins.md) explains the fields, the states and how to build a skin from an art pack.

## Project settings

**Edit > Project Settings > Weave UI** holds the settings for the optional Tailwind pipeline: whether it is on, the
Tailwind CLI to run, which tokens it uses, the files it scans, where it writes, and watch mode. It also shows the CLI
it found and its version, and has **Select the CLI…**, **Run Tailwind now** and **Open report** buttons. The settings are saved to
`ProjectSettings/WeaveUISettings.asset`; commit it to share them. Searching Project Settings for *Weave*, *Tailwind*,
*USS* or *tokens* finds the page.

[Tailwind pipeline: Settings](tailwind-pipeline.md#settings) describes every setting.

## Live reload in play mode

Two things update while the game runs in the editor:

- **Layouts.** When a screen's UXML file is reimported (for example, saved from UI Builder), open screens using it are
  rebuilt in place. They keep their place in the stack and their view model, and `OnCreated`, plus `OnShow` if the
  screen is visible, run again. This works with or without the Tailwind pipeline.
- **Tailwind styles.** With the pipeline's **Watch** on, saving a file its **Sources** match regenerates
  `tailwind.uss`, and panels using the project theme restyle. See [Watch mode](tailwind-pipeline.md#watch-mode).

## Build Synty Fantasy Menus Skin

The [Builder + Themes sample](../Samples/BuilderThemes) includes an editor script that builds a `UISkin` from Synty's
*INTERFACE - Fantasy Menus* pack. Synty's art can't be redistributed, so the sample ships only the script; you import
the pack into your own project first.

Run **Tools > Weave UI Samples > Build Synty Fantasy Menus Skin**. It:

1. Finds the pack's button, panel and frame sprites, wherever the pack was imported.
2. Flattens each button state, and the panel, into one sprite (a tinted fill with the frame on top), because USS
   draws a single background image per element. The sprites keep Synty's 9-slice borders.
3. Writes the sprites and a skin named `synty-fantasy` into a `WeaveSkin` folder next to the pack, with the skin under
   `Resources/WeaveUISynty/SyntyFantasySkin`, and generates its USS.

A dialog reports the result. If the pack isn't in the project, the dialog says so and gives its Synty Store address.
Afterwards, press Play in the Builder + Themes sample scene and pick Synty from the **Skin** dropdown.

The script (`SyntySkinBuilder.cs`) is also a worked example of building a skin from code; see [Skins](skins.md) for
the general approach.

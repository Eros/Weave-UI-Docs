# Theming

Weave UI changes the look of a running UI without touching its markup. `ThemeService` switches between light and dark
mode, swaps in a palette stylesheet that overrides colour variables, and applies an art skin. This page covers all
three, how they layer, and the theme style sheet underneath them.

## How it works

Every `ScreenHost` creates a `ThemeService` for its screen container (the element named `weave-screens`, which carries
the `weave-root` class). The service only adds and removes classes and stylesheets on that element:

| Setting | What changes on the container |
|---|---|
| `Mode` | The class `theme-dark` (the default) or `theme-light`. |
| `Palette` | One extra stylesheet that overrides colour variables. |
| `Skin` | A skin's generated stylesheet and its `skin-<name>` class. |

The screen stack and the overlay layer (tooltips, drag visuals) are both inside the container, so every screen and
tooltip follows the theme. Because nothing in the hierarchy is rebuilt, bindings, focus and scroll positions survive a
switch.

```csharp
var host = ScreenHost.Create();
host.Theme.Mode = ThemeMode.Light;      // dark by default
host.Theme.Palette = brandColours;      // a StyleSheet; null removes it
host.Theme.Skin = fantasySkin;          // a UISkin; null returns to utility styling
```

## Light and dark mode

`ThemeMode` has two values, `Dark` and `Light`. Dark is the default. Set `Mode`, or call `ToggleMode()`:

```csharp
host.Theme.Mode = ThemeMode.Light;
host.Theme.ToggleMode();                // back to dark
bool dark = host.Theme.IsDark;
```

Mode affects two things.

### Semantic variables

The controls never use raw colours. They read semantic variables such as `--weave-primary`, `--weave-surface` and
`--weave-text`. The controls stylesheet defines the dark values on `:root, .weave-root` and the light values on
`.theme-light`, so switching mode recolours every `weave:` control, tooltip and modal at once. Controls fade their
colours over 0.12 s, so the switch animates.

| Variable | Used for |
|---|---|
| `--weave-primary`, `--weave-primary-hover`, `--weave-primary-active` | Primary buttons, checked toggles, slider handles, progress fills, the selected tab underline. |
| `--weave-on-primary` | Text and icons on primary and danger buttons. |
| `--weave-danger`, `--weave-danger-hover`, `--weave-danger-active` | Danger buttons, danger progress bars, invalid grid drops. |
| `--weave-surface`, `--weave-surface-raised` | Panels, dialogs and lists; raised surfaces for fields, toggle boxes, menus and hover states. |
| `--weave-control`, `--weave-control-hover`, `--weave-control-active` | Default buttons, slider tracks, progress backgrounds, grid items. |
| `--weave-selection` | Selected list rows and grid cells. |
| `--weave-text`, `--weave-text-muted` | Body text and hints. |
| `--weave-border` | Control and panel borders. |
| `--weave-focus` | The border of the focused control. |
| `--weave-tooltip`, `--weave-tooltip-text` | Tooltip background and text. |
| `--weave-backdrop` | The dimmed layer behind modal screens. |
| `--weave-radius` | Corner radius of buttons and lists. |

Use the same variables in your own USS so your screens follow the mode too. The Builder + Themes sample's stylesheet
does this throughout:

```css
.sheet-column {
    border-color: var(--weave-border);
    background-color: var(--weave-surface-raised);
}

.stat__fill { background-color: var(--weave-primary); }
```

### `dark:` variants

Utility classes with a `dark:` prefix apply only under `theme-dark`. The light palette covers only the semantic
variables, so a utility colour in your own markup stays the same in both modes unless you pair it with a `dark:`
variant:

```xml
<ui:VisualElement class="bg-slate-100 text-slate-900 dark:bg-slate-900 dark:text-slate-100" />
```

The built-in utilities include `dark:` forms of the background, text and border colours and of opacity; the
[Tailwind pipeline](tailwind-pipeline.md) can generate any utility with a `dark:` variant.

Ancestor-scoped variants need an alias class next to the class you wrote. Screens add the aliases after cloning their
layout, and the builder's `.Class(...)` adds them too. If you add a `dark:` class from code later, use
`AddWeaveClass` rather than `AddToClassList`:

```csharp
badge.AddWeaveClass("bg-amber-200 dark:bg-amber-700");
```

See [Styling](styling.md) for the full set of variants.

## Palettes

A palette is a USS file that overrides variables on the screen container. Use one for a brand colour scheme, a
faction colour, or an accessibility palette. It can override the semantic variables (to recolour the controls), the
colour tokens such as `--color-emerald-500` (to recolour every utility that uses them), and your own variables.

### Writing a palette

Target `.weave-root`, the class on the screen container. This is the Builder + Themes sample's
[EmberPalette.uss](../Samples/BuilderThemes/Resources/WeaveUISamples/BuilderThemes/EmberPalette.uss), shortened:

```css
.weave-root {
    --weave-primary: var(--color-orange-500);
    --weave-primary-hover: var(--color-orange-400);
    --weave-primary-active: var(--color-orange-600);
    --weave-on-primary: var(--color-stone-950);
    --weave-surface: rgb(33, 22, 18);
    --weave-surface-raised: rgb(46, 31, 25);
    --weave-control: rgb(68, 46, 37);
    --weave-text: var(--color-orange-50);
    --weave-text-muted: rgb(196, 160, 140);
    --weave-border: rgb(82, 55, 44);
    --weave-focus: var(--color-amber-300);
    --sheet-page: rgb(18, 11, 9);
}
```

`--sheet-page` is the sample's own variable. Its character sheet reads it with a fallback, so the page background
changes only while the palette is on:

```css
.sheet { background-color: var(--sheet-page, var(--color-slate-950)); }
```

Put the file in a `Resources` folder (or reference it from a serialized field) and load it as a `StyleSheet`:

```csharp
var ember = Resources.Load<StyleSheet>("WeaveUISamples/BuilderThemes/EmberPalette");
host.Theme.Palette = ember;
```

Setting `Palette` replaces the previous palette; setting it to `null` removes it. Only one palette is active at a
time, so combine overrides into one file if you need several.

### Palettes and mode

The palette sheet is added to the container itself, after the theme style sheet, so its `.weave-root` rule beats the
built-in `.theme-light` and `.weave-root` rules at equal specificity. A palette written like Ember therefore applies in
both modes. To give a palette separate light values, add a more specific rule to the same file:

```css
.weave-root { --weave-primary: var(--color-orange-500); }
.weave-root.theme-light { --weave-primary: var(--color-orange-700); }
```

## Switching at runtime from a view model

Keep the player's choice in a view model and apply it to `ThemeService` when it changes. The screen binds its toggle
or buttons to the view model as usual, and a small component turns the value into theme calls.

The [Settings Menu sample](../Samples/SettingsMenu) binds a toggle two-way to a `LightTheme` property and applies it
in its `SettingsMenuSample` component (shortened here):

```xml
<weave:Toggle name="light-theme" class="setting-row__toggle">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="LightTheme" binding-mode="TwoWay" />
    </Bindings>
</weave:Toggle>
```

```csharp
[RequireComponent(typeof(ScreenHost))]
public sealed class SettingsMenuSample : MonoBehaviour
{
    private ScreenHost _host;
    private SettingsViewModel _settings;

    void Start()
    {
        _host = GetComponent<ScreenHost>();
        _settings = new SettingsViewModel();
        _settings.propertyChanged += (_, e) =>
        {
            if (e.propertyName == nameof(SettingsViewModel.LightTheme))
            {
                ApplyToUi();
            }
        };
        ApplyToUi();
        _host.Screens.Push(new SettingsScreen(_settings));
    }

    void ApplyToUi() => _host.Theme.Mode = _settings.LightTheme ? ThemeMode.Light : ThemeMode.Dark;
}
```

The [Builder + Themes sample](../Samples/BuilderThemes) sets all three layers from one view model. Its theme choice is
an enum (`Dark`, `Light`, `Ember`) and its skin choice a string picked from a dropdown:

```csharp
void ApplyTheme()
{
    var theme = _host.Theme;
    theme.Mode = _character.Theme == ThemeChoice.Light ? ThemeMode.Light : ThemeMode.Dark;
    theme.Palette = _character.Theme == ThemeChoice.Ember ? _ember : null;
    theme.Skin = _skins.TryGetValue(_character.Skin, out var skin) ? skin : null;
}
```

To react to theme changes elsewhere, for example to update a label or save the setting, subscribe to `Changed`:

```csharp
host.Theme.Changed += () => PlayerPrefs.SetInt("dark-mode", host.Theme.IsDark ? 1 : 0);
```

`Changed` fires when `Mode` or `Palette` changes to a different value, and every time `Skin` is set.

## How mode, palette and skin layer

The three settings are independent; any combination works.

1. **Theme style sheet** (on the panel settings): Unity's default runtime theme, the design tokens, the controls
   stylesheet with its dark and light semantic values, transitions, then the utility classes.
2. **Mode**: `theme-dark` or `theme-light` on the container chooses which semantic values the controls use and whether
   `dark:` utilities apply.
3. **Palette**: overrides variables on the container. It wins over the built-in dark and light values.
4. **Skin**: replaces backgrounds, borders and text colours with 9-slice sprites on the parts it covers. Skin rules
   outrank single-class utilities. Anything a skin doesn't set still comes from the variables, so mode and palette
   keep working on unskinned parts and on the properties the skin leaves alone.

For example, the Builder + Themes sample's "Ember" choice is dark mode plus the Ember palette, and you can put the
Synty or Kenney skin on top of either mode. Skins are covered in [Skins](skins.md).

## The theme style sheet and panel settings

When its `UIDocument` has no panel settings, `ScreenHost` creates them with `CreateDefaultPanelSettings()`: the Weave
UI theme style sheet, Scale With Screen Size at a 1920 x 1080 reference resolution, matching width and height equally.
The theme comes from `ScreenHost.LoadTheme()`:

- `Resources/WeaveUI/WeaveProjectTheme.tss` if the [Tailwind pipeline](tailwind-pipeline.md) has written one;
- otherwise the package's `Resources/WeaveUI/WeaveDefaultTheme.tss`.

The default theme imports, in order:

```css
@import url("unity-theme://default");
@import url("../../Generated/tokens.uss");
@import url("../../Controls/controls.uss");
@import url("../../Controls/transitions.uss");
@import url("../../Generated/utilities.uss");
```

Utilities come last so they win over the control defaults. If you assign your own panel settings, set their Theme
Style Sheet to one of these themes (or a theme that imports the same files), otherwise the controls and utilities have
no styles. To build panel settings in code:

```csharp
var document = gameObject.AddComponent<UIDocument>();
document.panelSettings = ScreenHost.CreateDefaultPanelSettings();   // a new instance on every call
```

### Theming without a ScreenHost

`ThemeService` works on any element. Outside a `ScreenHost`, create one for your root and give the root the
`weave-root` class so palettes written against `.weave-root` match:

```csharp
var root = document.rootVisualElement;
root.AddToClassList("weave-root");
var theme = new ThemeService(root, ThemeMode.Dark);
theme.Mode = ThemeMode.Light;
```

## Reference

### ThemeService

| Member | Description |
|---|---|
| `ThemeService(VisualElement root, ThemeMode mode = ThemeMode.Dark)` | Creates a service for `root` and applies the mode class straight away. Throws `ArgumentNullException` if `root` is null. |
| `VisualElement Root` | The element the classes and stylesheets go on. For a `ScreenHost`, its `Container`. |
| `ThemeMode Mode` | `Dark` or `Light`. Setting it swaps the `theme-dark` and `theme-light` classes. |
| `bool IsDark` | True when `Mode` is `Dark`. |
| `void ToggleMode()` | Switches between dark and light. |
| `StyleSheet Palette` | An extra stylesheet of variable overrides, or null. Setting it removes the previous one. |
| `UISkin Skin` | The active skin, or null for utility styling. Setting it removes the previous skin's stylesheet and class, then adds the new one's. Logs a warning if the skin has no generated stylesheet. |
| `void RefreshSkin()` | Re-applies the current skin's stylesheet, for example after regenerating it in Play mode. |
| `event Action Changed` | Raised after `Mode` or `Palette` changes, and after `Skin` is set. |
| `const string DarkClass` | `"theme-dark"`. |
| `const string LightClass` | `"theme-light"`. |

### ThemeMode

| Value | Description |
|---|---|
| `Dark` | The default. Adds `theme-dark`; `dark:` utilities apply. |
| `Light` | Adds `theme-light`; the controls switch to their light semantic values. |

### ScreenHost theming members

| Member | Description |
|---|---|
| `ThemeService Theme` | The host's theme service, created in `Awake`. |
| `UISkin Skin` | Shortcut for `Theme.Skin` (read-only). |
| `void SetSkin(UISkin skin)` | Shortcut for setting `Theme.Skin`. |
| `void RefreshSkin()` | Shortcut for `Theme.RefreshSkin()`. |
| `VisualElement Container` | The screen container that carries the theme, skin and breakpoint classes. |
| `static ThemeStyleSheet LoadTheme()` | The project theme if the Tailwind pipeline wrote one, otherwise the default theme. |
| `static PanelSettings CreateDefaultPanelSettings()` | New panel settings using `LoadTheme()`, scaled to 1920 x 1080. |
| `const string DefaultThemePath` | `"WeaveUI/WeaveDefaultTheme"` (a `Resources` path). |
| `const string ProjectThemePath` | `"WeaveUI/WeaveProjectTheme"` (a `Resources` path). |

## Checking a theme in the editor

*Window > Weave UI > Preview* shows every control normal, disabled, checked and live (to hover, press and focus) without entering Play
mode. Its toolbar picks a
skin and light or dark mode. See [Editor tools](editor-tools.md).

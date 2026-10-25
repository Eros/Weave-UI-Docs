# Skins

A skin replaces the flat, utility-styled look of your controls with your own 9-slice art, without changing any markup.
This page covers the `UISkin` asset and its inspector, the stylesheet it generates, how to apply a skin to a whole UI
or a single control, and how to build a skin from a third-party art pack.

## How skins work

A `UISkin` lists components (a button, a panel, a slider handle) and, for each, the states it styles (normal, hover,
pressed, and so on). Each state can set a sprite, drawn 9-sliced, plus a tint, text colour, padding and font size.

Pressing **Generate USS** on the asset writes `skin-<name>.uss`. Every rule in it is scoped to a `.skin-<name>`
ancestor, so the skin only takes effect where that class is present. Applying a skin adds the stylesheet and the class
to the screen container; removing it takes them away again and the utility styling comes back. The same UXML works with
any skin or none.

```csharp
host.SetSkin(fantasySkin);   // every screen now draws the skin's sprites
host.SetSkin(null);          // back to utility styling
```

USS draws one background image per element, so each state of each component is one sprite.

## Creating a skin

1. Choose *Assets > Create > Weave UI > Skin*. The new asset is called `UISkin`.
2. Set **Skin Name** to a lower-case identifier such as `fantasy`. It becomes the class `skin-fantasy`.
3. Click **Add control** and pick a preset, such as *Button / Primary* or *Grid / Slot*. Each preset adds a component
   with the right selector and an empty entry for each state the control has.
4. Give each state a sprite and any other settings it needs. A state you leave empty keeps the normal state's sprite.
5. Click **Generate USS**.

Set each sprite's 9-slice border in the Sprite Editor before you use it; the skin uses the sprite's own border unless a
state overrides it.

### The skin inspector

The top of the inspector shows the skin's root class and where the stylesheet will be written, followed by any
validation warnings, then three buttons:

| Button | What it does |
|---|---|
| **Generate USS** | Writes `Generated/skin-<name>.uss` next to the asset, imports it and assigns it to the skin's **Style Sheet** field. In Play mode it also re-applies the skin on every `ScreenHost` that is using it, so you see the change at once. |
| **Add missing states** | Adds an empty entry for every `SkinStateKind` a component doesn't list yet. |
| **Add control** | A menu of presets (below). |

Below the buttons is the usual list of components and states.

### Validation

The inspector warns about:

- a skin name that isn't lower-case letters, digits and `-`;
- a component with no selector;
- a state listed more than once in a component (the first one is used);
- a sprite with no 9-slice border and no slice override, which will stretch.

### Presets

The presets use Unity's own part classes (`.unity-button`, `.unity-base-slider__dragger`) or Weave UI's component
classes (`.btn--primary`, `.grid-cell`), so they skin both `ui:` and `weave:` controls.

| Menu | Selector | States |
|---|---|---|
| Button / Any button | `.unity-button` | Normal, Hover, Active, Focus, Disabled |
| Button / Primary | `.btn--primary` | Normal, Hover, Active, Focus, Disabled |
| Button / Secondary | `.btn--secondary` | Normal, Hover, Active, Focus, Disabled |
| Button / Danger | `.btn--danger` | Normal, Hover, Active, Focus, Disabled |
| Button / Icon button | `.btn--icon` | Normal, Hover, Active, Focus, Disabled |
| Toggle / Box | `.unity-toggle{state} .unity-toggle__checkmark` | Normal, Hover, Focus, Checked, Disabled |
| Slider / Track | `.unity-base-slider{state} .unity-base-slider__tracker` | Normal, Disabled |
| Slider / Handle | `.unity-base-slider{state} .unity-base-slider__dragger` | Normal, Hover, Focus, Disabled |
| Dropdown / Field | `.unity-base-popup-field{state} .unity-base-popup-field__input` | Normal, Hover, Focus, Disabled |
| Dropdown / Open menu | `.unity-base-dropdown__container-inner` | Normal |
| Text field | `.unity-base-text-field{state} .unity-base-text-field__input` | Normal, Hover, Focus, Disabled |
| Progress bar / Background | `.unity-progress-bar__background` | Normal |
| Progress bar / Fill | `.unity-progress-bar__progress` | Normal |
| Tabs / Header | `.unity-tab__header` | Normal, Hover, Checked |
| List / Row | `.unity-collection-view__item` | Normal, Hover |
| List / Selected row | `.unity-collection-view__item--selected` | Normal |
| Grid / Slot | `.grid-cell` | Normal, Hover |
| Grid / Empty slot | `.grid-cell--empty` | Normal |
| Grid / Selected slot | `.grid-cell--selected` | Normal |
| Grid / Cursor (keyboard, gamepad) | `.weave-grid--focused .grid-cell--cursor` | Normal |
| Grid / Drop target | `.grid-cell--drop-target` | Normal |
| Grid / Invalid drop | `.grid-cell--invalid` | Normal |
| Grid / Item (multi-cell) | `.grid-item` | Normal, Hover |
| Panel | `.panel` | Normal |
| Modal / Dialog panel | `.modal__panel` | Normal |
| Modal / Backdrop | `.modal` | Normal |
| Tooltip | `.tooltip` | Normal |

Components are written in list order, and a later rule wins a tie, so put variants after their base: *Button / Primary*
after *Button / Any button*.

## The skin model

### Selectors and `{state}`

A component's selector is relative to the skin root. The state's pseudo-class is appended to it:
`.unity-button` becomes `.unity-button:hover` for the hover state. When the state belongs to an ancestor of the part
that carries the art, put `{state}` where the pseudo-class goes:

| Selector | Checked state becomes |
|---|---|
| `.unity-button` | `.unity-button:checked` |
| `.unity-toggle{state} .unity-toggle__checkmark` | `.unity-toggle:checked .unity-toggle__checkmark` |

The toggle's box is the checkmark element, but `:checked` is on the toggle, hence the `{state}` placeholder.

### States

| `SkinStateKind` | Pseudo-class | Typical use |
|---|---|---|
| `Normal` | none | The resting look. |
| `Hover` | `:hover` | Pointer over the element. |
| `Active` | `:active` | Pressed. |
| `Focus` | `:focus` | Keyboard or gamepad focus. |
| `Disabled` | `:disabled` | Disabled with `SetEnabled(false)`. |
| `Checked` | `:checked` | Toggles and tab headers that are on. |

### State settings

| Field | Description |
|---|---|
| `kind` | The `SkinStateKind` this entry styles. |
| `sprite` | Drawn as the background, 9-sliced. Uses the sprite's own Sprite Editor border unless `overrideSlice` is on. |
| `overrideSlice`, `slice` | A slice to use instead of the sprite's border: left, top, right, bottom in sprite pixels. |
| `sliceScale` | Scales the slice borders on screen. Default 1. Use less than 1 for high-resolution art, for example 0.25 for 1024 px sprites. |
| `useTint`, `tint` | Tints the sprite (`-unity-background-image-tint-color`). |
| `useTextColor`, `textColor` | Sets the text `color`. |
| `usePadding`, `padding` | Sets padding: left, top, right, bottom in px. |
| `useFontSize`, `fontSize` | Sets the font size in px. Default 16. |

Each `use…` box turns its setting on; a setting that is off isn't written, so the control keeps its normal style for
that property.

### Component settings

| Field | Description |
|---|---|
| `name` | A label for the component; it appears as a comment in the generated USS. |
| `selector` | The USS selector, relative to the skin root, with an optional `{state}`. |
| `resetUtilityStyling` | On by default. Clears background colour, border width and corner radius under the skin, so only the sprite shows, and clears the background colour in the Hover, Active, Focus, Disabled and Checked states so `hover:bg-*` and similar utilities don't show through. |
| `states` | The `SkinState` entries. |

## The generated stylesheet

The generator writes one rule per component state to `Generated/skin-<name>.uss`, next to the skin asset. This is part
of the stylesheet for Weave UI's Basic Menu sample skin, whose skin name is `placeholder`:

```css
/* Generated by Weave UI from skin 'PlaceholderSkin'. Do not edit: change the skin asset and regenerate. */

/* Button */
.skin-placeholder .unity-button, .skin-placeholder.unity-button { background-color: transparent; border-width: 0; border-radius: 0; background-image: url("../Sprites/button-normal.png#button-normal"); -unity-slice-left: 10; -unity-slice-top: 10; -unity-slice-right: 10; -unity-slice-bottom: 10; -unity-slice-scale: 1px; color: #f6ead0; }
.skin-placeholder .unity-button:hover, .skin-placeholder.unity-button:hover { background-color: transparent; background-image: url("../Sprites/button-hover.png#button-hover"); -unity-slice-left: 10; -unity-slice-top: 10; -unity-slice-right: 10; -unity-slice-bottom: 10; -unity-slice-scale: 1px; }

/* Toggle box */
.skin-placeholder .unity-toggle .unity-toggle__checkmark, .skin-placeholder.unity-toggle .unity-toggle__checkmark { background-color: transparent; border-width: 0; border-radius: 0; background-image: url("../Sprites/toggle-box.png#toggle-box"); -unity-slice-left: 10; -unity-slice-top: 10; -unity-slice-right: 10; -unity-slice-bottom: 10; -unity-slice-scale: 0.4px; }
```

Things to note:

- **Scoping.** Each rule has two selectors: `.skin-placeholder .unity-button` matches inside a skinned container, and
  `.skin-placeholder.unity-button` matches a control that carries the skin itself (see
  [One control](#one-control)).
- **Sprite URLs.** When the sprite and the stylesheet are under the same root (`Assets`, or the same package), the URL
  is relative to the stylesheet, so the skin keeps working when the folder is moved or copied. Otherwise the generator
  writes a `project://database/...` URL with the asset's GUID. The `#name` fragment makes USS load the Sprite, with its
  borders, rather than the texture.
- **Don't edit it.** Generating again overwrites the file. Change the asset, or tune it from your own stylesheet as
  described in [Tuning a skin for one screen](#tuning-a-skin-for-one-screen).

## Slice and slice scale

A 9-sliced sprite keeps its corners at their drawn size and stretches the edges and centre. Set the border in the
Sprite Editor (or turn on `overrideSlice`) so the corners and frame fall outside the stretched area.

`sliceScale` sets how big those corners are on screen. At 1, a 16 px corner is 16 px in the panel. Art drawn at high
resolution needs a smaller scale: a 1024 x 256 button with 500 px of ornament in its left slice is drawn with a scale of
0.2, which makes that ornament 100 px wide. Some guidelines:

- An element must be at least as wide as its left and right borders combined, times the scale, and as tall as its top
  and bottom borders times the scale, or the corners overlap.
- Art with a fixed aspect (a gem, a shield) looks squashed if the element is shorter than the sprite's height times
  the scale. Give the state enough padding to keep the element tall enough, or lower the scale.
- Use padding to keep text clear of ornaments in the slices.
- The same component can use a different scale per state, but keep it the same if the sprites are the same size, or
  the frame will jump on hover.

## Applying a skin

### Every screen

Set the skin on the host's `ThemeService`, or use the `ScreenHost` shortcut. Both add the generated stylesheet and the
`skin-<name>` class to the screen container and remove the class and stylesheet they applied for the previous skin.

```csharp
[SerializeField] UISkin _skin;

void Start()
{
    var host = GetComponent<ScreenHost>();
    host.SetSkin(_skin);            // same as host.Theme.Skin = _skin
}
```

A skin with no generated stylesheet logs a warning asking you to press **Generate USS**. Setting the skin to `null`
returns to utility styling. Skins combine freely with light and dark mode and palettes; see [Theming](theming.md).

### One control

Every `weave:` control has a `skin` attribute that skins only that control and its children, independently of the
screen-wide skin:

```xml
<weave:Button text="Forge" variant="primary" skin="fantasy" />
```

```csharp
Button("Forge").Variant("primary").Skin("fantasy");
```

The control looks the name up through `WeaveSkins`: a skin registered with `WeaveSkins.Register(skin)` (keyed by its
skin name), otherwise a `UISkin` asset in `Resources/WeaveUI/Skins` whose skin name matches, whatever its file is
called (a file named `<name>` is the last resort). A name that can't be found logs one warning.

```csharp
WeaveSkins.Register(_fantasySkin);   // before the screens that use it are shown
```

## Skins outrank utility classes

A skin rule has at least two classes (`.skin-x .unity-button`), and a state rule two classes and a pseudo-class. A
plain or state-variant utility has one class, or one class and a pseudo-class. So on the properties a skin sets, the
skin wins whatever order the stylesheets load in: a `bg-red-500` on a skinned button doesn't show through. Utilities for properties the
skin leaves alone, such as `w-96`, `mb-2` or `flex-1`, still apply.

## Tuning a skin for one screen

When one screen needs a different size or slice scale, add rules to that screen's stylesheet that are more specific
than the skin's. The Builder + Themes sample's character sheet has small `+` and `-` step buttons, 24 px tall, which are
much shorter than the Synty skin's buttons. Its
[CharacterSheet.uss](../Samples/BuilderThemes/Resources/WeaveUISamples/BuilderThemes/CharacterSheet.uss) shrinks them
only while that skin is on:

```css
.skin-synty-fantasy .sheet .stat__step,
.skin-synty-fantasy .sheet .stat__step:hover,
.skin-synty-fantasy .sheet .stat__step:focus,
.skin-synty-fantasy .sheet .stat__step:active {
    min-width: 60px;
    height: 24px;
    padding: 0 8px 0 32px;
    -unity-slice-scale: 0.09px;
}
```

Two details make this work:

- Scoping the rule with `.skin-synty-fantasy` means it does nothing under other skins or with no skin.
- The skin sets the slice scale in each state, and its `:hover` rule has the same specificity as a three-class rule, so
  the override lists every state the skin styles.

## Building a skin from an art pack

UI art packs often draw a control in layers: a plain fill that the pack's prefabs tint per state, with a separate frame
on top. USS gives each element one background image, and a skin never adds elements, so layered art has to be
**flattened** into one sprite per state: tint the fill, composite the frame over it, and keep the shared 9-slice
border. Doing it with an editor script means you can rebuild the skin at any time, and change colours in one place.

### Worked example: Synty INTERFACE - Fantasy Menus

The [Builder + Themes sample](../Samples/BuilderThemes) includes
[SyntySkinBuilder.cs](../Samples/BuilderThemes/Editor/SyntySkinBuilder.cs), which builds a skin from your own import of
Synty's [INTERFACE - Fantasy Menus](https://syntystore.com/products/interface-fantasy-menus) pack. The pack isn't
included: import it from the Synty Store or the Package Manager, then choose
*Tools > Weave UI Samples > Build Synty Fantasy Menus Skin*.

The script:

1. **Finds the source sprites** by name with `AssetDatabase.FindAssets`, wherever the pack was imported: a button fill
   (`SPR_FantasyMenus_Menu_Button_01_Background`) and frame (`..._Menu_Button_01_Frame`), and a panel fill
   (`..._Frame_Box_Medium_01_Mask`) and frame (`..._Frame_Box_Medium_01`). If they're missing, it says which pack to
   import.
2. **Flattens each state.** It reads the PNG files directly, so the pack's import settings don't matter, multiplies the
   fill by a colour for the state, and alpha-blends the frame over it:

   ```csharp
   var under = a[i] * tint;
   var over = b[i];
   var alpha = over.a + under.a * (1 - over.a);
   var rgb = alpha <= 0 ? Color.clear : (over * over.a + under * under.a * (1 - over.a)) / alpha;
   ```

   Each result is saved as a PNG and imported as a single sprite with the pack's 9-slice border and no mipmaps. Normal,
   hover and active buttons, primary buttons and the panel each get their own colour.
3. **Writes the skin asset** (`skinName = "synty-fantasy"`) with three components: `.panel`, `.unity-button` and
   `.btn--primary`. The button states use a slice scale of 0.2 and padding that keeps the text clear of the gem in the
   left slice; the normal state also sets a cream text colour.
4. **Generates the stylesheet** with `SkinUssGenerator.Generate(skin)`.

The sprites and skin go in a `WeaveSkin` folder next to the pack, with the asset at
`Resources/WeaveUISynty/SyntyFantasySkin`, where `BuilderThemesSample` looks for it. Press Play and pick Synty from the
sample's Skin dropdown.

To adapt the script to another pack, change the sprite names, the state colours and the borders. Note that
`TextureImporter.spriteBorder` is in the order left, bottom, right, top, while the skin's `slice` and `padding` fields
are left, top, right, bottom.

### Licensing: ship the script, not the art

Vendor art licences usually let you use the art in your game but forbid redistributing the source files. The
flattened sprites are made from that art, so treat them the same way. To share a skin built from a paid pack (in a
public repository, a sample or an Asset Store package), ship the builder script and let each user run it against
their own copy, as the Builder + Themes sample does. Keep the generated sprites, skin asset and stylesheet out of
anything you publish.

### Free option: Kenney UI Pack (RPG Expansion)

[Kenney](https://kenney.nl)'s UI Pack (RPG Expansion) is CC0, so you can use, modify and redistribute it freely. Weave
UI's Basic Menu sample, imported from the Package Manager, ships a skin built from it (`skin-placeholder`), sprites
included. It covers panels, buttons, toggles, sliders, text and dropdown fields, progress bars, tabs, dialogs,
tooltips and grid slots, and its *Kenney skin* toggle switches the whole menu between utility styling and the
skin. The Builder + Themes sample lists the same skin as Kenney when the Basic Menu sample is in your project. It is a
good starting point for your own skin, or a stand-in until your final art arrives.

## Checking a skin

*Window > Weave UI > Preview* shows every control in every state with any skin in the project, in light or dark mode,
without entering Play mode. See [Editor tools](editor-tools.md).

## Reference

### UISkin

`Nerdova.WeaveUI.UISkin`, a `ScriptableObject` created from *Assets > Create > Weave UI > Skin*.

| Member | Description |
|---|---|
| `string skinName` | Lower-case identifier. Default `"custom"`. |
| `List<SkinComponent> components` | The skinned parts, written in order. |
| `StyleSheet styleSheet` | The generated stylesheet, assigned by **Generate USS**. |
| `string ClassName` | `skin-<skinName>`, the class that switches the skin on. |

### SkinComponent

| Member | Description |
|---|---|
| `string name` | Label, written as a comment in the USS. Default `"Button"`. |
| `string selector` | Selector relative to the skin root, with optional `{state}`. Default `".unity-button"`. |
| `bool resetUtilityStyling` | Clear utility backgrounds, borders and radii under the skin. Default `true`. |
| `List<SkinState> states` | The styled states. A new component has one `Normal` state. |
| `SkinState Get(SkinStateKind kind)` | The first state of that kind, or null. |

### SkinState

| Member | Description |
|---|---|
| `SkinState()`, `SkinState(SkinStateKind kind)` | Creates an empty state (`Normal` by default). |
| `SkinStateKind kind` | The state this entry styles. |
| `Sprite sprite` | Background sprite, drawn 9-sliced. |
| `bool overrideSlice`, `Vector4 slice` | Slice override: left, top, right, bottom in sprite pixels. |
| `float sliceScale` | Scale of the slice borders on screen. Default `1`. |
| `bool useTint`, `Color tint` | Sprite tint. |
| `bool useTextColor`, `Color textColor` | Text colour. |
| `bool usePadding`, `Vector4 padding` | Padding: left, top, right, bottom in px. |
| `bool useFontSize`, `float fontSize` | Font size in px. Default `16`. |

### WeaveSkins

| Member | Description |
|---|---|
| `static void Register(UISkin skin)` | Makes a skin available to `skin="name"` under its `skinName`. |
| `static UISkin Get(string name)` | A registered skin, else a skin in `Resources/WeaveUI/Skins` with that skin name (or file name), else null. |

### SkinUssGenerator (editor)

`Nerdova.WeaveUI.Editor.SkinUssGenerator`, for build scripts that create skins in code.

| Member | Description |
|---|---|
| `static string Generate(UISkin skin)` | Writes and imports the stylesheet, assigns it to `skin.styleSheet`, refreshes hosts using the skin in Play mode, and returns the path. The file is only rewritten when its contents change. |
| `static string OutputPath(UISkin skin)` | `<asset folder>/Generated/skin-<name>.uss`. |
| `static string Export(UISkin skin, string ussPath)` | The USS text, without writing it. `ussPath` is where it will live, for relative sprite URLs. |
| `static IEnumerable<string> Validate(UISkin skin)` | The warnings the inspector shows. |

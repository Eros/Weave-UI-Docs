# Controls

Weave UI's controls are thin wrappers over Unity's own controls that add BEM classes, variant and size modifiers,
per-control skins and UXML handler names. This page covers each control's UXML attributes, the classes it carries,
how to create it in C#, and runtime tooltips.

## Using the controls

Declare the `weave` namespace in UXML and use the controls next to Unity's:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:weave="Nerdova.WeaveUI">
    <ui:VisualElement class="panel w-96">
        <weave:Slider name="volume" label="Volume" low-value="0" high-value="100" change="OnVolume" />
        <weave:Toggle name="music" label="Music" />
        <weave:Button name="play" text="Play" variant="primary" size="lg" click="Play" />
    </ui:VisualElement>
</ui:UXML>
```

Each control derives from the Unity control it wraps, so it keeps that control's UXML attributes, data binding, focus
and keyboard or gamepad navigation. The default look comes from `controls.uss` and reads the
[semantic variables](styling.md#semantic-variables), so palettes and light mode restyle every control at once.

| UXML element | C# type | Unity base | Block class | Handler attribute |
|---|---|---|---|---|
| `weave:Button` | `WeaveButton` | `Button` | `btn` | `click` |
| `weave:IconButton` | `WeaveIconButton` | `WeaveButton` | `btn` (+ `btn--icon`) | `click` |
| `weave:Toggle` | `WeaveToggle` | `Toggle` | `toggle` | `change` |
| `weave:Slider` | `WeaveSlider` | `Slider` | `slider` | `change` |
| `weave:Dropdown` | `WeaveDropdown` | `DropdownField` | `dropdown` | `change` |
| `weave:TextField` | `WeaveTextField` | `TextField` | `textfield` | `change` |
| `weave:ProgressBar` | `WeaveProgressBar` | `ProgressBar` | `progress` | None |
| `weave:ListView` | `WeaveListView` | `ListView` | `list` | None |
| `weave:TabView` | `WeaveTabView` | `TabView` | `tabs` | None |
| `weave:Grid` | `WeaveGrid` | `VisualElement` | `weave-grid` | `click`, `drop` |

`weave:Grid` is covered in [Grid](grid.md).

## Common attributes

Every control takes these attributes:

| Attribute | Values | Description |
|---|---|---|
| `variant` | `primary`, `secondary`, `ghost`, `danger`, or your own | Adds the modifier class `<block>--<variant>`, such as `btn--primary`. Trimmed and lower-cased; empty removes it |
| `size` | `sm`, `md`, `lg`, or your own | Adds `<block>--<size>`, such as `btn--sm`. `md` is the default and adds no class |
| `skin` | A skin name | Skins this control and its children with the `UISkin` of that name, independent of the screen-wide skin |

Any value adds its class, so you can add your own variants and sizes in USS:

```css
.btn--gold { background-color: var(--color-amber-500); color: var(--color-stone-950); }
.btn--gold:hover { background-color: var(--color-amber-400); }
```

```xml
<weave:Button text="Buy" variant="gold" />
```

The built-in styles cover these combinations:

| Control | Styled variants | Styled sizes |
|---|---|---|
| Button, IconButton | `primary`, `secondary`, `ghost`, `danger` | `sm`, `lg` |
| ProgressBar | `danger` | `sm`, `lg` |
| Other controls | None (the classes are added for your USS and skins) | None |

### The skin attribute

`skin="name"` looks the skin up with `WeaveSkins.Get`: first the skins registered with `WeaveSkins.Register`, then a
`UISkin` asset in `Resources/WeaveUI/Skins` with that skin name (or, failing that, that file name). When the control is attached to a panel, it gets the skin's class
(`skin-<name>`) and stylesheet. An unknown name logs one warning and leaves the control unskinned. See [Skins](skins.md).

```csharp
WeaveSkins.Register(fantasySkin);   // skinName "fantasy"
```

```xml
<weave:Button text="Enter the dungeon" variant="primary" skin="fantasy" />
```

| Member | Description |
|---|---|
| `WeaveSkins.Register(UISkin skin)` | Makes a skin available to `skin="..."` under its `skinName` |
| `WeaveSkins.Get(string name)` | The registered skin, else a skin in `Resources/WeaveUI/Skins` with that skin name (or file name), else null |

### Handler attributes

`click="Method"` on buttons and `change="Method"` on fields name a method to call. When a screen builds its layout, it
looks for the method on its view model first, then on the screen itself, and binds it with the same parameter
injection as `[OnClick]` and `[OnValueChanged]`:

```xml
<weave:Dropdown name="quality" label="Quality" choices="Low,Medium,High,Ultra" change="QualityChanged" />
<weave:Button name="apply" text="Apply" variant="primary" click="Apply" />
```

```csharp
// On the view model: the new value and, with [Previous], the old one are injected.
[Preserve]
public void QualityChanged(string quality, [Previous] string was) => Status = $"Quality: {was} → {quality}";
```

- **Missing methods fail early.** If no method has that name, building the screen throws an
  `InteractionBindingException` that names the attribute, the control and the classes searched.
- **Mark the methods `[Preserve]`.** Only the UXML refers to them, so managed code stripping would otherwise remove
  them from player builds.
- **`change` fires when the player (or your code) changes the value,** not when a data binding writes the model's
  value into the control, such as its first write when the screen appears.
- **Handler names are read when the screen builds its layout.** Controls created later, such as list rows or elements
  added in code, need C# handlers (`OnClick`, `OnValueChanged`) instead.

See [Interaction](interaction.md) for the values a handler can ask for.

### C# usage

Create controls with `new` and set the same properties, or use the [builder](builder.md) factories:

```csharp
using static Nerdova.WeaveUI.Ui;

var play = new WeaveButton("Play") { name = "play", Variant = "primary", Size = "lg" };
var apply = Button("Apply").Variant("primary").Size("sm").OnClick(() => viewModel.Apply());
var volume = Slider("Volume", 0, 100, 80).OnValueChanged((float value) => viewModel.Volume = value);
```

Every control implements `IWeaveControl`:

| Member | Description |
|---|---|
| `Block` | The BEM block class: `btn`, `toggle`, ... |
| `Variant` | The variant modifier, or null |
| `Size` | The size modifier, or null for the default |
| `Skin` | The per-control skin name, or null |
| `Handlers` | The UXML handler names as `(InteractionKind kind, string method)` pairs |

### Making your own control

`ControlState` holds the variant, size and skin logic, so your own controls can behave like Weave's:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;

[UxmlElement]
public partial class Badge : Label, IWeaveControl
{
    readonly ControlState _state;

    public Badge() => _state = new ControlState(this, Block);

    public string Block => "badge";

    [UxmlAttribute] public string Variant { get => _state.Variant; set => _state.Variant = value; }
    [UxmlAttribute] public string Size { get => _state.Size; set => _state.Size = value; }
    [UxmlAttribute] public string Skin { get => _state.Skin; set => _state.Skin = value; }

    public IEnumerable<(InteractionKind kind, string method)> Handlers => Array.Empty<(InteractionKind, string)>();
}
```

`new ControlState(element, block)` adds the block class at once; its `Variant`, `Size` and `Skin` properties swap the
modifier classes and the skin as they change.

## Button

A text button. `click` names the handler.

```xml
<weave:Button text="Default" />
<weave:Button text="Play" variant="primary" size="lg" click="Play" />
<weave:Button text="Restore defaults" variant="ghost" click="AskDefaults" class="mr-2" />
<weave:Button text="Delete save" variant="danger" tooltip="This can't be undone" />
```

| Attribute | Values | Description |
|---|---|---|
| `text` | Text | The label (Unity's `Button`) |
| `variant` | `primary`, `secondary`, `ghost`, `danger`, or your own | Colour scheme. None is the neutral control colour |
| `size` | `sm`, `md`, `lg` | Padding and font size: `sm` 4px 10px at `--text-sm`, `md` 8px 16px at `--text-base`, `lg` 12px 24px at `--text-xl` |
| `click` | Method name | Called on click, with parameter injection |
| `skin` | Skin name | Skins this button |
| `tooltip` | Text | Shown on hover at runtime (see [Tooltips](#tooltips)) |
| `enabled` | `true`, `false` | Unity's attribute; disabled buttons fade to 50% opacity |

| Class | Style |
|---|---|
| `btn` | `--weave-control` background, `--weave-text`, 2px transparent border, `--weave-radius` corners, colour transitions over 0.12s. Hover and active use `--weave-control-hover` and `--weave-control-active`; focus shows a `--weave-focus` border |
| `btn--primary` | `--weave-primary` background, `--weave-on-primary` bold text; `-hover` and `-active` shades |
| `btn--secondary` | Transparent with a `--weave-border` border; raised surface on hover |
| `btn--ghost` | Transparent with `--weave-text-muted` text; raised surface and `--weave-text` on hover |
| `btn--danger` | `--weave-danger` background, `--weave-on-primary` text; `-hover` and `-active` shades |
| `btn--sm`, `btn--lg` | Smaller or larger padding and text |

| C# | Description |
|---|---|
| `new WeaveButton()`, `new WeaveButton(string text)` | Creates a button |
| `ClickHandler` | The `click` attribute's method name |
| `Ui.Button(string text, string name = null)` | Builder factory; the name defaults to a slug of the text (`"Load game"` → `load-game`) |

## IconButton

An icon-only button. The icon is a child element (class `btn__icon`, name `icon`) whose background image is tinted with
the button's text colour, so it follows variants, palettes and light mode. Pair it with a `tooltip`.

```xml
<weave:IconButton name="settings" variant="ghost" tooltip="Settings" click="OpenSettings" />
```

```csharp
Root.Q<WeaveIconButton>("settings").IconSprite = Resources.Load<Sprite>("UI/Icons/gear");
```

| Attribute | Values | Description |
|---|---|---|
| `icon` | `Texture2D` asset | The icon image. Pick it in UI Builder, which writes the asset reference |
| `icon-sprite` | `Sprite` asset | The icon as a sprite (also an asset reference) |
| `variant`, `size`, `skin`, `click`, `tooltip` | As for `Button` | |

| Class | Style |
|---|---|
| `btn btn--icon` | Square padding: 8px, 4px with `btn--sm`, 12px with `btn--lg` |
| `btn__icon` | 20x20 (16 with `btn--sm`, 28 with `btn--lg`), scaled to fit, tinted `--weave-text`; `--weave-on-primary` inside `btn--primary` and `btn--danger` |

| C# | Description |
|---|---|
| `Icon` | The icon as a `Texture2D` |
| `IconSprite` | The icon as a `Sprite` |
| `Ui.IconButton(Sprite icon, string tooltip = null, string name = null)` | Builder factory |

`WeaveIconButton` derives from `WeaveButton`, so it has all of `Button`'s attributes and classes.

## Toggle

A checkbox. Unity draws the tick; Weave styles the box.

```xml
<weave:Toggle name="vsync" label="Vertical sync">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="VSync" binding-mode="TwoWay" />
    </Bindings>
</weave:Toggle>
```

| Attribute | Values | Description |
|---|---|---|
| `label` | Text | The field label (Unity's `Toggle`) |
| `value` | `true`, `false` | Checked state |
| `text` | Text | Text next to the box (Unity's `Toggle`) |
| `change` | Method name | Called with the new `bool` value |
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

| Class or part | Style |
|---|---|
| `toggle` | 4px vertical margin; 50% opacity when disabled |
| `.toggle > .unity-base-field__label` | `--weave-text`, `--text-base` |
| `.toggle .unity-toggle__checkmark` | The box: 22x22, 2px `--weave-border` border, `--radius-md`, `--weave-surface-raised`; tick tinted `--weave-on-primary` |
| `:checked` | Box filled with `--weave-primary` |
| `:hover`, `:focus` | Box border `--weave-control-hover`, `--weave-focus` |

C#: `Ui.Toggle(string label, bool value = false)`; `ChangeHandler` holds the `change` method name.

## Slider

A horizontal slider with a filled handle.

```xml
<weave:Slider name="master" label="Master volume" low-value="0" high-value="100" change="OnVolume" />
```

| Attribute | Values | Description |
|---|---|---|
| `label` | Text | The field label |
| `low-value`, `high-value` | Numbers | The range (Unity's `Slider`) |
| `value` | Number | The current value |
| `change` | Method name | Called with the new `float` value |
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

Unity's other `Slider` attributes, such as `page-size`, `show-input-field`, `direction` and `inverted`, still apply.

| Class or part | Style |
|---|---|
| `slider` | 4px vertical margin; 50% opacity when disabled |
| `.slider .unity-base-slider__tracker` | 6px track, `--weave-control`, rounded |
| `.slider .unity-base-slider__dragger` | 18px round handle, `--weave-primary` with a `--weave-surface` ring; `--weave-primary-hover` on hover, `--weave-focus` ring on focus |

C#: `Ui.Slider(string label, float low = 0, float high = 1, float value = 0)`; `ChangeHandler`.

## Dropdown

A choice from a list.

```xml
<weave:Dropdown name="quality" label="Quality" choices="Low,Medium,High,Ultra" change="QualityChanged">
    <Bindings>
        <ui:DataBinding property="index" data-source-path="Quality" binding-mode="TwoWay" />
    </Bindings>
</weave:Dropdown>
```

| Attribute | Values | Description |
|---|---|---|
| `label` | Text | The field label |
| `choices` | Comma-separated text | The options (Unity's `DropdownField`) |
| `index` | Integer | The selected option |
| `value` | Text | The selected option's text |
| `change` | Method name | Called with the new `string` value |
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

| Class or part | Style |
|---|---|
| `dropdown` | 4px vertical margin; 50% opacity when disabled |
| `.dropdown .unity-base-popup-field__input` | 2px `--weave-border` border, `--radius-md`, `--weave-surface-raised`; border `--weave-control-hover` on hover, `--weave-focus` on focus |
| `.dropdown .unity-base-popup-field__arrow` | Tinted `--weave-text-muted` |
| `.unity-base-dropdown__container-inner`, `__label`, `__item`, `__checkmark` | The open menu |

The open menu belongs to the panel root rather than the dropdown, so its styles in `controls.uss` are global: they
apply to every dropdown menu on the panel, including plain `ui:DropdownField`s.

C#: `Ui.Dropdown(string label, IEnumerable<string> choices, int index = 0)`; `ChangeHandler`.

## TextField

A single-line text input.

```xml
<weave:TextField name="player" label="Player name" change="NameChanged">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="PlayerName" binding-mode="TwoWay" />
    </Bindings>
</weave:TextField>
```

| Attribute | Values | Description |
|---|---|---|
| `label` | Text | The field label |
| `value` | Text | The text |
| `change` | Method name | Called with the new `string` value, on every edit |
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

Unity's `TextField` attributes, such as `max-length`, `multiline` and `password`, still apply.

| Class or part | Style |
|---|---|
| `textfield` | 4px vertical margin; 50% opacity when disabled |
| `.textfield .unity-base-text-field__input` | 6px 10px padding, 2px `--weave-border` border, `--radius-md`, `--weave-surface-raised`, `--weave-text`; border `--weave-control-hover` on hover, `--weave-focus` on focus |

C#: `Ui.TextField(string label, string value = "")`; `ChangeHandler`.

## ProgressBar

A read-only bar for health, loading or experience.

```xml
<weave:ProgressBar name="xp" class="mt-4">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="Xp" binding-mode="ToTarget" />
        <ui:DataBinding property="title" data-source-path="XpTitle" binding-mode="ToTarget" />
    </Bindings>
</weave:ProgressBar>
```

| Attribute | Values | Description |
|---|---|---|
| `title` | Text | Text drawn over the bar (Unity's `ProgressBar`) |
| `value` | Number | Progress between `low-value` and `high-value` (0 to 100 by default) |
| `variant` | `danger`, or your own | `danger` fills with `--weave-danger` |
| `size` | `sm`, `md`, `lg` | Bar height: 6px, 14px, 24px |
| `skin` | Skin name | Skins this bar |

| Class or part | Style |
|---|---|
| `progress` | 4px vertical margin |
| `.progress .unity-progress-bar__background` | The track: 14px minimum height, rounded, `--weave-control` |
| `.progress .unity-progress-bar__progress` | The fill: `--weave-primary` (`--weave-danger` with `progress--danger`) |
| `.progress .unity-progress-bar__title` | `--weave-text`, `--text-sm` |

C#: `Ui.ProgressBar(string title = null, float value = 0)`. The builder's `Bind(path)` binds `value`:

```csharp
ProgressBar("Poison 20%", 20).Variant("danger")
```

## ListView

A virtualised list whose rows get their data source from `itemsSource` automatically, so `DataBinding`s in an item
template bind to each item. Bind `itemsSource` to a list on the view model:

```xml
<weave:ListView name="saves" fixed-item-height="44" item-template="project://database/Assets/UI/SaveSlot.uxml" class="flex-1">
    <Bindings>
        <ui:DataBinding property="itemsSource" data-source-path="Saves" binding-mode="ToTarget" />
    </Bindings>
</weave:ListView>
```

| Attribute | Values | Description |
|---|---|---|
| `item-template` | `VisualTreeAsset` | The row template; its bindings resolve against the row's item |
| `fixed-item-height` | Number | Row height (Unity's `ListView`) |
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

Unity's other `ListView` attributes, such as `selection-type` and `show-alternating-row-backgrounds`, still apply.

| Class or part | Style |
|---|---|
| `list` | 1px `--weave-border` border, `--weave-radius`, `--weave-surface` background |
| `.list .unity-collection-view__item` | `--weave-text`, 8px side padding; `--weave-surface-raised` on hover |
| `.list .unity-collection-view__item--selected` | `--weave-selection` background |

Rows are created after the screen added its [class aliases](styling.md#class-aliases), so a row template that uses
`dark:` classes needs `WeaveClassAliases.Apply` in a custom `makeItem`:

```csharp
var template = Resources.Load<VisualTreeAsset>("UI/SaveSlot");
Root.Q<WeaveListView>("saves").makeItem = () =>
{
    var row = template.Instantiate();
    WeaveClassAliases.Apply(row);
    return row;
};
```

`WeaveListView` sets `bindingSourceSelectionMode` to `AutoAssign`; apart from the classes and modifiers above, that is
the only behaviour it adds to `ListView`.

## TabView

Unity's `TabView` with Weave styling. Put Unity's `ui:Tab` elements inside it:

```xml
<weave:TabView name="tabs" class="settings__tabs">
    <ui:Tab label="Audio">
        <ui:ScrollView class="flex-1">...</ui:ScrollView>
    </ui:Tab>
    <ui:Tab label="Video">
        <ui:ScrollView class="flex-1">...</ui:ScrollView>
    </ui:Tab>
</weave:TabView>
```

| Attribute | Values | Description |
|---|---|---|
| `variant`, `size`, `skin` | | See [Common attributes](#common-attributes) |

Unity's `TabView` and `Tab` attributes, such as the tab's `label`, still apply.

| Class or part | Style |
|---|---|
| `tabs` | The block |
| `.tabs .unity-tab-view__header-container` | 1px `--weave-border` bottom border |
| `.tabs .unity-tab__header`, `__header-label` | 8px 16px padding, transparent, `--weave-text-muted` at `--text-base`; `--weave-text` on hover |
| `.tabs .unity-tab__header:checked` | Bold `--weave-text` label and a 2px `--weave-primary` underline (`.unity-tab__header-underline`) |
| `.tabs .unity-tab__content-container` | 12px top padding |

The [Settings menu sample](../Samples/SettingsMenu) uses a `TabView` with four tabs.

## Grid

`weave:Grid` (`WeaveGrid`) is the virtualised slot grid for inventories, hotbars and shops, with drag and drop. It takes
`variant`, `size` and `skin` like the other controls, plus `click` and `drop` handler names. See [Grid](grid.md).

## Tooltips

UI Toolkit's standard `tooltip` attribute only shows in the editor. Weave UI's `TooltipController` shows it at runtime:
hovering any element with a `tooltip` shows a label after a short delay.

```xml
<weave:IconButton name="close" variant="ghost" tooltip="Back to the menu (Esc)" click="Back" />
<ui:VisualElement class="stock-card" tooltip="Restores 50 health over 5 seconds" />
```

```csharp
Toggle("Auto-loot").Tooltip("Pick up gold and crafting materials on the way past");
```

The `tooltip` property can be data-bound, as the [Shop + inventory sample](../Samples/ShopInventory) does for its stock
cards:

```xml
<ui:VisualElement class="stock-card">
    <Bindings>
        <ui:DataBinding property="tooltip" data-source-path="Description" binding-mode="ToTarget" />
    </Bindings>
</ui:VisualElement>
```

`ScreenHost` creates a controller for its screens (`host.Tooltips`) and puts the label in its overlay layer, above
every screen. How it behaves:

- **Target:** the nearest element under the pointer, or an ancestor within the controller's scope, with a non-empty
  `tooltip`. Moving onto a child keeps the same tooltip.
- **Delay:** 500 ms by default (`DelayMilliseconds`).
- **Placement:** centred below the element with an 8-unit gap (`Offset`), or above it when there's no room below, and
  kept inside the layer.
- **Hiding:** when the pointer leaves the element, and on any pointer press, submit or key press, since these may open
  a dialog or change the screen.
- **Keyboard focus** doesn't show tooltips. For a focus-driven UI, call `ShowFor` yourself:

```csharp
host.Container.RegisterCallback<FocusInEvent>(evt =>
{
    if (evt.target is VisualElement element && !string.IsNullOrEmpty(element.tooltip))
        host.Tooltips.ShowFor(element);
});
```

The label has class `tooltip` and name `weave-tooltip`. Its default style: 4px 8px padding, 1px `--weave-border` border,
`--radius-md`, `--weave-tooltip` background, `--weave-tooltip-text` at `--text-sm`, wrapping at 320 units. Restyle it
with those variables, your own `.tooltip` rule, or a skin's *Tooltip* part.

Outside a `ScreenHost`, create a controller yourself. The layer should cover the scope and ignore the pointer:

```csharp
var layer = new VisualElement { pickingMode = PickingMode.Ignore };
layer.style.position = Position.Absolute;
layer.style.left = 0;
layer.style.top = 0;
layer.style.right = 0;
layer.style.bottom = 0;
root.Add(layer);

var tooltips = new TooltipController(root, layer) { DelayMilliseconds = 300 };
// Dispose() when the UI goes away.
```

| Member | Description |
|---|---|
| `TooltipController(VisualElement scope, VisualElement layer)` | Shows tooltips for elements under `scope`, in `layer` |
| `Label` | The tooltip `Label` (class `tooltip`) |
| `DelayMilliseconds` | Hover time before the tooltip appears. Default 500 |
| `Offset` | Gap between the element and the tooltip, in panel units. Default 8 |
| `Showing` | The element whose tooltip is showing, or null |
| `ShowFor(VisualElement element)` | Shows `element`'s tooltip now, without the delay |
| `Hide()` | Hides the tooltip |
| `Dispose()` | Hides the tooltip, stops listening and removes the label |

## Other styled classes

`controls.uss` also styles these classes, which you can use on plain elements:

| Class | Use |
|---|---|
| `panel` | A raised card or dialog surface (the builder's `Panel()` adds it) |
| `title` | A heading (the builder's `Title()` adds it) |
| `modal`, `modal__panel`, `modal__title`, `modal__body`, `modal__actions` | Modal screens and their dialog parts; see [Screens](screens.md) |
| `grid-cell`, `grid-item`, `grid-ghost` and their modifiers | Grid slots, placed items and the drag preview; see [Grid](grid.md) |

For utility classes, variants and the semantic variables these styles read, see [Styling](styling.md).

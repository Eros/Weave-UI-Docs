# Data binding

Weave UI uses UI Toolkit's runtime data binding. A screen's view model is the data source for its layout, so
`DataBinding`s in UXML and `.Bind()` calls in C# resolve against it with no wiring. This page covers the `ViewModel`
base class, binding from UXML and C#, data sources for list rows and grid cells, binding styles and enabled state,
two-way binding of controls, and the mistakes that most often stop a binding updating.

## View models

A view model holds the state a screen shows. Derive from `ViewModel`, mark each property you bind with
`[CreateProperty]` (namespace `Unity.Properties`) and set backing fields through `SetProperty`:

```csharp
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;

public sealed class HudViewModel : ViewModel
{
    const float MaxHealth = 120;

    float _health = 104;

    [CreateProperty]
    public string PlayerName => "Ada";

    [CreateProperty]
    public float Health
    {
        get => _health;
        set
        {
            if (!SetProperty(ref _health, Mathf.Clamp(value, 0, MaxHealth)))
                return;

            Notify(nameof(HealthFill));
            Notify(nameof(HealthText));
        }
    }

    [CreateProperty]
    public StyleLength HealthFill => Length.Percent(100 * _health / MaxHealth);

    [CreateProperty]
    public string HealthText => $"{_health:0} / {MaxHealth:0}";
}
```

`ViewModel` implements UI Toolkit's `INotifyBindablePropertyChanged` and `IDataSourceViewHashProvider`, so UI
Toolkit only updates bindings whose source actually changed.

| Member | Description |
|---|---|
| `protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)` | Compares with `EqualityComparer<T>.Default`. If the value differs, sets the field, notifies `propertyName` (the calling property by default) and returns `true`. Returns `false` and does nothing if the value is unchanged. |
| `protected void Notify([CallerMemberName] string propertyName = null)` | Notifies bindings that a property changed, without setting anything. Use it for computed properties and for values changed outside a setter. |
| `event EventHandler<BindablePropertyChangedEventArgs> propertyChanged` | Raised by every `SetProperty` change and every `Notify`. Bindings listen to it; your own code can too. |
| `long GetViewHashCode()` | A version number that goes up with every notification. UI Toolkit compares it to decide whether to update bindings. |

Properties you don't bind don't need `[CreateProperty]`. The samples keep game-logic values such as a cooldown timer
as plain properties and expose only the display values to bindings.

### Computed properties

A get-only property derived from other state (`HealthFill`, `HealthText` above) has no setter to notify for it.
Notify it yourself whenever one of its inputs changes. The usual pattern is to notify dependants only when
`SetProperty` reports a change:

```csharp
[CreateProperty]
public int Count
{
    get => _count;
    set
    {
        if (SetProperty(ref _count, value))
            Notify(nameof(CountText));
    }
}

[CreateProperty]
public string CountText => _count > 0 ? _count.ToString() : "";
```

Always pass `nameof(...)`: the name must match the property exactly, and a typo means the binding never updates.

When many values change at once (loading, Apply, Revert), notify each affected property. The
[Settings Menu sample](../Samples/SettingsMenu) keeps a list of its property names and notifies all of them after
Revert. It also routes every setter through one helper that notifies the property, its dependant and the footer's
`HasChanges` and `Status`:

```csharp
void Set<T>(ref T field, T value, string dependant = null, [CallerMemberName] string property = null)
{
    if (EqualityComparer<T>.Default.Equals(field, value))
        return;

    field = value;
    Notify(property);
    if (dependant != null)
        Notify(dependant);
    Notify(nameof(HasChanges));
    Notify(nameof(Status));
}

[CreateProperty] public float Master { get => _edit.master; set => Set(ref _edit.master, value, nameof(MasterText)); }
[CreateProperty] public string MasterText => $"{_edit.master:0}%";
```

### Listening for changes in code

Subscribe to `propertyChanged` when code, not a binding, has to react: a class toggle, a sound, an animation. The
event's `propertyName` is a `BindingId`; compare it with the property name. Subscribe while the screen is shown, as
the [Builder + Themes sample](../Samples/BuilderThemes) does (see [Screens](screens.md#lifecycle)):

```csharp
protected override void OnShow() => ViewModel.propertyChanged += OnChanged;

protected override void OnHide() => ViewModel.propertyChanged -= OnChanged;

void OnChanged(object sender, BindablePropertyChangedEventArgs e)
{
    if (e.propertyName == nameof(CharacterViewModel.Theme))
        UpdateSegments();
}
```

## The screen's data source

`Screen<TViewModel>` (and `ModalScreen<TViewModel>`) take the view model in their constructor and set it as the
`dataSource` of the screen's root element. This happens when the root is built, before `OnCreated` runs, so every
binding in the layout resolves against the view model:

```csharp
[ScreenLayout("MyGame/Hud")]
public sealed class HudScreen : Screen<HudViewModel>
{
    public HudScreen(HudViewModel viewModel) : base(viewModel) { }

    protected override void OnCreated()
    {
        // ViewModel is the root's data source here; bindings in Hud.uxml name HudViewModel properties.
    }
}
```

The screen exposes it as `ViewModel`. Passing `null` throws `ArgumentNullException`. `TViewModel` can be any class,
but deriving from `ViewModel` gives you the change notifications described above.

Children inherit the nearest ancestor's data source, so a `data-source-path` is always relative to it. Give an
element its own data source to bind a subtree to something else, such as a list item
(see [Data sources for rows and cells](#data-sources-for-rows-and-cells)).

The screen's view model is also where handlers are looked up. See [Interaction](interaction.md).

## Binding in UXML

Add a `Bindings` block with one `ui:DataBinding` per bound property:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:weave="Nerdova.WeaveUI">
    <ui:Label class="bar__text">
        <Bindings>
            <ui:DataBinding property="text" data-source-path="HealthText" binding-mode="ToTarget" />
        </Bindings>
    </ui:Label>

    <weave:ProgressBar name="xp">
        <Bindings>
            <ui:DataBinding property="value" data-source-path="Xp" binding-mode="ToTarget" />
            <ui:DataBinding property="title" data-source-path="XpTitle" binding-mode="ToTarget" />
        </Bindings>
    </weave:ProgressBar>
</ui:UXML>
```

| Attribute | Values | Description |
|---|---|---|
| `property` | An element property path | What to set on the element: `text`, `value`, `index`, `title`, `tooltip`, `enabledSelf`, `style.width`, `ItemsSource`, ... |
| `data-source-path` | A property path on the data source | What to read: a `[CreateProperty]` property name, or a nested path such as `Player.Name`. |
| `binding-mode` | `ToTarget`, `TwoWay`, `ToSource`, `ToTargetOnce` | Direction. Use `ToTarget` for display values and `TwoWay` for controls the player edits. The samples always set it explicitly. |

Properties the samples bind:

| Element | `property` | Source type |
|---|---|---|
| `Label` | `text` | `string` |
| Any element | `tooltip` | `string` |
| Any element | `enabledSelf` | `bool` |
| Any element | `style.width`, `style.height` | `StyleLength` |
| Any element | `style.backgroundImage` | `StyleBackground` |
| Any element | `style.backgroundColor` | `StyleColor` |
| `weave:Slider`, `weave:Toggle`, `weave:TextField` | `value` | `float`, `bool`, `string` |
| `weave:Dropdown` | `index` (or `value`) | `int` (or `string`) |
| `weave:ProgressBar` | `value`, `title` | `float`, `string` |
| `weave:Grid` | `ItemsSource` | `IList` |
| `weave:ListView` | `itemsSource` | `IList` |

The grid's property is `ItemsSource` with a capital I; `ListView`'s is Unity's `itemsSource`.

## Binding from C#

The C# builder binds with `.Bind(path)`, and `.DataSource(source)` sets an element's data source. With
`using static Nerdova.WeaveUI.Ui;`:

```csharp
Column().Children(
    Text().Bind(nameof(CharacterViewModel.Subtitle)),                         // Label text
    TextField("Epithet").Bind(nameof(CharacterViewModel.Epithet)),            // value, two-way
    Slider("Companion distance", 1, 10).Bind(nameof(CharacterViewModel.CompanionDistance)),
    El().Class("stat__fill").Bind(nameof(Stat.Fill), "style.width"),          // any property, ToTarget
    Button("Confirm").Bind(nameof(CharacterViewModel.HasSpent)));             // whether it is enabled
```

`Bind<T>(this T element, string path, string property = null, BindingMode? mode = null)` adds a `DataBinding` from
`path` on the data source to `property` on the element. Without `property`, it binds the element's main property:

| Element | Main property | Default mode |
|---|---|---|
| `WeaveGrid` | `ItemsSource` | `ToTarget` |
| `ProgressBar` | `value` | `ToTarget` |
| `Button` (including `weave:Button` and `weave:IconButton`) | `enabledSelf` | `ToTarget` |
| `Label` and other `TextElement`s | `text` | `ToTarget` |
| Any field (`INotifyValueChanged<T>`: Toggle, Slider, Dropdown, TextField, ...) | `value` | `TwoWay` |

An explicit `property` defaults to `ToTarget`; pass `mode` to change it. An element with no main property, such as
a plain `VisualElement`, throws `ArgumentException` unless you pass one. `Ui.MainProperty(element)` returns the
property and mode `Bind` would use.

A Button binds `enabledSelf`, not `text`. To bind a button's label, name the property:

```csharp
Button(binding.KeyText).Class("keycap").DataSource(binding).Bind(nameof(KeyBinding.KeyText), "text");
```

A Dropdown's main property is its `string` value. To bind its index instead:

```csharp
Dropdown("Quality", qualities).Bind(nameof(SettingsViewModel.Quality), "index", BindingMode.TwoWay);
```

`Bind` is a shortcut for UI Toolkit's own API, which you can still use directly:

```csharp
label.SetBinding("text", new DataBinding
{
    dataSourcePath = new PropertyPath(nameof(HudViewModel.HealthText)),
    bindingMode = BindingMode.ToTarget,
});
```

See [C# builder](builder.md) for the rest of the builder.

## Data sources for rows and cells

Repeated UI usually binds each row or cell to its own object. Make the item a `ViewModel` too, so a change to one
item updates only its row.

### Rows built in C#

Set the row's data source with `.DataSource(item)`. Bindings inside the row then name the item's properties. From the
[Builder + Themes sample](../Samples/BuilderThemes):

```csharp
Column().Children(ViewModel.Stats, StatRow);

VisualElement StatRow(Stat stat) =>
    Column().Class("mb-4").DataSource(stat).Children(
        Row().Class("items-center justify-between mb-1").Children(
            Text(stat.Name).Class("stat__name"),
            Text().Bind(nameof(Stat.ValueText)).Class("stat__value")),
        Row().Class("items-center").Children(
            El().Class("stat__bar flex-1 mr-3").Children(
                El().Class("stat__fill").Bind(nameof(Stat.Fill), "style.width")),
            Button("+").Size("sm").Variant("primary")
                .Bind(nameof(Stat.CanIncrease))
                .OnClick(() => ViewModel.Increase(stat))));
```

### Grid cells

Each `weave:Grid` cell's data source is the item in its slot, or `null` for an empty slot. Bindings in the
`cell-template` therefore name item properties. From the [Game HUD sample](../Samples/GameHud):

```xml
<!-- HotbarSlot.uxml: one cell. Its data source is the slot's HotbarSlot. -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="flex-1">
        <ui:VisualElement name="item" class="flex-1">
            <Bindings>
                <ui:DataBinding property="tooltip" data-source-path="Name" binding-mode="ToTarget" />
            </Bindings>
            <ui:VisualElement class="slot__icon" picking-mode="Ignore">
                <Bindings>
                    <ui:DataBinding property="style.backgroundImage" data-source-path="IconBackground" binding-mode="ToTarget" />
                </Bindings>
            </ui:VisualElement>
            <ui:VisualElement class="slot__cooldown" picking-mode="Ignore">
                <Bindings>
                    <ui:DataBinding property="style.height" data-source-path="CooldownHeight" binding-mode="ToTarget" />
                </Bindings>
            </ui:VisualElement>
        </ui:VisualElement>
        <ui:Label name="key" class="slot__key" picking-mode="Ignore" />
    </ui:VisualElement>
</ui:UXML>
```

The grid itself binds its `ItemsSource` to a list on the screen's view model:

```xml
<weave:Grid name="hotbar" columns="8" slot-count="8" cell-size="64" gap="8">
    <Bindings>
        <ui:DataBinding property="ItemsSource" data-source-path="Hotbar" binding-mode="ToTarget" />
    </Bindings>
</weave:Grid>
```

Use `BindCell` for anything per slot rather than per item, such as the slot's key number. Cells are pooled and
rebound as the grid scrolls, so a cell's data source changes over time. See [Grid](grid.md).

### List views

`weave:ListView` sets each row's data source to its item automatically (Unity's
`BindingSourceSelectionMode.AutoAssign`), so `DataBinding`s in the item template bind to the item. Bind the list
itself with `itemsSource`:

```xml
<weave:ListView name="saves" fixed-item-height="44" class="flex-1">
    <Bindings>
        <ui:DataBinding property="itemsSource" data-source-path="Saves" binding-mode="ToTarget" />
    </Bindings>
</weave:ListView>
```

Rows are created after the screen has prepared its layout, so rows that use `dark:`, breakpoint or `group-hover:`
classes need `WeaveClassAliases.Apply(row)` in `makeItem`. Weave UI's Basic Menu sample, imported from the Package
Manager, shows this. See [Styling](styling.md).

## Binding enabled state and styles

`enabledSelf` is bindable. Bind it to a `bool` to disable a control, or a whole row with every control in it. From the
Settings Menu sample:

```xml
<ui:VisualElement class="setting-row">
    <Bindings>
        <ui:DataBinding property="enabledSelf" data-source-path="AudioEnabled" binding-mode="ToTarget" />
    </Bindings>
    <!-- label and volume slider -->
</ui:VisualElement>

<weave:Button name="apply" text="Apply" variant="primary" click="Apply">
    <Bindings>
        <ui:DataBinding property="enabledSelf" data-source-path="HasChanges" binding-mode="ToTarget" />
    </Bindings>
</weave:Button>
```

```csharp
[CreateProperty] public bool Mute { get => _edit.mute; set => Set(ref _edit.mute, value, nameof(AudioEnabled)); }
[CreateProperty] public bool AudioEnabled => !_edit.mute;
```

To bind a style, expose the matching `Style*` type and bind `style.<property>`:

| Style type | Example source | Bound to |
|---|---|---|
| `StyleLength` | `Length.Percent(100 * _health / MaxHealth)` | `style.width`, `style.height` |
| `StyleBackground` | `new StyleBackground(texture)`, or `new StyleBackground(StyleKeyword.None)` for no image | `style.backgroundImage` |
| `StyleColor` | A `Color` (converts implicitly), or `new StyleColor(StyleKeyword.Null)` to fall back to the stylesheet | `style.backgroundColor`, `style.color` |

```csharp
public sealed class Item : ViewModel
{
    static readonly Color Affordable = new(0.99f, 0.83f, 0.30f);
    static readonly Color TooExpensive = new(0.98f, 0.44f, 0.52f);

    bool _canAfford = true;

    public Item(Texture2D icon) =>
        IconBackground = icon != null ? new StyleBackground(icon) : new StyleBackground(StyleKeyword.None);

    [CreateProperty] public StyleBackground IconBackground { get; }

    public bool CanAfford
    {
        get => _canAfford;
        set
        {
            if (SetProperty(ref _canAfford, value))
                Notify(nameof(PriceColor));
        }
    }

    [CreateProperty] public StyleColor PriceColor => _canAfford ? Affordable : TooExpensive;
}
```

A bound style is an inline style, so it overrides your USS for that property, and a USS `transition` on that
property still animates the change. The Game HUD binds two bars to the same `HealthFill`; one has a slower, delayed
width transition and becomes the "damage taken" bar.

## Two-way binding

Fields bound `TwoWay` write the player's edits back to the view model's setter, and show the view model's value when
it changes. The [Settings Menu sample](../Samples/SettingsMenu) edits a working copy this way and offers Apply and
Revert:

```xml
<weave:Slider name="master" low-value="0" high-value="100">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="Master" binding-mode="TwoWay" />
    </Bindings>
</weave:Slider>
<weave:Toggle name="mute">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="Mute" binding-mode="TwoWay" />
    </Bindings>
</weave:Toggle>
<weave:Dropdown name="quality" choices="Low,Medium,High,Ultra" change="QualityChanged">
    <Bindings>
        <ui:DataBinding property="index" data-source-path="Quality" binding-mode="TwoWay" />
    </Bindings>
</weave:Dropdown>
```

```csharp
[CreateProperty]
public float FrameCap
{
    get => _edit.frameCap;
    set => Set(ref _edit.frameCap, Mathf.Round(value), nameof(FrameCapText));
}
```

- The setter can validate or round the value, as `FrameCap` does; the binding shows what the property returns.
- In C#, `.Bind(path)` on a field is `TwoWay` already.
- To react to an edit as well as store it, add a `change="Method"` handler or an `[OnValueChanged]` attribute. See
  [Interaction](interaction.md).

## Pitfalls

- **Bindings update on the panel's next update, not immediately.** UI Toolkit applies data bindings while it updates
  the panel. After you set a view model property, the element shows the new value on the next frame, and reading
  `label.text` straight away still returns the old one. Bindings only run for elements on a panel. In tests, assert
  on the view model rather than on bound element values.
- **A change that isn't notified isn't shown.** Writing a backing field directly, or changing an input of a computed
  property without `Notify`, leaves the view hash unchanged, and UI Toolkit skips the update.
- **Missing `[CreateProperty]`.** The binding can't resolve a property that isn't marked, so the element never
  updates.
- **Mistyped names.** `data-source-path`, `Notify("...")` and `.Bind("...")` names must match the property exactly.
  Prefer `nameof(...)` in C#.
- **`change` handlers skip a binding's writes, except through a converter.** A `change="Method"` or
  `[OnValueChanged]` handler runs when the player (or your code) changes the value, not when a binding writes the
  model's value into the control. A binding with a converter still fires it. See
  [Known limitations](known-limitations.md).
- **Changing a bound list in place.** `WeaveGrid` refreshes when its `ItemsSource` is set to a different list, or
  when a list that implements `INotifyCollectionChanged` (such as `ObservableCollection<T>`) raises
  `CollectionChanged`. Notifying the same `List<T>` instance after adding or removing items doesn't refresh it; call
  `grid.Refresh()` (or `RefreshSlot(slot)`) instead.
- **Item properties need notifying too.** A grid cell updates when its item notifies a property change. If an item's
  getter starts returning something else without a notification, its cell stays stale.
- **A Button's main property is `enabledSelf`.** `Button("Buy").Bind(nameof(ShopViewModel.ActionText))` binds a
  `string` to `enabledSelf`. Pass `"text"` as the property to bind the label.
- **Unsubscribe from `propertyChanged`.** A screen that subscribes to its view model's event in `OnShow` should
  unsubscribe in `OnHide`, or a view model that outlives the screen keeps it alive.

## See also

- [Screens](screens.md): pushing screens with their view models.
- [Interaction](interaction.md): handlers that receive the view model and the bound item as parameters.
- [Grid](grid.md): cell templates, `BindCell` and occupancy maps.
- [C# builder](builder.md): `.Bind`, `.DataSource` and the other chaining methods.
- [Settings Menu sample](../Samples/SettingsMenu), [Game HUD sample](../Samples/GameHud),
  [Shop + Inventory sample](../Samples/ShopInventory), [Builder + Themes sample](../Samples/BuilderThemes).

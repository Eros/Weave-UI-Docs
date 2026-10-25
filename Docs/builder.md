# C# builder

The fluent builder (`Nerdova.WeaveUI.Ui`) builds UI in C# with the same controls, classes, bindings and handlers as UXML. This
page covers the factories and chain methods, how they map to UXML, building a whole screen in `CreateRoot`, filling
parts of a UXML screen from code, and a full example adapted from the
[Builder + Themes sample](../Samples/BuilderThemes).

## When to use it

Use UXML for layouts that are mostly fixed, so you can edit them in UI Builder. Use the builder for UI whose shape
depends on data: quest lists, log lines, toasts, generated menus, stat rows, or a whole screen you'd rather write in
code. The two mix freely: a UXML screen can fill an empty container with builder output.

## Setup

Import the namespace and the builder's static methods:

```csharp
using UnityEngine.UIElements;
using Nerdova.WeaveUI;
using static Nerdova.WeaveUI.Ui;
```

Then each factory reads like a tag, and each chain method returns the element so calls nest:

```csharp
Panel("menu").Class("p-4 gap-2").Children(
    Title("Biking It"),
    Button("Play").Variant("primary").OnClick(StartGame),
    Button("Settings").Bind(nameof(MenuViewModel.CanOpenSettings)));
```

If a factory name clashes with another type or method in your file, call it through the class, for example
`Ui.Toggle("Music")`.

## Factories

Factories create Weave controls (`WeaveButton`, `WeaveToggle`, ...), so variants, sizes, skins and handlers work as
they do in UXML. Controls with a label get an element name made from it: lower case, with runs of other characters
replaced by `-`. `Button("Delete save")` is named `delete-save`, and `Toggle("Auto-loot")` is named `auto-loot`, so
attribute handlers such as `[OnClick("delete-save")]` find them. A label with no letters or digits is named after its
code points: `Button("−")` is `u2212` and `Button("+")` is `u002b`. Pass a name for a readable one:
`Button("−", "decrease")`.

| Factory | Returns | Description |
|---|---|---|
| `El(string name = null)` | `VisualElement` | A plain container. |
| `Panel(string name = null)` | `VisualElement` | A raised surface with the `panel` class, the usual skin target for cards. |
| `Row(string name = null)` | `VisualElement` | A container with an inline `flex-direction: row`. |
| `Column(string name = null)` | `VisualElement` | A container with an inline `flex-direction: column`. |
| `Title(string text)` | `Label` | A heading, with the `title` class. |
| `Text(string text = null)` | `Label` | A label. Leave the text out when you bind it. |
| `Button(string text, string name = null)` | `WeaveButton` | A button. Named from its text unless you pass a name. |
| `IconButton(Sprite icon, string tooltip = null, string name = null)` | `WeaveIconButton` | An icon-only button. |
| `Toggle(string label, bool value = false)` | `WeaveToggle` | A toggle, named from its label. |
| `Slider(string label, float low = 0, float high = 1, float value = 0)` | `WeaveSlider` | A float slider, named from its label. |
| `Dropdown(string label, IEnumerable<string> choices, int index = 0)` | `WeaveDropdown` | A dropdown, named from its label. |
| `TextField(string label, string value = "")` | `WeaveTextField` | A text field, named from its label. |
| `ProgressBar(string title = null, float value = 0)` | `WeaveProgressBar` | A progress bar. Not named. |
| `Grid(int columns, float cellSize = 64, float gap = 4, string name = null)` | `WeaveGrid` | A slot grid. See [Grid](grid.md). |
| `Image(Sprite sprite)` | `Image` | A UI Toolkit image, scaled to fit. |

`Row()` and `Column()` set their direction as an inline style, which wins over classes, so don't add `flex-col` to a
`Row()`. Use `El()` when a class should decide the direction.

There are no factories for `ListView` or `TabView`. Create `new WeaveListView()` or `new WeaveTabView()` and chain on
them like any other element. See [Controls](controls.md).

## Chain methods

Every chain method is an extension method that returns the element it was called on, with its own type, so
`Button("Play").Variant("primary")` is still a `WeaveButton`.

### Structure and styling

| Method | Description |
|---|---|
| `Class(string classes)` | Adds classes, space-separated. Tailwind names are welcome, and the alias classes that `dark:`, `md:` and `group-hover:` variants need are added too. |
| `Name(string name)` | Sets the element name. |
| `Style(Action<IStyle> style)` | Sets inline styles: `.Style(s => s.width = 200)`. |
| `Tooltip(string tooltip)` | Sets the tooltip. A `ScreenHost` shows it on hover. |
| `Enabled(bool enabled)` | Calls `SetEnabled`. |
| `Children(params VisualElement[] children)` | Adds children. Nulls are skipped, so `condition ? X() : null` works. |
| `Children<TItem>(IEnumerable<TItem> items, Func<TItem, VisualElement> make)` | Adds one child per item. Items for which `make` returns null are skipped. |

### Controls only

These need a Weave control (an `IWeaveControl`): the factories above that return `Weave*` types, and `WeaveGrid`.
Plain containers from `El()`, `Panel()`, `Row()` and `Column()` don't have them.

| Method | Description |
|---|---|
| `Variant(string variant)` | Sets the variant modifier, e.g. `btn--primary`. Calling it again replaces the previous variant. |
| `Size(string size)` | `sm`, `md` (no class) or `lg`. |
| `Skin(string skin)` | Skins only this control with a registered skin. See [Skins](skins.md). |

### Data

| Method | Description |
|---|---|
| `DataSource(object source)` | Sets the data source that the element's bindings, and its children's, resolve against. |
| `Bind(string path, string property = null, BindingMode? mode = null)` | Binds a property to a path on the data source. Without `property`, binds the element's main property (below). With `property`, binds that property one way unless you pass `mode`. |

`Bind(path)` picks the main property from the element's type:

| Element | Property | Mode |
|---|---|---|
| `WeaveGrid` | `ItemsSource` | To the element |
| `ProgressBar` | `value` | To the element |
| `Button` | `enabledSelf` (whether it is enabled) | To the element |
| `Label` and other text elements | `text` | To the element |
| Fields (`Toggle`, `Slider`, `Dropdown`, `TextField`, ...) | `value` | Two-way |

Any other element throws an `ArgumentException`, so pass the property: `El().Bind(nameof(Stat.Fill), "style.width")`.
`Ui.MainProperty(element)` returns the property and mode `Bind` would use.

See [Data binding](data-binding.md) for paths, modes and view models.

### Handlers

The interaction extension methods also chain. Each takes a delegate whose parameters are injected like an attribute
handler's (see [Interaction](interaction.md)); qualifiers such as `[Slot]` can't be put on lambda parameters, so request
`InteractionContext` when you need those values.

| Method | Description |
|---|---|
| `OnClick(handler, MouseButton button = MouseButton.LeftMouse, int clicks = 1)` | A click. `handler` takes 0 to 3 parameters. On a button, keyboard and gamepad submit count as clicks. |
| `OnRightClick(handler)` | A right click. 0 to 2 parameters. |
| `OnLongPress(handler, int milliseconds = 500)` | Press and hold. 0 to 2 parameters. |
| `OnHover(Action<HoverPhase> handler)`, `OnHover(Action<HoverPhase, T2> handler)` | Pointer enter and exit. |
| `OnValueChanged(handler)` | A field's value changed. 1 to 3 parameters; a parameter of the field's value type gets the new value. |
| `FluentBindings()` | The handlers added to an element with these methods. |
| `ClearInteractions()` | Removes them. |

```csharp
Button("Refund points").Variant("ghost").OnClick(() => ViewModel.ResetPoints());
Slider("Volume", 0, 100).OnValueChanged((float volume) => audio.Volume = volume);
Panel().OnHover((HoverPhase phase) => preview.Show(phase == HoverPhase.Enter));
```

Fluent handlers belong to the element, not the screen, and live as long as the element does.

## How it maps to UXML

| UXML | Builder |
|---|---|
| `<ui:VisualElement name="card" class="panel p-4" />` | `Panel("card").Class("p-4")` |
| `<ui:Label text="Shop" class="title" />` | `Title("Shop")` |
| `<weave:Button text="Buy" variant="primary" size="lg" />` | `Button("Buy").Variant("primary").Size("lg")` |
| `click="Buy"` | `.OnClick(Buy)`, or an `[OnClick("buy")]` attribute handler |
| `tooltip="..."` | `.Tooltip("...")` |
| `style="width: 200px;"` | `.Style(s => s.width = 200)` |
| `<ui:DataBinding property="text" data-source-path="Name" binding-mode="ToTarget" />` on a label | `.Bind("Name")` |
| `<ui:DataBinding property="style.width" data-source-path="Fill" binding-mode="ToTarget" />` | `.Bind("Fill", "style.width")` |
| `data-source="..."` | `.DataSource(stat)` |
| `<weave:Grid columns="8" cell-size="64" gap="4" />` | `Grid(8, 64, 4)` |
| Nested elements | `.Children(...)` |

The Tailwind pipeline scans `.Class("...")` strings in your C# files as well as UXML, so utility classes used only in
code are generated too. See [Tailwind pipeline](tailwind-pipeline.md).

## Building a whole screen

Override `CreateRoot()` instead of using `[ScreenLayout]`, and return the root element:

```csharp
public sealed class PauseScreen : Screen<PauseViewModel>
{
    public PauseScreen(PauseViewModel viewModel) : base(viewModel) { }

    protected override VisualElement CreateRoot() =>
        El("pause").Class("flex-1 items-center justify-center").Children(
            Panel("card").Class("w-96 p-6 gap-3").Children(
                Title("Paused"),
                Button("Resume").Variant("primary").OnClick(() => Manager.Pop()),
                Button("Settings").Variant("secondary"),
                Toggle("Show hints").Bind(nameof(PauseViewModel.ShowHints))));

    [OnClick("settings")]
    void OpenSettings() => Manager.Push(new SettingsScreen(new SettingsViewModel()));
}
```

After `CreateRoot()` returns, the screen treats the tree exactly like a cloned UXML layout:

1. Class aliases and layout utilities (`gap-*`, `grid grid-cols-N`) are applied to the whole tree.
2. The root's data source becomes the view model, so `Bind` paths name view model properties.
3. Attribute handlers on the screen and view model are bound to elements by name (`[OnClick("settings")]` finds the
   button named `settings`).
4. `OnCreated()` runs.

`PauseViewModel`, `SettingsScreen` and `SettingsViewModel` stand for your own types. See [Screens](screens.md) for
`Manager`, `Push` and `Pop`.

## Filling part of a UXML screen

A UXML screen can leave a container empty and fill it in `OnCreated`. The Game HUD's layout has empty `quests` and
`log` containers, and `HudScreen` fills them:

```xml
<ui:VisualElement name="log" class="hud-card absolute left-8 bottom-8 p-4" style="width: 440px;" picking-mode="Ignore" />
```

```csharp
protected override void OnCreated()
{
    Root.Q("quests").Children(ViewModel.Quests, QuestRow);
    Root.Q("log").Children(ViewModel.Log, line => Text(line).Class("log-line"));
}

static VisualElement QuestRow(Quest quest) =>
    Row().Class("items-start mb-3").Children(
        El().Class(quest.Done ? "quest-check quest-check--done" : "quest-check"),
        Column().Class("flex-1").Children(
            Text(quest.Title).Class(quest.Done ? "text-sm text-slate-400" : "text-base font-bold text-amber-100"),
            Text(quest.Step).Class(quest.Done ? "text-xs text-slate-500" : "text-sm text-slate-300")));
```

`Children` is an extension on any `VisualElement`, so it works on the result of `Root.Q(...)`. Elements added this
way, in `OnCreated` or later, come after the screen's setup, so:

- `.Class()` adds alias classes itself, so `dark:` and `md:` variants work.
- Layout utilities aren't attached. If the new elements use `gap-*` or `grid grid-cols-N`, call
  `UtilityLayout.Apply(element)` on them after adding them.
- Attribute handlers were bound before `OnCreated`, and an attribute handler whose element doesn't exist yet fails to
  bind. Use `.OnClick(...)` and the other fluent handlers on elements you add later.

The same applies to elements built at any later point, such as the Game HUD's toasts:

```csharp
void ShowToast(Toast toast)
{
    var element = Column().Class($"toast toast--{toast.Tone} toast--hidden").Children(
        Text(toast.Title).Class("toast__title"),
        Text(toast.Body).Class("toast__body"));
    _toasts.Add(element);
    element.schedule.Execute(() => element.RemoveFromClassList("toast--hidden")).StartingIn(50);
}
```

## Full example

This is the Builder + Themes sample's character sheet, shortened. It is written entirely in C#: a header with a theme
switcher, a column of attribute rows that each bind to their own `Stat`, a column of fields bound to the character,
and a footer. `CharacterViewModel`, `Stat` and `ThemeChoice` are the sample's types.

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;
using static Nerdova.WeaveUI.Ui;

public sealed class CharacterSheetScreen : Screen<CharacterViewModel>
{
    private static readonly string[] Stances = { "Balanced", "Aggressive", "Defensive" };
    private readonly Dictionary<ThemeChoice, WeaveButton> _segments = new();

    public CharacterSheetScreen(CharacterViewModel viewModel) : base(viewModel) { }

    protected override VisualElement CreateRoot()
    {
        var root = El("sheet").Class("sheet flex-1 items-center justify-center").Children(
            Panel("card").Class("sheet__card").Children(
                Header(),
                Row().Class("items-stretch gap-5").Children(
                    Attributes(),
                    Derived(),
                    Preferences()),
                Footer()));
        root.styleSheets.Add(Resources.Load<StyleSheet>("WeaveUISamples/BuilderThemes/CharacterSheet"));
        return root;
    }

    protected override void OnShow()
    {
        ViewModel.propertyChanged += OnChanged;
        UpdateSegments();
    }

    protected override void OnHide() => ViewModel.propertyChanged -= OnChanged;

    VisualElement Header() =>
        Row().Class("items-center mb-6").Children(
            Column().Class("flex-1").Children(
                Title("Ada Thornfield").Class("mb-0"),
                Text().Bind(nameof(CharacterViewModel.Subtitle)).Class("muted text-sm")),
            Row("themes").Class("segments mr-4").Children(
                Segment(ThemeChoice.Dark),
                Segment(ThemeChoice.Light),
                Segment(ThemeChoice.Ember)));

    WeaveButton Segment(ThemeChoice choice)
    {
        var button = Button(choice.ToString()).Size("sm").Class("segment").OnClick(() => ViewModel.Theme = choice);
        _segments[choice] = button;
        return button;
    }

    VisualElement Attributes() =>
        Column("attributes").Class("sheet-column").Children(
            Row().Class("items-center justify-between mb-3").Children(
                Text("ATTRIBUTES").Class("section-heading"),
                Text().Bind(nameof(CharacterViewModel.PointsText)).Class("points")),
            Column().Children(ViewModel.Stats, StatRow));

    /// <summary>One attribute; the row's data source is the Stat, so its bindings name Stat properties.</summary>
    VisualElement StatRow(Stat stat) =>
        Column().Class("mb-4").DataSource(stat).Children(
            Row().Class("items-center justify-between mb-1").Children(
                Text(stat.Name).Class("stat__name"),
                Text().Bind(nameof(Stat.ValueText)).Class("stat__value")),
            Row().Class("items-center").Children(
                El().Class("stat__bar flex-1 mr-3").Children(
                    El().Class("stat__fill").Bind(nameof(Stat.Fill), "style.width")),
                Button("−").Size("sm").Variant("secondary").Class("stat__step mr-1")
                    .Bind(nameof(Stat.CanDecrease)).OnClick(() => ViewModel.Decrease(stat)),
                Button("+").Size("sm").Variant("primary").Class("stat__step")
                    .Bind(nameof(Stat.CanIncrease)).OnClick(() => ViewModel.Increase(stat))));

    VisualElement Derived() =>
        Column("derived").Class("sheet-column").Children(
            Text("DERIVED").Class("section-heading mb-3"),
            Figure("Health", nameof(CharacterViewModel.HealthText)),
            Figure("Mana", nameof(CharacterViewModel.ManaText)),
            Text("RESISTANCES").Class("section-heading mt-5 mb-2"),
            ProgressBar("Fire  35%", 35),
            ProgressBar("Poison  20%", 20).Variant("danger"));

    static VisualElement Figure(string label, string path) =>
        Row().Class("figure").Children(
            Text(label).Class("flex-1 muted"),
            Text().Bind(path).Class("figure__value"));

    VisualElement Preferences() =>
        Column("preferences").Class("sheet-column").Children(
            Text("PREFERENCES").Class("section-heading mb-3"),
            TextField("Epithet").Bind(nameof(CharacterViewModel.Epithet)),
            Dropdown("Stance", Stances).Bind(nameof(CharacterViewModel.Stance)),
            Slider("Companion distance", 1, 10).Bind(nameof(CharacterViewModel.CompanionDistance)),
            Toggle("Auto-loot").Bind(nameof(CharacterViewModel.AutoLoot))
                .Tooltip("Pick up gold and crafting materials on the way past"));

    VisualElement Footer() =>
        Row().Class("items-center mt-6").Children(
            Text().Bind(nameof(CharacterViewModel.Status)).Class("flex-1 status"),
            Button("Refund points").Variant("ghost").Class("mr-2").OnClick(() => ViewModel.ResetPoints()),
            Button("Confirm").Variant("primary").Bind(nameof(CharacterViewModel.HasSpent)).OnClick(() => ViewModel.Confirm()));

    void OnChanged(object sender, BindablePropertyChangedEventArgs e)
    {
        if (e.propertyName == nameof(CharacterViewModel.Theme))
        {
            UpdateSegments();
        }
    }

    /// <summary>The active theme's button becomes a primary button; the others are ghosts.</summary>
    void UpdateSegments()
    {
        foreach (var (choice, button) in _segments)
        {
            button.Variant(choice == ViewModel.Theme ? "primary" : "ghost");
        }
    }
}
```

Points to note:

- **Per-row data sources.** Each stat row calls `.DataSource(stat)`, so `Bind(nameof(Stat.ValueText))` resolves
  against that `Stat` while the rest of the sheet binds to the character.
- **A button's main property is whether it is enabled.** `Button("+").Bind(nameof(Stat.CanIncrease))` disables the
  button when the stat can't go higher.
- **Fields bind two-way.** The text field, dropdown, slider and toggle write back to the view model as the player
  changes them.
- **Variants change at runtime.** `UpdateSegments` calls `.Variant(...)` on existing buttons; the previous variant's
  class is removed.
- **The stylesheet is attached in code.** `CharacterSheet.uss` colours the sheet with Weave's semantic variables, so the
  same screen follows light and dark mode, palettes and skins. See [Theming](theming.md).

## Reference

All members are on the static class `Nerdova.WeaveUI.Ui`, except the handler methods, which are on `Nerdova.WeaveUI.Interactions`.

| Member | Signature |
|---|---|
| `El` | `VisualElement El(string name = null)` |
| `Panel` | `VisualElement Panel(string name = null)` |
| `Row` | `VisualElement Row(string name = null)` |
| `Column` | `VisualElement Column(string name = null)` |
| `Title` | `Label Title(string text)` |
| `Text` | `Label Text(string text = null)` |
| `Button` | `WeaveButton Button(string text, string name = null)` |
| `IconButton` | `WeaveIconButton IconButton(Sprite icon, string tooltip = null, string name = null)` |
| `Toggle` | `WeaveToggle Toggle(string label, bool value = false)` |
| `Slider` | `WeaveSlider Slider(string label, float low = 0, float high = 1, float value = 0)` |
| `Dropdown` | `WeaveDropdown Dropdown(string label, IEnumerable<string> choices, int index = 0)` |
| `TextField` | `WeaveTextField TextField(string label, string value = "")` |
| `ProgressBar` | `WeaveProgressBar ProgressBar(string title = null, float value = 0)` |
| `Grid` | `WeaveGrid Grid(int columns, float cellSize = 64, float gap = 4, string name = null)` |
| `Image` | `Image Image(Sprite sprite)` |
| `Class` | `T Class<T>(this T element, string classes)` |
| `Name` | `T Name<T>(this T element, string name)` |
| `Style` | `T Style<T>(this T element, Action<IStyle> style)` |
| `Tooltip` | `T Tooltip<T>(this T element, string tooltip)` |
| `Enabled` | `T Enabled<T>(this T element, bool enabled)` |
| `Children` | `T Children<T>(this T element, params VisualElement[] children)` |
| `Children` | `T Children<T, TItem>(this T element, IEnumerable<TItem> items, Func<TItem, VisualElement> make)` |
| `DataSource` | `T DataSource<T>(this T element, object source)` |
| `Variant` | `T Variant<T>(this T control, string variant)` where `T : VisualElement, IWeaveControl` |
| `Size` | `T Size<T>(this T control, string size)` where `T : VisualElement, IWeaveControl` |
| `Skin` | `T Skin<T>(this T control, string skin)` where `T : VisualElement, IWeaveControl` |
| `Bind` | `T Bind<T>(this T element, string path, string property = null, BindingMode? mode = null)` |
| `MainProperty` | `(string property, BindingMode mode) MainProperty(VisualElement element)` |
| `OnClick` | `T OnClick<T>(this T e, Action handler, MouseButton button = MouseButton.LeftMouse, int clicks = 1)`, and overloads taking `Action<T1>`, `Action<T1, T2>`, `Action<T1, T2, T3>` |
| `OnRightClick` | `T OnRightClick<T>(this T e, Action handler)`, and overloads taking `Action<T1>`, `Action<T1, T2>` |
| `OnLongPress` | `T OnLongPress<T>(this T e, Action handler, int milliseconds = 500)`, and overloads taking `Action<T1>`, `Action<T1, T2>` |
| `OnHover` | `T OnHover<T>(this T e, Action<HoverPhase> handler)`, `T OnHover<T, T2>(this T e, Action<HoverPhase, T2> handler)` |
| `OnValueChanged` | `T OnValueChanged<T, T1>(this T e, Action<T1> handler)`, and overloads taking `Action<T1, T2>`, `Action<T1, T2, T3>` |
| `FluentBindings` | `IReadOnlyList<InteractionBinding> FluentBindings(this VisualElement element)` |
| `ClearInteractions` | `void ClearInteractions(this VisualElement element)` |

Every `T` is constrained to `VisualElement`.

# Interaction

Weave UI's interaction system binds input handlers to elements and passes each handler exactly the values it asks
for as parameters: the mouse button, the new slider value, the clicked grid slot, the view model, the dragged item.
This page covers the handler attributes, UXML handler names, fluent handlers, every value a handler can request, how
requests are checked when a screen is built, and how to add your own values and test handlers.

## Three ways to bind a handler

```csharp
// 1. An attribute on a method of the screen or its view model, naming the element.
[OnClick("play")]
void Play(EventModifiers modifiers) => ViewModel.Play(quick: (modifiers & EventModifiers.Shift) != 0);
```

```xml
<!-- 2. A method name in UXML, on a weave: control. -->
<weave:Button name="apply" text="Apply" variant="primary" click="Apply" />
```

```csharp
// 3. A fluent call in C#, usually with the builder.
Button("Back").Variant("ghost").OnClick(() => Manager.Pop());
```

All three resolve parameters the same way. Screens bind attribute handlers and UXML names automatically when they
build their root; fluent handlers are bound when you call them.

## Handler attributes

Put these on methods of a screen or its view model. Each takes the element's `name` in the layout. They are in the
`Nerdova.WeaveUI` namespace and derive from `InteractionAttribute`.

| Attribute | Properties | Fires when |
|---|---|---|
| `[OnClick("name")]` | `Button` (`MouseButton`, default `LeftMouse`), `Clicks` (`int`, default 1) | The element is clicked. On a `Button` with the defaults, keyboard and gamepad submit count too. `Button = MouseButton.RightMouse` makes it a right-click handler. |
| `[OnDoubleClick("name")]` | | A left double-click (the same as `[OnClick("name", Clicks = 2)]`). |
| `[OnRightClick("name")]` | | A right-button press and release on the element. |
| `[OnLongPress("name")]` | `Milliseconds` (`int`, default 500) | The left button or a touch is held for `Milliseconds`. Releasing, leaving the element, a pointer cancel or moving more than 10 px cancels it. |
| `[OnHover("name")]` | | The pointer enters and leaves the element. Request `HoverPhase` to tell the two apart. |
| `[OnValueChanged("name")]` | | The element's value changes. The element must implement `INotifyValueChanged<T>` (Toggle, Slider, Dropdown, TextField, ...). |
| `[OnCellClick("grid")]` | `Button` (`MouseButton`, default `LeftMouse`), `Clicks` (`int`, default 1) | A grid cell is clicked (or submitted with keyboard or gamepad). The same as `[OnClick]` on a grid, but binding fails if the element isn't a `WeaveGrid`. |
| `[OnDrop("grid")]` | | An item is dropped on a cell of the grid. |
| `[OnDragStart("grid")]` | | A drag starts from a cell of the grid. |
| `[OnDragOver("grid")]` | | A drag moves onto another cell of the grid. |
| `[OnDragCancel("grid")]` | | A drag from the grid ends without a drop: released outside, rejected, or cancelled with Escape. |

### Click filters

`Clicks` filters by click count:

- `Clicks = 1` (the default) accepts every click, including the second click of a double-click.
- `Clicks = 2` or more fires only when the click count equals that number, for the left or the right button.

A screen with both a single-click and a double-click handler on the same element therefore runs the single-click
handler twice and the double-click handler once for a double-click. From the
[Shop + Inventory sample](../Samples/ShopInventory), where a click selects and a double-click buys:

```csharp
[OnCellClick("vendor")]
void SelectStock(Item item) => ViewModel.Select(item, fromStock: true);

[OnCellClick("vendor", Clicks = 2)]
void QuickBuy(Item item) => Buy(item);
```

Keyboard and gamepad submit count as a click on a `Button` whose handler uses the default left button and one
click, and on a grid's cursor cell. Other handlers listen to pointer clicks only.

### Grids

A `WeaveGrid` dispatches per cell. `[OnClick]`, `[OnRightClick]` and `[OnHover]` work on a grid too and fire per cell;
`[OnCellClick]` and the drag attributes only work on grids. `[OnLongPress]` and `[OnValueChanged]` can't be bound to
a grid. See [Grid](grid.md) for drag modes, drop rules and occupancy maps.

What an unqualified request means depends on the event:

| Attribute | Unqualified item and `[Slot] int` | Fired on |
|---|---|---|
| `[OnCellClick]`, `[OnClick]`, `[OnRightClick]`, `[OnHover]` | The clicked or hovered slot and its item | The grid |
| `[OnDragStart]` | The dragged item and its slot | The source grid |
| `[OnDragOver]` | The slot under the pointer and the item in it | The grid under the pointer |
| `[OnDrop]` | The target slot and the item that was in it before the drop | The target grid |
| `[OnDragCancel]` | The dragged item and its slot | The source grid |

In drag handlers, `[Source]` requests describe the dragged item instead. From the Shop + Inventory sample:

```csharp
[OnDrop("vendor")]
void DroppedOnVendor([Source] Item item) => ViewModel.Sell(item);

[OnDrop("bag")]
void DroppedInBag(GridDrag drag, [Slot] int slot) { /* buy or move to the slot */ }

// A rejected drop (no room, or not enough gold) snaps back; say why.
[OnDragCancel("bag")]
[OnDragCancel("vendor")]
void Cancelled(Item item) => ViewModel.ExplainRejected(item);
```

### Several elements, several attributes

- An attribute binds **every** element with that name, so one method can serve a repeated element.
- The attributes allow multiple use: stack them to bind one method to several elements or events, as `Cancelled`
  does above.
- An attribute whose name matches no element is an error (see [Errors](#errors-at-build-time)).

### Handler rules

- Return `void`.
- Take at most five parameters. No `ref` or `out` parameters.
- Any access level works, including `private`. Static methods work too. Methods declared on base classes are found.
  An overridden method is bound once, as its most-derived override.

## Where handlers are looked up

When a screen builds its root, it binds:

1. The handler attributes on the **screen**.
2. The handler attributes on the **view model** (for a `Screen<TViewModel>`).
3. The UXML handler names in the layout. Each name is looked up on the **view model first, then the screen**. The
   first one with a method of that name wins.

Attribute handlers on the screen and the view model are all bound: if both have an `[OnClick("play")]`, both run.
UXML names bind to one method only. The [Settings Menu sample](../Samples/SettingsMenu) relies on the lookup order:
`click="Apply"` finds `SettingsViewModel.Apply`, and `click="AskDefaults"`, which the view model doesn't have, finds
the screen's method.

Handlers are rebound whenever the screen's root is rebuilt (for example by hot reload in the editor).

To bind handlers outside a screen, call `Interactions.Bind(target, root)`, which binds `target`'s attribute
handlers to elements under `root`, or `Interactions.BindUxmlHandlers(root, screen, targets)`. Both return an
`InteractionBindingSet`; dispose it to unbind.

## UXML handler attributes

The `weave:` controls take a method name for their events. The method gets the same parameter injection as the
equivalent attribute.

| Element | Attribute | Bound like |
|---|---|---|
| `weave:Button`, `weave:IconButton` | `click` | `[OnClick]`: left button, one click, keyboard and gamepad submit |
| `weave:Toggle`, `weave:Slider`, `weave:Dropdown`, `weave:TextField` | `change` | `[OnValueChanged]` |
| `weave:Grid` | `click` | `[OnCellClick]` |
| `weave:Grid` | `drop` | `[OnDrop]` |

`weave:ProgressBar`, `weave:ListView` and `weave:TabView` have no handler attributes, and Unity's own `ui:` elements
don't take them; use a handler attribute for those.

```xml
<weave:Dropdown name="quality" choices="Low,Medium,High,Ultra" change="QualityChanged" />
<weave:Grid name="bag" columns="8" cell-size="64" drag="default" click="Use" drop="Dropped" />
```

```csharp
[Preserve]
public void QualityChanged(string quality, [Previous] string was) => SetStatus($"Quality: {was} → {quality}");
```

The name must match exactly one method on the class where it is found. Overloads with that name are an error.

### Keep UXML handlers with [Preserve]

Managed code stripping removes methods that nothing references. The handler attributes derive from Unity's
`PreserveAttribute`, so methods marked with them are always kept. A method named only in UXML has no attribute, so
mark it `[Preserve]` (namespace `UnityEngine.Scripting`), or a stripped player build can't find it:

```csharp
using UnityEngine.Scripting;

/// <summary>click="Act" on the details panel's button.</summary>
[Preserve]
void Act() { ... }
```

## Requesting values

Declare what a handler needs as parameters, in any order:

```csharp
[OnClick("play")]                   void Play(EventModifiers modifiers, MainMenuViewModel vm) { ... }
[OnClick("item", Clicks = 2)]       void Open([ClickCount] int clicks) { ... }
[OnRightClick("item")]              void Menu([PanelPosition] Vector2 position) { ... }
[OnValueChanged("volume")]          void Volume(float value, [Previous] float was) { ... }
[OnCellClick("bag")]                void Use(Item item, [Slot] int slot, [Row] int row) { ... }
[OnDrop("bag")]                     void Dropped([Source] Item dragged, Item onto, DropOutcome outcome) { ... }
```

Each parameter is matched to a provider by its type and its qualifier attribute (`[Slot]`, `[Source]`, ...). Two
parameters of the same type are told apart by qualifiers: `[ClickCount] int` and `[Slot] int`.

### Available values

| Parameter | Value | Available for |
|---|---|---|
| `MouseButton` | The button. `LeftMouse` for keyboard and gamepad submit | Click, right-click, long-press, hover |
| `EventModifiers` | Modifier keys held (Shift, Control, Alt, Command, ...) | Every interaction |
| `[ClickCount] int` | The click count: 2 for a double-click | Click, right-click, long-press, hover |
| `Vector2` | The pointer position in the element's local coordinates | Click, right-click, long-press, hover |
| `[PanelPosition] Vector2` | The pointer position in panel coordinates | Click, right-click, long-press, hover |
| `HoverPhase` | `Enter` or `Exit` | Hover |
| `EventBase` | The UI Toolkit event. `null` for long-press, grid drags and `Simulate`. Only `EventBase` itself: a `ClickEvent` or `ChangeEvent<T>` request fails at binding | Every interaction |
| `InteractionContext` | Everything about the interaction (see below) | Every interaction |
| The field's value type (`bool`, `float`, `int`, `string`, ...) | The new value | `[OnValueChanged]` and `change`, on an `INotifyValueChanged<T>` element |
| `[Previous]` and the field's value type | The value before the change | `[OnValueChanged]` and `change` |
| `bool` | A Toggle's current value | Any interaction on a `Toggle` |
| `float` | A Slider's current value | Any interaction on a `BaseSlider<float>` (`Slider`) |
| `int` | A SliderInt's current value | Any interaction on a `BaseSlider<int>` (`SliderInt`) |
| `int` | A Dropdown's selected index | Any interaction on a `DropdownField` |
| `string` | A Dropdown's selected value | Any interaction on a `DropdownField` |
| `string` | A TextField's current text | Any interaction on a `TextField` |
| `[Slot] int` | The slot index | Grid interactions |
| `[Row] int` | The slot's row | Grid interactions |
| `[Column] int` | The slot's column | Grid interactions |
| `GridCell` | The cell element; `null` when the slot is scrolled out of view | Grid interactions |
| `GridDrag` | The drag: source grid, slot and item, target grid, slot and item, size, outcome, position | Grid drag events |
| `DropOutcome` | What the drop does or would do: `Reject`, `Move`, `Swap` or `Merge` | Grid drag events |
| `[Source] int` | The dragged item's slot | Grid drag events |
| `[Source] WeaveGrid` | The grid the drag started from | Grid drag events |
| `[Source]` and the item's class | The dragged item, as that class (`null` if it is another type) | Grid drag events |
| The element's type, or any base type (`Button`, `Slider`, `VisualElement`, `WeaveGrid`, ...) | The element the handler is bound to | Every interaction |
| A `WeaveScreen` type | The screen that bound the handler; `null` for fluent handlers and `Interactions.Bind` without a screen | Every interaction |
| Any other class (not `string`, a delegate, a `UnityEngine.Object`, a `VisualElement`, an event, `InteractionContext` or `GridDrag`) | The nearest data source of that type: the view model, or a row's item. In grid interactions the slot's item comes first | Every interaction |

The last row is how handlers receive the view model, or the item in a clicked grid slot. The lookup walks from the
element up through its ancestors' `dataSource`, and only runs when the handler is called. In a grid interaction
it returns the slot's item first, so `Item item` is the clicked item and `null` for an empty slot (unless an ancestor's
data source is also an `Item`).

### Parameter attributes

| Attribute | Applies to | Description |
|---|---|---|
| `[ClickCount]` | `int` | The click count. |
| `[PanelPosition]` | `Vector2` | Panel coordinates instead of local ones. |
| `[Previous]` | The field's value type | The value before a value change. |
| `[Slot]` | `int` | The grid slot. |
| `[Row]` | `int` | The grid slot's row. |
| `[Column]` | `int` | The grid slot's column. |
| `[Source]` | `int`, `WeaveGrid`, the item's class | In drag handlers: the dragged item's slot, grid or item, instead of the target's. |

All derive from `RequestQualifierAttribute`. Derive from it to make your own (see
[Adding your own values](#adding-your-own-values)).

### Resolution order

Each parameter is matched once, in this order:

1. The value carried by a value change: the new value, or `[Previous]`.
2. Registered providers, best scope first: control values, then grid values, then your project's providers, then the
   common values (button, modifiers, positions, ...). Two matching providers in the same scope is an error.
3. `[Source]` with a class type in a grid drag handler: the dragged item.
4. A `VisualElement` type: the element itself, if it is that type.
5. A `WeaveScreen` type: the screen.
6. Any other class: the nearest data source of that type.

So a Toggle's `[OnValueChanged]` handler gets the new value for `bool`, while its `[OnClick]` handler gets the
Toggle's current value. A Dropdown's `change` handler gets the new value for `string` and the selected index for
`int`.

### InteractionContext

Request `InteractionContext` for everything at once. It is pooled and reused, so use it only during the handler call;
don't store it.

| Member | Description |
|---|---|
| `Kind` | The `InteractionKind`: `Click`, `RightClick`, `LongPress`, `HoverEnter`, `HoverExit`, `ValueChanged`, `DragStart`, `DragOver`, `Drop` or `DragCancel`. |
| `Element` | The element the handler is bound to. |
| `Screen` | The screen that bound it, or `null`. |
| `Event` | The UI Toolkit event, or `null` for synthetic interactions. |
| `Button`, `ClickCount`, `Modifiers` | Pointer facts. |
| `LocalPosition`, `PanelPosition` | The pointer position in local and panel coordinates. |
| `PointerType` | `"mouse"`, `"touch"`, `"pen"`, or `"keyboard"` for navigation submits. |
| `Slot` | Grid interactions: the slot, otherwise -1. |
| `Item` | Grid interactions: the slot's item (`null` for an empty slot). For drops, the target's item before the drop; for drag start and cancel, the dragged item. |
| `Cell` | Grid interactions: the `GridCell`, when it is on screen. |
| `Drag` | Grid drag interactions: the `GridDrag`. |
| `DataSource<T>()` | The nearest data source of type `T` (the slot's item first in grid interactions). |

## Errors at build time

Requests are checked when a handler is bound, not when it is first clicked. For a screen, that is when it is first
pushed, so a mistake throws `InteractionBindingException` from `ScreenManager.Push`. The message names the handler,
the request, the element and everything that the handler could request there:

```text
MenuScreen.Use: can't provide '[Slot] int slot' for Click on Button 'play' (nothing provides it here).
Available here: MouseButton, EventModifiers, [ClickCount] int, Vector2, [PanelPosition] Vector2, EventBase,
InteractionContext, Button (or a base type), the screen, a data source class (e.g. the view model).
```

Other binding errors:

| Mistake | Message (abridged) |
|---|---|
| No element with the attribute's name | `MenuScreen.Play: no element named 'play' in the layout.` |
| A UXML name with no method | `click="Nope" on WeaveButton 'apply': no method by that name on SettingsViewModel or SettingsScreen.` (`change="..."` and `drop="..."` are named the same way) |
| A UXML name with overloads | `SettingsScreen.Apply: 2 overloads; UXML handlers need a single method.` |
| A request for the wrong element type | `... can't provide 'Toggle t' ... (the element is a Button, not a Toggle) ...` |
| Two providers in one scope | `... (it is ambiguous: 2 Project providers match) ...` |
| A grid attribute on another element | `Button 'buy' isn't a grid: cell and drag handlers need a WeaveGrid.` |
| `[OnValueChanged]` on an element without a value | `VisualElement 'card' has no value to change (it isn't an INotifyValueChanged<T>).` |
| `[OnLongPress]` or `[OnValueChanged]` on a grid | `Grid 'bag': LongPress isn't supported on grid cells (use click, right-click, hover or the drag events).` |
| A non-void return, more than five parameters, `ref`/`out` | `handlers must return void.`, `handlers take at most 5 parameters ...`, `ref/out parameters can't be provided.` |

## Allocation

Handlers are bound once, with reflection, when the screen is built. Each one gets a typed delegate and one provider
per parameter. When an interaction happens, the binding rents a pooled `InteractionContext`, fills in the raw facts
from the event and calls the delegate.

- A handler whose parameters are values (enums, `int`, `Vector2`, the element, the screen, the view model, the
  item) allocates nothing per interaction. The package's tests check this for 100 clicks through a five-parameter
  handler that also requests the view model, and for 100 grid cell clicks.
- The data source lookup only runs for handlers that request a data-source class.
- Your handler's own code can still allocate (string formatting, LINQ). A fluent lambda's closure is created once,
  when you call `OnClick`, not per click.
- This was verified on Mono and on IL2CPP for Windows x64. See [Known limitations](known-limitations.md) for other
  platforms.

## Fluent handlers

The fluent methods bind a lambda or method to one element and return the element, so they chain in the builder:

```csharp
Button("Play").OnClick(StartGame).Class("w-full");
Button("Inspect").OnClick((MouseButton button, EventModifiers modifiers) => Inspect(button, modifiers));
slot.OnRightClick((Vector2 position) => ShowMenu(position));
card.OnHover(phase => card.EnableInClassList("card--hot", phase == HoverPhase.Enter));
volume.OnValueChanged((float value) => audio.SetVolume(value));
```

| Method | Handler parameters | Options |
|---|---|---|
| `OnClick` | 0 to 3 | `button` (default `LeftMouse`), `clicks` (default 1) |
| `OnRightClick` | 0 to 2 | |
| `OnLongPress` | 0 to 2 | `milliseconds` (default 500) |
| `OnHover` | `HoverPhase`, plus 0 or 1 more | |
| `OnValueChanged` | 1 to 3 | |

- Give lambda parameters explicit types (`(MouseButton b) => ...`); they decide what is injected. `OnHover`'s first
  parameter is always `HoverPhase`.
- Fluent requests can't carry qualifiers, so `[Slot]`, `[ClickCount]` and the like aren't available. Request
  `InteractionContext` and read `Slot`, `ClickCount` and so on from it.
- Fluent handlers have no screen: a `WeaveScreen` request gives `null`. Capture the screen in the lambda instead.
- An impossible request throws when the fluent method is called.
- `element.FluentBindings()` lists an element's fluent handlers; `element.ClearInteractions()` removes them all.

## Adding your own values

`ReturnRequest.Register` adds a provider that any handler can request:

```csharp
public static void Register<T>(Func<InteractionContext, T> provider, Type qualifier = null, Type appliesTo = null,
    InteractionKind kinds = InteractionKind.All)
```

| Parameter | Description |
|---|---|
| `provider` | Computes the value from the `InteractionContext`. Runs each time a handler that requests it is called, so keep it cheap and allocation-free. |
| `qualifier` | A `RequestQualifierAttribute` subclass that tells this value apart from other values of type `T`. Any other type throws `ArgumentException`. |
| `appliesTo` | Limits the provider to elements of this type (and derived types), such as `typeof(WeaveGrid)`. |
| `kinds` | Limits it to some interaction kinds, such as `InteractionKind.Pointer` or `InteractionKind.Drag`. |

Register before any screen is built, because requests are resolved when handlers are bound. A
`RuntimeInitializeOnLoadMethod` works well. Project providers are cleared when Play mode starts (also without a domain
reload), so register them in every session, at `BeforeSceneLoad` or later. Registering the same type, qualifier and
`appliesTo` again replaces the earlier provider, so it is safe to run twice.

```csharp
using UnityEngine;
using Nerdova.WeaveUI;

/// <summary>Requests the local player: void Use(Item item, [LocalPlayer] Character who).</summary>
public sealed class LocalPlayerAttribute : RequestQualifierAttribute { }

/// <summary>Requests whether Shift was held: void Buy(Item item, [Bulk] bool all).</summary>
public sealed class BulkAttribute : RequestQualifierAttribute { }

static class GameRequests
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        ReturnRequest.Register(ctx => Party.LocalPlayer, typeof(LocalPlayerAttribute));
        ReturnRequest.Register(ctx => (ctx.Modifiers & EventModifiers.Shift) != 0, typeof(BulkAttribute),
            kinds: InteractionKind.Pointer);
    }
}
```

```csharp
[OnCellClick("bag")]
void Use(Item item, [LocalPlayer] Character who, [Bulk] bool all) => who.Use(item, all);
```

`Party.LocalPlayer` and `Character` stand for your own game code.

Project providers rank below control and grid values and above the common values. An unqualified provider for a type
the package already provides (such as `Vector2` or `MouseButton`) replaces the built-in value for every handler, so
prefer a qualifier. `ReturnRequest.Available(site)` lists what a handler could request at a `BindingSite`; the
error messages use it.

## Testing handlers

Handlers can be run without real input, which is useful in edit-mode tests and for input the binding doesn't listen
to (number keys for a hotbar, for example).

| Member | Description |
|---|---|
| `WeaveScreen.InteractionBindings` | The screen's attribute and UXML handler bindings. |
| `element.FluentBindings()` | An element's fluent handler bindings. |
| `InteractionBinding.Element`, `.Screen`, `.Spec` | What a binding is bound to. `Spec.Kind` is its `InteractionKind`. |
| `InteractionBinding.Simulate(InteractionKind? kind = null)` | Runs the handler as if the interaction happened: left button (right for a right-click), one click, no modifiers, no event. Defaults to the binding's kind (`HoverEnter` for a hover handler). A value-change handler gets the element's current value, and the type's default for `[Previous]`. |
| `WeaveGrid.Simulate(InteractionKind kind, int slot)` | Runs the grid's handlers for `kind` on `slot`, with that slot's item. Drag kinds run without a drag: `GridDrag` is null, `DropOutcome` is `Reject` and `[Source] int` is -1. |
| `WeaveGrid.DropTo(int sourceSlot, WeaveGrid target, int targetSlot)` | Drops an item as a pointer drag would: validation, the data change and the drop handlers. Returns the `DropOutcome`. |

`ScreenManager` doesn't need a panel, so a test can push a screen onto one created with a plain `VisualElement`:

```csharp
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;

public class MainMenuTests
{
    [Test]
    public void PlayStartsTheGame()
    {
        var screens = new ScreenManager(new VisualElement());
        var viewModel = new MainMenuViewModel();
        var screen = screens.Push(new MainMenuScreen(viewModel));

        screen.InteractionBindings.First(b => b.Element.name == "play").Simulate();

        Assert.That(viewModel.Status, Is.EqualTo("Started once."));
    }
}
```

`WeaveGrid.Simulate` passes no drag: a drag handler gets a null `GridDrag`, `DropOutcome.Reject`, a `[Source] int`
of -1 and a null `[Source]` item. Use `DropTo` to test real drops.

## See also

- [Data binding](data-binding.md): view models and data sources, which handlers can request.
- [Grid](grid.md): cells, selection, drag modes and drop rules.
- [Controls](controls.md): the `weave:` controls and their attributes.
- [C# builder](builder.md): chaining fluent handlers.
- [Shop + Inventory sample](../Samples/ShopInventory), [Settings Menu sample](../Samples/SettingsMenu),
  [Game HUD sample](../Samples/GameHud).

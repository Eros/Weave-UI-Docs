# Screens

A Weave UI interface is a stack of screens: full-size pages of UI, each with its own layout and, usually, a view
model. This page covers `ScreenHost`, which puts the stack on screen, the screen classes and their lifecycle,
`ScreenManager` navigation and Back, transitions and modal dialogs.

## ScreenHost

`ScreenHost` is a MonoBehaviour that sets up everything a screen needs. Add it to a GameObject, or call
`ScreenHost.Create()`, then push screens onto `host.Screens`.

```csharp
using UnityEngine;
using Nerdova.WeaveUI;

[RequireComponent(typeof(ScreenHost))]
public sealed class GameUi : MonoBehaviour
{
    void Start()
    {
        var host = GetComponent<ScreenHost>();
        host.Screens.DefaultTransition = ScreenTransition.Fade;
        host.Screens.Push(new MainMenuScreen(new MainMenuViewModel()));
    }
}
```

`ScreenHost.Create(string name = "Weave UI")` creates a GameObject with a host on it, for when you'd rather not set up
the scene. The host is ready as soon as `Create` returns:

```csharp
var host = ScreenHost.Create();
host.Screens.Push(new MainMenuScreen(new MainMenuViewModel()));
```

When it wakes, the host:

1. Uses the `UIDocument` on its GameObject (it requires one, so Unity adds it).
2. Creates panel settings if the document has none (see below).
3. Adds a full-size container to the document with two layers: the screen stack, and above it the `Overlay` layer for
   tooltips, toasts and drag visuals, which ignores the pointer.
4. Creates the `ScreenManager` (`Screens`), a `ThemeService` (`Theme`), a `BreakpointController` (`Breakpoints`) and
   a `TooltipController` (`Tooltips`).

The container (`Container`, with the `weave-root` class) carries the theme, skin and breakpoint classes, so they apply
to every screen. If the `UIDocument` replaces its root, for example when it is disabled and enabled again, the host
moves the container to the new root. Destroying the host clears the screen stack.

### Panel settings and theme

If you assign panel settings to the `UIDocument`, the host uses them. Otherwise it creates its own with
`ScreenHost.CreateDefaultPanelSettings()`:

| Setting | Value |
|---|---|
| Theme | `ScreenHost.LoadTheme()` |
| Scale mode | Scale With Screen Size |
| Reference resolution | 1920 x 1080 |
| Screen match mode | Match Width Or Height, match 0.5 |

`LoadTheme()` returns the project theme written by the [Tailwind pipeline](tailwind-pipeline.md) when there is one
(`Resources/WeaveUI/WeaveProjectTheme`), otherwise the package's default theme (`Resources/WeaveUI/WeaveDefaultTheme`):
Unity's runtime theme plus the default design tokens, utility classes and control styles. Panel settings the host
creates are destroyed with it.

Your own panel settings need a theme that includes Weave UI's styles: assign the package's `WeaveDefaultTheme` theme
file to them, or set the theme from code:

```csharp
panelSettings.themeStyleSheet = ScreenHost.LoadTheme();
```

### Theme, skin and tooltips

`host.Theme` switches light and dark mode, palettes and skins for every screen; `host.SetSkin(skin)` is a shortcut for
`host.Theme.Skin = skin`. See [Theming](theming.md) and [Skins](skins.md).

```csharp
host.Theme.Mode = ThemeMode.Light;
```

`host.Tooltips` shows each element's `tooltip` attribute in the overlay layer. See [Controls](controls.md).

### Breakpoints

USS has no media queries, so the host's `BreakpointController` adds `bp-sm`, `bp-md`, `bp-lg`, `bp-xl` and `bp-2xl`
classes to the container by its width, with min-width semantics: at 1100 wide, `bp-sm`, `bp-md` and `bp-lg` are all
on. Responsive variants such as `md:p-4` match through these classes; see [Styling](styling.md).

| Breakpoint | Minimum width |
|---|---|
| `sm` | 640 |
| `md` | 768 |
| `lg` | 1024 |
| `xl` | 1280 |
| `2xl` | 1536 |

Widths are in panel units, so with Scale With Screen Size they follow the reference resolution rather than the
device's pixels. To use other breakpoints, assign a design tokens asset to the host's **Tokens** field; its breakpoint
list replaces the defaults.

To react to breakpoints in code, read `Current` (the widest active breakpoint name, such as `"md"`, or null below the
smallest) or subscribe to `Changed`:

```csharp
host.Breakpoints.Changed += breakpoint => hud.Compact = breakpoint == null || breakpoint == "sm";
```

## Screens

Derive from one of these base classes:

| Class | Use it for |
|---|---|
| `Screen<TViewModel>` | A screen with a view model. The view model becomes the root's `dataSource`, so UXML bindings resolve against it, and its handler methods are bound too. |
| `WeaveScreen` | A screen without a view model. `Screen<TViewModel>` derives from it. |
| `ModalScreen`, `ModalScreen<TViewModel>` | A dialog over the current screen. See [Modal screens](#modal-screens). |

### Layout from UXML

`[ScreenLayout]` names the UXML file to clone, as a path under any `Resources` folder without the extension:

```csharp
[ScreenLayout("WeaveUISamples/SettingsMenu/Settings")]   // Resources/WeaveUISamples/SettingsMenu/Settings.uxml
public sealed class SettingsScreen : Screen<SettingsViewModel>
{
    public SettingsScreen(SettingsViewModel viewModel) : base(viewModel) { }
}
```

The layout is loaded when the screen is first pushed. A screen with neither `[ScreenLayout]` nor a `CreateRoot`
override, or whose UXML isn't found, throws an `InvalidOperationException` naming the problem.

In Play mode, saving a screen's UXML rebuilds every running screen that uses it in place, keeping its position in the
stack, its visibility and its view model. The rebuilt screen gets `OnCreated()` again, and `OnShow()` if it is visible.
The new root replaces the old one only once its handlers are bound: if binding fails, for example on a mistyped
`click=` name, the error is logged and the old root stays.

### Layout from code

Override `CreateRoot()` to build the root another way, for example with the [C# builder](builder.md). The
[Builder + Themes sample](../Samples/BuilderThemes) builds its whole screen this way:

```csharp
using UnityEngine.UIElements;
using Nerdova.WeaveUI;
using static Nerdova.WeaveUI.Ui;

public sealed class CharacterSheetScreen : Screen<CharacterViewModel>
{
    public CharacterSheetScreen(CharacterViewModel viewModel) : base(viewModel) { }

    protected override VisualElement CreateRoot() =>
        El("sheet").Class("sheet flex-1 items-center justify-center").Children(
            Panel("card").Children(
                Title("Ada Thornfield"),
                Text().Bind(nameof(CharacterViewModel.Subtitle))));
}
```

Screens built in code aren't rebuilt when a UXML file changes.

### What a screen does when it is built

The first time a screen is pushed, it:

1. Calls `CreateRoot()` and stretches the root over the screen stack (absolute position, all edges at 0). The root
   gets the `weave-screen` class, and `modal` too for a modal screen.
2. Adds the alias classes that `dark:`, `md:` and similar variants match through, and attaches the runtime layout
   for `gap-*` and `grid` utilities.
3. Sets the root's `dataSource` to the view model (`Screen<TViewModel>` only).
4. Binds input handlers: `[OnClick]` and the other attributes on the screen and its view model, then UXML handler
   names such as `click="Apply"`. See [Interaction](interaction.md).
5. Calls `OnCreated()`.

Elements you add after this, in `OnCreated` or later, don't get steps 2 and 4 automatically. See
[Styling](styling.md) and [Interaction](interaction.md) for adding them yourself.

### Lifecycle

Override these hooks to respond as a screen moves through the stack. They are all optional.

| Hook | Called |
|---|---|
| `OnCreated()` | Once, after the root is built and bound, before the first show. Query elements and hook events here. |
| `OnShow()` | Each time the screen becomes visible: pushed, or uncovered when the screen above it is removed. |
| `OnHide()` | When a non-modal screen covers this one, or before it is removed while visible. |
| `OnClosed()` | When the screen is removed from its stack, after `OnHide()`. |
| `OnBack()` | When Back or cancel is pressed while this screen is on top. Return true if you handled it. |

The order for common operations, with screen A under screen B:

| Operation | Calls |
|---|---|
| Push A onto an empty stack | `A.OnCreated`, `A.OnShow` |
| Push B over A | `A.OnHide`, `B.OnCreated`, `B.OnShow` |
| Pop B | `B.OnHide`, `B.OnClosed`, `A.OnShow` |
| Replace B with C | `B.OnHide`, `B.OnClosed`, `C.OnCreated`, `C.OnShow` (A stays hidden) |
| Push a modal M over A | `M.OnCreated`, `M.OnShow` (A stays visible, with no `OnHide`) |
| Pop M | `M.OnHide`, `M.OnClosed` (A was never hidden, so no `OnShow`) |

Hidden screens keep their elements and state; they are only set to `display: none`. A screen that was removed can be
pushed again: it keeps its root and gets `OnShow`, not another `OnCreated`. A screen can be on one stack at a time;
pushing it while it is on a stack throws an `InvalidOperationException`.

The hooks run immediately and don't wait for transitions: when `OnHide` runs, the screen may still be animating out.

The [Settings Menu sample](../Samples/SettingsMenu) uses three of the hooks:

```csharp
protected override void OnCreated()
{
    Root.Q("bindings").Children(ViewModel.Bindings, BindingRow);
    Root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
}

protected override void OnShow() => Root.Q<Slider>("master").Focus();

/// <summary>Escape cancels a rebind instead of leaving the screen.</summary>
protected override bool OnBack()
{
    if (_listening == null)
        return false;

    StopListening();
    return true;
}
```

Subscribe to view model events in `OnShow` and unsubscribe in `OnHide`, as the
[Builder + Themes sample](../Samples/BuilderThemes) does. Pairing `OnCreated` with `OnClosed` breaks when you push the
screen again after closing it, because `OnCreated` doesn't run again. Refresh anything the events drive in `OnShow`
too, since changes made while the screen was hidden weren't seen:

```csharp
protected override void OnShow()
{
    ViewModel.propertyChanged += OnChanged;
    UpdateSegments();
}

protected override void OnHide() => ViewModel.propertyChanged -= OnChanged;
```

### Closing a screen

`Close()` removes the screen from its stack wherever it is, and shows the screens under it as needed. Inside a screen,
`Manager` is the `ScreenManager` showing it, so `Manager.Push(...)` opens the next screen:

```csharp
[OnClick("settings")]
void OpenSettings() => Manager.Push(new SettingsScreen(ViewModel));

[OnClick("back")]
void Back() => Close();
```

## ScreenManager

`ScreenManager` is the stack. Only the top screen is displayed, plus the screens under it while it is modal. Use the
host's (`host.Screens`), or create one on any element with `new ScreenManager(container)`.

| Method | What it does |
|---|---|
| `Push(screen)` | Covers the current screen with `screen` and returns it. A modal screen leaves the current screen displayed. |
| `Pop()` | Removes the top screen and shows the one under it. Returns the removed screen, or null if the stack was empty. |
| `Replace(screen)` | Swaps the top screen for `screen` without showing the one under it. Use it for flows such as title screen to game. |
| `PopTo(screen)` | Removes screens until `screen` is on top, or until it leaves the stack (a closing screen may remove it). Does nothing if `screen` isn't on the stack. |
| `Remove(screen)` | Removes `screen` wherever it is. Returns false if it isn't on the stack. `WeaveScreen.Close()` calls this. |
| `Clear()` | Removes every screen. |
| `Back()` | Asks the top screen's `OnBack()`, then pops it if it didn't handle Back, unless it is the last screen. Returns true if something handled it. |

```csharp
var menu = host.Screens.Push(new MainMenuScreen(menuViewModel));
host.Screens.Push(new SettingsScreen(settingsViewModel));

host.Screens.PopTo(menu);                           // back to the main menu
host.Screens.Replace(new GameScreen(gameViewModel)); // the main menu is closed, not hidden
```

`Current` is the top screen (null when empty), `Count` the number of screens and `Stack` the screens from bottom to
top.

### Back and cancel

The manager listens for UI Toolkit's `NavigationCancelEvent`, which the Escape key and the gamepad's cancel button (B
on an Xbox controller) send, and calls `Back()`:

1. The top screen's `OnBack()` runs first. Return true to keep the screen, for example to close an inner panel or
   cancel a key rebind.
2. Otherwise the top screen is popped, unless it is the only screen on the stack. The bottom screen is never popped by
   Back, so pressing Escape on the main menu does nothing unless its `OnBack()` acts.

When `Back()` handles the event, it stops the event immediately, so with two managers on one panel only one goes
back per press. Call `host.Screens.Back()` yourself to wire Back
to another input, such as an on-screen back button.

## Transitions

Screens can fade, slide or scale in and out. Choose a transition for every screen with `DefaultTransition`, or for one
screen by overriding `Transition`:

```csharp
host.Screens.DefaultTransition = ScreenTransition.Fade;   // the default is None
```

```csharp
public sealed class InventoryScreen : Screen<InventoryViewModel>
{
    public InventoryScreen(InventoryViewModel viewModel) : base(viewModel) { }

    public override ScreenTransition? Transition => ScreenTransition.SlideUp;
}
```

A screen's `Transition` wins; null, the default, uses the manager's `DefaultTransition`. `ModalScreen` and
`ModalScreen<TViewModel>` return `Scale`. `ReduceMotion` on the manager overrides both (see
[Reduced motion](#reduced-motion)). The property is read each time the screen appears or disappears, so it can
depend on settings.

| `ScreenTransition` | USS class | Effect |
|---|---|---|
| `None` | (none) | Appears and disappears immediately |
| `Fade` | `fade-in` | Fades |
| `SlideUp` | `slide-up` | Fades and moves 24px up into place |
| `SlideDown` | `slide-down` | Fades and moves 24px down into place |
| `SlideLeft` | `slide-left` | Fades and moves 48px left into place |
| `SlideRight` | `slide-right` | Fades and moves 48px right into place |
| `Scale` | `scale-in` | Fades and grows from 94% |

### How transitions work

The presets are USS transitions in the package's `transitions.uss`, timed by the `--duration-200` and `--ease-out`
design tokens. Each preset class sets `transition-property: opacity, translate, scale` and has two states:

- **Entering:** the element gets the preset class and `weave-enter`, which puts it in the preset's "before" state
  with no animation. A frame later `weave-enter` is removed, and USS animates the element into place.
- **Leaving:** the element gets `weave-leave`, which animates it back to the "before" state. When the transition ends,
  the screen is hidden or removed. If no transition event arrives, this happens after one second at the latest.

A leaving element ignores the pointer, and clicks and submits on its children are blocked, so a closing screen's
buttons can't fire again. A removed screen also drops focus. Pushing a screen that is still leaving cancels the leave.

### Transitions on any element

`WeaveTransitions` plays the same presets on any element, such as a panel you show and hide inside a screen:

```csharp
// Show
details.style.display = DisplayStyle.Flex;
WeaveTransitions.Enter(details, ScreenTransition.SlideLeft);

// Hide, then take it out of the layout when the animation ends
WeaveTransitions.Leave(details, ScreenTransition.SlideLeft, () => details.style.display = DisplayStyle.None);
```

Calling `Enter` while a `Leave` is in progress cancels the leave, and its callback doesn't run. With
`ScreenTransition.None`, or on an element that isn't on a panel, both happen immediately.

| Member | Description |
|---|---|
| `Enter(element, transition)` | Animates the element in. |
| `Leave(element, transition, done)` | Animates the element out, then calls `done`. |
| `IsLeaving(element)` | Whether the element is animating out. |
| `Complete(element)` | Finishes a leave in progress now and runs its callback. |
| `ClassFor(transition)` | The preset class for a transition, or null for `None`. |
| `EnterClass`, `LeaveClass` | `"weave-enter"` and `"weave-leave"`. |
| `FallbackMilliseconds` | 1000: the longest a leave waits for its transition to end. |

For hover and press motion on buttons and cards, `transitions.uss` also has `hover-lift`, `hover-grow` and
`press-shrink`. See [Styling](styling.md).

### Reduced motion

Set `ReduceMotion` on the manager to turn every screen transition off, including modals' scale and any screen's own
`Transition`. Screens then appear and disappear immediately. The [Settings Menu sample](../Samples/SettingsMenu) binds
it to its Reduce motion toggle:

```csharp
host.Screens.DefaultTransition = ScreenTransition.Fade;
host.Screens.ReduceMotion = settings.ReduceMotion;
```

`ReduceMotion` only covers screen transitions. Motion utilities such as `hover-lift` and `press-shrink`, and your own
`WeaveTransitions.Enter`/`Leave` calls, aren't affected.

## Modal screens

A modal screen is a dialog over the current screen. The screens under it stay displayed but can't be used until it
closes. Derive from `ModalScreen`, or from `ModalScreen<TViewModel>` for a dialog with a view model.

The modal's root is the backdrop: it covers the whole stack, gets the `modal` class (which centres its content on a
translucent `--weave-backdrop` background) and blocks the pointer from the screens below. Put the dialog itself in an
element with the `modal__panel` class. The built-in dialog styles are:

| Class | Use |
|---|---|
| `modal__panel` | The dialog's surface: padding, border, rounded corners, 360px minimum width, 80% maximum width |
| `modal__title` | A large bold heading |
| `modal__body` | Muted, wrapping body text |
| `modal__actions` | A right-aligned row of buttons |

This dialog from the [Settings Menu sample](../Samples/SettingsMenu) asks before restoring defaults:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:weave="Nerdova.WeaveUI">
    <ui:VisualElement class="modal__panel" style="width: 460px;">
        <ui:Label name="title" class="modal__title" />
        <ui:Label name="body" class="modal__body" />
        <ui:VisualElement class="modal__actions">
            <weave:Button name="cancel" text="Cancel" variant="ghost" click="Cancel" />
            <weave:Button name="confirm" variant="danger" click="Confirm" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

```csharp
using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;

[ScreenLayout("WeaveUISamples/SettingsMenu/Confirm")]
public sealed class ConfirmDialog : ModalScreen
{
    readonly string _title;
    readonly string _body;
    readonly string _confirm;
    readonly Action _onConfirm;

    public ConfirmDialog(string title, string body, string confirm, Action onConfirm) =>
        (_title, _body, _confirm, _onConfirm) = (title, body, confirm, onConfirm);

    protected override void OnCreated()
    {
        Root.Q<Label>("title").text = _title;
        Root.Q<Label>("body").text = _body;
        Root.Q<Button>("confirm").text = _confirm;
    }

    [Preserve]
    void Confirm()
    {
        Close();
        _onConfirm?.Invoke();
    }

    [Preserve]
    void Cancel() => Close();
}
```

Open it like any other screen:

```csharp
Manager.Push(new ConfirmDialog(
    "Restore defaults?",
    "Every setting on every tab goes back to its default.",
    "Restore defaults",
    ViewModel.RestoreDefaults));
```

The [Shop + Inventory sample](../Samples/ShopInventory) uses `ModalScreen<Item>`, so the dialog's bindings read the
item being bought.

### Closing a modal

A modal closes when:

- Its code calls `Close()`.
- The player clicks the backdrop, anywhere outside the `modal__panel` element. Turn this off for dialogs that need an
  explicit answer:

  ```csharp
  protected override bool CloseOnBackdropClick => false;
  ```

- The player presses Escape or the gamepad's cancel button, unless the modal's `OnBack()` returns true or the modal
  is the only screen on the stack.

Closing a modal doesn't call `OnShow` on the screen under it, because that screen was never hidden.

### Focus

When a modal is pushed, focus moves to its first focusable, enabled element, unless its `OnShow` already focused an
element inside it. Either order the dialog's elements with the default choice first (both sample dialogs put Cancel
first), or call `Focus()` on the default choice in `OnShow`. While the modal is on top, keyboard or gamepad
navigation that moves focus to a screen under it sends focus back to the modal's first focusable element.

### Stacking

You can push a modal over a modal; every screen down to the first non-modal one stays displayed. Pushing a non-modal
screen over a modal hides both the modal and the screens under it, and popping that screen shows them again.

Any screen can be modal: override `IsModal` to return true. `ModalScreen` and `ModalScreen<TViewModel>` do this and
also default `Transition` to `Scale`.

## Reference

### ScreenHost

| Member | Description |
|---|---|
| `static Create(string name = "Weave UI")` | Creates a GameObject with a `ScreenHost` (and a `UIDocument`) and returns the host. |
| `Document` | The `UIDocument` the host draws into. |
| `Screens` | The `ScreenManager` for the screen stack. |
| `Theme` | The `ThemeService`: light/dark mode, palette and skin for every screen. |
| `Skin` | The active skin, or null for plain utility styling. Same as `Theme.Skin`. |
| `SetSkin(UISkin skin)` | Applies a skin to every screen; null returns to utility styling. |
| `RefreshSkin()` | Re-applies the current skin's stylesheet after it was regenerated in Play mode. |
| `Breakpoints` | The `BreakpointController` that sets `bp-*` classes on `Container`. |
| `Tooltips` | The `TooltipController` that shows `tooltip` attributes in `Overlay`. |
| `Container` | The element holding the screen stack and the overlay. Carries the `weave-root`, theme, skin and breakpoint classes. |
| `Overlay` | A layer above every screen for tooltips, toasts and drag visuals. It ignores the pointer. |
| `static LoadTheme()` | The project theme if the Tailwind pipeline has written one, otherwise the package default theme. |
| `static CreateDefaultPanelSettings()` | New panel settings using `LoadTheme()`, scaled to 1920 x 1080. Each call creates a new instance. |
| `DefaultThemePath` | `"WeaveUI/WeaveDefaultTheme"`: the Resources path of the package theme. |
| `ProjectThemePath` | `"WeaveUI/WeaveProjectTheme"`: the Resources path of the theme the Tailwind pipeline writes. |
| **Tokens** (Inspector) | Optional design tokens asset whose breakpoints replace the defaults. |

### WeaveScreen

`Screen<TViewModel>`, `ModalScreen` and `ModalScreen<TViewModel>` have all of these members.

| Member | Description |
|---|---|
| `Root` | The screen's root element. Null until the screen is first pushed. |
| `Manager` | The `ScreenManager` showing the screen, or null when it isn't on a stack. |
| `IsVisible` | Whether the screen is displayed. |
| `IsModal` | Virtual. Whether the screen is a dialog over the screens under it. False by default; true for `ModalScreen`. |
| `Transition` | Virtual `ScreenTransition?`. How the screen appears and disappears; null uses `ScreenManager.DefaultTransition`. Ignored while `ScreenManager.ReduceMotion` is true. |
| `InteractionBindings` | The handlers bound from attributes and UXML handler names. |
| `Close()` | Removes the screen from its stack, wherever it is. |
| `CreateRoot()` | Protected, virtual. Builds the root. The default clones the UXML named by `[ScreenLayout]`. |
| `OnCreated()` | Protected, virtual. Called once, after the root is built and before the first show. |
| `OnShow()` | Protected, virtual. Called each time the screen becomes visible. |
| `OnHide()` | Protected, virtual. Called when another screen covers this one, or before it is removed. |
| `OnClosed()` | Protected, virtual. Called when the screen is removed from its stack. |
| `OnBack()` | Protected, virtual. Called for Back or cancel while the screen is on top. Return true if handled; otherwise the manager pops it. |
| `CloseOnBackdropClick` | Protected, virtual. For a modal: whether a click outside `.modal__panel` closes it. True by default. |

| `Screen<TViewModel>` member | Description |
|---|---|
| `ViewModel` | The view model passed to the constructor; also the root's `dataSource`. |
| `Screen(TViewModel viewModel)` | Protected constructor. Throws `ArgumentNullException` for a null view model. |

| Attribute | Description |
|---|---|
| `[ScreenLayout(string resourcesPath)]` | On a screen class: the UXML to clone, as a path under a `Resources` folder without the extension. |

### ScreenManager

| Member | Description |
|---|---|
| `ScreenManager(VisualElement container)` | Creates a stack whose screens are added to `container`. |
| `Container` | The element screens are added to. |
| `Current` | The top screen, or null when the stack is empty. |
| `Count` | The number of screens on the stack. |
| `Stack` | The screens, bottom first. |
| `DefaultTransition` | The transition for screens whose `Transition` is null. `ScreenTransition.None` by default. |
| `ReduceMotion` | While true, every screen appears and disappears without a transition, whatever its `Transition` (modals included). False by default. |
| `Push<T>(T screen)` | Covers the current screen with `screen` and returns it. Throws if `screen` is null or already on a stack. |
| `Pop()` | Removes the top screen and shows the one under it. Returns the removed screen, or null if the stack was empty. |
| `Replace<T>(T screen)` | Swaps the top screen for `screen` without showing the one under it. Returns `screen`. |
| `PopTo(WeaveScreen screen)` | Pops screens until `screen` is on top. Does nothing if it isn't on the stack. |
| `Remove(WeaveScreen screen)` | Removes `screen` wherever it is in the stack. Returns false if it isn't on it. |
| `Clear()` | Removes every screen. |
| `Back()` | The top screen's `OnBack()` first; otherwise pops the top screen unless it is the last. Returns true if handled. |

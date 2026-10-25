# Getting started

Weave UI is a UI Toolkit package for Unity 6 runtime UI. It gives you screens bound to view models, Tailwind-named
utility classes on design tokens, a themed controls library, art skins, an inventory grid and an interaction system
that hands each handler the values it asks for.

This page covers requirements and installation, then builds a first screen end to end: a view model, a UXML layout, a
screen class with a click handler, and a bootstrap script that puts it on screen.

## Requirements

- Unity 6000.3 or newer. See [Known limitations](known-limitations.md) for why earlier Unity 6 releases aren't
  supported.
- UI Toolkit runtime UI (the built-in `com.unity.modules.uielements` module). No other packages are needed.

## Installing

Weave UI will be available from the Unity Asset Store; installation instructions will follow once it is approved.
Once the Weave UI package (`dev.nerdova.weaveui`) is in your project:

- Its runtime types are in the `Nerdova.WeaveUI` namespace (`using Nerdova.WeaveUI;`).
- If your scripts are in an assembly definition, add a reference to the `Nerdova.WeaveUI` assembly.
- In UXML, declare the controls namespace with `xmlns:weave="Nerdova.WeaveUI"` to use `<weave:Button>` and the other
  controls.

## Your first screen

A Weave screen has three parts:

- **A view model** holds the data and is the data source for the layout's bindings.
- **A UXML layout** under a `Resources` folder, styled with utility classes.
- **A screen class** that names the layout, receives the view model and handles input.

A `ScreenHost` component then creates the UI document, panel settings and theme, and shows the screen.

### 1. The view model

Derive from `ViewModel`, mark each bindable property with `[CreateProperty]` and set backing fields through
`SetProperty`, which notifies bindings only when the value changes.

```csharp
using Unity.Properties;
using Nerdova.WeaveUI;

public sealed class MainMenuViewModel : ViewModel
{
    string _status = "Press Play to start.";
    int _plays;

    [CreateProperty]
    public string Title => "My Game";

    [CreateProperty]
    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public void Play()
    {
        _plays++;
        Status = _plays == 1 ? "Started once." : $"Started {_plays} times.";
    }
}
```

[Data binding](data-binding.md) covers computed properties, `Notify` and two-way bindings.

### 2. The layout

Save this as `Assets/UI/Resources/MyGame/MainMenu.uxml`. Any folder named `Resources` works; the screen refers to the
file by its path inside that folder, without the extension.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:weave="Nerdova.WeaveUI">
    <ui:VisualElement class="flex-1 items-center justify-center bg-slate-950">
        <ui:VisualElement class="panel w-96 gap-2">
            <ui:Label class="title text-center">
                <Bindings>
                    <ui:DataBinding property="text" data-source-path="Title" binding-mode="ToTarget" />
                </Bindings>
            </ui:Label>
            <ui:Label class="text-sm text-slate-400 text-center mb-4">
                <Bindings>
                    <ui:DataBinding property="text" data-source-path="Status" binding-mode="ToTarget" />
                </Bindings>
            </ui:Label>
            <weave:Button name="play" text="Play" variant="primary" size="lg" />
            <weave:Button name="quit" text="Quit" variant="ghost" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

- `flex-1`, `items-center`, `bg-slate-950` and the other utility classes ship with the package; `gap-2` is laid out
  at runtime because USS has no `gap`. See [Styling](styling.md).
- `panel` and `title` are built-in control styles, and `<weave:Button>` takes a `variant` and a `size`. See
  [Controls](controls.md).
- The bindings have no data source of their own: the screen sets its view model as the layout's data source.

### 3. The screen

`[ScreenLayout]` names the UXML to clone. Deriving from `Screen<TViewModel>` makes the view model the root's data
source, and `[OnClick("play")]` binds the method to every element named `play` in the layout.

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Nerdova.WeaveUI;

[ScreenLayout("MyGame/MainMenu")]   // Resources/MyGame/MainMenu.uxml
public sealed class MainMenuScreen : Screen<MainMenuViewModel>
{
    public MainMenuScreen(MainMenuViewModel viewModel) : base(viewModel) { }

    // Give keyboard and gamepad users a starting point each time the screen appears.
    protected override void OnShow() => Root.Q<Button>("play").Focus();

    [OnClick("play")]
    void Play() => ViewModel.Play();

    [OnClick("quit")]
    void Quit() => Application.Quit();
}
```

Handlers can be private. They can also ask for values and Weave UI injects them:
`void Play(EventModifiers modifiers)` receives the modifier keys held during the click, and
`void Play(Vector2 position)` the pointer position inside the button.

If a handler names an element the layout doesn't have, or asks for a value that can't be provided, the screen throws
when it is built rather than on the first click. You can also name the method in UXML instead of using an attribute
(`<weave:Button text="Play" click="Play" />`); mark such methods `[Preserve]`, because only the UXML refers to them.
See [Interaction](interaction.md).

### 4. The bootstrap script

`ScreenHost` owns the screen stack. Add it as a component, or create it from code.

**As a component.** Create an empty GameObject and add this script. `[RequireComponent]` adds a `ScreenHost`, which
in turn adds a `UIDocument`.

```csharp
using UnityEngine;
using Nerdova.WeaveUI;

[RequireComponent(typeof(ScreenHost))]
public sealed class MainMenuBootstrap : MonoBehaviour
{
    void Start()
    {
        var host = GetComponent<ScreenHost>();
        host.Screens.DefaultTransition = ScreenTransition.Fade;
        host.Screens.Push(new MainMenuScreen(new MainMenuViewModel()));
    }
}
```

**From code.** `ScreenHost.Create()` makes a GameObject named "Weave UI" with a host on it. Use it when no scene
setup is wanted:

```csharp
using UnityEngine;
using Nerdova.WeaveUI;

public sealed class GameUi : MonoBehaviour
{
    void Start()
    {
        var host = ScreenHost.Create();
        host.Screens.Push(new MainMenuScreen(new MainMenuViewModel()));
    }
}
```

Either way, the host leaves panel settings you have assigned to the `UIDocument` alone. When there are none, it
creates them with the Weave UI theme, scaled to a 1920 x 1080 reference resolution. See
[Screens](screens.md#screenhost).

### 5. Press Play

The panel appears in the middle of the Game view with the Play button focused. Clicking Play, or pressing Enter or
the gamepad's submit button while it has focus, updates the status line through the binding.

While the game is running, edit and save `MainMenu.uxml`: the screen is rebuilt in place with the same view model,
so layout changes show without leaving Play mode.

## Where next

- [Screens](screens.md): more screens, Back, transitions and modal dialogs.
- [Data binding](data-binding.md): view models, properties and bindings in UXML and C#.
- [Interaction](interaction.md): handler attributes, injected parameters and UXML handler names.
- [Styling](styling.md): utility classes, variants, design tokens and breakpoints.
- [Controls](controls.md): the `weave:` controls and tooltips.
- [Grid](grid.md): inventories, hotbars and shops with drag and drop.
- [C# builder](builder.md): building UI in code with `Ui.*`.
- [Theming](theming.md) and [Skins](skins.md): light and dark, palettes, and art skins at runtime.
- [Tailwind pipeline](tailwind-pipeline.md): the optional Tailwind CSS pipeline for breakpoints, arbitrary values
  and the full Tailwind class set.
- [Editor tools](editor-tools.md) and [Known limitations](known-limitations.md).

The samples show complete screens:

- [Game HUD sample](../Samples/GameHud): bound bars, toasts and a hotbar grid.
- [Settings Menu sample](../Samples/SettingsMenu): two-way binding, Apply and Revert, key rebinding and a modal.
- [Shop + Inventory sample](../Samples/ShopInventory): drag to buy and sell, multi-slot items and a confirmation
  modal.
- [Builder + Themes sample](../Samples/BuilderThemes): a screen written in C#, restyled in several themes.

Weave UI's Basic Menu sample, imported from the Package Manager, puts most features in one small game menu.

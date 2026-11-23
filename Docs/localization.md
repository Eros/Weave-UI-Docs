# Localization and right-to-left

Weave UI lays screens out right to left for Arabic, Hebrew, Persian, Urdu and other right-to-left languages, and works
with Unity's Localization package for translated text.

## Right-to-left layout

Set the direction on a screen host's theme:

```csharp
host.Theme.Direction = TextDirection.RightToLeft;   // dir-rtl on the screen container
host.Theme.Direction = TextDirection.LeftToRight;   // dir-ltr, the default
```

With the Localization package installed, a `ScreenHost` sets the direction from the selected locale for you; see
[Following the locale](#following-the-locale).

Under `dir-rtl`:

- The logical utility classes swap sides: `ps-*`, `pe-*`, `ms-*`, `me-*`, `start-*`, `end-*`, `text-start`,
  `text-end`, `rounded-s-*`, `rounded-e-*`, `rounded-ss-*`, `rounded-se-*`, `rounded-es-*`, `rounded-ee-*`,
  `border-s-*` and `border-e-*`.
- `flex-row` runs right to left, as CSS does for `dir="rtl"`, and so does the builder's `Row()`.
- The controls mirror. Fields put their label on the right, and field labels, text inputs, titles, dialogs and tooltips
  align right.

### Logical and physical classes

Use the logical classes where a side should follow the language, and the physical ones where it shouldn't:

| Logical (follows the direction) | Physical (never moves) |
|---|---|
| `ps-4`, `pe-4` (padding start and end) | `pl-4`, `pr-4` |
| `ms-2`, `me-2` (margin start and end) | `ml-2`, `mr-2` |
| `start-0`, `end-0` | `left-0`, `right-0` |
| `text-start`, `text-end` | `text-left`, `text-right` |
| `rounded-s-lg`, `rounded-e-lg` | `rounded-l-lg`, `rounded-r-lg` |
| `border-s-2`, `border-e-2` | `border-l-2`, `border-r-2` |

Plain labels keep Unity's left alignment, so add `text-start` to the ones that should follow the language.

With the [Tailwind pipeline](tailwind-pipeline.md), logical CSS properties mirror the same way, and the `rtl:` and
`ltr:` variants style one direction only:

```xml
<ui:VisualElement class="ps-4 rtl:bg-amber-900 ltr:bg-slate-900" />
```

### Arabic letters and mixed text

Arabic, Persian and Urdu letters only join, and mixed-direction text only orders correctly, with Unity's advanced text
generator:

1. Turn on *Project Settings → UI Toolkit → Advanced Text Generator*.
2. Add a rule to a palette or project stylesheet:

   ```css
   .dir-rtl, .dir-rtl .unity-text-element { -unity-text-generator: advanced; }
   ```

Weave UI doesn't set this itself, because the property logs an error while the project setting is off.

### A language's direction

`WeaveDirection.For(string languageCode)` returns a language's direction from a code such as `ar`, `he-IL`, `ur_PK`
or `ku-Arab-IQ`. It works without the Localization package:

```csharp
host.Theme.Direction = WeaveDirection.For(playerLanguage);
```

It uses a fixed list of right-to-left languages rather than `CultureInfo`, which IL2CPP builds with invariant
globalization report as left to right.

## Localized text

These need Unity's Localization package, version 1.5 or newer. Weave UI detects it and compiles them only when it is
installed.

### In UXML

Use the Localization package's own binding on any Weave control:

```xml
<weave:Button name="play">
    <Bindings>
        <UnityEngine.Localization.LocalizedString property="text" table="UI" entry="play" />
    </Bindings>
</weave:Button>
```

### In C#

With `using static Nerdova.WeaveUI.Ui;`:

```csharp
LocalizedTitle(new LocalizedString("UI", "shop-title"));
LocalizedButton(new LocalizedString("UI", "buy")).OnClick(Buy);      // named after the entry key: "buy"
Toggle("Music").Localize("UI", "music");                              // a field's label
IconButton(gear).Localize("UI", "settings-tip", "tooltip");           // any property
```

`.Localize(...)` binds a localized string to an element's text: a label's or button's `text`, or a field's `label`.
Pass a property name to localize something else, such as `"tooltip"`. An element with no text throws unless you pass
one. The text updates when the locale or the string's arguments change.

| Member | Description |
|---|---|
| `Localize(LocalizedString text, string property = null)` | Binds `text` to the element's text, or to `property` |
| `Localize(TableReference table, TableEntryReference entry, string property = null)` | The same, for a table and entry |
| `Ui.LocalizedText(LocalizedString text)` | A label |
| `Ui.LocalizedTitle(LocalizedString text)` | A heading, with the `title` class |
| `Ui.LocalizedButton(LocalizedString text, string name = null)` | A button, named after the entry key unless you pass a name |

The builder uses separate `Localized*` names rather than overloads of `Text` and `Button`, so code that doesn't use
localization never needs a reference to the Localization package.

Dropdown choices and tab labels aren't localized by Weave UI; bind them yourself.

## Following the locale

A `ScreenHost` sets `Theme.Direction` from the selected locale, now and whenever it changes. Turn this off to set the
direction yourself:

```csharp
host.FollowLocaleDirection = false;
```

Outside a `ScreenHost`, follow the locale with your own `ThemeService`:

```csharp
IDisposable follow = WeaveLocalization.FollowLocale(theme);
// follow.Dispose() to stop.
```

`WeaveLocalization.DirectionOf(Locale locale)` gives one locale's direction.

## Limitations

- **Direction is set for a whole host.** A `dir-ltr` class lower down doesn't undo `dir-rtl` for its subtree. To keep a
  row left to right, set its direction inline (`style.flexDirection = FlexDirection.Row`), which isn't mirrored.
- **Inline styles don't mirror,** such as `style.flexDirection` or `style.paddingLeft`.
- **Not mirrored:** grid layouts (`grid-cols-*`), `WeaveGrid` slot order, slider fill direction, and dropdown menus.

See [Known limitations](known-limitations.md#right-to-left) for the full list.

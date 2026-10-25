# Grid

`WeaveGrid` (`<weave:Grid>` in UXML) is a virtualised grid of slots for inventories, hotbars, shops, stashes and level
select screens. This page covers its layout attributes, binding items to it, cell templates, selection and keyboard or
gamepad navigation, multi-cell items with `OccupancyMap`, drag and drop, the handlers you can bind to a grid, and the
USS classes it uses.

The [Shop + inventory sample](../Samples/ShopInventory) and the [Game HUD sample](../Samples/GameHud) are the worked
examples used throughout.

## When to use it

USS has no CSS grid, so `WeaveGrid` lays its cells out itself. Use it when:

- Every slot has the same size and is a place an item can be (a bag, a hotbar, a vendor's stock, a 1,000-slot stash).
- Players click, select, navigate between or drag items between slots.
- Items can cover several slots (a 1 × 3 sword in a Diablo-style bag).

For a static layout of different-sized children, use the `grid grid-cols-N` utility classes instead (see
[Styling](styling.md)). The `weave-grid` class is the control's own class and isn't related to the `grid` utility.

## A first grid

The Game HUD's hotbar is a plain grid: eight slots, bound to a list on the view model, with a cell template.

```xml
<weave:Grid name="hotbar" class="hotbar" columns="8" slot-count="8" cell-size="64" gap="8" drag="default"
            style="width: 568px; height: 64px;">
    <Bindings>
        <ui:DataBinding property="ItemsSource" data-source-path="Hotbar" binding-mode="ToTarget" />
    </Bindings>
</weave:Grid>
```

```csharp
[CreateProperty]
public List<HotbarSlot> Hotbar { get; } = new()
{
    new HotbarSlot("Ashen Blade", "sword", 0.8f),
    new HotbarSlot("Kite Shield", "shield", 6f),
    null,                                           // an empty slot
    // ...
};
```

The screen sets the template and handles clicks:

```csharp
protected override void OnCreated()
{
    _hotbar = Root.Q<WeaveGrid>("hotbar");
    _hotbar.CellTemplate = Resources.Load<VisualTreeAsset>("WeaveUISamples/GameHud/HotbarSlot");
    _hotbar.BindCell = BindSlot;
}

[OnCellClick("hotbar")]
void Use([Slot] int slot) => ViewModel.Use(slot);
```

The grid scrolls inside its own box, so give it a width and height. A grid that shows every slot without scrolling is
`columns × cell-size + (columns − 1) × gap` wide: 8 × 64 + 7 × 8 = 568 for the hotbar.

## UXML attributes

| Attribute | Values | Description |
|---|---|---|
| `mode` | `Columns` (default), `AutoFit`, `Horizontal` | How slots are arranged. See [Layout modes](#layout-modes). |
| `columns` | int, default `4`, minimum 1 | Columns in `Columns` mode. |
| `rows` | int, default `1`, minimum 1 | Rows in `Horizontal` mode. |
| `cell-size` | float, default `64` | Cell width in panel units; the minimum width in `AutoFit` mode. |
| `cell-height` | float, default `0` | Cell height. `0` keeps cells square. |
| `gap` | float, default `4` | Space between cells. |
| `gap-y` | float, default `-1` | Vertical space between cells. Negative uses `gap`. |
| `slot-count` | int, default `0` | Shows at least this many slots, even when the items list is shorter. The extra slots are empty. |
| `cell-template` | a UXML asset | Instantiated for each cell; its data source is the slot's item. |
| `selection` | `None`, `Single` (default), `Multiple` | Unity's `SelectionType`. See [Selection](#selection-cursor-and-navigation). |
| `drag` | `none` (default), `default`, `manual` | Whether items can be dragged from and dropped onto this grid. See [Drag and drop](#drag-and-drop). |
| `click` | method name | A click handler, bound like `[OnCellClick]`. |
| `drop` | method name | A drop handler, bound like `[OnDrop]`. |
| `variant` | free text | Adds `weave-grid--<variant>`. |
| `size` | `sm`, `md`, `lg` | Adds `weave-grid--sm` or `weave-grid--lg`. |
| `skin` | a registered skin name | Skins only this grid. See [Skins](skins.md). |

Each attribute is also a C# property: `Mode`, `Columns`, `Rows`, `CellSize`, `CellHeight`, `Gap`, `GapY`,
`SlotCountMinimum`, `CellTemplate`, `Selection`, `DragMode`, `ClickHandler`, `DropHandler`, `Variant`, `Size` and
`Skin`.

Dragging is off unless you set `drag`. The stash in Weave UI's Basic Menu sample (imported from the Package Manager)
leaves it out, so its items can be clicked but not moved.

### Layout modes

| `GridMode` | Layout | Scrolls |
|---|---|---|
| `Columns` | A fixed number of columns. Rows grow with the slot count. | Vertically |
| `AutoFit` | As many columns as fit the grid's width at `cell-size`. Cells stretch to fill the row. | Vertically |
| `Horizontal` | A fixed number of rows. Slots fill column by column and columns grow. | Horizontally |

In occupancy mode (see [Multi-cell items](#multi-cell-items-occupancy-maps)) the map's width sets the columns, and
`mode`, `columns` and `rows` are ignored.

## Items

### ItemsSource

`ItemsSource` is an `IList` with one entry per slot, and `null` for an empty slot. It is a bindable property, so bind
it from UXML as above, or from C#:

```csharp
grid.ItemsSource = viewModel.Bag;
grid.Bind(nameof(InventoryViewModel.Bag));      // fluent builder: a grid's main property is ItemsSource
```

`SlotCount` is the longer of the list and `slot-count`. The vendor in the Shop sample binds a nine-item list to a
three-column grid:

```xml
<weave:Grid name="vendor" columns="3" cell-size="128" cell-height="164" gap="12" drag="manual"
            style="width: 408px; height: 516px;">
    <Bindings>
        <ui:DataBinding property="ItemsSource" data-source-path="Stock" binding-mode="ToTarget" />
    </Bindings>
</weave:Grid>
```

The grid refreshes when:

- `ItemsSource` is replaced.
- The list implements `INotifyCollectionChanged` (for example `ObservableCollection<T>`) and raises `CollectionChanged`.
  Replacing a single item rebinds only its slot.
- A drop completes on it.
- You call `Refresh()` (every visible cell) or `RefreshSlot(slot)` (one slot).

A plain `List<T>` doesn't notify, so after changing it in code, call `Refresh()` or `RefreshSlot(slot)`. Property
changes on an item that notifies (a `ViewModel`) update its cell through the template's bindings.

`SetItem(slot, item)` writes into `ItemsSource`, growing a resizable list with `null` slots if needed. It throws if the
grid has no `ItemsSource`, or if the slot is past the end of a fixed-size list such as an array.

### Cell templates

Each cell holds one content element. The grid makes it, in order of preference, from:

1. `MakeCell`, a `Func<VisualElement>` you supply.
2. `CellTemplate`, a `VisualTreeAsset`.
3. An empty `VisualElement`.

The cell's data source is the slot's item, so the template binds to the item's properties. This is the Shop sample's
vendor card, `StockCard.uxml`:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <!-- One vendor slot. Its data source is the slot's Item. -->
    <ui:VisualElement class="stock-card">
        <Bindings>
            <ui:DataBinding property="tooltip" data-source-path="Description" binding-mode="ToTarget" />
        </Bindings>
        <ui:VisualElement class="stock-card__icon" picking-mode="Ignore">
            <Bindings>
                <ui:DataBinding property="style.backgroundImage" data-source-path="IconBackground" binding-mode="ToTarget" />
            </Bindings>
        </ui:VisualElement>
        <ui:Label class="stock-card__name" picking-mode="Ignore">
            <Bindings>
                <ui:DataBinding property="text" data-source-path="Name" binding-mode="ToTarget" />
            </Bindings>
        </ui:Label>
    </ui:VisualElement>
</ui:UXML>
```

Set `CellTemplate` in `OnCreated`, before the grid is shown. Changing it later discards every cell and rebuilds them.
`cell-template` in UXML takes the same asset as a UXML asset reference.

Without `BindCell`, a cell creates its content the first time it shows an item and hides it while the slot is empty.
Content elements get the `grid-cell__content` class and fill the cell. The grid applies class aliases and layout
utilities to new content, so `dark:`, `md:` and `gap-*` classes in a template work.

### BindCell and UnbindCell

`BindCell` is an `Action<VisualElement, object, int>`: the content element, the item (or `null`) and the slot. When you
set it, every cell has content, including empty ones, and the grid calls `BindCell` instead of hiding the content. Use
it for anything a binding can't do, or for per-slot data that isn't on the item. The hotbar shows each slot's key
number, even on empty slots:

```csharp
static void BindSlot(VisualElement content, object item, int slot)
{
    content.Q<Label>("key").text = (slot + 1).ToString();
    content.Q("item").style.display = item == null ? DisplayStyle.None : DisplayStyle.Flex;
}
```

`UnbindCell` has the same signature. The grid calls it before a cell that showed an item is reused for another slot or
item, so you can unhook anything `BindCell` hooked up.

With `MakeCell` and `BindCell` you can skip UXML altogether:

```csharp
var stash = Grid(10, cellSize: 52, gap: 4, name: "stash").Bind(nameof(InventoryViewModel.Stash));
stash.Mode = GridMode.AutoFit;
stash.MakeCell = () => new Label();
stash.BindCell = (content, item, slot) => ((Label)content).text = item?.ToString() ?? "";
```

`Grid(...)` and `.Bind(...)` are from the [fluent builder](builder.md).

## Virtualisation

Only the cells in view exist, plus one row (or column, in `Horizontal` mode) either side. Cells are pooled: when the
grid scrolls, cells that leave the view are rebound to the slots coming into view. A 1,000-slot stash creates a few
dozen cells.

- `ActiveCellCount` is the number of cells currently created.
- `CellAt(slot)` returns the on-screen `GridCell` for a slot, or `null` if the slot is scrolled out of view.
- `ScrollToSlot(slot)` scrolls until a slot is fully visible.

Because cells are reused, don't keep references to cells or their content between frames, and don't store per-slot
state on a cell. Keep it on the item, or set it in `BindCell`.

In occupancy mode the slots are virtualised but placed items are not: every placed item has its own element.

## Selection, cursor and navigation

### Selection

`selection` decides what a click does to the selection:

| `SelectionType` | Behaviour |
|---|---|
| `None` | Clicks don't select. Setting it clears the selection. |
| `Single` | A click selects one slot. |
| `Multiple` | A click selects one slot. Ctrl (Cmd on macOS) and click toggles a slot. Shift and click selects the range from the last clicked slot. |

Selected cells get `grid-cell--selected`. In code:

```csharp
grid.Select(0);                                   // as a click would
grid.Select(5, EventModifiers.Control);           // toggle, in Multiple mode
grid.ClearSelection();
grid.SelectionChanged += () => Debug.Log(grid.SelectedSlot);
```

- `SelectedSlot` is the most recently selected slot, or -1.
- `SelectedSlots` holds every selected slot.

Selection is stored by slot index, so it stays on the slot when items move. The Game HUD selects the first hotbar slot
when it appears:

```csharp
protected override void OnShow() => _hotbar.Select(0);
```

### Cursor and keyboard or gamepad navigation

The grid is focusable. The cursor is the slot keyboard and gamepad input acts on:

- When the grid gains focus without a cursor, the cursor moves to the selected slot, or to slot 0.
- Arrow keys and the gamepad's navigation move the cursor one slot and scroll it into view.
- At the edge of the grid, navigation isn't consumed, so focus moves on to the next control.
- Submit selects the cursor's slot and runs the grid's click handlers, as a left click would.
- A pointer click moves the cursor to the clicked slot.

The cursor's cell has `grid-cell--cursor`. The default styles outline it only while the grid has focus (the grid gets
`weave-grid--focused`). Set `Cursor` to move it from code; it is clamped to the slot range (also when the slot count shrinks), and -1 means
no cursor.

To give a screen keyboard and gamepad input from the start, focus a grid when the screen shows:

```csharp
protected override void OnShow() => _vendor.Focus();
```

## Multi-cell items (occupancy maps)

For Tetris- or Diablo-style inventories, where an item covers W × H slots, give the grid an `OccupancyMap` instead of
an items list. The grid draws the map's `Width × Height` slots as empty cells and each placed item as one element
spanning its slots, and validates drops against the map.

```csharp
public OccupancyMap Bag { get; } = new OccupancyMap(8, 6);

// In the view model's constructor:
Bag.TryPlace(Catalog.Bow.Copy(), 0, 0, 1, 3);       // 1 wide, 3 tall, top-left at (0, 0)
Bag.TryPlace(Catalog.Shield.Copy(), 1, 0, 2, 2);
```

```xml
<weave:Grid name="bag" cell-size="56" gap="4" drag="manual" style="width: 476px; height: 356px;" />
```

```csharp
protected override void OnCreated()
{
    _bag = Root.Q<WeaveGrid>("bag");
    _bag.CellTemplate = Resources.Load<VisualTreeAsset>("WeaveUISamples/ShopInventory/BagItem");
    _bag.Occupancy = ViewModel.Bag;
    _bag.ItemSize = item => ((Item)item).Size;
}
```

`Occupancy` is a C# property; there is no UXML attribute or binding for it. Setting it replaces `ItemsSource`. The
grid redraws whenever the map changes, so you change the map, not the grid.

Coordinates are (x, y) slots from the top-left. A slot index is `y * Width + x`.

| Member | Description |
|---|---|
| `OccupancyMap(int width, int height)` | A map of `width × height` slots. Throws if either is less than 1. |
| `Width`, `Height` | The map's size in slots. |
| `TryPlace(object item, int x, int y, int width, int height)` | Places an item, or moves it if already placed, with its top-left at (x, y). Returns false and changes nothing if it doesn't fit. Throws on a null item. |
| `TryMove(object item, int x, int y)` | Moves a placed item, keeping its size. |
| `TryAdd(object item, int width, int height)` | Places an item in the first free spot, row by row. Returns false if there is none. |
| `FindFreeSpot(int width, int height, out int x, out int y)` | The first top-left slot where a W × H item fits. |
| `CanPlace(int x, int y, int width, int height, object ignore = null)` | Whether a W × H item fits at (x, y): inside the map and over empty slots, or slots of `ignore` (the item being moved). |
| `Remove(object item)` | Removes an item. Returns false if it wasn't placed. |
| `RemoveAll()` | Empties the map. |
| `ItemAt(int x, int y)` | The item covering a slot, or `null`. |
| `TryGetRect(object item, out RectInt rect)` | An item's slots. |
| `Items` | Every placed item and its `RectInt`. |
| `Changed` | Raised after any placement or removal. |

Items are keyed by reference, so place a separate instance for each copy. The Shop sample's vendor never runs out:
buying places `stock.Copy()`, not the stock item itself.

In occupancy mode:

- `ItemAt(slot)` returns the item covering a slot, and `PieceFor(item)` returns the element drawn for a placed item.
- Clicks, selection and the cursor belong to the item covering a slot, and `[Slot]` is the item's top-left slot.
- Slots covered by an item get `grid-cell--occupied`; free slots get `grid-cell--empty`.
- Item elements are `GridCell`s with the `grid-item` class instead of `grid-cell`. Their template data source is the
  item, and they stretch to the item's shape, so make the template flexible (the Shop sample's `BagItem.uxml` scales
  its icon to fit).

### ItemSize

An item dragged into an occupancy grid from a plain grid has no size of its own. `ItemSize`, a
`Func<object, Vector2Int>`, gives it one; without it, the item takes 1 × 1. Items dragged within a map, or from another
map, keep the size they were placed with.

## Drag and drop

### Drag modes

`drag` (`DragMode` in C#, a `GridDragMode`) sets whether items can be dragged from a grid and dropped onto it, and who
changes the data when they are:

| `GridDragMode` | Behaviour |
|---|---|
| `None` (default) | Nothing can be dragged from this grid or dropped onto it. |
| `Default` | Drops are validated and applied by the grid: it moves or swaps the items in `ItemsSource` or the occupancy map, then runs the drop handlers. If the item can't be placed after all, the drop becomes a `Reject` and the source's drag-cancel handlers run instead. |
| `Manual` | Drops are validated in the same way, but the grid doesn't change any data. Your drop handlers apply the drop, and both grids refresh afterwards. |

Use `Default` when a drop only rearranges items (a hotbar, a bag). Use `Manual` when a drop means something in your
game: buying, selling, equipping, crafting. The Shop sample uses `manual` on both grids because dragging from the
vendor to the bag is a purchase.

A drag starts when the pointer is pressed on an item and moves 6 panel units. A ghost of the cell follows the pointer.
Releasing over a valid slot drops the item. Releasing anywhere else, onto a rejected slot, pressing Escape, or the grid
leaving the panel cancels the drag. One pointer drags at a time: a second finger is ignored while the first is pressed
or dragging. Drags work between any two grids on the same panel.

### Outcomes

While dragging, the grid under the pointer works out what a drop there would do, a `DropOutcome`, and works it out
again on release, so changes to the data during the drag count:

| `DropOutcome` | Meaning |
|---|---|
| `Reject` | Not allowed. Releasing here cancels the drag. |
| `Move` | The item moves to an empty slot (or, in occupancy mode, to free slots). |
| `Swap` | The two items trade slots. |
| `Merge` | A game rule such as stacking. The grid never applies it; your drop handler does. |

The default outcome:

| Drop | Outcome |
|---|---|
| Onto a grid with `drag="none"`, outside the slots, or back onto its own slot | `Reject` |
| Plain grid: onto an empty slot past the end of a fixed-size list such as an array | `Reject` |
| Plain grid: onto an empty slot | `Move` |
| Plain grid: onto an occupied slot | `Swap`, or `Reject` if the item came from an occupancy map |
| Occupancy grid: the item's whole footprint fits at the target | `Move` |
| Occupancy grid: it doesn't fit, or runs off the map | `Reject` |

In occupancy mode the target slot is where the item's top-left would go, taking into account which of its slots the
pointer grabbed. The target slots are highlighted with `grid-cell--drop-target`, or `grid-cell--invalid` for
`Reject`.

### DropRule

`DropRule`, a `Func<GridDrag, DropOutcome>` on the target grid, overrides the default outcome. The default is already
in `drag.Outcome`, so return it to keep it. The Shop sample uses two rules:

```csharp
// The vendor only accepts items from the bag (selling them).
_vendor.DropRule = drag => drag.Source == _bag ? DropOutcome.Merge : DropOutcome.Reject;

// The bag refuses vendor items the player can't afford; otherwise the default (does it fit?) stands.
_bag.DropRule = drag => drag.Source == _vendor && !ViewModel.CanAfford((Item)drag.SourceItem) ? DropOutcome.Reject : drag.Outcome;
```

For stacking, return `Merge` when two items can combine and apply it in the drop handler. Here and below,
`InventoryItem` stands for your own item class:

```csharp
grid.DropRule = drag =>
    drag.SourceItem is InventoryItem dragged && dragged.CanStackWith(drag.TargetItem as InventoryItem)
        ? DropOutcome.Merge
        : drag.Outcome;
```

The rule runs only while the pointer is over a slot of a grid whose `drag` isn't `none`. Whatever it returns, the drop
is rejected if the dragged item is no longer in its source slot (sold, moved or replaced during the drag).

### GridDrag

A `GridDrag` describes one drag. Drop rules and drag handlers receive it.

| Member | Description |
|---|---|
| `Source` | The grid the item was dragged from. |
| `SourceSlot` | The item's slot (its top-left slot in occupancy mode). |
| `SourceItem` | The dragged item. |
| `Size` | The item's size in slots: from its occupancy map, or the target map's `ItemSize`, or 1 × 1. |
| `Grab` | Which of the item's slots the pointer grabbed, relative to its top-left. |
| `Target` | The grid under the pointer, or `null`. |
| `TargetSlot` | The target slot (the top-left slot in occupancy mode), or -1. |
| `TargetItem` | The item in the target slot now, or `null` if it is empty. Always `null` on an occupancy grid. |
| `Outcome` | What a drop would do. Settable. |
| `Position` | The pointer, in panel coordinates. |

`ActiveDrag` on a grid is the drag in progress from it, or `null`. `CancelDrag()` cancels it.

### Applying drops in Manual mode

In `Manual` mode the drop handler makes the change. The Shop sample's bag buys from the vendor or moves within the bag:

```csharp
/// <summary>A drop on the bag: buy from the vendor, or move within the bag. The slot is the item's new top-left.</summary>
[OnDrop("bag")]
void DroppedInBag(GridDrag drag, [Slot] int slot)
{
    var item = (Item)drag.SourceItem;
    var (x, y) = (slot % ViewModel.Bag.Width, slot / ViewModel.Bag.Width);
    if (drag.Source == _vendor)
    {
        ViewModel.Buy(item, x, y);
    }
    else
    {
        ViewModel.Move(item, x, y);
    }
}

[OnDrop("vendor")]
void DroppedOnVendor([Source] Item item) => ViewModel.Sell(item);

/// <summary>A rejected drop (no room, or not enough gold) snaps back; say why.</summary>
[OnDragCancel("bag")]
[OnDragCancel("vendor")]
void Cancelled(Item item) => ViewModel.ExplainRejected(item);
```

In `Default` mode, handlers run after the grid has moved or swapped the items, so they are for feedback, or for
applying a `Merge`:

```csharp
[OnDrop("bag")]
void Dropped([Source] InventoryItem dragged, InventoryItem onto, GridDrag drag)
{
    if (drag.Outcome != DropOutcome.Merge)
        return;

    var moved = Mathf.Min(dragged.Count, onto.Kind.MaxStack - onto.Count);
    onto.Count += moved;
    dragged.Count -= moved;
    if (dragged.Count == 0)
        drag.Source.SetItem(drag.SourceSlot, null);
}
```

### Cross-grid drags

A drop between two grids follows the target's `drag` mode and `DropRule`. The source only needs `drag` set to
something other than `none` so its items can be picked up. In `Default` mode:

- Plain grid to plain grid: `Move` empties the source slot; `Swap` puts the target's item in the source slot.
- Into an occupancy map: the item is removed from the source (its list slot set to `null`, or removed from its map) and
  placed in the target map.
- From an occupancy map onto an occupied plain slot is rejected, because the other item might not fit the map.

After a drop both grids refresh.

### Drops without a pointer

`DropTo(int sourceSlot, WeaveGrid target, int targetSlot)` drops a slot's item onto a slot of another grid (or the
same one) as a pointer drag would: it evaluates the outcome and `DropRule`, changes the data in `Default` mode, and
runs the drop handlers. It returns the outcome, and does nothing on `Reject`. Use it for gamepad pick-up and put-down,
or for tests:

```csharp
int _held = -1;

/// <summary>Gamepad: the first submit picks an item up, the second puts it down.</summary>
[OnCellClick("bag")]
void PickOrPlace(WeaveGrid grid, [Slot] int slot)
{
    if (_held < 0)
    {
        if (grid.ItemAt(slot) != null)
            _held = slot;
        return;
    }

    grid.DropTo(_held, grid, slot);
    _held = -1;
}
```

## Handlers

Bind handlers to a grid by its name, as for any element (see [Interaction](interaction.md)). They fire per cell, and
can request values about the slot. Attribute handlers go on the screen or its view model.

| Attribute | Fires | Unqualified item is |
|---|---|---|
| `[OnCellClick("grid")]` | A click on a cell, or submit on the cursor. Takes `Button` and `Clicks` like `[OnClick]`. Fails at registration if the element isn't a grid. | The clicked slot's item |
| `[OnClick("grid")]`, `[OnDoubleClick("grid")]`, `[OnRightClick("grid")]` | As on any element, per cell. A right click fires when the button is pressed and released on the same cell. | The slot's item |
| `[OnHover("grid")]` | The pointer enters or leaves a cell; the exit also fires when the hovered cell is recycled. Request `HoverPhase`. | The slot's item |
| `[OnDragStart("grid")]` | A drag leaves a cell of this grid (the source). | The dragged item |
| `[OnDragOver("grid")]` | A drag moves onto another slot of this grid (the target). | The target slot's item |
| `[OnDrop("grid")]` | An item is dropped on this grid (the target). | The target slot's item before the drop |
| `[OnDragCancel("grid")]` | A drag from this grid (the source) ends without a drop. | The dragged item |

`[OnLongPress]` and `[OnValueChanged]` can't be bound to a grid; registration fails with an
`InteractionBindingException`.

A single-click handler (`Clicks = 1`, the default) also fires on the second click of a double-click, so a cell with
both handlers runs the single-click one twice and then the double-click one. The Shop sample uses this: a click selects
an item, and a double-click selects and buys it.

```csharp
[OnCellClick("vendor")]
void SelectStock(Item item) => ViewModel.Select(item, fromStock: true);

[OnCellClick("vendor", Clicks = 2)]
void QuickBuy(Item item) => Buy(item);
```

### Values a grid handler can request

| Parameter | Value |
|---|---|
| `[Slot] int` | The slot. For drag handlers, the target slot (drop, drag over) or the dragged item's slot (drag start, cancel). |
| `[Row] int`, `[Column] int` | The slot's row and column. |
| `GridCell` | The cell element, or `null` if it is scrolled out of view. In occupancy mode, the item's element. |
| Your item class, e.g. `Item item` | The slot's item, as in the table above, or `null`. |
| `WeaveGrid` | The grid the handler is bound to. |
| `GridDrag` | Drag handlers: the drag. |
| `DropOutcome` | Drag handlers: the outcome. |
| `[Source] int` | Drag handlers: the dragged item's slot in its source grid. |
| `[Source] WeaveGrid` | Drag handlers: the source grid. |
| `[Source] Item` (any item class) | Drag handlers: the dragged item. |

The values [Interaction](interaction.md) lists for any element work too, where the event has them: `MouseButton` and
`[ClickCount] int` for clicks, `EventModifiers`, the screen, the view model, `InteractionContext`. A class parameter
that isn't the slot's item falls back to the nearest data source of that type, so `ShopViewModel vm` gets the view
model.

```csharp
[OnDrop("bag")]
void Dropped([Source] Item dragged, [Source] int from, Item replaced, [Slot] int to, DropOutcome outcome) { ... }

[OnRightClick("bag")]
void Inspect(InventoryItem item, [Row] int row, [Column] int column) { ... }
```

### UXML handlers

`click` and `drop` name a method on the screen's view model or the screen, with the same parameter injection. Mark the
method `[Preserve]`, because only the UXML refers to it.

```xml
<weave:Grid name="bag" columns="8" cell-size="64" gap="6" drag="default" click="UseItem" drop="ItemDropped" />
```

```csharp
[Preserve]
void UseItem(Item item, [Slot] int slot) { ... }

[Preserve]
void ItemDropped([Source] Item dragged, DropOutcome outcome) { ... }
```

`click` takes left clicks of any count and submit; use the attributes for double-clicks and the other events.

### Simulating interactions

`Simulate(InteractionKind kind, int slot)` runs the handlers bound to a grid as if `kind` happened on a slot, with one
click of the left button (the right button for `RightClick`). Use it for tests, or to route input the grid doesn't
listen to itself, such as number keys on a hotbar. Drag kinds run without a drag: handlers get a null `GridDrag`,
`DropOutcome.Reject` and a `[Source] int` of -1, so use `DropTo` to test real drops.

```csharp
_hotbar.Simulate(InteractionKind.Click, 2);     // as if slot 3 was clicked
```

## Styling

Cells are `GridCell` elements, positioned absolutely. Each cell's state is a BEM modifier class:

| Class | On |
|---|---|
| `weave-grid` | The grid. `weave-grid--focused` while it has focus, `weave-grid--horizontal` in `Horizontal` mode, plus `--<variant>` and `--<size>` modifiers. |
| `weave-grid__scroll`, `weave-grid__content` | The grid's scroll view and its content. |
| `grid-cell` | Each slot. |
| `grid-cell--empty` | A slot with no item. |
| `grid-cell--occupied` | Occupancy mode: a slot covered by an item. |
| `grid-cell--hover` | The slot under the pointer. |
| `grid-cell--selected` | A selected slot. |
| `grid-cell--cursor` | The keyboard or gamepad cursor. |
| `grid-cell--drop-target` | Slots a drag would drop into. |
| `grid-cell--invalid` | Slots where a drop would be rejected. |
| `grid-cell--dragging` | The slot a drag started from. |
| `grid-cell__content` | The content element (your template) inside a cell. |
| `grid-item` | Occupancy mode: a placed item. Takes `grid-item--selected`, `--cursor`, `--hover` and `--dragging`. |
| `grid-ghost` | The copy of the cell that follows the pointer while dragging. |

`GridCell.ClassFor(CellState state, bool item = false)` returns a state's class name, and `cell.Has(CellState.Selected)`
checks one.

The default styles use Weave's semantic variables:

```css
.grid-cell {
    border-width: 2px;
    border-color: var(--weave-border);
    border-radius: var(--radius-md);
    background-color: var(--weave-surface-raised);
}

.grid-cell--empty { background-color: var(--weave-surface); }
.grid-cell--selected { border-color: var(--weave-primary); background-color: var(--weave-selection); }
.weave-grid--focused .grid-cell--cursor { border-color: var(--weave-focus); }
.grid-cell--drop-target { border-color: var(--weave-primary); background-color: var(--weave-selection); }
.grid-cell--invalid { border-color: var(--weave-danger); }
.grid-cell--dragging { opacity: 0.35; }
.grid-ghost { opacity: 0.85; }
```

To recolour one grid, override the variables on it instead of restyling each class. The Game HUD's hotbar does this:

```css
.hotbar {
    --weave-surface: rgba(15, 23, 42, 0.85);
    --weave-surface-raised: rgba(30, 41, 59, 0.9);
    --weave-border: rgba(148, 163, 184, 0.28);
    --weave-control-hover: var(--color-amber-200);
    --weave-primary: var(--color-amber-300);
    --weave-selection: rgba(251, 191, 36, 0.16);
}
```

The ghost is added to the screen host's overlay layer when there is one, so it draws above every screen and picks up
the active skin. A skin can replace the slot, empty slot, selected slot, cursor, drop target, invalid drop and
multi-cell item art; see [Skins](skins.md).

## Reference

### WeaveGrid

| Member | Description |
|---|---|
| `Mode`, `Columns`, `Rows`, `CellSize`, `CellHeight`, `Gap`, `GapY`, `SlotCountMinimum` | Layout. See [UXML attributes](#uxml-attributes). |
| `IList ItemsSource` | One item per slot, `null` for empty. Bindable. |
| `OccupancyMap Occupancy` | Multi-cell mode. Replaces `ItemsSource`. |
| `VisualTreeAsset CellTemplate` | The cell content template. |
| `Func<VisualElement> MakeCell` | Creates cell content instead of `CellTemplate`. |
| `Action<VisualElement, object, int> BindCell` | Fills a cell's content for a slot: content, item (or `null`), slot. |
| `Action<VisualElement, object, int> UnbindCell` | Called before a cell is reused for another slot. |
| `int SlotCount` | The map's size in occupancy mode, else the longer of `ItemsSource` and `SlotCountMinimum`. |
| `GridMetrics Metrics` | The current layout geometry. |
| `object ItemAt(int slot)` | The item in a slot (in occupancy mode, the item covering it), or `null`. |
| `GridCell CellAt(int slot)` | The on-screen cell for a slot, or `null`. |
| `GridCell PieceFor(object item)` | Occupancy mode: the element drawn for a placed item. |
| `int ActiveCellCount` | Cells currently created. |
| `void Refresh()` | Re-measures and rebinds every visible cell. |
| `void RefreshSlot(int slot)` | Rebinds one slot. |
| `void ScrollToSlot(int slot)` | Scrolls a slot fully into view. |
| `void SetItem(int slot, object item)` | Writes into `ItemsSource`, growing a resizable list if needed. |
| `SelectionType Selection` | `None`, `Single` or `Multiple`. |
| `IReadOnlyCollection<int> SelectedSlots` | The selected slots. |
| `int SelectedSlot` | The most recently selected slot, or -1. |
| `event Action SelectionChanged` | Raised when the selection changes. |
| `void Select(int slot, EventModifiers modifiers = EventModifiers.None)` | Selects a slot as a click would. |
| `void ClearSelection()` | Clears the selection. |
| `int Cursor` | The keyboard and gamepad cursor slot, or -1. |
| `GridDragMode DragMode` | `None`, `Default` or `Manual`. |
| `Func<GridDrag, DropOutcome> DropRule` | Overrides the default outcome of drops onto this grid. |
| `Func<object, Vector2Int> ItemSize` | The size of items dropped into this grid's map from a plain grid. |
| `GridDrag ActiveDrag` | The drag in progress from this grid, or `null`. |
| `DropOutcome DropTo(int sourceSlot, WeaveGrid target, int targetSlot)` | Drops an item without a pointer. |
| `void CancelDrag()` | Cancels the drag in progress from this grid. |
| `void Simulate(InteractionKind kind, int slot)` | Runs the bound handlers as if `kind` happened on a slot. |
| `string ClickHandler`, `string DropHandler` | The `click` and `drop` UXML handler names. |
| `string Variant`, `string Size`, `string Skin` | Modifier classes and per-control skin. |
| `const string BlockClass` | `"weave-grid"`. |

### GridCell

A slot (`grid-cell`) or, in occupancy mode, a placed item (`grid-item`).

| Member | Description |
|---|---|
| `WeaveGrid Grid` | The grid it belongs to. |
| `int Slot` | The slot it shows (the top-left slot for an item), or -1 while pooled. |
| `object Item` | The item in the slot, or `null`. |
| `bool IsItem` | Whether it is an occupancy-mode item rather than a slot. |
| `VisualElement Content` | The content element, or `null` until it is first needed. |
| `string Block` | `grid-cell` or `grid-item`. |
| `bool Has(CellState state)` | Whether the cell has a state's class. |
| `static string ClassFor(CellState state, bool item = false)` | A state's modifier class, e.g. `grid-cell--selected`. |
| `CellClass`, `ItemClass`, `ContentClass` | `"grid-cell"`, `"grid-item"`, `"grid-cell__content"`. |

`CellState` values: `Empty`, `Selected`, `Cursor`, `Hover`, `DropTarget`, `Invalid`, `Dragging`, `Occupied`.

### GridMetrics

`GridMetrics` is the grid's geometry as a value: where each slot is, how big the content is, and which slots a scrolled
viewport shows. It is pure maths, so you can use it to lay out something that lines up with a grid, or in tests.

| Member | Description |
|---|---|
| `static GridMetrics Create(GridMode mode, int count, float available, int columns, int rows, float cellWidth, float cellHeight, float gapX, float gapY)` | The metrics for `count` slots. `available` is the viewport width (`AutoFit` only). A `cellHeight` of 0 keeps cells square. |
| `Count`, `Columns`, `Rows`, `CellWidth`, `CellHeight`, `GapX`, `GapY` | The layout. |
| `ColumnMajor` | Slots fill column by column (`Horizontal` mode). |
| `StepX`, `StepY` | Cell size plus gap. |
| `Vector2 ContentSize` | Every slot, with gaps between them but not around them. |
| `int RowOf(int slot)`, `int ColumnOf(int slot)` | A slot's row and column. |
| `int SlotAt(int row, int column)` | The slot at a row and column, or -1. |
| `int SlotAt(Vector2 position)` | The slot under a content-space position, or -1 in a gap or outside. |
| `Rect CellRect(int slot)` | A slot's rectangle in content space. |
| `(int first, int last) VisibleRange(float offset, float viewport, int bufferLines = 1)` | The slots a viewport shows at a scroll offset, plus buffer lines. |
| `int Neighbour(int slot, int dRow, int dColumn)` | The slot one step away, or -1 at the edge. |

## Limitations

- One pointer drag at a time, and no auto-scroll near the edge of a scrolling grid.
- Placed items in an occupancy map aren't virtualised.
- A cell updates when the grid refreshes or its item notifies a property change. Changing what an item's properties
  return without notifying doesn't update its cell.

See [Known limitations](known-limitations.md) for the full list.

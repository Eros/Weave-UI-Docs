using UnityEngine;
using UnityEngine.UIElements;
using static WeaveUI.Ui;

namespace WeaveUI.Samples.GameHud
{
    [ScreenLayout("WeaveUISamples/GameHud/Hud")]
    public sealed class HudScreen : Screen<HudViewModel>
    {
        const int ToastMilliseconds = 4000;

        private WeaveGrid _hotbar;
        private VisualElement _toasts;

        public HudScreen(HudViewModel viewModel) : base(viewModel) { }

        protected override void OnCreated()
        {
            _hotbar = Root.Q<WeaveGrid>("hotbar");
            _hotbar.CellTemplate = Resources.Load<VisualTreeAsset>("WeaveUISamples/GameHud/HotbarSlot");
            _hotbar.BindCell = BindSlot;

            Root.Q("quests").Children(ViewModel.Quests, QuestRow);
            Root.Q("log").Children(ViewModel.Log, line => Text(line).Class("log-line"));

            _toasts = Root.Q("toasts");
            ViewModel.Toasted += ShowToast;
        }

        protected override void OnShow() => _hotbar.Select(0);

        protected override void OnClosed() => ViewModel.Toasted -= ShowToast;

        /// <summary>Click a slot to use it (number keys would call the same method from your input code).</summary>
        [OnCellClick("hotbar")]
        void Use([Slot] int slot) => ViewModel.Use(slot);

        /// <summary>Every slot shows its key; only filled slots show the item part, which the template binds.</summary>
        static void BindSlot(VisualElement content, object item, int slot)
        {
            content.Q<Label>("key").text = (slot + 1).ToString();
            content.Q("item").style.display = item == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        static VisualElement QuestRow(Quest quest) =>
            Row().Class("items-start mb-3").Children(
                El().Class(quest.Done ? "quest-check quest-check--done" : "quest-check"),
                Column().Class("flex-1").Children(
                    Text(quest.Title).Class(quest.Done ? "text-sm text-slate-400" : "text-base font-bold text-amber-100"),
                    Text(quest.Step).Class(quest.Done ? "text-xs text-slate-500" : "text-sm text-slate-300")));

        /// <summary>Slides a toast in, then out again after a few seconds (the movement is a USS transition).</summary>
        void ShowToast(Toast toast)
        {
            var element = Column().Class($"toast toast--{toast.Tone} toast--hidden").Children(
                Text(toast.Title).Class("toast__title"),
                Text(toast.Body).Class("toast__body"));
            _toasts.Add(element);
            element.schedule.Execute(() => element.RemoveFromClassList("toast--hidden")).StartingIn(50);
            element.schedule.Execute(() => element.AddToClassList("toast--hidden")).StartingIn(ToastMilliseconds);
            element.schedule.Execute(element.RemoveFromHierarchy).StartingIn(ToastMilliseconds + 400);
        }
    }
}

using UnityEngine;

namespace Nerdova.WeaveUI.Samples.GameHud
{
    /// <summary>
    /// The sample's only component. It puts a painted backdrop behind the screens (standing in for your game's camera),
    /// pushes the HUD, and fakes a little gameplay so the bars, gold and toasts move.
    /// </summary>
    [RequireComponent(typeof(ScreenHost))]
    public sealed class GameHudSample : MonoBehaviour
    {
        HudViewModel _hud;
        float _nextEvent;

        void Start()
        {
            var host = GetComponent<ScreenHost>();
            host.Document.rootVisualElement.Insert(0, DemoBackdrop.Create());
            _hud = new HudViewModel();
            host.Screens.Push(new HudScreen(_hud));
        }

        void Update()
        {
            _hud.Tick(Time.deltaTime);
            if (Time.time < _nextEvent)
                return;

            _nextEvent = Time.time + Random.Range(2f, 4f);
            switch (Random.Range(0, 3))
            {
                case 0:
                    _hud.TakeDamage(Random.Range(8, 22));
                    break;
                case 1:
                    _hud.GainGold(Random.Range(5, 40));
                    break;
                default:
                    _hud.GainXp(Random.Range(4, 12));
                    break;
            }
        }
    }
}

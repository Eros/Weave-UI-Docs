using UnityEngine;

namespace WeaveUI.Samples.GameHud
{
    [RequireComponent(typeof(ScreenHost))]
    public sealed class GameHudSample : MonoBehaviour
    {
        private HudViewModel _hud;
        private float _nextEvent;

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
            {
                return;
            }

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

using UnityEngine;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.GameHud
{
    public static class DemoBackdrop
    {
        public static VisualElement Create()
        {
            var backdrop = new VisualElement { name = "demo-backdrop", pickingMode = PickingMode.Ignore };
            backdrop.style.position = Position.Absolute;
            backdrop.style.left = 0;
            backdrop.style.top = 0;
            backdrop.style.right = 0;
            backdrop.style.bottom = 0;
            backdrop.style.backgroundImage = Resources.Load<Texture2D>("WeaveUISamples/GameHud/Backdrop");
            backdrop.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
            // Dimmed towards dusk, so the HUD's light text keeps its contrast over the daytime scene.
            backdrop.style.unityBackgroundImageTintColor = new Color(0.42f, 0.4f, 0.55f);
            return backdrop;
        }
    }
}

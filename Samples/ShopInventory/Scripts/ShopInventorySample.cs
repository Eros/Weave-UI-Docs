using UnityEngine;

namespace Nerdova.WeaveUI.Samples.ShopInventory
{
    /// <summary>The sample's only component: the host builds the UI document and theme; this pushes the shop.</summary>
    [RequireComponent(typeof(ScreenHost))]
    public sealed class ShopInventorySample : MonoBehaviour
    {
        void Start()
        {
            var host = GetComponent<ScreenHost>();
            host.Screens.DefaultTransition = ScreenTransition.Fade;
            host.Screens.Push(new ShopScreen(new ShopViewModel()));
        }
    }
}

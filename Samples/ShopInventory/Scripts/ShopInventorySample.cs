using UnityEngine;

namespace WeaveUI.Samples.ShopInventory
{
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

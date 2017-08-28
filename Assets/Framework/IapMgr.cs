using UnityEngine;
using UnityEngine.Purchasing;

public class IapMgr : MgrBase, IStoreListener
{
    public static IapMgr It;
    void Awake() { It = this; }

    private IStoreController m_Controller;
    private IAppleExtensions m_AppleExtensions;
    private bool m_PurchaseLock = false;

    public override void Init()
    {
        var module = StandardPurchasingModule.Instance();
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        var builder = ConfigurationBuilder.Instance(module);
        builder.AddProduct("RemoveAds", ProductType.NonConsumable, new IDs {
            {"RemoveAds", AppleAppStore.Name},
        });
        UnityPurchasing.Initialize(this, builder);
    }

    public override void UnInit()
    {

    }

    // 激活某个商品
    public bool PreparePurchase(string name)
    {
        if (m_Controller == null || m_PurchaseLock) return false;
        m_PurchaseLock = true;
        m_Controller.InitiatePurchase(name);
        return true;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP_OnInitialized]" + controller + "," + extensions);
        m_Controller = controller;
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        m_AppleExtensions.RegisterPurchaseDeferredListener(OnIAP_Deferred);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("[IAP_OnInitializeFailed]");
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                Debug.Log("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                Debug.Log("No products available for purchase!");
                break;
        }
    }

    private void OnIAP_Deferred(Product item)
    {
        Debug.Log("[OnDeferred]Purchase deferred: " + item.definition.id);
    }

    /// Called when a purchase completes.
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        SoundMgr.It.SoundPlay("gift");
        m_PurchaseLock = false;
        if (e.purchasedProduct.transactionID == "RemoveAds")
        {
            //User.It.SetNoAds(true);
        }
        return PurchaseProcessingResult.Complete;
    }

    /// This will be called is an attempted purchase fails.
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        Debug.Log("Purchase failed: " + item.definition.id + "," + r);
        m_PurchaseLock = false;
    }
}

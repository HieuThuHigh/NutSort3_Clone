#if USE_IAP
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;
using UnityEngine;
using UnityEngine.Purchasing;
using static GameToolSample.Scripts.Enum.AnalyticID;
using System.Linq;
using GameTool.API.Analytics.Analytics;
using GameTool.API.Scripts;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
#if USE_FIREBASE_IAP
using GameToolSample.Scripts.FirebaseServices;
#endif

// NEED TO INSTALL UNITY IAP

public class IAPManager : SingletonMonoBehaviour<IAPManager>, IStoreListener
{
    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    public List<IAPProduct> products = new List<IAPProduct>();

    public IAPProduct RemoveAds => IAPUlts.GetProduct(IAPItemName.RemoveAds);
    // AUTO GENERATE

    void Start()
    {
        StartCoroutine(WaitForFirebaseGotData());
    }

    IEnumerator WaitForFirebaseGotData()
    {
        yield return new WaitForSecondsRealtime(2);

#if USE_FIREBASE_IAP
        foreach (var product in products)
        {
            string fieldName = product.name; // Assuming keys are in lowercase
            var fieldValue = FirebaseRemote.Instance.GetIAPId().GetType().GetField(fieldName)
                ?.GetValue(FirebaseRemote.Instance.GetIAPId());
            if (fieldValue != null)
            {
                product.key = fieldValue.ToString();
            }
        }
        yield return new WaitUntil(() => FirebaseRemote.IsFirebaseGetDataCompleted);
#endif

        InitIAP();
    }

    void InitIAP()
    {
#if USE_FIREBASE_IAP
       foreach (var product in products)
        {
            string fieldName = product.name.ToLower(); // Assuming keys are in lowercase
            var fieldValue = FirebaseRemote.Instance.GetIAPId().GetType().GetField(fieldName)
                ?.GetValue(FirebaseRemote.Instance.GetIAPId());

            if (fieldValue != null)
            {
                product.key = fieldValue.ToString();
            }
        }
#endif

        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }
    
    void CheckAllPendingPurchases()
    {
        Debug.Log("=== CHECKING PENDING PURCHASES ===");

        if (!IsInitialized())
        {
            return;
        }

        foreach (var product in m_StoreController.products.all)
        {
            if (product.hasReceipt)
            {
                // Kiểm tra nếu đây là consumable đã được reward trước đó
                if (!HasRewardBeenGiven(product))
                {
                    ProcessPendingProduct(product);
                }
            }
        }

        Debug.Log("=== COMPLETE CHECKING PENDING PURCHASES ===");
    }
    
    bool HasRewardBeenGiven(Product product)
    {
        Debug.Log($"[IAPManager] Checking reward status for product: {product.definition.id}, transactionID: {product.transactionID}");
        
        if (string.IsNullOrEmpty(product.transactionID))
        {
            return true;
        }
        
        return PlayerPrefs.GetInt("reward_" + product.transactionID, 0) == 1;
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var product in products)
        {
            Debug.Log("IAP builder: " + product.key);
            builder.AddProduct(product.key, product.productType);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public string GetProducePriceFromStore(string name)
    {
        string productId = products.FirstOrDefault(product => product.name == name).key;
        if (m_StoreController != null && m_StoreController.products != null)
        {
            return m_StoreController.products.WithID(productId).metadata.localizedPriceString;
        }
        else
        {
            return "";
        }
    }

    public void BuyProductID(string name)
    {
        string productId = products.FirstOrDefault(product => product.name == name).key;

        API.Instance.ActiveLoading(true, "Connecting...");
        if (API.Instance.HasTurnOffInternet())
        {
            API.Instance.SetLoadingInfoText("No Internet!");
            API.Instance.DisableLoadingWithTime();
        }

        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log(
                    "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                API.Instance.SetLoadingInfoText("Connecting failed!");
                API.Instance.ActiveLoading(false);
            }
        }
        else
        {
            InitIAP();
            Debug.Log("BuyProductID FAIL. Not initialized.");
            API.Instance.SetLoadingInfoText("Connecting failed!");
            API.Instance.DisableLoadingWithTime();
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            this.PostEvent(EventID.ShowToast, new object[] { "Restore Purchases Succesful" });
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                Debug.Log("RestorePurchases continuing: " + result +
                          ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
        
        CheckAllPendingPurchases();
        
        var m_GooglePlayExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        var m_IOSExtensions = extensions.GetExtension<IAppleExtensions>();

        // Check Subscripton here
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].productType == ProductType.Subscription)
            {
                SubscriptionReward(i);
            }
        }
    }

    bool IsSubscriptionValid(Product subscription)
    {
        // If the product doesn't have a receipt, then it wasn't purchased and the user is therefore not subscribed.
        if (subscription.receipt == null)
        {
            return false;
        }

        //The intro_json parameter is optional and is only used for the App Store to get introductory information.
        var subscriptionManager = new SubscriptionManager(subscription, null);

        // The SubscriptionInfo contains all of the information about the subscription.
        // Find out more: https://docs.unity3d.com/Packages/com.unity.purchasing@3.1/manual/UnityIAPSubscriptionProducts.html
        var info = subscriptionManager.getSubscriptionInfo();

        return info.isSubscribed() == Result.True && info.isExpired() == Result.False;
    }

    public void SubscriptionReward(int indexProduct)
    {
        var keyProduct = products[indexProduct].key;
        var nameProduct = products[indexProduct].name;
        var subscriptionProduct = m_StoreController.products.WithID(keyProduct);
        bool isSubValid = IsSubscriptionValid(subscriptionProduct);
        // TODO: Xử lí sub
        // True là vẫn còn đăng kí (nhận được quà)
        // False: ngược lại (Hết hạn...)
        // TODO: Ví dụ có một gói sub là RemoveAds
        if (nameProduct == IAPItemName.RemoveAds.ToString())
        {
            if (isSubValid)
            {
                // TODO: Xử lí nhận quà
            }
            else
            {
                // TODO: Xử lí nếu hết hạn
            }
            return;
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        API.Instance.ActiveLoading(false);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        return ProcessPendingProduct(product);
    }

    private PurchaseProcessingResult ProcessPendingProduct(Product product)
    {
        bool complete = true;
        if (String.Equals(product.definition.id, RemoveAds.key, StringComparison.Ordinal))
        {
            API.Instance.IsRemoveAds = true;
            API.Instance.HideBanner();
        }
        else
        {
            complete = false;
        }

        if (complete)
        {
            this.PostEvent(EventID.UpdateData);
            Debug.Log($"ProcessPurchase: COMPLETE. Product: '{product.definition.id}'");
            SuccessfulPurchaseTrack(product.definition.id, LocationTracking.shop_popup);
#if USE_FALCON
            new FInAppLog(args.purchasedProduct.definition.id, args.purchasedProduct.metadata.localizedPrice,
                args.purchasedProduct.metadata.isoCurrencyCode, "shop_popup", args.purchasedProduct.transactionID, "").Send();      
#endif
            PlayerPrefs.SetInt("reward_" + product.transactionID, 1);
            return PurchaseProcessingResult.Complete;
        }
        
        Debug.Log($"ProcessPurchase: FAIL. Unrecognized product: '{product.definition.id}'");

        FailedPurchaseTrack(product.definition.id, LocationTracking.shop_popup);

        return PurchaseProcessingResult.Pending;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        API.Instance.ActiveLoading(false);
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId,
            failureReason));
        this.PostEvent(EventID.ShowToast, new object[] { "Failed to purchase" });
    }

    void SuccessfulPurchaseTrack(string id, LocationTracking location)
    {
        TrackingManager.Instance.TrackIAP(id, location, MonetizationState.completed);
        this.PostEvent(EventID.ShowToast, new object[] { "Successful purchase" });
        API.Instance.ActiveLoading(false);
    }

    void FailedPurchaseTrack(string id, LocationTracking location)
    {
        TrackingManager.Instance.TrackIAP(id, location, MonetizationState.failed);
        API.Instance.ActiveLoading(false);
    }

    [Serializable]
    public class IAPProduct
    {
        public string name;
        public ProductType productType;
        public string key;
    }

    public enum IAPItemName
    {
        RemoveAds
    }
}
#else

using UnityEngine;

public class IAPManager : MonoBehaviour { }

#endif
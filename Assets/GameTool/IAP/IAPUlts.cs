#if USE_IAP

public static class IAPUlts
{
    public static IAPManager.IAPProduct GetProduct(IAPManager.IAPItemName nameIap)
    {
        return IAPManager.Instance.products.Find(product => product.name == nameIap.ToString());
    }
}

#endif
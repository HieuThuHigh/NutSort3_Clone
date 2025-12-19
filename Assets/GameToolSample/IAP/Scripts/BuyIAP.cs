using System.Collections;
#if USE_IAP
using GameTool.Assistants.Helper.SelectableString.Scripts;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameToolSample.IAP.Scripts
{
    public class BuyIAP : MonoBehaviour
    {
#if USE_IAP
        [SelectableString(typeof(IAPManager.IAPItemName))]
#endif
        public string itemType;
        [SerializeField] protected Button button;
        [SerializeField] protected Text priceText;
        [SerializeField] protected TextMeshProUGUI priceTextmesh;

        [SerializeField] protected TextMeshProUGUI txtReward;

        protected virtual bool NeedGotIAPInited => true;

        protected virtual void OnEnable()
        {
            LoadIAPReward();
        }

        protected virtual void Start()
        {
            SetTextPrice("Error");

            StartCoroutine(LoadPriceRoutine());
            if (button)
            {
                button.onClick.AddListener(ClickBuy);
            }
        }

        public virtual void ClickBuy()
        {
#if USE_IAP
            IAPManager.Instance.BuyProductID(itemType.ToString());
#endif
        }

        public virtual void LoadIAPReward()
        {
            // switch(itemType)
            // {
            // TODO: TEMPLATE
            // case IAPManager.IAPItemName.removeAdsID:
            // {
            //     if (txtReward)
            //     {
            //         txtReward.text = "";
            //     }
            //
            //     break;  
            // }
            // }
        }

        protected virtual IEnumerator LoadPriceRoutine()
        {
            if (NeedGotIAPInited)
            {
#if USE_IAP
                while (!IAPManager.Instance.IsInitialized())
                    yield return null;
#endif
            }

            OnLoadedPrice(GetPrice());
            yield return null;
        }

        protected virtual string GetPrice()
        {
#if USE_IAP
            return IAPManager.Instance.GetProducePriceFromStore(itemType);
#endif
            return "Error";
        }

        protected virtual void OnLoadedPrice(string price)
        {
            SetTextPrice(price);
        }

        protected virtual void SetTextPrice(string price)
        {
            if (priceText)
            {
                priceText.text = price;
            }

            if (priceTextmesh)
            {
                priceTextmesh.text = price;
            }
        }
    }
}
using DG.Tweening;
using GameTool.ObjectPool.Scripts;
using GameToolSample.Scripts.UI.ResourcesItems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.UI.Scripts.ItemUI
{
    public class ItemShowUIController : BasePooling
    {
        [SerializeField] ItemResourceType itemResourceType = ItemResourceType.Coin;
        [SerializeField] RectTransform rectTrans;
        [SerializeField] Image displayImage;
        [SerializeField] TextMeshProUGUI valueText;

        public void SetItemType(ItemResourceType itemResourceType)
        {
            this.itemResourceType = itemResourceType;

            ItemResourceInfo info = AddItemEffectManager.Instance.itemResourceData.GetItemResourceInfo(this.itemResourceType);

            displayImage.sprite = info.icon;
        }

        public void SetEffectMoveDone(float endScale = 1f, float plusValue = 0.5f, bool effectMoveDone = true)
        {
            if (effectMoveDone)
            {
                transform.DOScale(endScale + plusValue, 0.5f).SetEase(Ease.Linear).SetUpdate(true);
                displayImage.DOFade(0f, 0.5f).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => { MoveDone(); });
            }
            else
            {
                MoveDone();
            }
        }

        public void FadeInEffect(float time = 0.5f)
        {
            displayImage.color = new Color32(255, 255, 255, 0);
            displayImage.DOFade(1f, time).SetEase(Ease.Linear);
        }

        public void MoveDone()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            displayImage.color = new Color32(255, 255, 255, 255);
            Disable();
        }

        public void SetValueText(string value)
        {
            valueText.text = value;
        }

        public void EnableDisplayImage(bool value)
        {
            displayImage.enabled = value;
        }

        public void SetImageDisplay(Sprite sprite)
        {
            displayImage.sprite = sprite;
        }

        public Vector3 AnchorPos3D
        {
            get
            {
                return rectTrans.anchoredPosition3D;
            }
            set
            {
                rectTrans.anchoredPosition3D = value;
            }
        }

        public Vector2 AnchorPos
        {
            get
            {
                return rectTrans.anchoredPosition;
            }
            set
            {
                rectTrans.anchoredPosition = value;
            }
        }

        public RectTransform rect => rectTrans;
    }
}
using DG.Tweening;
using GameTool.ObjectPool.Scripts;
using GameToolSample.Scripts.UI.ResourcesItems;
using UnityEngine;

namespace GameTool.UI.Scripts.ItemUI
{
    public class ItemShowSpriteController : BasePooling
    {
        [SerializeField] ItemResourceType itemResourceType = ItemResourceType.Coin;
        [SerializeField] SpriteRenderer displayImage;

        public override void Disable()
        {
            transform.DOKill();
            displayImage.DOKill();
            base.Disable();
        }

        public void SetItemType(ItemResourceType itemResourceType)
        {
            this.itemResourceType = itemResourceType;

            ItemResourceInfo info = AddItemEffectManager.Instance.itemResourceData.GetItemResourceInfo(this.itemResourceType);

            displayImage.sprite = info.icon;
        }

        public void SetEffectMoveDone(float endScale = 1f, float time = 0.5f)
        {
            transform.DOScale(endScale + 0.5f, 0.5f).SetEase(Ease.Linear).SetUpdate(true);
            displayImage.DOFade(0f, time).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                transform.DOKill();
                transform.localScale = Vector3.one;
                displayImage.color = new Color32(255, 255, 255, 255);
                Disable();
            });
        }

        public void EnableDisplayImage(bool value)
        {
            displayImage.enabled = value;
        }

        public void SetImageDisplay(Sprite sprite)
        {
            displayImage.sprite = sprite;
        }
    }
}

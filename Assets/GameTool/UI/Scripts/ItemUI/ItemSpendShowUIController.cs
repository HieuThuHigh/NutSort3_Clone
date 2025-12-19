using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTool.UI.Scripts.ItemUI
{
    public class ItemSpendShowUIController : MonoBehaviour
    {
        [Header("COMPONENT")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image iconImg;
        [SerializeField] TextMeshProUGUI valueTxt;
        [SerializeField] RectTransform childRect;

        private void OnDisable()
        {
            canvasGroup.DOKill();
            childRect.DOKill();
            childRect.anchoredPosition = Vector2.zero;
        }

        public void SetData(Sprite icon, string text, bool isSpend = true)
        {
            iconImg.sprite = icon;
            valueTxt.text = text;

            if (isSpend)
            {
                valueTxt.color = Color.red;
            }
            else
            {
                valueTxt.color = Color.green;
            }
        }

        public void ShowEffect()
        {
            canvasGroup.DOKill();
            childRect.DOKill();
            childRect.anchoredPosition = Vector2.zero;

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.Linear).SetUpdate(true).SetAutoKill();

            childRect.DOAnchorPosY(-100f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true).SetAutoKill().OnComplete(() =>
            {
                canvasGroup.DOFade(0f, 0.5f).SetDelay(1f).SetEase(Ease.Linear).SetUpdate(true).SetAutoKill();
            });
        }
    }
}

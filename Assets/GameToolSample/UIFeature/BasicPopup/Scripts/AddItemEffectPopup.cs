using DatdevUlts.Ults;
using DG.Tweening;
using GameTool.Assistants.DesignPattern;
using GameTool.ObjectPool.Scripts;
using GameTool.UI.Scripts.CanvasPopup;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.ObjectPool;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.UI.ResourcesItems;
using GameToolSample.UIManager;
using System;
using GameToolSample.UIFeature.BasicPopup.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameToolSample.UIFeature.BasicPopup
{
    public class AddItemEffectPopup : SingletonUI<AddItemEffectPopup>
    {
        public new static AddItemEffectPopup Instance
        {
            get
            {
                if (!CanvasManager.Instance.IsShowing(eUIName.AddItemEffectPopup))
                {
                    CanvasManager.Instance.Push(eUIName.AddItemEffectPopup);
                }

                return singleton;
            }
        }

        [SerializeField] RectTransform rectTransform;

        private void OnEnable()
        {
            foreach (Transform item in transform)
            {
                item.gameObject.SetActive(false);
            }
        }

        public void ShowCoinUI(int value, RectTransform posStart, RectTransform posEnd,
            AnalyticID.LocationTracking locationTracking,
            Action callback = null)
        {
            if (!CurrencyManager.Instance)
            {
                GameData.Instance.CollectItem(new CurrencyInfo(ItemResourceType.Coin, value), true, locationTracking);
                this.PostEvent(EventID.UpdateData);
                return;
            }
            else
            {
                GameData.Instance.CollectItem(new CurrencyInfo(ItemResourceType.Coin, value), false, locationTracking);
            }

            ShowCoin(value, () =>
                {
                    var localPos = (Vector2)rectTransform.InverseTransformPoint(posStart.position);
                    var vpStart = localPos + Math2DUlts.AddAngle(
                        rectTransform.sizeDelta / 2 * RandomUlts.Range(0, 1f), RandomUlts.Range(0, 360));
                    return rectTransform.LocalPositionToViewport(vpStart);
                },
                () => { return rectTransform.WorldPositionToViewport(posEnd.position); },
                rectTransform, locationTracking, callback);
        }

        public void ShowCoin(int value, Func<Vector2> viewPortStart, Func<Vector2> viewPortEnd, RectTransform rect,
            AnalyticID.LocationTracking locationTracking, Action callback = null)
        {
            var mainCamera = Camera.main;
            GameData.Instance.CollectItem(new CurrencyInfo(ItemResourceType.Coin, value), false,
                locationTracking);
            var countItem = GetCountItem(value);
            for (int i = 0; i < countItem; i++)
            {
                var index = i;
                this.DelayedCall(0.01f * index, () =>
                {
                    var coin = PoolingManager.Instance.GetObject(ePrefabPool.Coin, this.transform);
                    var posStart = rect.ViewportToCanvasPosition(viewPortStart.Invoke());
                    var posEnd = rect.ViewportToCanvasPosition(viewPortEnd.Invoke());
                    Vector3 startPoint = posStart + new Vector3(Random.Range(-180, 180), Random.Range(-180, 180));
                    coin.transform.localPosition = posStart;
                    coin.transform.localRotation = Quaternion.Euler(mainCamera.transform.localRotation.x, 0, 0);
                    coin.transform.SetAsFirstSibling();
                    FollowFromManyCanvasPooling traiVfx =
                        (FollowFromManyCanvasPooling)PoolingManager.Instance.GetObject(ePrefabPool.CoinTrail,
                            position: coin.transform.position);
                    traiVfx.SetData(coin.transform, rectTransform, 9f);
                    traiVfx.Camera = mainCamera;
                    traiVfx.Disable(2);
                    coin.transform.DOLocalMove(startPoint, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(0.3f + 0.05f * index, () =>
                        {
                            coin.transform.DOLocalMove(posEnd, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                            {
                                coin.Disable();
                                if (index != countItem - 1)
                                {
                                    GameData.Instance.CoinFake += value / countItem;
                                    this.PostEvent(EventID.UpdateData);
                                }
                                else if (index == countItem - 1)
                                {
                                    GameData.Instance.CoinFake += (value / countItem + value % countItem);
                                    this.PostEvent(EventID.UpdateData);
                                    DOVirtual.DelayedCall(0.1f, () =>
                                    {
                                        CanvasManager.Instance.Pop(eUIName.AddItemEffectPopup);
                                        callback?.Invoke();
                                    });
                                }
                            });
                        });
                    });
                    coin.transform.localScale = Vector3.zero;
                    coin.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                }, true);
            }
        }

        public int GetCountItem(int value)
        {
            return Mathf.RoundToInt(Mathf.Clamp(value, 0, 15));
        }
    }
}
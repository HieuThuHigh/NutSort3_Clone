using System;
using System.Collections;
using DG.Tweening;
using GameTool.Assistants.DesignPattern;
using GameTool.Audio.Scripts;
using GameTool.ObjectPool.Scripts;
using GameTool.UI.Scripts.CanvasPopup;
using GameTool.Vibration;
using GameToolSample.Audio;
using GameToolSample.ObjectPool;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.Layers_Tags;
using GameToolSample.Scripts.UI.ResourcesItems;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameTool.UI.Scripts.ItemUI
{
    public class AddItemEffectManager : SingletonMonoBehaviour<AddItemEffectManager>
    {
        public ItemResourceData itemResourceData;

        [Space]
        [Header("UI REFERENCES")]
        [SerializeField] Canvas canvas;
        [SerializeField] RectTransform canvasUI;
        [SerializeField] RectTransform posRewardUI;
        [SerializeField] PositionShowItemEffect[] listPosShowItemEffect;
        [SerializeField] TypeEffect typeEffectUI = TypeEffect.Type1;
        [SerializeField] AnimationCurve animationCurveSprite;
        [SerializeField] AnimationCurve animationCurveUI;
        int countShowUI;

        [Header("SPEND EFFECT")]
        [SerializeField] PositionShowItemSpendEffect[] listPosShowItemSpendEffect;

        protected override void Awake()
        {
            base.Awake();

            this.RegisterListener(EventID.UpdateCanvas, UpdateCanvasEventRegisterListener);
            this.RegisterListener(EventID.ShowSpendItemUI, ShowSpendItemUIEventRegisterListener);
        }

        private void OnDestroy()
        {
            this.RemoveListener(EventID.UpdateCanvas, UpdateCanvasEventRegisterListener);
            this.RemoveListener(EventID.ShowSpendItemUI, ShowSpendItemUIEventRegisterListener);
        }

        void UpdateCanvasEventRegisterListener(Component component, object[] obj = null)
        {
            SetupFollowScene();
        }

        void ShowSpendItemUIEventRegisterListener(Component component, object[] obj = null)
        {
            if (obj != null && obj.Length > 0)
            {
                ItemResourceType itemType = ItemResourceType.None;
                string value = "";
                bool isSpend = true;

                if (obj[0] != null)
                {
                    itemType = (ItemResourceType)obj[0];
                }

                if (obj[1] != null)
                {
                    value = (string)obj[1];
                }

                if (obj[2] != null)
                {
                    isSpend = (bool)obj[2];
                }

                ShowSpendItemUI(itemType, value, isSpend);
            }
        }

        public void SetupFollowScene()
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            if (CanvasManager.IsInstanceValid())
            {
                Canvas sceneCanvas = CanvasManager.Instance.GetComponent<Canvas>();
                canvas.renderMode = sceneCanvas.renderMode;
                canvas.worldCamera = sceneCanvas.worldCamera;
                canvas.sortingLayerName = LayerName.UI;
                canvas.sortingOrder = 1;
            }
        }

        public void StopAll()
        {
            StopAllCoroutines();
        }

        #region SPEND EFFECT
        public void ShowSpendItemUI(ItemResourceType type, string value, bool isSpend)
        {
            PositionShowItemSpendEffect item = GetPositionShowItemSpendEffect(type);
            if (item != null)
            {
                item.rectrans.SetData(itemResourceData.GetItemResourceInfo(type).icon, value, isSpend);
                item.rectrans.ShowEffect();
            }
        }

        PositionShowItemSpendEffect GetPositionShowItemSpendEffect(ItemResourceType type)
        {
            foreach (PositionShowItemSpendEffect item in listPosShowItemSpendEffect)
            {
                if (item.itemResourceType == type)
                {
                    return item;
                }
            }

            return null;
        }

        #endregion

        public void ShowItemsSpriteToPos(ItemResourceType itemType, Transform startPosTrans, Transform endPosTrans, int amount, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 10f, Action callbackOne = null, Action callback = null)
        {
            StartCoroutine(WaitEffectItemSpriteToPos(itemType, startPosTrans, endPosTrans, amount, startScale, endScale, startAngle, endAngle, timeBetween, timeMove, callbackOne, callback));
        }

        IEnumerator WaitEffectItemSpriteToPos(ItemResourceType itemType, Transform startPosTrans, Transform endPosTrans, int amount, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 10f, Action callbackOne = null, Action callback = null)
        {
            if (amount > 0)
            {
                Sprite icon = SpriteItem(itemType);
                for (int i = 0; i < amount; i++)
                {
                    eSoundName nameAudio = GetNameAudio(itemType);
                    Vector3 startPos = startPosTrans.position;
                    Vector3 endPos = endPosTrans.position;

                    ItemShowSpriteController itemShowSpriteController = PoolingManager.Instance.GetObject(ePrefabPool.ItemShowSpritePrefab, null, startPos, Vector3.one * startScale, Quaternion.Euler(0f, 0f, startAngle)).GetComponent<ItemShowSpriteController>();
                    itemShowSpriteController.SetImageDisplay(icon);

                    Vector2 posPath = startPos;
                    posPath.x += Random.Range(2f, 3f);
                    posPath.y += Random.Range(-0.5f, 0.5f);

                    itemShowSpriteController.transform.DOScale(endScale, 1f).SetSpeedBased(true).SetEase(Ease.Linear);
                    itemShowSpriteController.transform.DORotate(new Vector3(0f, 0f, endAngle), 100f, RotateMode.FastBeyond360).SetSpeedBased(true).SetEase(Ease.Linear);
                    itemShowSpriteController.transform.DOPath(new Vector3[] { startPos, posPath, endPos }, timeMove, PathType.CatmullRom).SetEase(animationCurveSprite).OnComplete(() =>
                    {
                        callbackOne?.Invoke();

                        itemShowSpriteController.SetEffectMoveDone(endScale);
                        AudioManager.Instance.Shot(nameAudio);
                        VibrationManager.Instance.Vibrate();
                    });

                    yield return new WaitForSeconds(timeBetween);
                }

                callback?.Invoke();
            }
        }

        public void ShowItemsUI(ItemResourceType itemType, int valueReward, int amountItem = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true, bool canSetData = true)
        {
            if (valueReward <= 0) return;
            countShowUI++;

            if (canSetData)
            {
                SetData(itemType, valueReward);
            }

            ItemShowUIController itemRewardUIShow = PoolingManager.Instance.GetObject(ePrefabPool.ItemRewardUIShow, posRewardUI.transform).GetComponent<ItemShowUIController>();
            itemRewardUIShow.transform.localPosition = Vector3.zero;
            itemRewardUIShow.transform.localScale = Vector2.zero;
            itemRewardUIShow.transform.SetSiblingIndex(posRewardUI.transform.childCount - 1);

            Vector3 anchor = itemRewardUIShow.AnchorPos3D;
            anchor.z = 0;
            itemRewardUIShow.AnchorPos3D = anchor;

            itemRewardUIShow.SetImageDisplay(SpriteItem(itemType));
            itemRewardUIShow.EnableDisplayImage(false);
            itemRewardUIShow.SetValueText("");

            itemRewardUIShow.transform.DOScale(1f, 0.2f).SetUpdate(true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                StartCoroutine(AnimateItemUI(itemType, itemRewardUIShow.transform, valueReward, amountItem, startScale, endScale, startAngle, endAngle, timeBetween, timeMove, callbackOne, callback, effectItemDone));
                itemRewardUIShow.transform.DOKill();
            });
        }

        IEnumerator AnimateItemUI(ItemResourceType itemType, Transform itemRewardShowTrans, int valueItem, int amount = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true)
        {
            if (amount > valueItem)
            {
                amount = valueItem;
            }

            if (amount > 0)
            {
                int valueOfOneItem = valueItem / amount;
                int surplus = valueItem % amount;

                ePrefabPool ePrefabPool = ePrefabPool.ItemShowUIPrefab;
                eSoundName nameAudio = GetNameAudio(itemType);
                eSoundName nameAudioMove = GetNameAudioMove(itemType);

                Sprite icon = SpriteItem(itemType);

                for (int i = 0; i < amount; i++)
                {
                    int count = i;

                    Vector3 startPos = itemRewardShowTrans.position;
                    Vector3 endPos = GetEndPosTrans(itemType);

                    EffectItemUI(itemType, startPos, endPos, ePrefabPool, nameAudio, nameAudioMove, icon, count, valueOfOneItem, surplus, amount, startScale, endScale, startAngle, endAngle, timeMove, callbackOne, effectItemDone);
                    yield return new WaitForSecondsRealtime(timeBetween);
                }
            }

            yield return new WaitForSecondsRealtime(GetTimeDelayCallback(typeEffectUI));

            itemRewardShowTrans.DOScale(0f, 0.2f).SetEase(Ease.InOutBack).SetUpdate(true).OnComplete(() =>
            {
                itemRewardShowTrans.gameObject.SetActive(false);

                countShowUI--;

                if (countShowUI <= 0)
                {

                }

                callback?.Invoke();
            });
        }

        public void ShowItemsUIWithStartPos(ItemResourceType itemType, Transform startPosTrans, int valueReward, int amountItem = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true, bool canSetData = true)
        {
            if (valueReward <= 0) return;
            countShowUI++;

            if (canSetData)
            {
                SetData(itemType, valueReward);
            }

            StartCoroutine(AnimateItemUIWithStartPos(itemType, startPosTrans, valueReward, amountItem, startScale, endScale, startAngle, endAngle, timeBetween, timeMove, callbackOne, callback, effectItemDone));
        }

        IEnumerator AnimateItemUIWithStartPos(ItemResourceType itemType, Transform startPosTrans, int valueItem, int amount = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true)
        {
            if (amount > valueItem)
            {
                amount = valueItem;
            }

            if (amount > 0)
            {
                int valueOfOneItem = valueItem / amount;
                int surplus = valueItem % amount;

                ePrefabPool ePrefabPool = ePrefabPool.ItemShowUIPrefab;
                eSoundName nameAudio = GetNameAudio(itemType);
                eSoundName nameAudioMove = GetNameAudioMove(itemType);

                Sprite icon = SpriteItem(itemType);

                for (int i = 0; i < amount; i++)
                {
                    int count = i;

                    Vector2 startPos = startPosTrans.position;
                    Vector2 endPos = GetEndPosTrans(itemType);

                    EffectItemUI(itemType, startPos, endPos, ePrefabPool, nameAudio, nameAudioMove, icon, count, valueOfOneItem, surplus, amount, startScale, endScale, startAngle, endAngle, timeMove, callbackOne, effectItemDone);

                    yield return new WaitForSecondsRealtime(timeBetween);
                }
            }

            yield return new WaitForSecondsRealtime(GetTimeDelayCallback(typeEffectUI));

            countShowUI--;

            if (countShowUI <= 0)
            {

            }

            callback?.Invoke();
        }

        public void ShowItemsUIWithEndPos(ItemResourceType itemType, Transform endPosTrans, int valueReward, int amountItem = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true, bool canSetData = true)
        {
            if (valueReward <= 0) return;
            countShowUI++;

            if (canSetData)
            {
                SetData(itemType, valueReward);
            }

            ItemShowUIController itemRewardUIShow = PoolingManager.Instance.GetObject(ePrefabPool.ItemRewardUIShow, posRewardUI.transform).GetComponent<ItemShowUIController>();
            itemRewardUIShow.transform.localScale = Vector2.zero;
            itemRewardUIShow.transform.SetSiblingIndex(posRewardUI.transform.childCount - 1);

            Vector3 anchor = itemRewardUIShow.AnchorPos3D;
            anchor.z = 0;
            itemRewardUIShow.AnchorPos3D = anchor;

            itemRewardUIShow.SetImageDisplay(SpriteItem(itemType));
            itemRewardUIShow.EnableDisplayImage(false);
            itemRewardUIShow.SetValueText("");

            itemRewardUIShow.transform.DOScale(1f, 0.2f).SetUpdate(true).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                StartCoroutine(AnimateItemUIEndPos(itemType, itemRewardUIShow.transform, endPosTrans, valueReward, amountItem, startScale, endScale, startAngle, endAngle, timeBetween, timeMove, callbackOne, callback, effectItemDone));
                itemRewardUIShow.transform.DOKill();
            });
        }

        IEnumerator AnimateItemUIEndPos(ItemResourceType itemType, Transform itemRewardShowTrans, Transform endPosTrans, int valueItem, int amount = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true)
        {
            if (amount > valueItem)
            {
                amount = valueItem;
            }

            if (amount > 0)
            {
                int valueOfOneItem = valueItem / amount;
                int surplus = valueItem % amount;

                ePrefabPool ePrefabPool = ePrefabPool.ItemShowUIPrefab;
                eSoundName nameAudio = GetNameAudio(itemType);
                eSoundName nameAudioMove = GetNameAudioMove(itemType);

                Sprite icon = SpriteItem(itemType);

                for (int i = 0; i < amount; i++)
                {
                    int count = i;

                    Vector2 startPos = itemRewardShowTrans.position;
                    Vector2 endPos = endPosTrans.position;

                    EffectItemUI(itemType, startPos, endPos, ePrefabPool, nameAudio, nameAudioMove, icon, count, valueOfOneItem, surplus, amount, startScale, endScale, startAngle, endAngle, timeMove, callbackOne, effectItemDone);

                    yield return new WaitForSecondsRealtime(timeBetween);
                }
            }

            yield return new WaitForSecondsRealtime(GetTimeDelayCallback(typeEffectUI));

            itemRewardShowTrans.DOScale(0f, 0.2f).SetEase(Ease.InOutBack).SetUpdate(true).OnComplete(() =>
            {
                itemRewardShowTrans.gameObject.SetActive(false);

                countShowUI--;

                if (countShowUI <= 0)
                {

                }

                callback?.Invoke();
            });
        }

        public void ShowItemsUIWithPos(ItemResourceType itemType, Transform startTrans, Transform endTrans, int valueReward, int amountItem = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true, bool canSetData = true)
        {
            if (valueReward <= 0) return;
            countShowUI++;
            if (canSetData)
            {
                SetData(itemType, valueReward);
            }
            StartCoroutine(AnimateItemUIWithPos(itemType, startTrans, endTrans, valueReward, amountItem, startScale, endScale, startAngle, endAngle, timeBetween, timeMove, callbackOne, callback, effectItemDone));
        }

        IEnumerator AnimateItemUIWithPos(ItemResourceType itemType, Transform startTrans, Transform endTrans, int valueItem, int amount = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeBetween = 0.1f, float timeMove = 1f, Action callbackOne = null, Action callback = null, bool effectItemDone = true)
        {
            if (amount > valueItem)
            {
                amount = valueItem;
            }

            if (amount > 0)
            {
                int valueOfOneItem = valueItem / amount;
                int surplus = valueItem % amount;

                ePrefabPool ePrefabPool = ePrefabPool.ItemShowUIPrefab;
                eSoundName nameAudio = GetNameAudio(itemType);
                eSoundName nameAudioMove = GetNameAudioMove(itemType);

                Sprite icon = SpriteItem(itemType);

                for (int i = 0; i < amount; i++)
                {
                    int count = i;

                    Vector2 startPos = startTrans.position;
                    Vector2 endPos = endTrans.position;

                    EffectItemUI(itemType, startPos, endPos, ePrefabPool, nameAudio, nameAudioMove, icon, count, valueOfOneItem, surplus, amount, startScale, endScale, startAngle, endAngle, timeMove, callbackOne, effectItemDone);

                    yield return new WaitForSecondsRealtime(timeBetween);
                }
            }

            yield return new WaitForSecondsRealtime(GetTimeDelayCallback(typeEffectUI));

            countShowUI--;

            if (countShowUI <= 0)
            {

            }

            callback?.Invoke();
        }

        void EffectItemUI(ItemResourceType itemType, Vector3 startPos, Vector3 endPos, ePrefabPool namePrefabBool, eSoundName nameAudio, eSoundName nameAudioMove, Sprite icon, int count, int valueOfOneItem, int surplus, int amount = 10, float startScale = 1f, float endScale = 1f, float startAngle = 0f, float endAngle = 0f, float timeMove = 1f, Action callbackOne = null, bool effectItemDone = true)
        {
            float randomX = 0f;
            float randomY = 0f;
            float timeFade = 0.5f;

            if (typeEffectUI == TypeEffect.Type1)
            {
                startPos.x += Random.Range(-0.2f, 0.2f);
                startPos.y += Random.Range(-0.5f, 0.5f);

                randomX = Random.Range(-1f, 1f);
                randomY = Random.Range(-1f, -0.5f);

                timeFade = 0.5f;
            }
            else if (typeEffectUI == TypeEffect.Type2)
            {
                startPos.x += 0f;
                startPos.y += 0f;

                randomX = Random.Range(-1.5f, 1.5f);
                randomY = Random.Range(-1.5f, 1.5f);

                timeFade = 0.1f;
            }

            ItemShowUIController itemShowUIController = PoolingManager.Instance.GetObject(namePrefabBool, canvasUI.transform, startPos, Vector3.one * startScale, Quaternion.Euler(0f, 0f, startAngle)).GetComponent<ItemShowUIController>();

            Vector3 anchor = itemShowUIController.AnchorPos3D;
            anchor.z = 0;
            itemShowUIController.AnchorPos3D = anchor;
            itemShowUIController.transform.SetSiblingIndex(0);

            itemShowUIController.SetImageDisplay(icon);

            endPos.z = itemShowUIController.rect.position.z;

            Vector3 newPosMove = startPos;
            newPosMove.z = itemShowUIController.rect.position.z;
            newPosMove.x += randomX;
            newPosMove.y += randomY;

            itemShowUIController.gameObject.SetActive(true);
            itemShowUIController.FadeInEffect(timeFade);
            itemShowUIController.transform.DORotate(new Vector3(0f, 0f, endAngle), 100f, RotateMode.FastBeyond360).SetUpdate(true).SetSpeedBased(true).SetEase(Ease.Linear);
            itemShowUIController.transform.DOScale(endScale, 1f).SetEase(Ease.Linear).SetUpdate(true);

            if (typeEffectUI == TypeEffect.Type1)
            {
                itemShowUIController.transform.DOPath(new[] { itemShowUIController.transform.position, newPosMove, endPos }, timeMove, PathType.CatmullRom).SetUpdate(true).SetEase(animationCurveUI).OnComplete(() =>
                {
                    SetDataDoneEffectItemUI(itemType, count, amount, valueOfOneItem, surplus, nameAudio, itemShowUIController, callbackOne, effectItemDone);
                });
            }
            else if (typeEffectUI == TypeEffect.Type2)
            {
                itemShowUIController.transform.DOMove(newPosMove, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(() =>
                {
                    Vector3 posCenter = CalculateMidPoint(newPosMove, endPos, randomX >= 0 ? 2f : -2f);

                    itemShowUIController.transform.DOPath(new[] { newPosMove, posCenter, endPos }, timeMove, PathType.CatmullRom).SetDelay(0.2f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        SetDataDoneEffectItemUI(itemType, count, amount, valueOfOneItem, surplus, nameAudio, itemShowUIController, callbackOne, effectItemDone);
                    });

                    //itemShowUIController.transform.DOMove(endPos, timeMove).SetDelay(0.2f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
                    //{
                    //    SetDataDoneEffectItemUI(itemType, count, amount, valueOfOneItem, surplus, nameAudio, itemShowUIController, callbackOne, effectItemDone);
                    //});
                });

                AudioManager.Instance.Shot(nameAudioMove);
            }
        }

        void SetData(ItemResourceType itemType, int amount)
        {
            // switch (itemType)
            // {
            //     case ItemResourceType.Coin:
            //         GameToolSample.GameDataScripts.Scripts.GameData.Instance.Coin += amount;
            //         break;
            //     case ItemResourceType.Diamond:
            //         GameToolSample.GameDataScripts.Scripts.GameData.Instance.Diamond += amount;
            //         break;
            // }
            //
            // this.PostEvent(EventID.UpdateData);
        }

        void SetDataDoneEffectItemUI(ItemResourceType itemType, int i, int amount, int valueOfOneItem, int surplus, eSoundName nameAudio, ItemShowUIController itemShowUIController, Action callbackOne = null, bool effectItemDone = true)
        {
            switch (itemType)
            {
                case ItemResourceType.Coin:
                    if (i == amount - 1)
                    {
                        GameToolSample.GameDataScripts.Scripts.GameData.Instance.CoinFake += valueOfOneItem + surplus;
                    }
                    else
                    {
                        GameToolSample.GameDataScripts.Scripts.GameData.Instance.CoinFake += valueOfOneItem;
                    }
                    break;
                case ItemResourceType.Diamond:
                    if (i == amount - 1)
                    {
                        GameToolSample.GameDataScripts.Scripts.GameData.Instance.DiamondFake += valueOfOneItem + surplus;
                    }
                    else
                    {
                        GameToolSample.GameDataScripts.Scripts.GameData.Instance.DiamondFake += valueOfOneItem;
                    }
                    break;
            }

            this.PostEvent(EventID.UpdateData);

            callbackOne?.Invoke();

            itemShowUIController.SetEffectMoveDone(plusValue: 0.2f, effectMoveDone: effectItemDone);

            AudioManager.Instance.Shot(nameAudio);
            VibrationManager.Instance.Vibrate();
        }

        public Sprite SpriteItem(ItemResourceType type = ItemResourceType.Coin)
        {
            foreach (ItemResourceInfo info in itemResourceData.listItemResource)
            {
                if (info.type == type)
                {
                    return info.icon;
                }
            }
            return null;
        }

        public eSoundName GetNameAudio(ItemResourceType type = ItemResourceType.Coin)
        {
            switch (type)
            {
                case ItemResourceType.Coin:
                    return eSoundName.CollectCoin;
                case ItemResourceType.Diamond:
                    return eSoundName.CollectDiamond;
                default:
                    return eSoundName.CollectCoin;
            }
        }

        public eSoundName GetNameAudioMove(ItemResourceType type = ItemResourceType.Coin)
        {
            return eSoundName.CollectCoin;
            // switch (type)
            // {
            //     case ItemResourceType.Coin:
            //         return eSoundName.CoinMove;
            //     case ItemResourceType.Diamond:
            //         return eSoundName.CoinMove;
            //     default:
            //         return eSoundName.CoinMove;
            // }
        }

        public Vector2 GetEndPosTrans(ItemResourceType type = ItemResourceType.Coin)
        {
            foreach (PositionShowItemEffect pos in listPosShowItemEffect)
            {
                if (pos.itemResourceType == type)
                {
                    return pos.rectrans.position;
                }
            }

            return Vector2.zero;
        }

        Vector3 CalculateMidPoint(Vector3 point1, Vector3 point2, float distance)
        {
            Vector3 middlePoint = Vector3.Lerp(point1, point2, 0.5f);
            float angle = AngleBetweenTwoPoint(point1, point2);

            float newX = middlePoint.x + distance * Mathf.Cos(angle);
            float newY = middlePoint.y + distance * Mathf.Sin(angle);

            // Tạo Vector3 mới với tọa độ mới
            Vector3 newPosition = new Vector3(newX, newY, middlePoint.z);
            return newPosition;
        }

        public float AngleBetweenTwoPoint(Vector3 pos1, Vector3 pos2)
        {
            // Tính toán sự chênh lệch giữa các tọa độ x và y của hai điểm
            float deltaX = pos2.x - pos1.x;
            float deltaY = pos2.y - pos1.y;

            // Sử dụng hàm Atan2 để tính góc giữa hai điểm (kết quả ở đơn vị radian)
            float angleInRadians = Mathf.Atan2(deltaY, deltaX);

            // Chuyển đổi radian thành độ
            float angleInDegrees = Mathf.Rad2Deg * angleInRadians;

            return angleInDegrees;
        }

        public float GetTimeDelayCallback(TypeEffect typeEffect)
        {
            switch (typeEffect)
            {
                case TypeEffect.Type1:
                    return 1f;
                case TypeEffect.Type2:
                    return 2f;
                default:
                    return 1f;
            }
        }

        [Serializable]
        public class PositionShowItemEffect
        {
            public ItemResourceType itemResourceType = ItemResourceType.None;
            public RectTransform rectrans;
        }

        [Serializable]
        public class PositionShowItemSpendEffect
        {
            public ItemResourceType itemResourceType = ItemResourceType.None;
            public ItemSpendShowUIController rectrans;
        }

        public enum TypeEffect
        {
            Type1,
            Type2
        }
    }
}
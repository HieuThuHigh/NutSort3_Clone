using System.Collections;
using System.Collections.Generic;
using DatdevUlts.Ults;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SnapScroll
{
    public class SnapGroupButton : MonoBehaviour
    {
        [SerializeField] ScrollSnapRect2 _scrollSnap;

        [FormerlySerializedAs("_buttonOn")] [SerializeField]
        Image _bgSelected;

        [SerializeField] HorizontalLayoutGroup _horizontalLayoutGroup;
        [SerializeField] List<HomeButton> _listHomeBtn = new List<HomeButton>();
        [SerializeField] private int _startIndex;
        [SerializeField] float _maxDuration = 0.1f;

        [FormerlySerializedAs("ScaleOn")] [SerializeField]
        float _scaleOn = 352.4f / 212.8f;

        [SerializeField] private float _lineWidth = 4;
        private float _duration;
        private int _preIndex = -1;
        private float _flexWidth;
        private float _flexWidthOff;
        private float FlexWidthOn => _flexWidth * _scaleOn;
        private float OffSetBgSelected => GetOffSet(_bgSelected.RectTransform(), FlexWidthOn);

        protected void Awake()
        {
            for (int i = 0; i < _listHomeBtn.Count; i++)
            {
                var indexCache = i;
                _listHomeBtn[i].BtnClick.onClick.AddListener(() => { ButtonClick(indexCache); });
                _listHomeBtn[i].ButtonTxt.DOFade(0, _duration);
            }
        }

        private void Start()
        {
            StartCoroutine(Wait());

            IEnumerator Wait()
            {
                yield return null;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_horizontalLayoutGroup.RectTransform());
                _flexWidth = _listHomeBtn[0].RectTransform().sizeDelta.x;
                var delta = _bgSelected.RectTransform().sizeDelta;
                delta.x = FlexWidthOn + 4;
                _bgSelected.RectTransform().sizeDelta = delta;
                _scrollSnap.SetPage(_startIndex);
                ButtonClick(_startIndex);
            }
        }

        public void ButtonClick(int number)
        {
            _scrollSnap.LerpToPage(number);
            ChangeButtonState(number);
        }

        public void CheckButtonIndex()
        {
            ChangeButtonState(_scrollSnap.CurrentPageIndex);
        }

        private void ChangeButtonState(int number)
        {
            var pre = _preIndex;
            for (int i = 0; i < _listHomeBtn.Count; i++)
            {
                var indexCache = i;
                _listHomeBtn[i].ButtonTxt.gameObject.SetActive(number == indexCache);
                _listHomeBtn[i].ButtonIconOff.SetActive(number != indexCache);
                _listHomeBtn[i].ButtonIconOn.SetActive(number == indexCache);

                HomeButton button = _listHomeBtn[indexCache];
                if (indexCache == number)
                {
                    if (_preIndex != indexCache)
                    {
                        _preIndex = indexCache;


                        if (_flexWidthOff == 0)
                        {
                            button.LayoutElement.flexibleWidth = -1;
                            button.LayoutElement.preferredWidth = button.RectTransform().sizeDelta.x;
                            button.LayoutElement.DOPreferredSize(new Vector2(_flexWidth * _scaleOn, -1), _duration)
                                .OnComplete(
                                    () =>
                                    {
                                        if (_flexWidthOff == 0)
                                        {
                                            StartCoroutine(Wait());

                                            IEnumerator Wait()
                                            {
                                                yield return null;
                                                LayoutRebuilder.ForceRebuildLayoutImmediate(_horizontalLayoutGroup
                                                    .RectTransform());
                                                _flexWidthOff = _listHomeBtn[0].RectTransform().sizeDelta.x;
                                                _duration = _maxDuration;
                                                _horizontalLayoutGroup.enabled = false;
                                                SetPreWithOffElements();
                                            }
                                        }
                                    }).SetEase(Ease.Linear);
                        }
                        else
                        {
                            var posXTo = indexCache * _flexWidthOff + GetOffSet(button.RectTransform(), FlexWidthOn);
                            button.RectTransform()
                                .DOSizeDelta(new Vector2(_flexWidth * _scaleOn, button.RectTransform().sizeDelta.y),
                                    _duration).SetEase(Ease.Linear);
                            button.RectTransform().DOAnchorPosX(posXTo, _duration).SetEase(Ease.Linear)
                                .OnUpdate(() => { button.UpdateLine(); }).OnComplete(() => { button.UpdateLine(); });
                        }

                        UpdateBgSelected(indexCache);
                    
                        button.ButtonTxt.DOFade(1, _duration);

                        button.ButtonIconOn.transform.RectTransform().anchoredPosition =
                            button.AnchorPosOff;
                        button.ButtonIconOn.transform.RectTransform()
                            .DOAnchorPos(button.AnchorPosOn, _duration);

                        button.ButtonIconOn.transform.RectTransform().sizeDelta =
                            button.SizeDeltaOff;
                        button.ButtonIconOn.transform.RectTransform()
                            .DOSizeDelta(button.SizeDeltaOn, _duration);
                    }
                }
                else
                {
                    if (_flexWidthOff == 0)
                    {
                        button.LayoutElement.preferredWidth = -1;
                        button.LayoutElement.flexibleWidth = 1;
                    }
                    else
                    {
                        float posXTo = GetOffSet(button.RectTransform(), _flexWidthOff);
                        if (indexCache < number)
                        {
                            posXTo += indexCache * _flexWidthOff;
                        }
                        else
                        {
                            posXTo += (indexCache - 1) * _flexWidthOff + FlexWidthOn;
                        }

                        button.RectTransform().DOAnchorPosX(posXTo, _duration).SetEase(Ease.Linear);
                        button.RectTransform()
                            .DOSizeDelta(new Vector2(_flexWidthOff, button.RectTransform().sizeDelta.y), _duration)
                            .SetEase(Ease.Linear).OnUpdate(() => { button.UpdateLine(); })
                            .OnComplete(() => { button.UpdateLine(); });
                    }

                    if (pre == indexCache)
                    {
                        button.ButtonTxt.DOFade(0, _duration);
                    
                        button.ButtonIconOff.transform.RectTransform().anchoredPosition =
                            button.AnchorPosOn;
                        button.ButtonIconOff.transform.RectTransform()
                            .DOAnchorPos(button.AnchorPosOff, _duration);

                        button.ButtonIconOff.transform.RectTransform().sizeDelta =
                            button.SizeDeltaOn;
                        button.ButtonIconOff.transform.RectTransform()
                            .DOSizeDelta(button.SizeDeltaOff, _duration);
                    }
                }
            }
        }

        private void UpdateBgSelected(int indexCache, bool imediate = false)
        {
            var posXTo = indexCache * _flexWidthOff + OffSetBgSelected;
            _bgSelected.rectTransform.DOKill();
            _bgSelected.rectTransform.DOAnchorPosX(posXTo, imediate ? 0 : _duration);
        }

        private void SetPreWithOffElements()
        {
            for (int i = 0; i < _listHomeBtn.Count; i++)
            {
                _listHomeBtn[i].LayoutElement.flexibleWidth = -1;
                _listHomeBtn[i].LayoutElement.preferredWidth = -1;
                if (i == _preIndex)
                {
                    var delta = _listHomeBtn[i].RectTransform().sizeDelta;
                    delta.x = FlexWidthOn;
                    _listHomeBtn[i].RectTransform().sizeDelta = delta;
                }
                else
                {
                    var delta = _listHomeBtn[i].RectTransform().sizeDelta;
                    delta.x = _flexWidthOff;
                    _listHomeBtn[i].RectTransform().sizeDelta = delta;
                }
            
                _listHomeBtn[i].UpdateLine();
            }

            UpdateBgSelected(_preIndex, true);
        }

        private float GetOffSet(RectTransform rectTransform, float width)
        {
            return Mathf.Lerp(0, width, rectTransform.pivot.x);
        }
    }
}
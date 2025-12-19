using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SnapScroll
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollSnapRect2 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Tooltip("Set starting page index - starting from 0")]
        public int startingPage;

        [Tooltip("Threshold time for fast swipe in seconds")]
        public float fastSwipeThresholdTime = 0.3f;

        [Tooltip("Threshold time for fast swipe in (unscaled) pixels")]
        public int fastSwipeThresholdDistance = 100;

        [Tooltip("How fast will page lerp to target position")]
        public float decelerationRate = 10f;

        [Tooltip("Button to go to the previous page (optional)")]
        public GameObject prevButton;

        [Tooltip("Button to go to the next page (optional)")]
        public GameObject nextButton;

        [Tooltip("Sprite for unselected page (optional)")]
        public Sprite unselectedPage;

        [Tooltip("Sprite for selected page (optional)")]
        public Sprite selectedPage;

        [Tooltip("Container with page images (optional)")]
        public Transform pageSelectionIcons;

        // fast swipes should be fast and short. If too long, then it is not fast swipe
        private int _fastSwipeThresholdMaxLimit;

        private ScrollRect _scrollRectComponent;
        private RectTransform _scrollRectRect;
        private RectTransform _container;

        [SerializeField] private bool _horizontal;

        // number of pages in container
        [SerializeField] private int _pageCount;
        [SerializeField] private int _currentPage;

        // whether lerping is in progress and target lerp position
        private bool _lerp;
        private Vector2 _lerpTo;

        // target position of every page
        private List<Vector2> _pagePositions = new List<Vector2>();

        // in draggging, when dragging started and where it started
        private bool _dragging;
        private float _timeStamp;
        private Vector2 _startPosition;

        // for showing small page icons
        private bool _showPageSelection;

        private int _previousPageSelectionIndex;

        // container with Image components - one Image for each page
        private List<Image> _pageSelectionImages;
        [SerializeField] GameObject[] pageDots;

        [Tooltip("On PageChange Event")] public UnityEvent onPageChangeEvent;
        public UnityEvent onChangeThresholdDistanceEvent;
        public int CurrentPageIndex => _currentPage;


        //------------------------------------------------------------------------
        void Start()
        {
            _scrollRectComponent = GetComponent<ScrollRect>();
            _scrollRectRect = GetComponent<RectTransform>();
            _container = _scrollRectComponent.content;
            _pageCount = _container.childCount;

            // is it horizontal or vertical scrollrect
            if (_scrollRectComponent.horizontal && !_scrollRectComponent.vertical)
            {
                _horizontal = true;
            }
            else if (!_scrollRectComponent.horizontal && _scrollRectComponent.vertical)
            {
                _horizontal = false;
            }
            else
            {
                Debug.LogWarning("Confusing setting of horizontal/vertical direction. Default set to horizontal.");
                _horizontal = true;
            }

            _lerp = false;

            // init
            SetPagePositions();
            SetPage(startingPage);
            InitPageSelection();
            SetPageSelection(startingPage);

            for (int i = 0; i < _pageCount; i++)
            {
                if (_currentPage != i)
                {
                    _container.GetChild(i).GetChild(0).gameObject.SetActive(false);
                }
            }

            // prev and next buttons
            if (nextButton)
                nextButton.GetComponent<Button>().onClick.AddListener(() => { NextScreen(); });

            if (prevButton)
                prevButton.GetComponent<Button>().onClick.AddListener(() => { PreviousScreen(); });
        }

        //------------------------------------------------------------------------
        void Update()
        {
            // if moving to target position
            if (_lerp)
            {
                //// prevent overshooting with values greater than 1
                //float decelerate = Mathf.Min(decelerationRate * Time.unscaledDeltaTime, 1f);
                //_container.anchoredPosition = Vector2.Lerp(_container.anchoredPosition, _lerpTo, decelerate);
                //// time to stop lerping?
                //if ((_container.anchoredPosition - _lerpTo).magnitude < 1f)
                //{
                //    // snap to target and stop lerping
                //    _container.anchoredPosition = _lerpTo;
                //    _lerp = false;
                //    // clear also any scrollrect move that may interfere with our lerping
                //    _scrollRectComponent.velocity = Vector2.zero;
                //}
                _lerp = false;

                _container.DOKill();
                _container.DOAnchorPos(_lerpTo, 0.18f).SetEase(Ease.Linear).OnUpdate(CheckSetActiveContent).OnComplete(() =>
                {
                    for (int i = 0; i < _pageCount; i++)
                    {
                        if (Mathf.Abs(i - _currentPage) <= 0)
                        {
                            _container.GetChild(i).GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            _container.GetChild(i).GetChild(0).gameObject.SetActive(false);
                        }
                    }

                    _container.DOKill();
                    _scrollRectComponent.velocity = Vector2.zero;
                    _container.anchoredPosition = _lerpTo;
                });
                // switches selection icon exactly to correct page
                if (_showPageSelection)
                {
                    SetPageSelection(GetNearestPage());
                }
            }
        }
        

        //------------------------------------------------------------------------
        private void SetPagePositions()
        {
            int offsetX;
            int offsetY;
            int containerWidth;
            int containerHeight;
            var width = (int)_scrollRectRect.rect.width;
            var height = (int)_scrollRectRect.rect.height;

            if (_horizontal)
            {
                offsetX = width / 2;
                offsetY = 0;

                containerWidth = (width) * _pageCount;
                containerHeight = (int)_container.sizeDelta.y;

                _fastSwipeThresholdMaxLimit = width;
            }
            else
            {
                offsetY = height / 2;
                offsetX = 0;
                containerHeight = height * _pageCount;
                containerWidth = (int)_container.sizeDelta.x;
                _fastSwipeThresholdMaxLimit = height;
            }

            // set width of container
            Vector2 newSize = new Vector2(containerWidth, containerHeight);
            _container.sizeDelta = newSize;
            Vector2 newPosition = new Vector2((float)containerWidth / 2, 0);
            _container.anchoredPosition = newPosition;

            // delete any previous settings
            _pagePositions.Clear();

            // iterate through all container childern and set their positions
            for (int i = 0; i < _pageCount; i++)
            {
                RectTransform child = _container.GetChild(i).GetComponent<RectTransform>();
                Vector2 childPosition;
                if (_horizontal)
                {
                    childPosition = new Vector2(i * width - (int)(containerWidth / 2f) + offsetX, 0);
                }
                else
                {
                    childPosition = new Vector2(offsetX, -(i * height - containerHeight / 2 + offsetY));
                }

                child.sizeDelta = new Vector2(width, height);
                child.anchoredPosition = childPosition;
                _pagePositions.Add(-childPosition);
            }
        }

        //------------------------------------------------------------------------
        public void SetPage(int aPageIndex)
        {
            _container.anchoredPosition = _pagePositions[aPageIndex];
            _currentPage = aPageIndex;
            if (nextButton)
                nextButton.SetActive(aPageIndex != _pageCount - 1);
            if (prevButton)
                prevButton.SetActive(aPageIndex != 0);
            SetDots(aPageIndex);
        }

        //------------------------------------------------------------------------
        public void LerpToPage(int aPageIndex)
        {
            aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
            _lerpTo = _pagePositions[aPageIndex];
            _lerp = true;
            _currentPage = aPageIndex;
            if (nextButton) nextButton.SetActive(aPageIndex != _pageCount - 1);
            if (prevButton) prevButton.SetActive(aPageIndex != 0);

            onPageChangeEvent?.Invoke();
        }

        //------------------------------------------------------------------------
        private void InitPageSelection()
        {
            // page selection - only if defined sprites for selection icons
            _showPageSelection = unselectedPage != null && selectedPage != null;
            if (_showPageSelection)
            {
                // also container with selection images must be defined and must have exatly the same amount of items as pages container
                if (pageSelectionIcons == null || pageSelectionIcons.childCount != _pageCount)
                {
                    Debug.LogWarning("Different count of pages and selection icons - will not show page selection");
                    _showPageSelection = false;
                }
                else
                {
                    _previousPageSelectionIndex = -1;
                    Debug.LogWarning("Init");

                    _pageSelectionImages = new List<Image>();

                    // cache all Image components into list
                    for (int i = 0; i < pageSelectionIcons.childCount; i++)
                    {
                        Image image = pageSelectionIcons.GetChild(i).GetComponent<Image>();
                        if (image == null)
                        {
                            Debug.LogWarning("Page selection icon at position " + i + " is missing Image component");
                        }

                        _pageSelectionImages.Add(image);
                    }
                }
            }
        }

        //------------------------------------------------------------------------
        private void SetPageSelection(int aPageIndex)
        {
            // nothing to change
            if (_previousPageSelectionIndex == aPageIndex)
            {
                return;
            }

            if (!_showPageSelection)
            {
                return;
            }

            // unselect old
            if (_previousPageSelectionIndex >= 0)
            {
                _pageSelectionImages[_previousPageSelectionIndex].sprite = unselectedPage;
                _pageSelectionImages[_previousPageSelectionIndex].SetNativeSize();
            }

            // select new
            _pageSelectionImages[aPageIndex].sprite = selectedPage;
            _pageSelectionImages[aPageIndex].SetNativeSize();

            _previousPageSelectionIndex = aPageIndex;
            SetDots(aPageIndex);
        }

        //------------------------------------------------------------------------
        private void NextScreen()
        {
            LerpToPage(_currentPage + 1);
            //LerpToPage(_currentPage);
            SetDots(_currentPage);
        }

        //------------------------------------------------------------------------
        private void PreviousScreen()
        {
            LerpToPage(_currentPage - 1);
            //LerpToPage(_currentPage);
            SetDots(_currentPage);
        }

        //------------------------------------------------------------------------
        private int GetNearestPage()
        {
            // based on distance from current position, find nearest page
            Vector2 currentPosition = _container.anchoredPosition;

            float distance = float.MaxValue;
            int nearestPage = _currentPage;

            for (int i = 0; i < _pagePositions.Count; i++)
            {
                float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
                if (testDist < distance)
                {
                    distance = testDist;
                    nearestPage = i;
                }
            }

            return nearestPage;
        }

        //------------------------------------------------------------------------
        private int GetShowingOtherCurrentPage()
        {
            // based on distance from current position, find nearest page
            Vector2 currentPosition = _container.anchoredPosition;

            float distance = float.MaxValue;
            int nearestPage = _currentPage;

            for (int i = 0; i < _pagePositions.Count; i++)
            {
                float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
                if (testDist < distance)
                {
                    distance = testDist;
                    nearestPage = i;
                }
            }

            if (nearestPage == _currentPage)
            {
                if (Mathf.Approximately(currentPosition.x, _pagePositions[_currentPage].x))
                {
                }
                else if (currentPosition.x < _pagePositions[_currentPage].x)
                {
                    nearestPage++;
                }
                else
                {
                    nearestPage--;
                }
            }

            return nearestPage;
        }

        //------------------------------------------------------------------------
        public void OnBeginDrag(PointerEventData aEventData)
        {
            // if currently lerping, then stop it as user is draging
            _lerp = false;
            // not dragging yet
            _dragging = false;

            CheckSetActiveContent();
        }

        private void CheckSetActiveContent()
        {
            for (int i = 0; i < _pageCount; i++)
            {
                if (i == GetShowingOtherCurrentPage() || i == _currentPage)
                {
                    _container.GetChild(i).GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    _container.GetChild(i).GetChild(0).gameObject.SetActive(false);
                }
            }
        }

        //------------------------------------------------------------------------
        public void OnEndDrag(PointerEventData aEventData)
        {
            // how much was container's content dragged
            float difference;
            if (_horizontal)
            {
                difference = _startPosition.x - _container.anchoredPosition.x;
            }
            else
            {
                difference = -(_startPosition.y - _container.anchoredPosition.y);
            }

            // test for fast swipe - swipe that moves only +/-1 item
            if (Time.unscaledTime - _timeStamp < fastSwipeThresholdTime &&
                Mathf.Abs(difference) > fastSwipeThresholdDistance &&
                Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit)
            {
                if (difference > 0)
                {
                    NextScreen();
                    //onChangeThresholdDistanceEvent?.Invoke();
                }
                else
                {
                    PreviousScreen();
                    //onChangeThresholdDistanceEvent?.Invoke();
                }
            }
            else
            {
                // if not fast time, look to which page we got to
                LerpToPage(GetNearestPage());
            }

            _dragging = false;
        }

        //------------------------------------------------------------------------
        public void OnDrag(PointerEventData aEventData)
        {
            if (!_dragging)
            {
                // dragging started
                _dragging = true;
                // save time - unscaled so pausing with Time.scale should not affect it
                _timeStamp = Time.unscaledTime;
                // save current position of cointainer
                _startPosition = _container.anchoredPosition;
                _container.DOKill();
            }
            else
            {
                if (_showPageSelection)
                {
                    SetPageSelection(GetNearestPage());
                }
            }
        
            CheckSetActiveContent();
        }

        void SetDots(int index)
        {
            for (int i = 0; i < pageDots.Length; i++)
            {
                if (i == index)
                {
                    pageDots[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                    pageDots[i].transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
}
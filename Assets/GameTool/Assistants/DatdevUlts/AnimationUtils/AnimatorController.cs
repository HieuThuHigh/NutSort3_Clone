using System;
using System.Collections;
using UnityEngine;

namespace DatdevUlts.AnimationUtils
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorController : MonoBehaviour
    {
        [SerializeField] private bool _loop;
        [SerializeField] private bool _pausing;
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private bool _ignoreTimeScale;
        [SerializeField] private float _defaultMixDuration = 0.25f;
        [SerializeField] private bool _keepUpdate;
        [SerializeField] private bool _enableLog;
        [AnimName] [SerializeField] private string _animName;
        private string m_animName;
        private Animator _animator;
        private AnimatorEvent _animatorEvent;
        private bool _pausingLoop;
        private bool _awaked;
        private bool _protectEnd;
        private float _allowEndTime;

        public bool Pause
        {
            get => _pausing;
            set => _pausing = value;
        }

        private int Pausing => _pausing ? 0 : 1;

        public bool IgnoreTimeScale
        {
            get => _ignoreTimeScale;
            set => _ignoreTimeScale = value;
        }

        public bool Loop
        {
            get => _loop;
            set => _loop = value;
        }

        public bool Awaked => _awaked;

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = value;
        }

        public string AnimName
        {
            get => m_animName;
            set => m_animName = value;
        }

        public bool EnableLog
        {
            get => _enableLog;
            set => _enableLog = value;
        }

        public Action OnStartAnim
        {
            get => _onStartAnim;
            set => _onStartAnim = value;
        }

        public Action OnEndAnim
        {
            get => _onEndAnim;
            set => _onEndAnim = value;
        }

        private bool PausingLoop
        {
            get => _pausingLoop;
            set => _pausingLoop = value;
        }

        private const float OffsetEnd = 0.0001f;

        private Action _onStartAnim;
        private Action _onEndAnim;

        public Animator Animator
        {
            get
            {
                SetupAnimator();

                return _animator;
            }
        }

        public AnimatorEvent AnimatorEvent
        {
            get
            {
                if (!_animatorEvent)
                {
                    _animatorEvent = GetComponent<AnimatorEvent>();
                }

                if (!_animatorEvent)
                {
                    _animatorEvent = gameObject.AddComponent<AnimatorEvent>();
                }
                
                return _animatorEvent;
            }
        }

        private void Awake()
        {
            Animator.enabled = false;
        }

        private void OnEnable()
        {
            Update(0);
            if (!_awaked)
            {
                StartCoroutine(CheckAwake());
            }

            IEnumerator CheckAwake()
            {
                yield return null;
                _awaked = true;
            }
        }

        private void SetupAnimator()
        {
            if (!_animator)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        public void Update()
        {
            var currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (_animName != m_animName)
            {
                SetAnimation(_animName);
                return;
            }

            if (_pausing)
            {
                return;
            }

            var length = currentAnimatorStateInfo.length;
            var currentTime = length *
                              (currentAnimatorStateInfo.normalizedTime - (int)currentAnimatorStateInfo.normalizedTime);

            var deltatime = Time.deltaTime * _timeScale;
            if (_ignoreTimeScale)
            {
                deltatime = Time.unscaledDeltaTime * _timeScale;
            }

            if (!_loop)
            {
                if (!PausingLoop && length - currentTime < deltatime && currentAnimatorStateInfo.IsName(m_animName))
                {
                    deltatime = length - currentTime - OffsetEnd;

                    Update(deltatime * Pausing);
                    PausingLoop = true;
                    OnEndAnim?.Invoke();
                }
                else if (!PausingLoop)
                {
                    Update(deltatime * Pausing);
                }
                else if (PausingLoop)
                {
                    if (_keepUpdate)
                    {
                        Update(0);
                    }
                }
            }
            else
            {
                if (length - currentTime < deltatime)
                {
                    deltatime = length - currentTime;
                    Update(deltatime * Pausing);
                    if (_protectEnd)
                    {
                        if (_allowEndTime < 0)
                        {
                            OnEndAnim?.Invoke();
                        }
                    }
                    else
                    {
                        OnEndAnim?.Invoke();
                    }

                    OnStartAnim?.Invoke();
                }
                else
                {
                    Update(deltatime * Pausing);
                }
            }

            _allowEndTime -= deltatime * Pausing;
        }

        public void SetAnimation(string animationName, bool loop, float timeScale = 1f, float mixDuration = -1f,
            Action onStart = null, Action onEnd = null, int layer = 0, bool quiet = true, bool protectEnd = true,
            bool forceCrossFade = true, bool forceReplay = false)
        {
            var has = Animator.HasState(layer, Animator.StringToHash(animationName));
            if (!has)
            {
                if (!quiet)
                {
                    if (_enableLog)
                    {
                        Debug.LogError($"State {animationName} of layer {layer} is NULL");
                    }

                    return;
                }
            }

            if (mixDuration <= -0.5f)
            {
                mixDuration = _defaultMixDuration;
            }

            if (layer == 0)
            {
                OnStartAnim = onStart;
                OnEndAnim = null;

                _loop = loop;
            }

            var currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(layer);

            var length = currentAnimatorStateInfo.length;
            var currentTime = length *
                              (currentAnimatorStateInfo.normalizedTime - (int)currentAnimatorStateInfo.normalizedTime);

            _protectEnd = protectEnd;
            if (_protectEnd)
            {
                _allowEndTime = mixDuration;
            }

            if (!forceCrossFade && mixDuration > length - currentTime - OffsetEnd)
            {
                mixDuration = length - currentTime - OffsetEnd * 2;
            }

            var animNameBefore = m_animName;
            
            if (layer == 0)
            {
                PausingLoop = false;

                m_animName = animationName;
                _animName = m_animName;
            }

            if (forceReplay || animNameBefore != m_animName)
            {
                if (!forceCrossFade && mixDuration <= OffsetEnd || mixDuration <= OffsetEnd)
                {
                    if (forceReplay)
                    {
                        Animator.Play(animationName, layer, 0);
                    }
                    else
                    {
                        Animator.Play(animationName, layer);
                    }
                }
                else
                {
                    Animator.CrossFadeInFixedTime(animationName, mixDuration, layer, 0);

                    if (Awaked)
                    {
                        if (!Pause)
                        {
                            Animator.Update(0);
                        }
                    }
                }
            }
            

            if (layer == 0)
            {
                _timeScale = timeScale;

                OnEndAnim = onEnd;
            }
        }

        public void SetAnimation(string animName)
        {
            SetAnimation(animName, _loop);
        }

        [ContextMenu("SetAnimation")]
        public void SetAnimation()
        {
            SetAnimation(m_animName);
        }

        public void Update(float deltaTime)
        {
            Animator.Update(deltaTime);
        }

        public void PostEvent(string eventName)
        {
            AnimatorEvent.InvokePostEvent(eventName);
        }

        public void RegistEvent(Action<string> callBack)
        {
            AnimatorEvent.InvokeRegistEvent(callBack);
        }

        public void RemoveEvent(Action<string> callBack)
        {
            AnimatorEvent.InvokeRemoveEvent(callBack);
        }
    }
}
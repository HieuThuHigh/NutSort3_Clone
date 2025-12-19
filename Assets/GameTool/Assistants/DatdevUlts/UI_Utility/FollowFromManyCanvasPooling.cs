using DatdevUlts.Ults;
using GameTool.ObjectPool.Scripts;
using UnityEngine;

public class FollowFromManyCanvasPooling : BasePooling
{
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] private bool _disableOnTargetDisable;
    [SerializeField] private Transform _target;
    [SerializeField] private RectTransform _fullRectParentTarget;
    [SerializeField] private RectTransform _fullRectParentThis;
    [SerializeField] private bool _isThisInCanvas;
    [SerializeField] private bool _followScale;
    [SerializeField] private float _scaleMultiply;
    [SerializeField] private float _planeDistance;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _delayDisable;
    private bool _disabled;

    public Camera Camera
    {
        get => _camera;
        set => _camera = value;
    }

    private void LateUpdate()
    {
        Follow();
    }

    public void Follow()
    {
        if (!_camera)
        {
            return;
        }

        if (_disableOnTargetDisable)
        {
            if (!_target || !_target.gameObject.activeInHierarchy)
            {
                _target = null;
                if (!_disabled)
                {
                    this.DelayedCall(_delayDisable, () => gameObject.SetActive(false));
                }

                _disabled = true;

                return;
            }
        }

        var localPos = _fullRectParentTarget.InverseTransformPoint(_target.position);

        var vp = _fullRectParentTarget.LocalPositionToViewport(
            localPos);

        if (_isThisInCanvas)
        {
            Vector3 locPos = _fullRectParentThis.ViewportToCanvasPosition(vp);
            transform.localPosition = locPos;
        }
        else
        {
            vp.z = _planeDistance;
            var pos = _camera.ViewportToWorldPoint(vp);
            transform.position = pos;
        }

        if (_followScale)
        {
            transform.localScale = _target.transform.localScale * _scaleMultiply;
        }

        if (_particleSystem && !_particleSystem.isPlaying)
        {
            _particleSystem.Play();
        }
    }

    public void SetData(Transform target, RectTransform fullRectParentTarget, RectTransform fullRectParentThis)
    {
        _target = target;
        _fullRectParentTarget = fullRectParentTarget;
        _fullRectParentThis = fullRectParentThis;
        _isThisInCanvas = true;
    }

    public void SetData(Transform target, RectTransform fullRectParentTarget, float planeDistance)
    {
        _target = target;
        _fullRectParentTarget = fullRectParentTarget;
        _planeDistance = planeDistance;
        _isThisInCanvas = false;
        _disabled = false;
        if (_particleSystem)
            _particleSystem.Stop();
    }
}
namespace DatdevUlts.Ults
{
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class SingletonList<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly List<T> _instances = new List<T>();

        public static event System.Action<T> OnInstanceChanged;

        public static T Instance
        {
            get
            {
                if (_instances.Count == 0)
                    return null;

                return _instances[_instances.Count - 1];
            }
        }

        protected virtual void OnEnable()
        {
            T self = this as T;
            if (self == null)
            {
                Debug.LogError($"[SingletonList<{typeof(T).Name}] 'this' is not of type {typeof(T).Name}");
                return;
            }

            if (!_instances.Contains(self))
            {
                _instances.Add(self);
                NotifyInstanceChanged();
            }
        }

        protected virtual void OnDisable()
        {
            T self = this as T;
            if (_instances.Remove(self))
            {
                NotifyInstanceChanged();
            }
        }

        protected virtual void OnDestroy()
        {
            T self = this as T;
            if (_instances.Remove(self))
            {
                NotifyInstanceChanged();
            }
        }

        public static IReadOnlyList<T> AllInstances => _instances.AsReadOnly();

        private static void NotifyInstanceChanged()
        {
            OnInstanceChanged?.Invoke(Instance);
        }

        [ContextMenu("Log Current Singleton Instance")]
        private void LogInstance()
        {
            Debug.Log($"[SingletonList<{typeof(T).Name}>] Current instance: {Instance?.name}", Instance);
        }
    }
}

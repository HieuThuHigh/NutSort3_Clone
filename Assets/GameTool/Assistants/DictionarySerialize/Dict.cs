using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameTool.Assistants.DictionarySerialize
{
    [Serializable]
    public class Dict<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        public class _SerialKVPair<TKeyx, TValuex>
        {
            public TKeyx key;
            public TValuex value;

            [HideInInspector] public bool isDuplicate;

            public _SerialKVPair<TKeyx, TValuex> Reset()
            {
                key = default;
                return this;
            }
        }
        
        public bool isDuplicate;

        [SerializeField] private List<_SerialKVPair<TKey, TValue>> list = new List<_SerialKVPair<TKey, TValue>>();
        public Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

        #region DICTIONARY METHODS

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (dict.ContainsKey(item.Key))
            {
                Debug.LogError("The dictionary already contains the key " + item.Key);
            }
            else
            {
                dict.Add(item.Key, item.Value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                Debug.LogError("The dictionary already contains the key " + key);
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public void Clear()
        {
            dict.Clear();
        }

        public bool Contains(TValue item)
        {
            return dict.ContainsValue(item);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return dict.Remove(item.Key);
        }

        public int Count => dict.Count;
        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dict.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => dict[key];
            set => dict[key] = value;
        }

        public TValue GetElementAtKey(TKey key)
        {
            return dict[key];
        }

        public TValue GetElementAtKey(TKey key, TValue defaultValue)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            
            dict.Add(key, defaultValue);
            return dict[key];
        }

        public TValue GetElementAtKey(TKey key, Func<TValue> onNotContainsKey)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            
            TValue value = onNotContainsKey != null ? onNotContainsKey() : default;
            dict[key] = value;
            return value;
        }

        public void SetElementAtKey(TKey key, TValue value)
        {
            dict[key] = value;
        }

        public ICollection<TKey> Keys => dict.Keys;
        public ICollection<TValue> Values => dict.Values;

        public KeyValuePair<TKey, TValue> GetRandomElement()
        {
            return dict.ElementAt(Random.Range(0, dict.Count));
        }

#if UNITY_EDITOR
        /// <summary>
        /// CHỈ DÙNG TRONG EDITOR NHẰM SERIALIZE RA BÊN NGOÀI CHO DỄ NHÌN
        /// </summary>
        /// <returns>
        /// True: Thay đổi key thành công;
        /// False: Thay đổi key thất bại (đã tồn tại newKey này hoặc chưa có oldKey)
        /// </returns>
        public bool RenameKey(TKey oldKey, TKey newKey, Predicate<TKey> match)
        {
            if (dict.ContainsKey(newKey))
            {
                return false;
            }

            var item = list.Find(pair => match.Invoke(pair.key));
            if (item != null)
            {
                dict.Clear();
                item.key = newKey;
                return true;
            }

            return false;
        }

        /// <summary>
        /// CHỈ DÙNG TRONG EDITOR NHẰM SERIALIZE RA BÊN NGOÀI CHO DỄ NHÌN
        /// </summary>
        /// <returns>
        /// Trả về Count của List được Serialize ra bên ngoài
        /// </returns>
        public int CountOfList => list.Count;

        /// <summary>
        /// CHỈ DÙNG TRONG EDITOR NHẰM SERIALIZE RA BÊN NGOÀI CHO DỄ NHÌN
        /// </summary>
        /// <returns>
        /// Trả về List được Serialize ra bên ngoài
        /// </returns>
        public List<_SerialKVPair<TKey, TValue>> List => list;

        /// <summary>
        /// CHỈ DÙNG TRONG EDITOR NHẰM SERIALIZE RA BÊN NGOÀI CHO DỄ NHÌN
        /// </summary>
        /// <returns>
        /// </returns>
        public TValue ElementAtList(int index)
        {
            return list[index].value;
        }
#endif

        #endregion DICTIONARY METHODS

        /// <summary>
        /// Apply dict vào list
        /// </summary>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            foreach (var value in list)
            {
                if (list.FindIndex(pair => pair.key.Equals(value.key)) !=
                    list.FindLastIndex(pair => pair.key.Equals(value.key)))
                {
                    return;
                }
            }
#endif
            int index = 0;
            foreach (var value in dict)
            {
                if (index >= list.Count)
                {
                    list.Add(new _SerialKVPair<TKey, TValue>() { key = value.Key, value = value.Value });
                }
                else
                {
                    if (list[index] != null)
                    {
                        list[index].key = value.Key;
                        list[index].value = value.Value;
                    }
                    else
                    {
                        list[index] = new _SerialKVPair<TKey, TValue>() { key = value.Key, value = value.Value };
                    }
                }

                index++;
            }

            for (int i = index; i < list.Count; i++)
            {
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Apply list vào dict
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (dict == null)
            {
                dict = new Dictionary<TKey, TValue>();
            }

            // Xoá dict trước
            dict.Clear();
#if UNITY_EDITOR
            isDuplicate = false;
#endif

            for (int i = 0; i < list.Count; i++)
            {
                if (!dict.ContainsKey(list[i].key))
                {
                    dict.Add(list[i].key, list[i].value);
#if UNITY_EDITOR
                    list[i].isDuplicate = false;
#endif
                }
                else
                {
#if UNITY_EDITOR
                    list.Find(pair => pair.key.Equals(list[i].key)).isDuplicate = true;
                    list[i].isDuplicate = true;
                    isDuplicate = true;
#endif
                }
            }
        }
    }
}
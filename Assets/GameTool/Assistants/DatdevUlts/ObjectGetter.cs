using System.Collections.Generic;
using GameTool.Assistants.DictionarySerialize;
using UnityEngine;

namespace DatdevUlts
{
    public class ObjectGetter : MonoBehaviour
    {
        [SerializeField] private List<Object> _listObject = new List<Object>();
        [SerializeField] private Dict<string, Object> _dictObject = new Dict<string, Object>();

        public List<Object> ListObject => _listObject;

        public Dict<string, Object> DictObject => _dictObject;
    }
}
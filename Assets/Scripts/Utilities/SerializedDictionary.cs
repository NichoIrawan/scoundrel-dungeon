using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new();
        [SerializeField] private List<TValue> values = new();

        public SerializedDictionary()
        {
        }

        public SerializedDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            var count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}

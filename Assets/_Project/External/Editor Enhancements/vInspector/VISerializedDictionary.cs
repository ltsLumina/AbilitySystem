#region
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
#endregion

namespace VInspector
{
[Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    public List<SerializedKeyValuePair<TKey, TValue>> serializedKvps = new ();

    [UsedImplicitly] public float dividerPos = .33f;

    public void OnBeforeSerialize()
    {
        foreach (KeyValuePair<TKey, TValue> kvp in this)
        {
            SerializedKeyValuePair<TKey, TValue> serializedKvp = serializedKvps.FirstOrDefault(r => Comparer.Equals(r.Key, kvp.Key));

            if (serializedKvp != null) serializedKvp.Value = kvp.Value;
            else serializedKvps.Add(kvp);
        }

        serializedKvps.RemoveAll(r => !ContainsKey(r.Key));

        for (int i = 0; i < serializedKvps.Count; i++) { serializedKvps[i].index = i; }
    }

    public void OnAfterDeserialize()
    {
        Clear();

        serializedKvps.RemoveAll(r => r.Key == null);

        foreach (SerializedKeyValuePair<TKey, TValue> serializedKvp in serializedKvps)
        {
            if (!(serializedKvp.isKeyRepeated = ContainsKey(serializedKvp.Key))) Add(serializedKvp.Key, serializedKvp.Value);
        }
    }

    [Serializable]
    public class SerializedKeyValuePair<TKey_, TValue_>
    {
        public TKey_ Key;
        public TValue_ Value;

        public int index;
        public bool isKeyRepeated;

        public SerializedKeyValuePair(TKey_ key, TValue_ value)
        {
            Key   = key;
            Value = value;
        }

        public static implicit operator SerializedKeyValuePair<TKey_, TValue_>(KeyValuePair<TKey_, TValue_> kvp) => new (kvp.Key, kvp.Value);

        public static implicit operator KeyValuePair<TKey_, TValue_>(SerializedKeyValuePair<TKey_, TValue_> kvp) => new (kvp.Key, kvp.Value);
    }
}
}

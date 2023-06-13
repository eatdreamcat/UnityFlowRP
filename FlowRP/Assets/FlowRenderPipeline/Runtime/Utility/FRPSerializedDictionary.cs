using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.Rendering.FlowPipeline
{
   [Serializable]
    public abstract class FRPSerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> m_KeyData = new List<TKey>();
	  
        [SerializeField]
        private List<TValue> m_ValueData = new List<TValue>();
   
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < this.m_KeyData.Count && i < this.m_ValueData.Count; i++)
            {
                this[this.m_KeyData[i]] = this.m_ValueData[i];
            }
        }
   
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.m_KeyData.Clear();
            this.m_ValueData.Clear();
   
            foreach (var item in this)
            {
                this.m_KeyData.Add(item.Key);
                this.m_ValueData.Add(item.Value);
            }
        }
    }
}
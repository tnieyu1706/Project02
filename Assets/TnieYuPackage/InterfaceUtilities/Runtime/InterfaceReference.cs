using System;
using UnityEngine;

namespace InterfaceUtilities.Runtime
{
    [Serializable]
    public class InterfaceReference<TInterface, TObject>
        where TInterface : class
        where TObject : UnityEngine.Object
    {
        [SerializeField] private TObject obj;

        public TInterface Value
        {
            get
            {
                if (obj is not TInterface interfaceValue)
                    return null;

                return interfaceValue;
            }
        }

        public TObject Object
        {
            get => obj;
            set => obj = value;
        }
    }

    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, UnityEngine.Object>
        where TInterface : class
    {
    }
}
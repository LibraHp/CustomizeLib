using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.Attributes;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomizeLib.BepInEx.ExtensionData.Unity
{
    #pragma warning disable
    public static class ExtensionDataUnity
    {
        public static ExtDataRef<T> GetOrInitData<T>(this UnityEngine.Object obj, string name, T defaultValue = default(T))
        {
            if (!obj.TryGetStoredData(name, out _)) obj.SetData(name, defaultValue);
            return obj.GetData<T>(name);
        }

        public static ExtDataRef<T> GetData<T>(this UnityEngine.Object obj, string name)
        {
            if (obj is GameObject go) return GetData<T>(go, name);
            if (obj is Component comp) return GetData<T>(comp, name);
            return null;
        }

        public static ExtDataRef<T> GetOrInitData<T>(this GameObject obj, string name, T defaultValue = default(T))
        {
            if (!obj.TryGetStoredData(name, out _)) obj.SetData(name, defaultValue);
            return obj.GetData<T>(name);
        }

        public static ExtDataRef<T> GetData<T>(this GameObject obj, string name)
        {
            var dataComp = obj.GetOrAddComponent<DataComponent>();
            return new ExtDataRef<T>(obj, name);
        }

        public static ExtDataRef<T> GetOrInitData<T>(this Component obj, string name, T defaultValue = default(T))
        {
            if (!obj.TryGetStoredData(name, out _)) obj.SetData(name, defaultValue);
            return obj.GetData<T>(name);
        }

        public static ExtDataRef<T> GetData<T>(this Component obj, string name)
        {
            var dataComp = obj.gameObject.GetOrAddComponent<DataComponent>();
            return new ExtDataRef<T>(obj, name);
        }

        public static object? GetExistingData(this UnityEngine.Object obj, string name)
        {
            if (obj is GameObject go) return go.GetExistingData(name);
            if (obj is Component comp) return comp.GetExistingData(name);
            return null;
        }

        public static object? GetExistingData(this GameObject obj, string name)
        {
            obj.TryGetStoredData(name, out var value);
            return value;
        }

        public static object? GetExistingData(this Component obj, string name) =>
            obj == null ? null : obj.gameObject.GetExistingData(name);

        public static bool TryGetStoredData(this UnityEngine.Object obj, string name, out object? value)
        {
            if (obj is GameObject go) return go.TryGetStoredData(name, out value);
            if (obj is Component comp) return comp.TryGetStoredData(name, out value);
            value = null;
            return false;
        }

        public static bool TryGetStoredData(this GameObject obj, string name, out object? value)
        {
            value = null;
            if (obj == null) return false;

            var dataComp = obj.GetComponent<DataComponent>();
            return dataComp != null && dataComp.TryGetData(name, out value);
        }

        public static bool TryGetStoredData(this Component obj, string name, out object? value)
        {
            if (obj == null)
            {
                value = null;
                return false;
            }

            return obj.gameObject.TryGetStoredData(name, out value);
        }

        public static void SetData(this UnityEngine.Object obj, string name, object value)
        {
            if (obj is GameObject go) SetData(go, name, value);
            if (obj is Component comp) SetData(comp, name, value);
        }

        public static void SetData(this GameObject obj, string name, object value)
        {
            var dataComp = obj.GetOrAddComponent<DataComponent>();
            dataComp.SetData(name, value);
        }

        public static void SetData(this Component obj, string name, object value)
        {
            var dataComp = obj.gameObject.GetOrAddComponent<DataComponent>();
            dataComp.SetData(name, value);
        }
    }

    public class DataComponent : MonoBehaviour
    {
        public Dictionary<string, object> datas = new();

        [HideFromIl2Cpp]
        public object? GetData(string name)
        {
            return datas.TryGetValue(name, out var value) ? value : null;
        }

        [HideFromIl2Cpp]
        public bool TryGetData(string name, out object? value) => datas.TryGetValue(name, out value);

        [HideFromIl2Cpp]
        public void SetData(string name, object value)
        {
            datas[name] = value;
        }
    }

    public class ExtDataRef<T>
    {
        public T? val
        {
            get
            {
                return (T)((parent.GetOrAddComponent<DataComponent>().GetData(name) == null ?
                    default(T) : parent.GetOrAddComponent<DataComponent>().GetData(name)));
            }
            set => parent.GetOrAddComponent<DataComponent>().SetData(name, value);
        }
        public string name = "";
        public UnityEngine.Object? parent = null;

        public ExtDataRef(UnityEngine.Object parent, string name)
        {
            this.parent = parent;
            this.name = name;
        }

        public static implicit operator T(ExtDataRef<T> extDataRef) => extDataRef.val;
    }
}

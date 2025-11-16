using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(
    fileName = "New MapGridPointFeatureProperties",
    menuName = "Turnroot/Maps/Tile Feature Properties"
)]
public class MapGridPointFeatureProperties : ScriptableObject
{
    [Header("Feature identity")]
    [Tooltip("ID used to match this asset to a feature type (e.g. 'treasure').")]
    public string featureId = string.Empty;

    [Tooltip("Optional friendly name for the feature defaults asset.")]
    public string featureName = string.Empty;

    public interface IProperty
    {
        string key { get; set; }
        object GetValue();
        void SetValue(object value);
    }

    [System.Serializable]
    public class StringProperty : IProperty
    {
        public string key = string.Empty;
        public string value = string.Empty;

        string IProperty.key
        {
            get => key;
            set => key = value;
        }

        public object GetValue() => value;

        public void SetValue(object val) => value = val as string ?? string.Empty;
    }

    [System.Serializable]
    public class ObjectProperty : IProperty
    {
        public string key = string.Empty;
        public UnityEngine.Object value = null;

        string IProperty.key
        {
            get => key;
            set => key = value;
        }

        public object GetValue() => value;

        public void SetValue(object val) => value = val as UnityEngine.Object;
    }

    [System.Serializable]
    public class BoolProperty : IProperty
    {
        public string key = string.Empty;
        public bool value = false;

        string IProperty.key
        {
            get => key;
            set => key = value;
        }

        public object GetValue() => value;

        public void SetValue(object val) => value = val is bool b && b;
    }

    [System.Serializable]
    public class IntProperty : IProperty
    {
        public string key = string.Empty;
        public int value = 0;

        string IProperty.key
        {
            get => key;
            set => key = value;
        }

        public object GetValue() => value;

        public void SetValue(object val) => value = val is int i ? i : 0;
    }

    [System.Serializable]
    public class FloatProperty : IProperty
    {
        public string key = string.Empty;
        public float value = 0f;

        string IProperty.key
        {
            get => key;
            set => key = value;
        }

        public object GetValue() => value;

        public void SetValue(object val) => value = val is float f ? f : 0f;
    }

    public List<StringProperty> stringProperties = new();
    public List<ObjectProperty> objectProperties = new();
    public List<BoolProperty> boolProperties = new();
    public List<IntProperty> intProperties = new();
    public List<FloatProperty> floatProperties = new();
}


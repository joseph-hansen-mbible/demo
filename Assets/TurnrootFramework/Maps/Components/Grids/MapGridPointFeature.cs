using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapGridPointFeature
{
    public string typeId = string.Empty;
    public string name = string.Empty;

    [System.Serializable]
    public class Property
    {
        public string key = string.Empty;
        public string value = string.Empty;
    }

    public List<Property> properties = new List<Property>();

    public MapGridPointFeature() { }

    // Helper: map a feature type id string to a single-letter marker used by the editor overlay.
    public static string GetFeatureLetter(string typeId)
    {
        if (string.IsNullOrEmpty(typeId))
            return null;
        string fid = typeId.ToLower();
        if (fid.StartsWith("treasure"))
            return "T";
        if (fid.StartsWith("door"))
            return "D";
        if (fid.StartsWith("warp"))
            return "W";
        if (fid.StartsWith("healing"))
            return "H";
        if (fid.StartsWith("ranged"))
            return "R";
        if (fid.StartsWith("mechanism"))
            return "M";
        if (fid.StartsWith("control"))
            return "C";
        if (fid.StartsWith("breakable"))
            return "B";
        if (fid.StartsWith("shelter"))
            return "S";
        if (fid.StartsWith("underground"))
            return "U";
        return "?";
    }
}

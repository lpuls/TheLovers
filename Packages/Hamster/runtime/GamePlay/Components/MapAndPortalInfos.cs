using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR

[System.Serializable]
public class PortalInfos {
    public List<int> Keys = new List<int>();
    public List<string> Names = new List<string>();
}

public class MapAndPortalInfos : ScriptableObject {
    public List<int> MapKeys = new List<int>();
    public List<string> MapNames = new List<string>();

    public List<int> PortalMapKeys = new List<int>();
    public List<PortalInfos> PortalMapValues = new List<PortalInfos>();
}
#endif
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "PCGTerrainData", menuName = "ScriptableObjects/PCGTerrainData", order = 1)]
public class PCGTerrainConfigSerializableObject : ScriptableObject
{


    public List<PCGTerrain.PCGTerrainConfig> Config = new List<PCGTerrain.PCGTerrainConfig>();

}

#endif
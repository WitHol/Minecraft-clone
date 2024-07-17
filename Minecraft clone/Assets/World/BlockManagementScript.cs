using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManagementScript : MonoBehaviour
{
    public List<BlockTypeInfo> blockTypes;
}


[System.Serializable()]
public class BlockTypeInfo
{
    public string name;
    public bool transparent;

    public Vector2Int textureAtlasPos;
}
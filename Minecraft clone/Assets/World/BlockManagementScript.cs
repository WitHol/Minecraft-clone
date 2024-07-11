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

    public Texture2D northFace;
    public Texture2D southFace;
    public Texture2D eastFace;
    public Texture2D westFace;
    public Texture2D topFace;
    public Texture2D bottomFace;
}
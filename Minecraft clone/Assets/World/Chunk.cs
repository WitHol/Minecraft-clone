using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    
    public BlockData[,,] blockData = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    // The size of a chunk in blocks
    const int CHUNK_SIZE = 32;

    // The length of a side of a block
    const float BLOCK_SIZE = 0.5f;

    // Info about texture atlas
    const int TEXTURE_SIZE = 16;
    const int TEXTURE_ATLAS_ROW_CAPACITY = 16;
    const int TEXTURE_ATLAS_ROW_SIZE = TEXTURE_SIZE * TEXTURE_ATLAS_ROW_CAPACITY;

    // Management script, that stores data about block types
    BlockManagementScript blockManager;

    // The game object's mesh filter
    MeshFilter filter;

    // The mesh of a chunk and its data, initialized here so that it can be immidiately assigned to the mesh
    Mesh mesh = new Mesh();
    int[] triangles = new int[3];
    Vector3[] vertices = new Vector3[3];
    Vector2[] uv = new Vector2[3];
    Vector3[] normals = new Vector3[3];
    

    /// <summary>
    /// A constructor for the chunk class
    /// </summary>
    public void Start()
    {
        // Temporary
        for (int x = 0; x < 32; ++x)
        {
            for (int y = 0; y < 32; ++y)
            {
                for (int z = 0; z < 32; ++z)
                {
                    blockData[x, y, z] = y > 16 ? new BlockData { ID = 0 } : new BlockData { ID = 1 } ;
                }
            }
        }
        blockData[3, 24, 4].ID = 1;

        // Getting the block manager and object's mesh filter
        blockManager = FindObjectOfType<BlockManagementScript>();
        filter = GetComponent<MeshFilter>();

        recalculateMesh();

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //mesh.uv = uv;
        //mesh.normals = normals;

        filter.sharedMesh = mesh;
    }


    /// <summary>
    /// A function for tweaking mesh data arrays after sth changed in a chunk
    /// </summary>
    private void recalculateMesh()
    {
        bool[,,,] cullFaces = recalculateFaceCulling();

        List<int> localTriangles = new List<int>();
        List<Vector3> localVertices = new List<Vector3>();
        List<Vector2> localUV = new List<Vector2>();
        List<Vector3> localNormals = new List<Vector3>();

        int vertexIndex = 0;

        // Dictionaries for getting the local X, Y and Z of a face
        Dictionary<int, Vector3> faceXDic = new Dictionary<int, Vector3>()
        {
            { 0, Vector3.forward },
            { 1, Vector3.back },
            { 2, Vector3.left },
            { 3, Vector3.right },
            { 4, Vector3.right },
            { 5, Vector3.left },
        };

        Dictionary<int, Vector3> faceYDic = new Dictionary<int, Vector3>()
        {
            { 0, Vector3.up},
            { 1, Vector3.up },
            { 2, Vector3.up },
            { 3, Vector3.up },
            { 4, Vector3.forward },
            { 5, Vector3.back },
        };

        Dictionary<int, Vector3> faceZDic = new Dictionary<int, Vector3>()
        {
            { 0, Vector3.right },
            { 1, Vector3.left },
            { 2, Vector3.forward },
            { 3, Vector3.back },
            { 4, Vector3.up },
            { 5, Vector3.down },
        };

        for (int x = 0; x < CHUNK_SIZE; ++x)
        {
            for (int y = 0; y < CHUNK_SIZE; ++y)
            {
                for (int z = 0; z < CHUNK_SIZE; ++z)
                {
                    Vector3 unitBlockPos = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);

                    for (int face = 0; face < 6; ++face)
                    {
                        if (!cullFaces[x, y, z, face])
                        {
                            int isFaceBottom = face == 5 ? 1 : 0;

                            Vector3 faceX = faceXDic[face];
                            Vector3 faceY = faceYDic[face];
                            Vector3 faceZ = faceZDic[face];

                            Vector3 faceCentre = unitBlockPos + faceZ/2;

                            // Vertices
                            localVertices.Add(faceCentre - faceX / 2 + faceY / 2);
                            localVertices.Add(faceCentre + faceX / 2 + faceY / 2);
                            localVertices.Add(faceCentre - faceX / 2 - faceY / 2);
                            localVertices.Add(faceCentre + faceX / 2 - faceY / 2);

                            // First triangle
                            localTriangles.Add(vertexIndex);
                            localTriangles.Add(vertexIndex + 3 - isFaceBottom);
                            localTriangles.Add(vertexIndex + 2 + isFaceBottom);

                            // Secound triangle
                            localTriangles.Add(vertexIndex);
                            localTriangles.Add(vertexIndex + 1 + isFaceBottom*2);
                            localTriangles.Add(vertexIndex + 3 - isFaceBottom*2);

                            // UV
                            int blockID = blockData[x, y, z].ID;
                            Vector2 baseUVPos = blockManager.blockTypes[blockID].textureAtlasPos / TEXTURE_ATLAS_ROW_CAPACITY;

                            localUV.Add(baseUVPos + new Vector2(0, 0));
                            localUV.Add(baseUVPos + new Vector2(0, 1 / TEXTURE_ATLAS_ROW_CAPACITY));
                            localUV.Add(baseUVPos + new Vector2(1 / TEXTURE_ATLAS_ROW_CAPACITY, 0));
                            localUV.Add(baseUVPos + new Vector2(1 / TEXTURE_ATLAS_ROW_CAPACITY, 1 / TEXTURE_ATLAS_ROW_CAPACITY));

                            // Normals
                            localNormals.Add(faceZ);
                            localNormals.Add(faceZ);
                            localNormals.Add(faceZ);
                            localNormals.Add(faceZ);

                            vertexIndex += 4;
                        }
                    }
                }
            }
        }

        vertices = localVertices.ToArray();
        triangles = localTriangles.ToArray();
        uv = localUV.ToArray();
        normals = localNormals.ToArray();
    }
    
    /// <summary>
    /// A function, that properly sets the "cull faces" array
    /// </summary>
    private bool[,,,] recalculateFaceCulling()
    {
        bool[,,,] cullFaces = new bool[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE, 6];

        for (int x = 0; x < CHUNK_SIZE; ++x)
        {
            for (int y = 0; y < CHUNK_SIZE; ++y)
            {
                for (int z = 0; z < CHUNK_SIZE; ++z)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if(blockManager.blockTypes[ blockData[x, y, z].ID ].name == "air")
                    {
                        cullFaces[x, y, z, 0] = true;
                        cullFaces[x, y, z, 1] = true;
                        cullFaces[x, y, z, 2] = true;
                        cullFaces[x, y, z, 3] = true;
                        cullFaces[x, y, z, 4] = true;
                        cullFaces[x, y, z, 5] = true;
                    }
                    else
                    {
                        cullFaces[x, y, z, 0] = false;
                        cullFaces[x, y, z, 1] = false;
                        cullFaces[x, y, z, 2] = false;
                        cullFaces[x, y, z, 3] = false;
                        cullFaces[x, y, z, 4] = false;
                        cullFaces[x, y, z, 5] = false;
                    }

                    if (isBlockTransparent(pos)) continue;

                    if (!isBlockTransparent(pos + Vector3Int.right)) cullFaces[x, y, z, 0] = true;
                    if (!isBlockTransparent(pos + Vector3Int.left)) cullFaces[x, y, z, 1] = true;
                    if (!isBlockTransparent(pos + Vector3Int.forward)) cullFaces[x, y, z, 2] = true;
                    if (!isBlockTransparent(pos + Vector3Int.back)) cullFaces[x, y, z, 3] = true;
                    if (!isBlockTransparent(pos + Vector3Int.up)) cullFaces[x, y, z, 4] = true;
                    if (!isBlockTransparent(pos + Vector3Int.down)) cullFaces[x, y, z, 5] = true;
                }
            }
        }

        return cullFaces;
    }

    private bool isBlockTransparent(Vector3Int pos)
    {
        if (pos.x < 0 || pos.x > 31 ||
            pos.y < 0 || pos.y > 31 ||
            pos.z < 0 || pos.z > 31 )
            return true;

        return blockManager.blockTypes[ blockData[pos.x, pos.y, pos.z].ID ].transparent;
    }
}



public struct BlockData
{
    public int ID;
}
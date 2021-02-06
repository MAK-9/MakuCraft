using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Block[,,] chunkBlocks;
    public Texture2D[] atlasTextures;
    private Material blockMaterial;

    private Dictionary<string, Rect> atlasDictionary = new Dictionary<string, Rect>();
    void Start()
    {
        Texture2D atlas = GetTextureAtlas();
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = atlas;
        blockMaterial = material;
        
        StartCoroutine(GenerateChunk(16));
    }

    //Generate texture atlas
    Texture2D GetTextureAtlas()
    {
        Texture2D textureAtlas = new Texture2D(8192, 8192);
        Rect[] rectCoordinates = textureAtlas.PackTextures(atlasTextures, 0, 8192, false);
        textureAtlas.Apply();
        for (int i = 0; i < rectCoordinates.Length; i++)
        {
            atlasDictionary.Add(atlasTextures[i].name.ToLower(), rectCoordinates[i]);
        }

        return textureAtlas;
    }
    IEnumerator GenerateChunk(int chunkSize)
    {
        chunkBlocks = new Block[chunkSize, chunkSize, chunkSize];
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    chunkBlocks[x, y, z] = new Block((Block.BlockType)Random.Range(0,4), this.gameObject,
                        new Vector3(x, y, z), atlasDictionary);
                }
            }
        }
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    chunkBlocks[x, y, z].RenderBlock();
                }
            }
        }
        
        CombineSides();
        yield return null;

    }

    void CombineSides()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combineSides = new CombineInstance[meshFilters.Length];

        int i = 0;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            combineSides[i].mesh = meshFilter.sharedMesh;
            combineSides[i].transform = meshFilter.transform.localToWorldMatrix;
            i++;
        }
        
        MeshFilter blockMeshFilter = this.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        blockMeshFilter.mesh = new Mesh();
        blockMeshFilter.mesh.CombineMeshes(combineSides);
        
        MeshRenderer blockMeshRenderer = this.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        blockMeshRenderer.material = blockMaterial;

        foreach (Transform side in this.transform)
        {
            Destroy(side.gameObject);
        }
    }
}

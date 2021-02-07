﻿using UnityEngine;

public class Chunk
{
    public Block[,,] chunkBlocks;
    private Material blockMaterial;
    public GameObject chunkObject;

    public Chunk(string name, Vector3 position, Material material)
    {
        this.chunkObject = new GameObject(name);
        this.chunkObject.transform.position = position;
        this.blockMaterial = material;
        GenerateChunk(16);
    }

    private void GenerateChunk(int chunkSize)
    {
        chunkBlocks = new Block[chunkSize, chunkSize, chunkSize];
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    chunkBlocks[x, y, z] = new Block(World.blockTypes[Random.Range(0,4)], this,
                        new Vector3(x, y, z), World.atlasDictionary);
                }
            }
        }
    }

    public void DrawChunk(int chunkSize)
    {
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
    }
    void CombineSides()
    {
        MeshFilter[] meshFilters = chunkObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combineSides = new CombineInstance[meshFilters.Length];

        int i = 0;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            combineSides[i].mesh = meshFilter.sharedMesh;
            combineSides[i].transform = meshFilter.transform.localToWorldMatrix;
            i++;
        }
        
        MeshFilter blockMeshFilter = chunkObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        blockMeshFilter.mesh = new Mesh();
        blockMeshFilter.mesh.CombineMeshes(combineSides);
        
        MeshRenderer blockMeshRenderer = chunkObject.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        blockMeshRenderer.material = blockMaterial;

        foreach (Transform side in chunkObject.transform)
        {
            GameObject.Destroy(side.gameObject);
        }
    }
}

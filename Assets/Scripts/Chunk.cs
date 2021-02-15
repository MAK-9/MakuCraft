﻿using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Block[,,] chunkBlocks;
    private Material[] blockMaterial;
    public GameObject chunkObject;
    public int vertexIndex { get; set; }
    
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<int> transparentTriangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();

    public enum chunkStatus { GENERATED, DRAWN, TO_DRAW };

    public chunkStatus status;

    public Chunk(string name, Vector3 position, Material[] material)
    {
        this.chunkObject = new GameObject(name);
        this.chunkObject.transform.position = position;
        this.blockMaterial = material;
        this.status = chunkStatus.GENERATED;
        GenerateChunk(16);
    }

    private void GenerateChunk(int chunkSize)
    {
        chunkBlocks = new Block[chunkSize, chunkSize, chunkSize];
        Biome biome = BiomeUtils.SelectBiome(this.chunkObject.transform.position);
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float worldX = x + chunkObject.transform.position.x;
                    float worldY = y + chunkObject.transform.position.y;
                    float worldZ = z + chunkObject.transform.position.z;
                    BlockType biomeBlock = biome.GenerateTerrain(worldX, worldY, worldZ);
                    chunkBlocks[x, y, z] = new Block(biomeBlock, this,
                        new Vector3(x, y, z));
                    
                    if(biomeBlock == World.blockTypes[BlockType.Type.AIR])
                    {
                        this.status = chunkStatus.TO_DRAW;
                        
                    }
                    
                    
                }
            }
        }

        if (status == chunkStatus.TO_DRAW)
        {
            string chunkName = (int) this.chunkObject.transform.position.x + "_" +
                               ((int) this.chunkObject.transform.position.y - 16) + "_" +
                               (int) this.chunkObject.transform.position.z;
            Chunk chunkBelow;

            if (World.chunks.TryGetValue(chunkName, out chunkBelow))
            {
                chunkBelow.status = chunkStatus.TO_DRAW;
            }
        }
    }

    public void RefreshChunk(string chunkName, Vector3 chunkPosition)
    {
        this.chunkObject = new GameObject(chunkName);
        this.chunkObject.transform.position = chunkPosition;

        foreach (Block block in chunkBlocks)
        {
            if (block.GetBlockType() == World.blockTypes[0])
            {
                this.status = chunkStatus.TO_DRAW;
                
                string name = (int) this.chunkObject.transform.position.x + "_" +
                                   ((int) this.chunkObject.transform.position.y - 16) + "_" +
                                   (int) this.chunkObject.transform.position.z;
                Chunk chunkBelow;

                if (World.chunks.TryGetValue(chunkName, out chunkBelow))
                {
                    chunkBelow.status = chunkStatus.TO_DRAW;
                }

                break;
            }
        }
    }

    public void DrawChunk(int chunkSize)
    {
        vertexIndex = 0;
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

        this.status = chunkStatus.DRAWN;
    }
    void CombineSides()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(),0);
        mesh.SetTriangles(transparentTriangles.ToArray(),1);
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        
        MeshFilter blockMeshFilter = chunkObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        blockMeshFilter.mesh = mesh;
        
        MeshRenderer blockMeshRenderer = chunkObject.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        blockMeshRenderer.materials = blockMaterial;

        chunkObject.AddComponent(typeof(MeshCollider));

        foreach (Transform side in chunkObject.transform)
        {
            GameObject.Destroy(side.gameObject);
        }
    }
}

﻿using System.CodeDom.Compiler;
using UnityEngine;

public class Chunk
{
    public Block[,,] chunkBlocks;
    private Material blockMaterial;
    public GameObject chunkObject;

    public enum chunkStatus { GENERATED, DRAWN, TO_DRAW };

    public chunkStatus status;

    public Chunk(string name, Vector3 position, Material material)
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
        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float worldX = x + chunkObject.transform.position.x;
                    float worldY = y + chunkObject.transform.position.y;
                    float worldZ = z + chunkObject.transform.position.z;
                    float blockTypeProbability = ChunkUtils.CalculateBlockProbability(worldX, worldY, worldZ);
                    int generated1stLayerY = (int)ChunkUtils.Generate1stLayerHeight(worldX,worldZ);
                    int generated2ndLayerY =
                        (int) ChunkUtils.Generate2ndLayerHeight(worldX, worldZ, generated1stLayerY);

                    if (worldY == generated1stLayerY)
                    {
                        // grass layer
                        chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.GRASS], this,
                            new Vector3(x, y, z));
                    }
                    else if (blockTypeProbability > 0.65f && worldY < generated1stLayerY - 5)
                        chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.AIR], this,
                            new Vector3(x, y, z));
                    else if (worldY < generated2ndLayerY)
                    {
                        if (blockTypeProbability < 0.3f)
                        {
                            chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.DIAMOND], this,
                                new Vector3(x, y, z));
                        }
                        else if (blockTypeProbability < 0.4f)
                        {
                            chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.CARBON], this,
                                new Vector3(x, y, z));
                        }
                        else
                        {
                            chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.STONE], this,
                                new Vector3(x, y, z));
                        }
                    }
                    else if (worldY < generated1stLayerY)
                    {
                        chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.DIRT], this,
                            new Vector3(x, y, z));
                    }
                    else
                    {
                        this.status = chunkStatus.TO_DRAW;
                        chunkBlocks[x, y, z] = new Block(World.blockTypes[BlockType.Type.AIR], this,
                            new Vector3(x, y, z));
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

        chunkObject.AddComponent(typeof(MeshCollider));

        foreach (Transform side in chunkObject.transform)
        {
            GameObject.Destroy(side.gameObject);
        }
    }
}

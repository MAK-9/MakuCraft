using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Linq;

public class World : MonoBehaviour
{
    public Texture2D[] atlasTextures;
    public static Dictionary<string, Rect> atlasDictionary = new Dictionary<string, Rect>();
    public static Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
    private Material blockMaterial;
    public int columnHeight = 16;
    public int chunkSize = 16;
    public int worldRadius = 2;

    private GameObject player;
    private Vector2 lastPlayerPosition;
    private Vector2 currentPlayerPosition;

    public static List<BlockType> blockTypes = new List<BlockType>();

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        player.SetActive(false);
        UpdatePlayerPosition();
    }
    
    void Start()
    {
        Texture2D atlas = GetTextureAtlas();
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = atlas;
        this.blockMaterial = material;

        ChunkUtils.GenerateRandomOffset();
        GenerateBlockTypes();
        GenerateWorld();
        StartCoroutine(BuildWorld(true));
    }

    private void Update()
    {
        UpdatePlayerPosition();
        if (currentPlayerPosition != lastPlayerPosition)
        {
            lastPlayerPosition = currentPlayerPosition;
            GenerateWorld();
            StartCoroutine(BuildWorld());
        }
    }

    //Generate a column of chunks
    IEnumerator BuildWorld(bool isFirst = false)
    {
        foreach (Chunk chunk in chunks.Values.ToList())
        {
            if (chunk.status == Chunk.chunkStatus.TO_DRAW)
            {
                chunk.DrawChunk(chunkSize);
            }
            
            yield return null;
        }

        if (isFirst)
        {
            player.SetActive(true);
        }
    }

    void GenerateWorld()
    {
        for (int x = -worldRadius + (int)currentPlayerPosition.x - 1; x < worldRadius + (int)currentPlayerPosition.x + 1; x++)
        {
            for (int z = -worldRadius + (int)currentPlayerPosition.y - 1; z < worldRadius + (int)currentPlayerPosition.y + 1; z++)
            {
                for (int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                    string chunkName = GenerateChunkName(chunkPosition);
                    Chunk chunk;

                    if (z == -worldRadius + (int) currentPlayerPosition.y - 1
                        || z == worldRadius + (int) currentPlayerPosition.y + 1
                        || x == -worldRadius + (int) currentPlayerPosition.x - 1
                        || x == worldRadius + (int) currentPlayerPosition.x + 1)
                    {
                        if (chunks.TryGetValue(chunkName, out chunk))
                        {
                            chunk.status = Chunk.chunkStatus.GENERATED;
                            Destroy(chunk.chunkObject);
                        }
                        
                        continue;
                    }

                    if (chunks.TryGetValue(chunkName, out chunk))
                    {
                        if (chunk.status == Chunk.chunkStatus.GENERATED)
                        {
                            chunk.RefreshChunk(chunkName, chunkPosition);
                            chunk.chunkObject.transform.parent = this.transform;
                        }
                    }
                    else
                    {
                        chunk = new Chunk(chunkName,chunkPosition,blockMaterial);
                        chunk.chunkObject.transform.parent = this.transform;
                        chunks.Add(chunkName,chunk);
                    }
                }
            }
        }
    }

    //Generate chunk name based on its position ex.: 0_0_0
    string GenerateChunkName(Vector3 chunkPosition)
    {
        return (int) chunkPosition.x + "_" + (int) chunkPosition.y + "_" + (int) chunkPosition.z;
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

    void GenerateBlockTypes()
    {
        // 0
        BlockType air = new BlockType("air", true, true);
        air.sideUV = SetBlockTypeUV("air");
        air.GenerateBlockUVs();
        blockTypes.Add(air);
        
        BlockType dirt = new BlockType("dirt", false, true);
        dirt.sideUV = SetBlockTypeUV("dirt");
        dirt.GenerateBlockUVs();
        blockTypes.Add(dirt);
        
        BlockType brick = new BlockType("brick", false, true);
        brick.sideUV = SetBlockTypeUV("brick");
        brick.GenerateBlockUVs();
        blockTypes.Add(brick);
        
        // 3
        BlockType grass = new BlockType("grass", false, false);
        grass.sideUV = SetBlockTypeUV("grass_side");
        grass.topUV = SetBlockTypeUV("grass");
        grass.bottomUV = SetBlockTypeUV("dirt");
        grass.GenerateBlockUVs();
        blockTypes.Add(grass);
        
        // 4
        BlockType stone = new BlockType("stone", false, true);
        stone.sideUV = SetBlockTypeUV("stone");
        stone.GenerateBlockUVs();
        blockTypes.Add(stone);
    }

    Vector2[] SetBlockTypeUV(string name)
    {
        if (name == "air")
        {
            return new Vector2[4] 
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
        }
        
        return new Vector2[4]
        {
            new Vector2(atlasDictionary[name].x,
                atlasDictionary[name].y),
            new Vector2(
                atlasDictionary[name].x +
                atlasDictionary[name].width,
                atlasDictionary[name].y),
            new Vector2(atlasDictionary[name].x,
                atlasDictionary[name].y +
                atlasDictionary[name].height),
            new Vector2(
                atlasDictionary[name].x +
                atlasDictionary[name].width,
                atlasDictionary[name].y +
                atlasDictionary[name].height)

        };
    }

    void UpdatePlayerPosition()
    {
        currentPlayerPosition.x = Mathf.Floor(player.transform.position.x / 16);
        currentPlayerPosition.y = Mathf.Floor(player.transform.position.z / 16);
    }
}

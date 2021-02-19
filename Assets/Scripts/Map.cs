﻿using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Map : MonoBehaviour
{
    int gui_settingsWindow;
    public Rect gui_settingsWindowRect;

    public Texture2D heightMapTexture;
    public Texture2D albedoMapTexture;

    float warp = 0.0f;
    float noiseScale = 4.0f;

    public int width = 24;
    public int height = 8;

    int chunkSize = 32;

    // Voxels, 3D indexed by z, y, x
    Color[] voxelMap;

    public MeshFilter chunkPrefab;

    private Dictionary<Vector2Int, GameObject> spawnedChunks;
    private Queue<Vector2Int> dirtyChunks;

    // Start is called before the first frame update
    void Start()
    {
        spawnedChunks = new Dictionary<Vector2Int, GameObject>();
        dirtyChunks = new Queue<Vector2Int>();

        GenerateVoxels();

        CreateChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if (dirtyChunks.Count > 0)
        {
            var key = dirtyChunks.Dequeue();

            var chunk = spawnedChunks[key];

            var startX = key.x * chunkSize;
            var startZ = key.y * chunkSize;

            var mesh = chunk.GetComponent<MeshFilter>();
            mesh.mesh.Clear();
            mesh.mesh = GenerateMeshForRange(startX, 0, startZ, startX + chunkSize, height, startZ + chunkSize);

            var collider = chunk.GetComponent<MeshCollider>();
            collider.sharedMesh = mesh.mesh;
        }
    }

    public void BreakVoxel(Vector3Int pos)
    {
        SetVoxel(pos, new Color(0.0f, 0.0f, 0.0f, 0.0f));
    }

    public void SetVoxel(Vector3Int location, Color color)
    {
        if (
            location.x >= 0 && location.x < width &&
            location.y >= 0 && location.y < height &&
            location.z >= 0 && location.z < width)
        {
            voxelMap[IndexArray3D(location)] = color;

            var chunkIdX = (int)Mathf.Floor(location.x / chunkSize);
            var chunkIdZ = (int)Mathf.Floor(location.z / chunkSize);

            // Mark changed chunk as dirty.
            dirtyChunks.Enqueue(new Vector2Int(chunkIdX, chunkIdZ));
        }
    }

    int IndexArray3D(int x, int y, int z)
    {
        return z * (width * height) + y * (width) + x;
    }

    int IndexArray3D(Vector3Int id)
    {
        return id.z * (width * height) + id.y * (width) + id.x;
    }

    Color GetVoxel(Vector3Int location)
    {
        return voxelMap[IndexArray3D(location)];
    }

    Color GetVoxel(int x, int y, int z)
    {
        return voxelMap[IndexArray3D(x, y, z)];
    }

    void GenerateHeightmap()
    {
        //Color[] map = new Color[width * height];
        var heightmap = new float[width * width];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float initial = Mathf.PerlinNoise(x / (float)width * noiseScale, y / (float)width * noiseScale) * warp;
                float value = Mathf.PerlinNoise(x /(float)width * noiseScale + initial, y / (float)width * noiseScale + initial);
                heightmap[y * width + x] = value;
            }
        }

        var tex = new Texture2D(width, width, TextureFormat.RFloat, false);
        tex.SetPixelData(heightmap, 0);
        tex.Apply();

        heightMapTexture = tex;
    }

    void GenerateVoxels()
    {
        voxelMap = new Color[width * width * height];

        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float groundHeight = Mathf.Max(1.0f / height, heightMapTexture.GetPixelBilinear((float)x / width, (float)z / width).r);
                for (int y = 0; y < height; y++)
                {
                    voxelMap[z * (width * height) + y * width + x] = (y < groundHeight * height) ? albedoMapTexture.GetPixelBilinear((float)x / width, (float)z / width) : new Color(0.0f, 0.0f, 0.0f, 0.0f);
                }
            }
        }
    }

    void CreateChunks()
    {
        int chunks = width / chunkSize;
        // TODO WT: Parallelize this a bit, it could be faster
        for (int z = 0; z < chunks; z++)
        {
            for (int x = 0; x < chunks; x++)
            {
                //int startX = x * chunkSize;
                //int endX = Mathf.Min(startX + chunkSize, width);
                //int startZ = z * chunkSize;
                //int endZ = Mathf.Min(startZ + chunkSize, width);
                //var mesh = GenerateMeshForRange(startX, 0, startZ, endX, height, endZ);

                var chunk = Instantiate(chunkPrefab);
                chunk.gameObject.name = "Chunk(" + x + ", " + z + ")";
                //chunk.mesh = mesh;

                //chunk.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

                spawnedChunks[new Vector2Int(x, z)] = chunk.gameObject;
                dirtyChunks.Enqueue(new Vector2Int(x, z));
            }
        }

        Debug.Log("Queued " + dirtyChunks.Count + " chunks for building");
    }

    private Mesh GenerateMeshForRange(int startX, int startY, int startZ, int endX, int endY, int endZ)
    {
        var builder = new MeshBuilder();

        for (int z = startZ; z < endZ; z++)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    var color = GetVoxel(x, y, z);
                    if (color.a == 0.0f) continue;

                    if (y == height - 1 || GetVoxel(x, y + 1, z).a == 0.0f)
                    {
                        // +y
                        builder.AddQuad(new Vector3[4]
                        {
                            new Vector3(x,          y + 1.0f,   z),
                            new Vector3(x,          y + 1.0f,   z + 1.0f),
                            new Vector3(x + 1.0f,   y + 1.0f,   z + 1.0f),
                            new Vector3(x + 1.0f,   y + 1.0f,   z),
                        },
                        color,
                        Vector3.up);
                    }

                    if (y > 0 && GetVoxel(x, y - 1, z).a == 0.0f)
                    {
                        // -y
                        builder.AddQuad(new Vector3[4]
                        {
                            new Vector3(x + 1.0f,   y,          z),
                            new Vector3(x + 1.0f,   y,          z + 1.0f),
                            new Vector3(x,          y,          z + 1.0f),
                            new Vector3(x,          y,          z),
                        },
                        color,
                        Vector3.down);
                    }


                    if (x == width - 1 || GetVoxel(x + 1, y, z).a == 0.0f)
                    {
                        // +x
                        builder.AddQuad(new Vector3[4]
                        {
                        new Vector3(x + 1.0f,   y,          z),
                        new Vector3(x + 1.0f,   y + 1.0f,   z),
                        new Vector3(x + 1.0f,   y + 1.0f,   z + 1.0f),
                        new Vector3(x + 1.0f,   y,          z + 1.0f),
                        },
                        color,
                        Vector3.right);
                    }

                    if (x == 0 || GetVoxel(x - 1, y, z).a == 0.0f)
                    {
                        // -x
                        builder.AddQuad(new Vector3[4]
                        {
                        new Vector3(x,          y,          z + 1.0f),
                        new Vector3(x,          y + 1.0f,   z + 1.0f),
                        new Vector3(x,          y + 1.0f,   z),
                        new Vector3(x,          y,          z),
                        },
                        color,
                        Vector3.left);
                    }



                    if (z == width - 1 || GetVoxel(x, y, z + 1).a == 0.0f)
                    {
                        // +z
                        builder.AddQuad(new Vector3[4]
                        {
                        new Vector3(x + 1.0f,   y,          z + 1.0f),
                        new Vector3(x + 1.0f,   y + 1.0f,   z + 1.0f),
                        new Vector3(x,          y + 1.0f,   z + 1.0f),
                        new Vector3(x,          y,          z + 1.0f),
                        },
                        color,
                        Vector3.forward);
                    }

                    if (z == 0 || GetVoxel(x, y, z - 1).a == 0.0f)
                    {
                        // -z
                        builder.AddQuad(new Vector3[4]
                        {
                        new Vector3(x,          y,          z),
                        new Vector3(x,          y + 1.0f,   z),
                        new Vector3(x + 1.0f,   y + 1.0f,   z),
                        new Vector3(x + 1.0f,   y,          z),
                        },
                        color,
                        Vector3.back);
                    }
                }
            }
        }

        return builder.Build();
    }

    void ClearChunks()
    {
        foreach (var chunk in spawnedChunks)
        {
            Destroy(chunk.Value.gameObject);
        }

        spawnedChunks.Clear();
    }

    //private void OnGUI()
    //{
    //    gui_settingsWindowRect = GUILayout.Window(gui_settingsWindow, gui_settingsWindowRect, DoSettingsWindow, "Height map generation");
    //    gui_settingsWindowRect.position = Vector2.Max(gui_settingsWindowRect.position, Screen.safeArea.position);
    //    gui_settingsWindowRect.position = Vector2.Min(gui_settingsWindowRect.position, Screen.safeArea.size - gui_settingsWindowRect.size);
    //}

    void DoSettingsWindow(int window)
    {
        //GUILayout.Label("World Size");
        //currentPower = (int)GUILayout.HorizontalSlider(currentPower, minPower, maxPower);
        //width = (int)Mathf.Pow(2, currentPower);
        //GUILayout.Label("Dimensions: " + Mathf.Pow(2, currentPower).ToString());

        //GUILayout.Label("Scale");
        //noiseScale = GUILayout.HorizontalSlider(noiseScale, 1.0f, 100.0f);

        //GUILayout.Label("Warp");
        //warp = GUILayout.HorizontalSlider(warp, 0.0f, 1.0f);

        GUILayout.Label("Chunk: " + chunkSize);
        chunkSize = (int)GUILayout.HorizontalSlider(chunkSize, 8, 64);

        GUILayout.Label("Size: " + width);
        width = (int)GUILayout.HorizontalSlider(width, chunkSize, 1024);

        GUILayout.Label("Height: " + height);
        height = (int)GUILayout.HorizontalSlider(height, 4, 128);



        GUI.DrawTexture(GUILayoutUtility.GetRect(200, 200), heightMapTexture, ScaleMode.ScaleToFit);

        if (GUILayout.Button("Clear"))
        {
            ClearChunks();
        }

        if (GUILayout.Button("Build mesh"))
        {
            ClearChunks();
            GenerateVoxels();
            CreateChunks();
        }

        //if (GUILayout.Button("Regenerate")) GenerateHeightmap();

        GUI.DragWindow();
    }
}

class MeshBuilder
{
    Mesh mesh;

    List<Vector3> positions;
    List<Color> colors;
    List<Vector3> normals;

    List<int> indices;

    public MeshBuilder()
    {
        mesh = new Mesh();
        positions = new List<Vector3>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        indices = new List<int>();
    }

    public void AddQuad(Vector3[] positions, Color color, Vector3 normal)
    {
        if (positions.Length != 4)
        {
            throw new System.Exception("Incorrect number of positions");
        }

        var startVert = this.positions.Count;

        this.positions.AddRange(positions);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        indices.Add(startVert);
        indices.Add(startVert + 1);
        indices.Add(startVert + 2);

        indices.Add(startVert);
        indices.Add(startVert + 2);
        indices.Add(startVert + 3);
    }

    public Mesh Build()
    {
        mesh.SetVertices(positions);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }
}
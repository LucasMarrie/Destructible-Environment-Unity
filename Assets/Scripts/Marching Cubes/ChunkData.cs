using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Search;
using UnityEngine;

public class ChunkData
{
    public static float AIR = 1; //max air
    public static float SOLID = -1; // max solid

    private static readonly Vector3Int[] NEIGHBOURS = {
        new (1, 0, 0),
        new (-1, 0, 0),
        new (0, 1, 0),
        new (0, -1, 0),
        new (0, 0, 1),
        new (0, 0, -1),
    };
    //density should be a value between 1 and -1 with 0 being surface level and values below 0 being solid
    float[,,] densities;
    bool[,,] anchors;
    Vector3Int size;
    float scale;
    Vector3 centerPoint;

    public ChunkData(Vector3Int size, float scale)
    {
        this.size = size;
        densities = new float[size.x, size.y, size.z];
        anchors = new bool[size.x, size.y, size.z];
        this.scale = scale;
        this.centerPoint = ((Vector3)size - Vector3.one) / 2;
    }


    public IEnumerable<Cube> GetCubes()
    {
        foreach ((int x, int y, int z) in OverflowLoop())
        {
            int[,] corners = {
                {x, y, z},
                {x, y+1, z},
                {x+1, y+1, z},
                {x+1, y, z},
                {x, y, z+1},
                {x, y+1, z+1},
                {x+1, y+1, z+1},
                {x+1, y, z+1}
            };
            float[] vertDensities = new float[8];
            for (int i = 0; i < corners.GetLength(0); i++)
            {
                int cubeX = corners[i, 0], cubeY = corners[i, 1], cubeZ = corners[i, 2];
                vertDensities[i] = InBound(cubeX, cubeY, cubeZ) ? GetDensity(cubeX, cubeY, cubeZ) : AIR;
            }
            Cube cube = new Cube(corners, vertDensities, scale, centerPoint);
            yield return cube;
        }
    }

    bool InBound(int x, int y, int z)
    {
        return x >= 0 && x < size.x
            && y >= 0 && y < size.y
            && z >= 0 && z < size.z;
    }

    bool InBound(Vector3Int pos)
    {
        return InBound(pos.x, pos.y, pos.z);
    }

    public float GetVolume(){
        float mass = 0;
        foreach ((int x, int y, int z) in Loop()){
            float density = densities[x, y, z];
            if(density < 0){
                mass += -density;
            }
        }
        //relative to volumetric scale of object (3d)
        return mass * MathF.Pow(scale, 3);
    }

    public float GetAnchorage(){
        float anchoring = 0;
        foreach ((int x, int y, int z) in Loop()){
            if(anchors[x, y, z] && densities[x, y, z] < 0) anchoring++;
        }
        //relative to surface scale of object (2d)
        return anchoring * MathF.Pow(scale, 2);
    }

    public IEnumerable<(int, int, int)> Loop()
    {
        for (int x = 0; x < densities.GetLength(0); x++)
        {
            for (int y = 0; y < densities.GetLength(1); y++)
            {
                for (int z = 0; z < densities.GetLength(2); z++)
                {
                    yield return (x, y, z);
                }
            }
        }
    }

    public IEnumerable<(int, int, int)> Loop(int startX, int endX, int startY, int endY, int startZ, int endZ)
    {
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    yield return (x, y, z);
                }
            }
        }
    }

    public IEnumerable<(int, int, int)> OverflowLoop()
    {
        return Loop(-1, size.x + 1, -1, size.y + 1, -1, size.z + 1);
    }

    public float GetDensity(Vector3Int pos)
    {
        return densities[pos.x, pos.y, pos.z];
    }


    public float GetDensity(int x, int y, int z)
    {
        return densities[x, y, z];
    }

    public void SetDensities(Vector3Int[] points, float density)
    {
        foreach (Vector3Int point in points)
        {
            SetDensity(point, density);
        }
    }

    public void SetDensity(Vector3Int point, float density)
    {
        SetDensity(point.x, point.y, point.z, density);
    }

    public void SetDensity(int x, int y, int z, float density)
    {
        densities[x, y, z] = density;
    }

    public bool GetAnchor(Vector3Int pos)
    {
        return anchors[pos.x, pos.y, pos.z];
    }

    public bool GetAnchor(int x, int y, int z)
    {
        return anchors[x, y, z];
    }

    public void SetAnchors(Vector3Int[] points, bool anchored)
    {
        foreach (Vector3Int point in points)
        {
            SetAnchor(point, anchored);
        }
    }

    public void SetAnchor(Vector3Int point, bool anchored)
    {
        SetAnchor(point.x, point.y, point.z, anchored);
    }
    public void SetAnchor(int x, int y, int z, bool anchored)
    {
        anchors[x, y, z] = anchored;
    }


    //split chunk into sub chunks, if there is only one chunk (the current one), return an empty list   
    public List<(ChunkData, Vector3)> SplitChunk()
    {
        List<(ChunkData, Vector3)> newChunks = new();
        List<ChunkPart> parts = FindParts();
        if(parts.Count > 1){
            foreach (ChunkPart part in parts)
            {
                ChunkData newChunk = CreateChunk(part);
                Vector3 offset = (part.GetCenterPoint() - centerPoint) * scale;
                newChunks.Add((newChunk, offset));
            }
        }
        return newChunks;
    }

    private List<ChunkPart> FindParts()
    {
        int[,,] explored = new int[size.x, size.y, size.z];
        List<ChunkPart> parts = new();
        foreach ((int x, int y, int z) in Loop())
        {
            if (explored[x, y, z] == 0 && densities[x, y, z] < 0)
            {
                ChunkPart partData = new(x, y, z);
                ExplorePart(new(x, y, z), explored, partData);
                parts.Add(partData);
            }
        }
        return parts;
    }

    private void ExplorePart(Vector3Int startCoords, int[,,] explored, ChunkPart partData)
    {
        Queue<Vector3Int> queue = new();
        queue.Enqueue(startCoords);
        explored[startCoords.x, startCoords.y, startCoords.z] = 1;
        while(queue.Count > 0){
            Vector3Int coord = queue.Dequeue();
            partData.UpdateValues(coord.x, coord.y, coord.z);
            foreach (Vector3Int direction in NEIGHBOURS)
            {
                Vector3Int neighbour = coord + direction;
                if (InBound(neighbour) && GetDensity(neighbour) < 0 && explored[neighbour.x, neighbour.y, neighbour.z] == 0)
                {
                    explored[neighbour.x, neighbour.y, neighbour.z] = 1;
                    queue.Enqueue(neighbour);
                }
            }
        }

        
    }

    private ChunkData CreateChunk(ChunkPart partData)
    { 
        ChunkData chunk = new(partData.maxCoord - partData.minCoord + Vector3Int.one, scale);
        BuildChunk(partData.baseCoord, chunk, partData);
        return chunk;
    }

    private void BuildChunk(Vector3Int startCoords, ChunkData chunk, ChunkPart partData)
    {
        Queue<Vector3Int> queue = new();
        queue.Enqueue(startCoords);
        chunk.SetDensity(startCoords - partData.minCoord, GetDensity(startCoords));
        chunk.SetAnchor(startCoords - partData.minCoord, GetAnchor(startCoords));
        while(queue.Count > 0){
            Vector3Int coord = queue.Dequeue();
            foreach (Vector3Int direction in NEIGHBOURS)
            {
                Vector3Int neighbour = coord + direction;
                if (InBound(neighbour) && GetDensity(neighbour) < 0 && chunk.GetDensity(neighbour - partData.minCoord) == 0)
                {
                    chunk.SetDensity(neighbour - partData.minCoord, GetDensity(neighbour));
                    chunk.SetAnchor(neighbour - partData.minCoord, GetAnchor(neighbour));
                    queue.Enqueue(neighbour);
                }
            }
        }
    }

    private IEnumerable<(int, int, int)> GetBlockPoints(Vector3 position, float width, float height, float depth){

        Vector3Int start = Vector3Int.FloorToInt((position - new Vector3(width, height, depth)) / scale + centerPoint);
        start = Vector3Int.Max(Vector3Int.zero, start);
        Vector3Int end = Vector3Int.CeilToInt((position + new Vector3(width, height, depth)) / scale + centerPoint);
        end = Vector3Int.Min(end, size);

        return Loop(start.x, end.x, start.y, end.y, start.z, end.z);
    }

    public void DestroySphere(Vector3 position, float radius)
    {
        foreach ((int x, int y, int z) in GetBlockPoints(position, radius, radius, radius))
        {
            Vector3 point = (new Vector3(x, y, z) - centerPoint) * scale;
            float distance = Vector3.Distance(point, position);
            if (distance <= radius) 
            {
                float density = Mathf.Lerp(0, AIR, (radius - distance) / scale * 2);
                SetDensity(x, y, z, density);
            }
            else if (distance > radius && distance - radius < scale)
            {
                float density = Mathf.Lerp(-0.01f, SOLID, (distance - radius) / scale * 2);
                if(GetDensity(x, y, z) < density){
                    SetDensity(x, y, z, density);
                }
            }
        }
    }

}

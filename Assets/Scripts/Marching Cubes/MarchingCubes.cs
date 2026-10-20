using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes
{
    private ChunkData chunkData;
    private bool interpolated;

    public MarchingCubes(ChunkData chunkData, bool interpolated = false)
    {
        this.chunkData = chunkData;
        this.interpolated = interpolated;
    }


    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        List<int> triangles = new List<int>();
        Dictionary<Vector3, int> vertIndeces = new Dictionary<Vector3, int>();
        List<Vector3> uniqueVerts = new List<Vector3>();
        foreach (Vector3 vertex in GetVertices())
        {
            int vertIndex;
            if (vertIndeces.ContainsKey(vertex))
            {
                vertIndex = vertIndeces[vertex];
            }
            else
            {
                vertIndex = uniqueVerts.Count;
                uniqueVerts.Add(vertex);
                //TODO: fix weird lighting caused by shared vertices
                //vertIndeces.Add(vertex, vertIndex);
            }
            triangles.Add(vertIndex);
        }
        mesh.vertices = uniqueVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private IEnumerable<Vector3> GetVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (Cube cube in chunkData.GetCubes())
        {
            int triangulationIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if(cube.Vertices[i] < 0){
                    triangulationIndex |= 1 << i;
                }
            }

            for (int i = 0; i < 16; i++)
            {
                int edge = Table.triTable[triangulationIndex, i];
                if (edge == -1) break;
                int indexA = Table.cornerIndexAFromEdge[edge];
                int indexB = Table.cornerIndexBFromEdge[edge];
                
                Vector3 vertexPos = interpolated ? cube.InterpolatedEdgePoint(indexA, indexB) : cube.GetEdgePoint(indexA, indexB);
                yield return vertexPos;
            }
        }
    }

}


using UnityEngine;

public class CubeConstructor : MeshConstructor
{
    [SerializeField] Vector3Int dimensions;

    void Start(){
        Chunk = new ChunkData(dimensions, Scale);

        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Chunk.SetDensity(x, y, z, ChunkData.SOLID);
                }
            }
        }
        Init();
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, (Vector3)dimensions * Scale);
    }
}

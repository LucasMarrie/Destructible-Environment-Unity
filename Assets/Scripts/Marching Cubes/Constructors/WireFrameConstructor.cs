using System;
using UnityEngine;

public class WireFrameConstructor : MeshConstructor
{
    [SerializeField] Vector3Int size;
    [SerializeField] int frameThickness = 1;
    // Start is called before the first frame update
    void Start()
    {
        Chunk = new ChunkData(size, Scale);
        TraverseFrame((Vector3Int point) => Chunk.SetDensity(point, ChunkData.SOLID), frameThickness);
        Init();
    }

    void TraverseFrame(Action<Vector3Int> operation, int thickness)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    int conditions = 0;
                    if(x < thickness || x >= size.x-thickness) conditions++;
                    if(y < thickness || y >= size.y-thickness) conditions++;
                    if(z < thickness || z >= size.z-thickness) conditions++;
                    if(conditions >= 2){
                        operation(new Vector3Int(x,y,z));
                    }
                }
            }
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawGizmoFrame();
    }

    void DrawGizmoFrame()
    {
        Action<Vector3Int> gizmoAction = (Vector3Int point) => 
            Gizmos.DrawSphere(((Vector3)point - ((Vector3)size - Vector3.one) / 2) * Scale + transform.position, 0.25f * Scale);
        TraverseFrame(gizmoAction, frameThickness);
    }


}

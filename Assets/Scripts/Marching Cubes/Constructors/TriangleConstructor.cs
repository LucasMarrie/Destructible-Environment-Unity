using System;
using UnityEngine;

public class TriangleConstructor : MeshConstructor
{
    [SerializeField] Vector3Int size;
    // Start is called before the first frame update
    void Start()
    {
        Chunk = new ChunkData(size, Scale);
        TraverseSphere((vector3Int) => Chunk.SetDensity(vector3Int, ChunkData.SOLID));
        Init();
    }

    void TraverseSphere(Action<Vector3Int> operation){
        //draw 
        
        
    }

    
    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        DrawGizmoTriangle();
    }

    void DrawGizmoTriangle(){
        Action<Vector3Int> gizmoAction = (Vector3Int point) => 
            Gizmos.DrawSphere(((Vector3)point - ((Vector3)size - Vector3.one) / 2) * Scale + transform.position, 0.05f);
        TraverseSphere(gizmoAction);
    }
}

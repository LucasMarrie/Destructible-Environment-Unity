using UnityEngine;

public class CustomConstructor : MeshConstructor
{
    [SerializeField] Vector3Int size;
    [SerializeField] Vector3Int[] solidPoints;
    [SerializeField] Vector3Int[] anchors;


    void Start(){
        Chunk = new ChunkData(size, Scale);
        Chunk.SetDensities(solidPoints, ChunkData.SOLID);
        Chunk.SetAnchors(anchors, true);
        Init();
    }

    void OnDrawGizmos(){
        foreach(Vector3 point in solidPoints){
            Gizmos.color = Color.red;
            Gizmos.DrawSphere((point - ((Vector3)size - Vector3.one) / 2) * Scale + transform.position, 0.05f);
        }

        foreach(Vector3 point in anchors){
            Gizmos.color = Color.green;
            Gizmos.DrawSphere((point - ((Vector3)size - Vector3.one) / 2) * Scale + transform.position, 0.05f);
        }
        
    }
    
}

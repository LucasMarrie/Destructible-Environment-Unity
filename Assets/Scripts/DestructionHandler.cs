using UnityEngine;

public class DestructionHandler : MonoBehaviour, IDestructable
{
    MeshConstructor meshConstructor;

    void Start(){
        meshConstructor = GetComponent<MeshConstructor>();
    }

    public void DestroySphere(Vector3 position, float radius)
    {
        Vector3 relativePosition = Quaternion.Inverse(transform.rotation) * (position - transform.position);
        meshConstructor.Chunk.DestroySphere(relativePosition, radius);
        meshConstructor.RequestUpdate();
    }

    
}

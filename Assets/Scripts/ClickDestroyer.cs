using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickDestroyer : MonoBehaviour
{
    [SerializeField] float destructionRadius = 1f;
    [SerializeField] float range = 5f;
    [SerializeField] LayerMask destructableLayers = 1 << 6;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
    }

    void Inputs(){
        if(Input.GetMouseButtonDown(0)){
            CauseDestruction();
        }
    }

    void CauseDestruction(){
        Ray ray = new Ray(transform.position, transform.forward * range);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit)){
            //check if object contains Idestructible and cause destruction based on destruction radius
            foreach(Collider collider in Physics.OverlapSphere(hit.point, destructionRadius, destructableLayers)){
                IDestructable destructable = collider.transform.GetComponent<IDestructable>();
                if(destructable != null){
                    destructable.DestroySphere(hit.point, destructionRadius);
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshConstructor : MonoBehaviour {

    ChunkData chunkData;
    public ChunkData Chunk {
        get => chunkData;
        set {
            chunkData = value;
            cubeMarcher = new MarchingCubes(chunkData);
        }
    }
    [SerializeField] Material material;
    [SerializeField] float density = 1000;
    [SerializeField] float anchorStrength = 10000;
    [SerializeField] bool immovable = false;
    [SerializeField] float scale = 1;
    protected float Scale {get => scale;} 
    MarchingCubes cubeMarcher;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Rigidbody rb;
    static float updateTime = 0.1f;
    float timeTillUpdate = updateTime;
    bool updateRequested = false;

    protected void Init(){
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        rb = gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<DestructionHandler>(); 
        BuildMesh();
    }

    public void RequestUpdate(){
        updateRequested = true;
    }   

    void Update(){
        timeTillUpdate -= Time.deltaTime;
        if(timeTillUpdate < 0){
            if(updateRequested){
                UpdateMesh();
                updateRequested = false;
            }
            timeTillUpdate += updateTime;
        }
    }

    void UpdateMesh(){
        BuildMesh();
    }

    void BuildMesh(){
        if(SplitMesh()) return;
        Mesh mesh = cubeMarcher.CreateMesh();   
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        UpdatePhysics();
    }

    void UpdatePhysics(){
        if(immovable){
            rb.isKinematic = true;
            return;
        }
        float anchorage = Chunk.GetAnchorage() * anchorStrength;
        float mass = Chunk.GetVolume() * density;
        rb.mass = mass;
        bool attached = anchorage > mass;
        //if there isn't enough anchorage, object will be affected by physics
        meshCollider.convex = !attached;
        rb.isKinematic = attached;
    }

    bool SplitMesh(){
        List<(ChunkData, Vector3)> chunks = chunkData.SplitChunk();
        if(chunks.Count == 0) return false;
        foreach((ChunkData chunk, Vector3 offset) in chunks){
            CreateNewConstructor(chunk, transform.position + transform.rotation * offset);
        }
        Destroy(gameObject);
        return true;
    }

    void CreateNewConstructor(ChunkData newChunk, Vector3 position){
        GameObject newObject = new($"{name}*");
        newObject.transform.position = position;
        newObject.transform.rotation = transform.rotation;
        MeshConstructor constructor = newObject.AddComponent<MeshConstructor>();
        constructor.Chunk = newChunk;
        //copy over previous object parameters
        newObject.layer = gameObject.layer;
        newObject.tag = gameObject.tag;
        constructor.material = material;
        constructor.density = density;
        constructor.anchorStrength = anchorStrength;
        constructor.immovable = immovable;
        //build new object
        constructor.Init();
    }

}
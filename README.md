# Destructible Environment (Marching Cubes Prototype)

A Unity prototype for fully destructible objects. Each object is stored as a 3D grid of density values and turned into a mesh with the **marching cubes** algorithm. The player can carve spherical holes into any object, and when a cut leaves an object in several disconnected pieces, each piece becomes its own physics object.

> **Status:** prototype / proof of concept. Built with **Unity 2022.3.13f1**.
>
> This project is a prototype of a simulation feature, made to test whether marching-cubes-based destruction can work. The scene is a sandbox for trying out the system. With more work on its features and performance (see [Possible Future Work](#possible-future-work)), it could become a destruction system for a game.

## Features

- **Procedural voxel meshes:** objects are generated from a density field using marching cubes, at any resolution (grid size and voxel scale can be set per object).
- **Spherical destruction:** left-click raycasts from the camera and removes a sphere of material at the hit point, on every destructible object inside the radius.
- **Automatic separation:** after each cut, a flood fill finds connected solid regions. If there is more than one, the object is replaced by one new object per region, each with its own mesh, collider and rigidbody.
- **Mass and anchoring:** an object's mass comes from its solid volume. Voxels can be marked as *anchors* (attached to the world), and an object stays fixed as long as its anchor strength is greater than its mass. Cutting off enough anchors makes it fall.
- **Several shape generators:** a solid box, a wireframe (box edges only), and a custom shape built from hand-placed voxels and anchors.

## Controls

| Input | Action |
|---|---|
| `W` `A` `S` `D` | Move |
| `Space` / `Left Shift` | Move up / down |
| Mouse | Look around |
| `Left Alt` | Lock / unlock the cursor |
| Left Mouse Button | Destroy a sphere at the targeted point |

## How It Works

### 1. Density field — `ChunkData`

Each destructible object holds a `ChunkData`: a 3D `float` array of densities in the range `[-1, 1]`.

- `-1` (`SOLID`) is fully inside the material, `1` (`AIR`) is empty space, and `0` is the surface.
- A parallel `bool` array marks **anchor** voxels.
- The grid is centred on the object's origin, and `scale` sets the world-space size of one voxel.

Constructors (`CubeConstructor`, `WireFrameConstructor`, `CustomConstructor`) inherit from `MeshConstructor`. They fill the density grid in `Start()` and then call `Init()`, which adds the `MeshRenderer`, `MeshFilter`, `MeshCollider`, `Rigidbody` and `DestructionHandler` components at runtime.

### 2. Mesh generation — `MarchingCubes`, `Cube`, `Table`

`ChunkData.GetCubes()` steps through every cell of the grid. It runs one cell past each edge (`OverflowLoop`) and treats out-of-range samples as air, so the mesh always closes at the borders of the grid. For each cell:

1. The 8 corner densities are read, and each corner inside the material (`< 0`) sets one bit of an 8-bit **cube index** (256 possible cases).
2. The index is looked up in the standard triangulation table (`Table.triTable`), which lists which cube edges the surface crosses and how those edges form triangles.
3. Each edge becomes a vertex, either at the **midpoint** of the edge (blocky look) or **interpolated** using the two corner densities so it sits where the density reaches 0 (smoother surfaces).

The triangles are combined into a single Unity `Mesh`, and normals are recalculated.

### 3. Destruction — `ClickDestroyer`, `IDestructable`, `DestructionHandler`

- `ClickDestroyer` casts a ray from the camera and uses `Physics.OverlapSphere` at the hit point to find every collider on the destructible layer.
- Any object that implements `IDestructable` gets `DestroySphere(point, radius)`. `DestructionHandler` converts the world-space hit point into the object's local space (so rotated objects are cut correctly) and passes it to `ChunkData`.
- `ChunkData.DestroySphere` only visits the voxels inside the sphere's bounding box:
  - Voxels **inside** the radius are set to air, with values that fade towards 0 near the edge of the sphere.
  - Voxels in a thin shell **just outside** the radius are pulled towards the surface value. This gives the interpolated surface a clean spherical shape without removing extra material.
- The mesh rebuild is **throttled**: `MeshConstructor` collects requests and rebuilds at most once every 0.1 s, so clicking quickly doesn't rebuild the mesh every frame.

### 4. Splitting into separate objects — `ChunkData.SplitChunk`, `ChunkPart`

Before rebuilding a mesh, the object checks whether it has been cut into pieces:

1. **Find the parts:** a breadth-first flood fill over solid voxels (6-way neighbours) labels each connected region. While the fill runs, a `ChunkPart` records the region's bounding box.
2. **If there is more than one part**, a new, tightly sized `ChunkData` is made for each one. A second flood fill copies only that region's densities and anchors into it.
3. Each new chunk gets its own GameObject, positioned at the original transform plus the part's offset from the old centre, and copies the original's material, layer, tag and physics settings.
4. The original object is destroyed.

### 5. Physics — `MeshConstructor.UpdatePhysics`

- **Mass** = solid volume (sum of solid density × `scale³`) × material `density`.
- **Anchorage** = number of solid anchor voxels × `scale²` × `anchorStrength`.
- If anchorage > mass, the object is **kinematic** (held in place) and can use an accurate **non-convex** `MeshCollider`.
- Otherwise it becomes a **dynamic rigidbody**. Unity requires dynamic rigidbodies to have a **convex** collider, so the collider becomes a convex hull of the mesh.

## Project Structure

```
Assets/Scripts/
├── CameraController.cs        # Free-fly camera movement and mouse look
├── ClickDestroyer.cs          # Raycast + sphere overlap on click
├── IDestructable.cs           # Interface for objects that can be destroyed
├── DestructionHandler.cs      # Converts hits to local space and forwards them to the chunk
└── Marching Cubes/
    ├── ChunkData.cs           # Density/anchor grid, sphere destruction, flood-fill splitting
    ├── ChunkPart.cs           # Bounding-box data for one connected region
    ├── Cube.cs                # One marching cell: corner densities + edge vertex positions
    ├── MarchingCubes.cs       # Turns a ChunkData into a Unity Mesh
    ├── MeshConstructor.cs     # Base component: components, rebuilds, splitting, physics
    ├── Table.cs               # Standard marching cubes edge/triangle lookup tables
    └── Constructors/
        ├── CubeConstructor.cs       # Solid box
        ├── WireFrameConstructor.cs  # Box edges with configurable thickness
        ├── CustomConstructor.cs     # Hand-placed voxels and anchors (shown as gizmos)
        └── TriangleConstructor.cs   # Placeholder, not implemented yet
```

## Getting Started

1. Clone the repository.
2. Open the folder in **Unity Hub** with Unity **2022.3.13f1** (other 2022.3 LTS versions will probably work too).
3. Open `Assets/Scenes/SampleScene.unity` and press **Play**.

To add your own destructible object, create an empty GameObject on the destructible layer (layer 6 by default), add one of the constructor components, set its size, scale and material, and press Play.

## Known Limitations

- **Convex colliders on moving pieces:** Unity doesn't allow non-convex `MeshCollider`s on dynamic rigidbodies, so a falling piece collides as its convex hull. Concave shapes (rings, L-shapes, hollow frames) collide as if they were filled in, and other objects can't sit inside their holes.
- **No structural stress:** pieces only separate when they are fully disconnected. Two large pieces joined by a thin bridge stay one rigid object, however unrealistic that is.
- **Flat shading:** vertices aren't shared between triangles yet (see the TODO in `MarchingCubes.cs`), so the mesh looks faceted.
- **CPU and single-threaded:** remeshing and flood fills run on the main thread, so high-resolution objects can cause frame drops.

## Possible Future Work

- **Accurate concave collisions:** split each piece into several convex colliders (for example with V-HACD), or build compound box colliders from the voxel grid.
- **Breaking under stress:** model the forces on thin connections, such as bending or shear from the weight and leverage of the pieces on either side, and break a connection when it is overloaded. The grid could be treated as a graph of voxels whose bond strength depends on the cross-section between them.
- **Performance:** optimise the system, for example by moving work to the GPU with compute shaders, to support higher resolutions and more destructible objects in a scene.
- **More shapes and tools:** convert existing 3D models into destructible objects by preprocessing their shape into a density grid (like turning a sculpture into voxels), and add more destruction shapes (cuts, impacts, explosions with force).

using UnityEngine;

class ChunkPart
{
    public Vector3Int baseCoord;
    public Vector3Int minCoord;
    public Vector3Int maxCoord;
    public Vector3 centerPoint;

    public ChunkPart(int x, int y, int z)
    {
        baseCoord = new Vector3Int(x, y, z);
        minCoord.x = maxCoord.x = x;
        minCoord.y = minCoord.y = y;
        minCoord.z = maxCoord.z = z;
    }

    public Vector3 GetCenterPoint(){
        Vector3 size = maxCoord - minCoord;
        return minCoord + size / 2;
    }

    public void UpdateValues(int x, int y, int z)
    {
        if (x < minCoord.x) minCoord.x = x;
        else if (x > maxCoord.x) maxCoord.x = x;

        if (y < minCoord.y) minCoord.y = y;
        else if (y > maxCoord.y) maxCoord.y = y;

        if (z < minCoord.z) minCoord.z = z;
        else if (z > maxCoord.z) maxCoord.z = z;
    }
}
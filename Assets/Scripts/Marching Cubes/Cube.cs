using UnityEngine;

public class Cube
{
    float[] vertDensity;
    int[,] corners;
    float scale;
    Vector3 center;

    public float[] Vertices { get => vertDensity; }

    public Cube(int[,] corners, float[] vertDensity, float scale, Vector3 center)
    {
        this.scale = scale;
        this.corners = corners;
        this.vertDensity = vertDensity;
        this.center = center;
    }

    public Vector3 GetEdgePoint(int vertA, int vertB)
    {
        Vector3 cornerA = new Vector3(corners[vertA, 0], corners[vertA, 1], corners[vertA, 2]);
        Vector3 cornerB = new Vector3(corners[vertB, 0], corners[vertB, 1], corners[vertB, 2]);
        Vector3 point = (cornerB + cornerA) / 2;
        return (point - center) * scale;
    }

    public Vector3 InterpolatedEdgePoint(int vertA, int vertB)
    {
        float interpolation = (-vertDensity[vertA]) / (vertDensity[vertB] - vertDensity[vertA]);
        Vector3 cornerA = new Vector3(corners[vertA, 0], corners[vertA, 1], corners[vertA, 2]);
        Vector3 cornerB = new Vector3(corners[vertB, 0], corners[vertB, 1], corners[vertB, 2]);
        Vector3 point = cornerA + (cornerB - cornerA) * interpolation;
        return (point - center) * scale;
    }


}

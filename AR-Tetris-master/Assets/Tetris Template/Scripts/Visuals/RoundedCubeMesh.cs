using System.Collections.Generic;
using UnityEngine;

// Procedural rounded cube, centered at origin. Built once and shared by all blocks.
public static class RoundedCubeMesh
{
    private static Mesh cached;

    public static Mesh Shared
    {
        get
        {
            if (cached == null)
                cached = Build(size: 0.94f, radius: 0.12f, segments: 6);
            return cached;
        }
    }

    // Each face is a grid; vertices are pushed onto a box with rounded edges.
    public static Mesh Build(float size, float radius, int segments)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        float half = size * 0.5f;
        float inner = half - radius;

        AddFace(Vector3.forward, Vector3.right, Vector3.up, half, inner, radius, segments, vertices, normals, triangles);
        AddFace(Vector3.back, Vector3.left, Vector3.up, half, inner, radius, segments, vertices, normals, triangles);
        AddFace(Vector3.right, Vector3.back, Vector3.up, half, inner, radius, segments, vertices, normals, triangles);
        AddFace(Vector3.left, Vector3.forward, Vector3.up, half, inner, radius, segments, vertices, normals, triangles);
        AddFace(Vector3.up, Vector3.right, Vector3.back, half, inner, radius, segments, vertices, normals, triangles);
        AddFace(Vector3.down, Vector3.right, Vector3.forward, half, inner, radius, segments, vertices, normals, triangles);

        var mesh = new Mesh { name = "RoundedCube" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddFace(Vector3 normal, Vector3 axisU, Vector3 axisV, float half, float inner, float radius,
        int segments, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
    {
        int start = vertices.Count;
        int line = segments + 1;

        for (int y = 0; y <= segments; y++)
        {
            for (int x = 0; x <= segments; x++)
            {
                float u = Mathf.Lerp(-half, half, (float)x / segments);
                float v = Mathf.Lerp(-half, half, (float)y / segments);
                Vector3 point = normal * half + axisU * u + axisV * v;

                Vector3 core = new Vector3(
                    Mathf.Clamp(point.x, -inner, inner),
                    Mathf.Clamp(point.y, -inner, inner),
                    Mathf.Clamp(point.z, -inner, inner));
                Vector3 n = (point - core).normalized;

                vertices.Add(core + n * radius);
                normals.Add(n);
            }
        }

        for (int y = 0; y < segments; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                // Axes satisfy Cross(axisU, axisV) == normal, so this winding faces outward.
                int i = start + y * line + x;
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + line);
                triangles.Add(i + 1);
                triangles.Add(i + line + 1);
                triangles.Add(i + line);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ProjectUniverse.Environment.Destruction
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshHolePunch : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        [SerializeField] private Mesh cylinderMesh; // Assign a cylinder mesh in inspector

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();

            // Create default cylinder if none assigned
            if (cylinderMesh == null)
            {
                cylinderMesh = CreateCylinderMesh(0.5f, 2f, 16);  // Unit cylinder with 0.5 radius
            }
        }

        public void PunchHole(float radius, Vector3 worldPosition, Vector3 worldRotation)
        {
            // Transform cylinder to world space
            // Double the radius to account for cylinder mesh scale (diameter vs radius)
            Matrix4x4 cylinderMatrix = Matrix4x4.TRS(
                worldPosition,
                Quaternion.Euler(worldRotation),
                Vector3.one * (radius * 2f)  // Changed from radius to radius * 2
            );

            // Transform to local space of this object
            Matrix4x4 localMatrix = transform.worldToLocalMatrix * cylinderMatrix;

            // Perform boolean subtraction
            Mesh originalMesh = meshFilter.mesh;
            Mesh newMesh = BooleanSubtract(originalMesh, cylinderMesh, localMatrix);

            meshFilter.mesh = newMesh;

            // Handle collider switching
            UpdateCollider(newMesh);
        }

        private void UpdateCollider(Mesh newMesh)
        {
            // Remove any existing box collider
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Destroy(boxCollider);
            }

            // Ensure we have a mesh collider
            if (meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = gameObject.AddComponent<MeshCollider>();
                }
            }

            // Update the mesh collider with the new mesh
            meshCollider.sharedMesh = newMesh;
            meshCollider.convex = false;  // Non-convex to allow passing through holes
        }

        private Mesh BooleanSubtract(Mesh meshA, Mesh meshB, Matrix4x4 transformB)
        {
            // Get mesh data
            BSPTree treeA = new BSPTree(meshA);
            BSPTree treeB = new BSPTree(meshB, transformB);

            // Perform subtraction: A - B
            BSPTree result = treeA.Subtract(treeB);

            // Convert back to mesh
            return result.ToMesh();
        }

        private class BSPTree
        {
            private Node root;
            private List<Triangle> triangles;

            public BSPTree(Mesh mesh, Matrix4x4 transform = default)
            {
                if (transform == default)
                    transform = Matrix4x4.identity;

                triangles = new List<Triangle>();
                Vector3[] vertices = mesh.vertices;
                Vector2[] uvs = mesh.uv;
                Vector3[] normals = mesh.normals;
                int[] indices = mesh.triangles;

                for (int i = 0; i < indices.Length; i += 3)
                {
                    Triangle tri = new Triangle();
                    for (int j = 0; j < 3; j++)
                    {
                        int idx = indices[i + j];
                        tri.vertices[j] = transform.MultiplyPoint3x4(vertices[idx]);
                        tri.uvs[j] = uvs.Length > idx ? uvs[idx] : Vector2.zero;
                        tri.normals[j] = transform.MultiplyVector(normals.Length > idx ? normals[idx] : Vector3.zero).normalized;
                    }
                    triangles.Add(tri);
                }

                Build();
            }

            private BSPTree()
            {
                triangles = new List<Triangle>();
            }

            private class Node
            {
                public Plane plane;
                public List<Triangle> coplanar = new List<Triangle>();
                public Node front;
                public Node back;

                public Node(Triangle tri)
                {
                    Vector3 normal = Vector3.Cross(
                        tri.vertices[1] - tri.vertices[0],
                        tri.vertices[2] - tri.vertices[0]).normalized;
                    plane = new Plane(normal, tri.vertices[0]);
                }
            }

            private class Triangle
            {
                public Vector3[] vertices = new Vector3[3];
                public Vector2[] uvs = new Vector2[3];
                public Vector3[] normals = new Vector3[3];

                public Triangle Clone()
                {
                    Triangle t = new Triangle();
                    Array.Copy(vertices, t.vertices, 3);
                    Array.Copy(uvs, t.uvs, 3);
                    Array.Copy(normals, t.normals, 3);
                    return t;
                }

                public void Flip()
                {
                    Array.Reverse(vertices);
                    Array.Reverse(uvs);
                    Array.Reverse(normals);
                }
            }

            private void Build()
            {
                if (triangles.Count == 0) return;
                root = new Node(triangles[0]);
                BuildNode(root, triangles);
            }

            private void BuildNode(Node node, List<Triangle> tris)
            {
                List<Triangle> front = new List<Triangle>();
                List<Triangle> back = new List<Triangle>();

                foreach (var tri in tris)
                {
                    SplitTriangle(tri, node.plane, node.coplanar, front, back);
                }

                if (front.Count > 0)
                {
                    if (node.front == null)
                        node.front = new Node(front[0]);
                    BuildNode(node.front, front);
                }

                if (back.Count > 0)
                {
                    if (node.back == null)
                        node.back = new Node(back[0]);
                    BuildNode(node.back, back);
                }
            }

            public BSPTree Subtract(BSPTree other)
            {
                BSPTree result = new BSPTree();
                result.root = CloneNode(root);

                if (other.root != null)
                {
                    result.Invert();
                    result.ClipTo(other);
                    other = other.Clone();
                    other.ClipTo(result);
                    other.Invert();
                    other.ClipTo(result);
                    result.AddTriangles(other.GetAllTriangles());
                    result.Invert();
                }

                return result;
            }

            private void Invert()
            {
                InvertNode(root);
            }

            private void InvertNode(Node node)
            {
                if (node == null) return;

                node.plane = node.plane.flipped;
                foreach (var tri in node.coplanar)
                    tri.Flip();

                Node temp = node.front;
                node.front = node.back;
                node.back = temp;

                InvertNode(node.front);
                InvertNode(node.back);
            }

            private void ClipTo(BSPTree other)
            {
                ClipNode(root, other.root);
            }

            private void ClipNode(Node node, Node clipper)
            {
                if (node == null || clipper == null) return;

                node.coplanar = ClipTriangles(node.coplanar, clipper);

                if (node.front != null)
                    ClipNode(node.front, clipper);
                if (node.back != null)
                    ClipNode(node.back, clipper);
            }

            private List<Triangle> ClipTriangles(List<Triangle> triangles, Node clipper)
            {
                if (clipper == null) return triangles;

                List<Triangle> result = new List<Triangle>();

                foreach (var tri in triangles)
                {
                    List<Triangle> clipped = ClipTriangleToNode(tri, clipper);
                    result.AddRange(clipped);
                }

                return result;
            }

            private List<Triangle> ClipTriangleToNode(Triangle tri, Node node)
            {
                if (node == null)
                    return new List<Triangle> { tri };

                List<Triangle> front = new List<Triangle>();
                List<Triangle> back = new List<Triangle>();
                List<Triangle> coplanar = new List<Triangle>();

                SplitTriangle(tri, node.plane, coplanar, front, back);

                List<Triangle> result = new List<Triangle>();

                if (node.front != null)
                {
                    foreach (var t in front)
                        result.AddRange(ClipTriangleToNode(t, node.front));
                }
                else
                {
                    result.AddRange(front);
                }

                if (node.back != null)
                {
                    foreach (var t in back)
                        result.AddRange(ClipTriangleToNode(t, node.back));
                }

                // Coplanar triangles are kept
                result.AddRange(coplanar);

                return result;
            }

            private void SplitTriangle(Triangle tri, Plane plane,
                List<Triangle> coplanar, List<Triangle> front, List<Triangle> back)
            {
                const float EPSILON = 0.001f;

                int[] sides = new int[3];
                float[] distances = new float[3];

                for (int i = 0; i < 3; i++)
                {
                    distances[i] = plane.GetDistanceToPoint(tri.vertices[i]);
                    if (distances[i] < -EPSILON)
                        sides[i] = -1;
                    else if (distances[i] > EPSILON)
                        sides[i] = 1;
                    else
                        sides[i] = 0;
                }

                int sideSum = sides[0] + sides[1] + sides[2];

                // All vertices on same side or coplanar
                if (sides[0] == sides[1] && sides[1] == sides[2])
                {
                    if (sides[0] == 1)
                        front.Add(tri);
                    else if (sides[0] == -1)
                        back.Add(tri);
                    else
                        coplanar.Add(tri);
                    return;
                }

                // Triangle spans the plane, need to split
                List<Vector3> frontVerts = new List<Vector3>();
                List<Vector3> backVerts = new List<Vector3>();
                List<Vector2> frontUVs = new List<Vector2>();
                List<Vector2> backUVs = new List<Vector2>();
                List<Vector3> frontNormals = new List<Vector3>();
                List<Vector3> backNormals = new List<Vector3>();

                for (int i = 0; i < 3; i++)
                {
                    int j = (i + 1) % 3;

                    if (sides[i] >= 0)
                    {
                        frontVerts.Add(tri.vertices[i]);
                        frontUVs.Add(tri.uvs[i]);
                        frontNormals.Add(tri.normals[i]);
                    }

                    if (sides[i] <= 0)
                    {
                        backVerts.Add(tri.vertices[i]);
                        backUVs.Add(tri.uvs[i]);
                        backNormals.Add(tri.normals[i]);
                    }

                    if (sides[i] != 0 && sides[j] != 0 && sides[i] != sides[j])
                    {
                        // Edge crosses plane
                        float t = distances[i] / (distances[i] - distances[j]);

                        Vector3 v = Vector3.Lerp(tri.vertices[i], tri.vertices[j], t);
                        Vector2 uv = Vector2.Lerp(tri.uvs[i], tri.uvs[j], t);
                        Vector3 n = Vector3.Lerp(tri.normals[i], tri.normals[j], t).normalized;

                        frontVerts.Add(v);
                        frontUVs.Add(uv);
                        frontNormals.Add(n);

                        backVerts.Add(v);
                        backUVs.Add(uv);
                        backNormals.Add(n);
                    }
                }

                // Triangulate front vertices
                if (frontVerts.Count >= 3)
                {
                    for (int i = 1; i < frontVerts.Count - 1; i++)
                    {
                        Triangle t = new Triangle();
                        t.vertices[0] = frontVerts[0];
                        t.vertices[1] = frontVerts[i];
                        t.vertices[2] = frontVerts[i + 1];
                        t.uvs[0] = frontUVs[0];
                        t.uvs[1] = frontUVs[i];
                        t.uvs[2] = frontUVs[i + 1];
                        t.normals[0] = frontNormals[0];
                        t.normals[1] = frontNormals[i];
                        t.normals[2] = frontNormals[i + 1];
                        front.Add(t);
                    }
                }

                // Triangulate back vertices
                if (backVerts.Count >= 3)
                {
                    for (int i = 1; i < backVerts.Count - 1; i++)
                    {
                        Triangle t = new Triangle();
                        t.vertices[0] = backVerts[0];
                        t.vertices[1] = backVerts[i];
                        t.vertices[2] = backVerts[i + 1];
                        t.uvs[0] = backUVs[0];
                        t.uvs[1] = backUVs[i];
                        t.uvs[2] = backUVs[i + 1];
                        t.normals[0] = backNormals[0];
                        t.normals[1] = backNormals[i];
                        t.normals[2] = backNormals[i + 1];
                        back.Add(t);
                    }
                }
            }

            private BSPTree Clone()
            {
                BSPTree clone = new BSPTree();
                clone.root = CloneNode(root);
                return clone;
            }

            private Node CloneNode(Node node)
            {
                if (node == null) return null;

                Node clone = new Node(node.coplanar[0]);
                clone.plane = new Plane(node.plane.normal, node.plane.distance);
                clone.coplanar = new List<Triangle>();

                foreach (var tri in node.coplanar)
                    clone.coplanar.Add(tri.Clone());

                clone.front = CloneNode(node.front);
                clone.back = CloneNode(node.back);

                return clone;
            }

            private void AddTriangles(List<Triangle> tris)
            {
                foreach (var tri in tris)
                {
                    AddTriangle(root, tri);
                }
            }

            private void AddTriangle(Node node, Triangle tri)
            {
                if (node == null) return;

                List<Triangle> front = new List<Triangle>();
                List<Triangle> back = new List<Triangle>();
                List<Triangle> coplanar = new List<Triangle>();

                SplitTriangle(tri, node.plane, coplanar, front, back);

                node.coplanar.AddRange(coplanar);

                foreach (var t in front)
                {
                    if (node.front != null)
                        AddTriangle(node.front, t);
                    else
                        node.coplanar.Add(t);
                }

                foreach (var t in back)
                {
                    if (node.back != null)
                        AddTriangle(node.back, t);
                    else
                        node.coplanar.Add(t);
                }
            }

            private List<Triangle> GetAllTriangles()
            {
                List<Triangle> result = new List<Triangle>();
                GetNodeTriangles(root, result);
                return result;
            }

            private void GetNodeTriangles(Node node, List<Triangle> result)
            {
                if (node == null) return;

                result.AddRange(node.coplanar);
                GetNodeTriangles(node.front, result);
                GetNodeTriangles(node.back, result);
            }

            public Mesh ToMesh()
            {
                List<Triangle> allTriangles = GetAllTriangles();

                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                List<Vector3> normals = new List<Vector3>();
                List<int> indices = new List<int>();

                foreach (var tri in allTriangles)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        vertices.Add(tri.vertices[i]);
                        uvs.Add(tri.uvs[i]);
                        normals.Add(tri.normals[i]);
                        indices.Add(vertices.Count - 1);
                    }
                }

                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.uv = uvs.ToArray();
                mesh.normals = normals.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.RecalculateBounds();

                return mesh;
            }
        }

        private Mesh CreateCylinderMesh(float radius, float height, int segments)
        {
            Mesh mesh = new Mesh();

            // Use 0.5 as base radius to create unit diameter cylinder
            float baseRadius = 0.5f;  // Changed from radius parameter

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // Create vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                float x = Mathf.Cos(angle) * baseRadius;  // Use baseRadius
                float z = Mathf.Sin(angle) * baseRadius;  // Use baseRadius

                vertices.Add(new Vector3(x, -height / 2, z));
                vertices.Add(new Vector3(x, height / 2, z));

                float u = i / (float)segments;
                uvs.Add(new Vector2(u, 0));
                uvs.Add(new Vector2(u, 1));
            }

            // Create side triangles (unchanged)
            for (int i = 0; i < segments; i++)
            {
                int current = i * 2;
                int next = ((i + 1) % (segments + 1)) * 2;

                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next + 1);

                triangles.Add(current);
                triangles.Add(next + 1);
                triangles.Add(next);
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
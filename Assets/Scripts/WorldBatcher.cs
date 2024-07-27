using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldBatcher : MonoBehaviour
{
    private MeshRenderer _renderer;
    public MeshCollider colliderShape = null;

    private float _ball1PeekSize;
    private float _ball2PeekSize;

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        List<Vector3> collisionVertices = new List<Vector3>();
        List<int> collisionIndices = new List<int>();

        var meshFilter = GetComponent<MeshFilter>();
        var children = GetComponentsInChildren<MeshFilter>();
        foreach(var child in children) {
            if (child == meshFilter) continue;

            int indexOffset = vertices.Count;

            if (child.sharedMesh) {
                for (int vertex = 0; vertex < child.sharedMesh.vertices.Length; ++vertex) {
                    vertices.Add(child.transform.localToWorldMatrix.MultiplyPoint(child.sharedMesh.vertices[vertex]));
                    normals.Add(child.transform.localToWorldMatrix.MultiplyVector(child.sharedMesh.normals[vertex]));
                    uvs.Add(child.sharedMesh.uv[vertex]);
                }

                foreach(int index in child.sharedMesh.GetIndices(0)) {
                    indices.Add(index + indexOffset);
                }

                if (colliderShape) {
                    foreach (int index in child.sharedMesh.GetIndices(0)) {
                        Vector3 vertex = child.transform.localToWorldMatrix.MultiplyPoint(child.sharedMesh.vertices[index]);
                        bool indexAdded = false;
                        for (int i = 0; i < collisionVertices.Count; ++i) {
                            Vector3 storedVertex = collisionVertices[i];
                            if((vertex - storedVertex).sqrMagnitude < 0.001) {
                                collisionIndices.Add(i);
                                indexAdded = true;
                                break;
                            }
                        }
                        if(!indexAdded) {
                            collisionIndices.Add(collisionVertices.Count);
                            collisionVertices.Add(vertex);
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(indices, 0);
        meshFilter.mesh = mesh;

        if(colliderShape) {
            Mesh collisionMesh = new Mesh();
            collisionMesh.SetVertices(collisionVertices);
            collisionMesh.SetTriangles(collisionIndices, 0);
            colliderShape.sharedMesh = collisionMesh;
        }

        foreach (var child in children)
            if(child != meshFilter)
                Destroy(child.gameObject);
    }

    void Update() {
        Vector4 ball1Pos = Vector4.zero;
        Vector4 ball2Pos = Vector4.zero;

        if(GameManager.instance.balls.Count >= 1) {
            ball1Pos = UpdateBall(ref _ball1PeekSize, GameManager.instance.balls[0], GameManager.instance.turn == 0);
        }

        if (GameManager.instance.balls.Count >= 2) {
            ball2Pos = UpdateBall(ref _ball2PeekSize, GameManager.instance.balls[1], GameManager.instance.turn == 1);
        }

        _renderer.sharedMaterial.SetVector("_Ball1Pos", ball1Pos);
        _renderer.sharedMaterial.SetVector("_Ball2Pos", ball2Pos);
    }

    Vector4 UpdateBall(ref float peekValue, Ball ball, bool turn) {
        Vector3 pos = ball.transform.position;
        float targetPeek = 0.0f;
        
        if(Physics.Raycast(pos, -Camera.main.transform.forward, 1000.0f, ~2)) {
            targetPeek = turn ? 1.0f : 0.3f;
        }

        peekValue = Mathf.Lerp(peekValue, targetPeek, 0.01f);

        return new Vector4(pos.x, pos.y, pos.z, peekValue * 0.3f);
    }
}

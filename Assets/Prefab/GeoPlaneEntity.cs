using UnityEngine;

[ExecuteInEditMode]
public class GeoPlaneEntity : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;
    public Transform pointD; // Có thể để trống cho các Lesson cũ

    private MeshFilter _meshFilter;

    void Start() {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Update() {
        if (pointA && pointB && pointC) {
            UpdateMesh();
        }
    }

    void UpdateMesh() {
        Mesh mesh = new Mesh();
        
        // Nếu có điểm D -> Vẽ tứ giác (4 đỉnh)
        if (pointD != null) {
            mesh.vertices = new Vector3[] {
                pointA.localPosition, pointB.localPosition, 
                pointC.localPosition, pointD.localPosition
            };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        } 
        // Nếu KHÔNG có điểm D -> Vẽ tam giác (3 đỉnh) như cũ
        else {
            mesh.vertices = new Vector3[] {
                pointA.localPosition, pointB.localPosition, pointC.localPosition
            };
            mesh.triangles = new int[] { 0, 1, 2 };
        }

        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }
}
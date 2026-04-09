using UnityEngine;

public class GeoIntersectionEntity : MonoBehaviour 
{
    [Header("Kéo Line và Plane cần tìm giao điểm vào đây")]
    public GeoLineEntity targetLine;
    public GeoPlaneEntity targetPlane;

    [Header("Prefab điểm để hiển thị giao điểm")]
    public GameObject intersectionPointPrefab;
    
    private GameObject _visualPoint;

    void Start() 
    {
        // Sinh ra một điểm để đánh dấu giao điểm
        if (intersectionPointPrefab != null) {
            _visualPoint = Instantiate(intersectionPointPrefab, transform);
            _visualPoint.name = "Intersection_Result";
            // Đổi màu điểm giao cho khác biệt (ví dụ màu Đỏ)
            _visualPoint.GetComponentInChildren<Renderer>().material.color = Color.red;
        }
    }

    void Update() 
    {
        if (targetLine == null || targetPlane == null) return;

        // 1. Lấy dữ liệu từ Line và Plane
        Vector3 p1 = targetLine.pointA.position;
        Vector3 p2 = targetLine.pointB.position;
        Vector3 a = targetPlane.pointA.position;
        Vector3 b = targetPlane.pointB.position;
        Vector3 c = targetPlane.pointC.position;

        // 2. Tính Vector pháp tuyến của mặt phẳng (Normal)
        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

        // 3. Tính toán giao điểm giữa đường thẳng vô hạn và mặt phẳng vô hạn
        Vector3 lineVec = p2 - p1;
        float dot = Vector3.Dot(normal, lineVec);

        // Nếu dot gần bằng 0 nghĩa là đường thẳng song song với mặt phẳng
        if (Mathf.Abs(dot) > 0.0001f) 
        {
            float t = Vector3.Dot(normal, a - p1) / dot;
            Vector3 intersection = p1 + t * lineVec;

            // 4. KIỂM TRA: Điểm đó có nằm trong TAM GIÁC ABC không?
            if (IsPointInTriangle(intersection, a, b, c)) {
                _visualPoint.SetActive(true);
                _visualPoint.transform.position = intersection;
            } else {
                _visualPoint.SetActive(false); // Nằm ngoài tam giác thì ẩn đi
            }
        } else {
            _visualPoint.SetActive(false); // Song song thì ẩn đi
        }
    }

    // Thuật toán kiểm tra điểm nằm trong tam giác (Barycentric Coordinates)
    bool IsPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c) 
    {
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;
        return (u >= 0) && (v >= 0) && (w >= 0);
    }
}
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class GeoAutoSystem : MonoBehaviour 
{
    [Header("Cấu hình Prefab")]
    public GameObject pointPrefab; 
    public float intersectionScale = 0.15f; 

    private List<GameObject> _spawnedPoints = new List<GameObject>();
    private string _namingChars = "HIKMNPQR"; 

    [Header("Cài đặt hiển thị")]
    public bool showLabels = true; 

    void Update() 
    {
        // 1. Phím L để ẩn/hiện nhãn (Dùng Input System)
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame) 
        {
            showLabels = !showLabels;
            UpdateAllMeasureLabels();
        }

        // 2. Dọn dẹp giao điểm cũ mỗi khung hình để cập nhật vị trí mới
        foreach (var p in _spawnedPoints) if(p != null) Destroy(p);
        _spawnedPoints.Clear();

        if (pointPrefab == null) return;

        // Tìm tất cả thực thể hình học trong Scene
        var allLines = FindObjectsByType<GeoLineEntity>(FindObjectsSortMode.None);
        var allPlanes = FindObjectsByType<GeoPlaneEntity>(FindObjectsSortMode.None);

        int nameIndex = 0;

        // 3. Tìm giao điểm ĐƯỜNG THẲNG - MẶT PHẲNG (Hỗ trợ 3 & 4 điểm)
        foreach (var line in allLines) {
            if (!line.gameObject.activeInHierarchy) continue; 
            
            foreach (var plane in allPlanes) {
                if (!plane.gameObject.activeInHierarchy) continue;
                Vector3 intersect;
                if (GetLinePlaneIntersection(line, plane, out intersect)) {
                    SpawnPoint(intersect, _namingChars[nameIndex % _namingChars.Length].ToString());
                    nameIndex++;
                }
            }
        }

        // 4. Tìm giao điểm ĐƯỜNG THẲNG - ĐƯỜNG THẲNG
        for (int i = 0; i < allLines.Length; i++) {
            if (!allLines[i].gameObject.activeInHierarchy) continue;
            for (int j = i + 1; j < allLines.Length; j++) {
                if (!allLines[j].gameObject.activeInHierarchy) continue;
                Vector3 intersect;
                if (GetLineLineIntersection(allLines[i], allLines[j], out intersect)) {
                    SpawnPoint(intersect, _namingChars[nameIndex % _namingChars.Length].ToString());
                    nameIndex++;
                }
            }
        }

        UpdateAllMeasureLabels();
    }

    // Hàm sinh giao điểm và quản lý Hierarchy
    void SpawnPoint(Vector3 pos, string label) {
        GameObject newPoint = Instantiate(pointPrefab, pos, Quaternion.identity);
        
        // Đưa vào làm con của GeoSystem_Brain để đóng gói Prefab sạch sẽ
        newPoint.transform.SetParent(this.transform); 
        
        newPoint.name = "Intersection_Point_" + label;
        newPoint.tag = "Intersection"; 
        newPoint.transform.localScale = Vector3.one * intersectionScale; 
        
        // Thiết lập màu sắc và hiệu ứng phát sáng
        var renderer = newPoint.GetComponentInChildren<Renderer>();
        if(renderer != null) {
            renderer.material.color = Color.red;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.red * 0.5f);
        }

        // Thiết lập nhãn tên (TextMeshPro)
        var tmp = newPoint.GetComponentInChildren<TextMeshPro>();
        if(tmp != null) {
            tmp.text = label;
            tmp.enabled = showLabels; 
            // Thêm hiệu ứng luôn xoay về Camera nếu chưa có
            if (!newPoint.GetComponent<BillboardEffect>()) newPoint.AddComponent<BillboardEffect>(); 
        }
        _spawnedPoints.Add(newPoint);
    }

    void UpdateAllMeasureLabels() {
        GeoMeasureLabel[] allM = FindObjectsByType<GeoMeasureLabel>(FindObjectsSortMode.None);
        foreach (var m in allM) { m.forceShow = showLabels; }
    }

    // --- LOGIC TOÁN HỌC GIAO ĐIỂM ---

    bool GetLinePlaneIntersection(GeoLineEntity line, GeoPlaneEntity plane, out Vector3 intersection) {
        intersection = Vector3.zero;
        if (line.pointA == null || line.pointB == null || plane.pointA == null) return false;

        Vector3 p1 = line.pointA.position; 
        Vector3 p2 = line.pointB.position;
        Vector3 a = plane.pointA.position; 
        Vector3 b = plane.pointB.position; 
        Vector3 c = plane.pointC.position;

        // Tính Vector pháp tuyến của mặt phẳng từ 3 điểm đầu tiên
        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
        Vector3 direction = p2 - p1;

        float dot = Vector3.Dot(normal, direction);
        if (Mathf.Abs(dot) < 0.0001f) return false; // Đường thẳng song song mặt phẳng

        float t = Vector3.Dot(normal, a - p1) / dot;
        if (t < 0 || t > 1) return false; // Giao điểm nằm ngoài đoạn thẳng giới hạn

        intersection = p1 + t * direction;

        // KIỂM TRA HYBRID: Kiểm tra trong tam giác ABC
        bool inPlane = IsPointInTriangle(intersection, a, b, c);
        
        // Nếu không nằm trong ABC, kiểm tra tiếp trong tam giác ADC (nếu có điểm D)
        if (!inPlane && plane.pointD != null) {
            inPlane = IsPointInTriangle(intersection, a, c, plane.pointD.position);
        }

        return inPlane;
    }

    bool GetLineLineIntersection(GeoLineEntity l1, GeoLineEntity l2, out Vector3 intersect) {
        intersect = Vector3.zero;
        if (l1.pointA == null || l2.pointA == null) return false;

        Vector3 p1 = l1.pointA.position, p2 = l1.pointB.position;
        Vector3 p3 = l2.pointA.position, p4 = l2.pointB.position;

        Vector3 u = p2 - p1, v = p4 - p3, w = p1 - p3;
        float a = Vector3.Dot(u, u), b = Vector3.Dot(u, v), c = Vector3.Dot(v, v), d = Vector3.Dot(u, w), e = Vector3.Dot(v, w);
        float D = a * c - b * b;

        if (D < 0.0001f) return false;

        float t = (b * e - c * d) / D;
        float s = (a * e - b * d) / D;

        if (t < 0 || t > 1 || s < 0 || s > 1) return false;

        Vector3 closestL1 = p1 + t * u;
        Vector3 closestL2 = p3 + s * v;

        if (Vector3.Distance(closestL1, closestL2) < 0.02f) {
            intersect = (closestL1 + closestL2) / 2f;
            return true;
        }
        return false;
    }

    // Thuật toán kiểm tra điểm nằm trong tam giác (Barycentric)
    bool IsPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0, v0), d01 = Vector3.Dot(v0, v1), d11 = Vector3.Dot(v1, v1), d20 = Vector3.Dot(v2, v0), d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        if (Mathf.Abs(denom) < 0.0001f) return false;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        return (v >= 0) && (w >= 0) && (v + w <= 1);
    }
}

// Script phụ để nhãn Text luôn quay về phía người xem
public class BillboardEffect : MonoBehaviour {
    void Update() {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
    }
}
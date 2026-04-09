using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Module4PrismBox : MonoBehaviour
{
    [Header("Tài nguyên")]
    public Material glassMaterial; // Material kính mờ cho mặt bên
    public GameObject pointPrefab;

    private List<GameObject> _sidePlanes = new List<GameObject>();
    private List<LineRenderer> _edges = new List<LineRenderer>();
    private GameObject _diagonalPoint;
    private List<LineRenderer> _diagonals = new List<LineRenderer>();

    // Thông số hình khối
    private Vector3[] _basePoints;
    private Vector3 _offset = new Vector3(0.5f, 2.0f, 0.2f); // Độ nghiêng và chiều cao lăng trụ
    private bool _isBoxMode = false;

    void Start()
    {
        transform.position = Vector3.zero;
        // Khởi tạo lăng trụ tam giác trước
        SetupPrism(3); 
    }

    void Update()
    {
        // Phím 1: Lăng trụ tam giác | Phím 2: Hình hộp (Lăng trụ đáy hình bình hành)
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetupPrism(3);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetupBox();

        // Hiệu ứng "Ngộ": Nhấn Space để highlight các cặp cạnh song song
        if (Keyboard.current.spaceKey.wasPressedThisFrame) StartCoroutine(HighlightParallelEdges());
    }

    void SetupPrism(int sides)
    {
        ClearCurrent();
        _isBoxMode = false;
        _basePoints = new Vector3[sides];
        
        // Tạo đáy là đa giác đều trên mặt phẳng Beta (y=0)
        for (int i = 0; i < sides; i++) {
            float angle = i * Mathf.PI * 2 / sides;
            _basePoints[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
        BuildSolid();
    }

    void SetupBox()
    {
        ClearCurrent();
        _isBoxMode = true;
        // Đáy là hình bình hành
        _basePoints = new Vector3[4] {
            new Vector3(0, 0, 0), new Vector3(1.5f, 0, 0),
            new Vector3(2.0f, 0, 1.0f), new Vector3(0.5f, 0, 1.0f)
        };
        BuildSolid();
        CreateDiagonals();
    }

    void BuildSolid()
    {
        int n = _basePoints.Length;
        // 1. Vẽ các mặt bên
        for (int i = 0; i < n; i++)
        {
            Vector3 b1 = _basePoints[i];
            Vector3 b2 = _basePoints[(i + 1) % n];
            Vector3 t1 = b1 + _offset;
            Vector3 t2 = b2 + _offset;

            GameObject side = CreatePlane($"Side_{i}", b1, b2, t2, t1, new Color(1, 1, 1, 0.2f));
            _sidePlanes.Add(side);

            // Vẽ các cạnh bên
            _edges.Add(CreateLine($"Edge_Side_{i}", b1, t1, Color.white, 0.02f));
            // Vẽ các cạnh đáy và đỉnh
            _edges.Add(CreateLine($"Edge_Base_{i}", b1, b2, Color.gray, 0.02f));
            _edges.Add(CreateLine($"Edge_Top_{i}", t1, t2, Color.gray, 0.02f));
        }
    }

    void CreateDiagonals()
    {
        // Chỉ dành cho Hình hộp: 4 đường chéo cắt nhau tại trung điểm
        Color diagColor = new Color(1, 0.5f, 0, 0.6f);
        for (int i = 0; i < 4; i++) {
            _diagonals.Add(CreateLine($"Diagonal_{i}", Vector3.zero, Vector3.one, diagColor, 0.015f));
        }
        
        _diagonalPoint = Instantiate(pointPrefab, transform);
        _diagonalPoint.GetComponentInChildren<Renderer>().material.color = Color.yellow;
        _diagonalPoint.transform.localScale = Vector3.one * 0.1f;
        
        UpdateDiagonals();
    }

    void UpdateDiagonals()
    {
        if (!_isBoxMode) return;
        // Đường chéo nối đỉnh đáy i với đỉnh đối diện trên mặt Alpha
        // Đỉnh đáy: 0, 1, 2, 3 -> Đỉnh trên: 4, 5, 6, 7 (Base + Offset)
        Vector3[] topPoints = new Vector3[4];
        for (int i = 0; i < 4; i++) topPoints[i] = _basePoints[i] + _offset;

        _diagonals[0].SetPositions(new Vector3[] { _basePoints[0], topPoints[2] });
        _diagonals[1].SetPositions(new Vector3[] { _basePoints[1], topPoints[3] });
        _diagonals[2].SetPositions(new Vector3[] { _basePoints[2], topPoints[0] });
        _diagonals[3].SetPositions(new Vector3[] { _basePoints[3], topPoints[1] });

        // Trung điểm O = trung bình cộng của các đỉnh
        Vector3 center = (_basePoints[0] + topPoints[2]) / 2f;
        _diagonalPoint.transform.position = center;
    }

    IEnumerator HighlightParallelEdges()
    {
        // Hiệu ứng "Ngộ": Cho các cạnh bên nhấp nháy cùng màu để thấy chúng song song và bằng nhau
        float timer = 0;
        while (timer < 3f) {
            float alpha = Mathf.PingPong(Time.time * 5, 1.0f);
            foreach (var edge in _edges) {
                if (edge.name.Contains("Edge_Side")) edge.startColor = edge.endColor = Color.Lerp(Color.white, Color.yellow, alpha);
            }
            yield return null;
            timer += Time.deltaTime;
        }
    }

    // --- HÀM PHỤ TRỢ ---
    void ClearCurrent() {
        foreach (var go in _sidePlanes) Destroy(go); _sidePlanes.Clear();
        foreach (var lr in _edges) Destroy(lr.gameObject); _edges.Clear();
        foreach (var lr in _diagonals) Destroy(lr.gameObject); _diagonals.Clear();
        if (_diagonalPoint) Destroy(_diagonalPoint);
    }

    GameObject CreatePlane(string n, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col) {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var ps = go.AddComponent<GeoPlaneEntity>();
        ps.pointA = CreateAnchor(a, go.transform); ps.pointB = CreateAnchor(b, go.transform);
        ps.pointC = CreateAnchor(c, go.transform); ps.pointD = CreateAnchor(d, go.transform);
        var ren = go.AddComponent<MeshRenderer>(); ren.material = new Material(glassMaterial); ren.material.color = col;
        go.AddComponent<MeshFilter>(); return go;
    }

    Transform CreateAnchor(Vector3 pos, Transform p) {
        GameObject go = new GameObject("Anchor"); go.transform.SetParent(p);
        go.transform.localPosition = pos; return go.transform;
    }

    LineRenderer CreateLine(string n, Vector3 s, Vector3 e, Color col, float w) {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var lr = go.AddComponent<LineRenderer>(); lr.positionCount = 2; lr.SetPositions(new Vector3[] { s, e });
        lr.startWidth = w; lr.endWidth = w; lr.material = new Material(Shader.Find("Unlit/Color")); lr.material.color = col;
        return lr;
    }
}
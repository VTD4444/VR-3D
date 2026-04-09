using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Module2Part2B : MonoBehaviour
{
    [Header("Tài nguyên")]
    public Material planeMaterial;
    public GameObject pointPrefab;

    private GameObject _planeAlpha, _planeBeta, _planeGamma;
    private LineRenderer _lrA, _lrB;
    private TextMeshPro _distanceLabel, _missionLabel;
    
    private float _rotationX = 20f;
    private bool _isTaskCompleted = false;
    private Vector3[] _gammaBasePoints = new Vector3[4];

    void Start()
    {
        CreateModule2B();
    }

    void CreateModule2B()
    {
        _planeAlpha = CreatePlane("Alpha", new Vector3(-2, 1, -1.5f), new Vector3(2, 1, -1.5f), new Vector3(2, 1, 1.5f), new Vector3(-2, 1, 1.5f), new Color(0, 0, 1, 0.2f));
        _planeBeta = CreatePlane("Beta", new Vector3(-2, 0, -1.5f), new Vector3(2, 0, -1.5f), new Vector3(2, 0, 1.5f), new Vector3(-2, 0, 1.5f), new Color(0, 0, 1, 0.2f));

        _gammaBasePoints[0] = new Vector3(-1.2f, 1.8f, 0); 
        _gammaBasePoints[1] = new Vector3(1.2f, 1.8f, 0);
        _gammaBasePoints[2] = new Vector3(1.2f, -0.8f, 0);
        _gammaBasePoints[3] = new Vector3(-1.2f, -0.8f, 0);
        _planeGamma = CreatePlane("Gamma", _gammaBasePoints[0], _gammaBasePoints[1], _gammaBasePoints[2], _gammaBasePoints[3], new Color(1, 0, 1, 0.4f));

        _lrA = CreateLine("Line_a", Color.yellow).GetComponent<LineRenderer>();
        _lrB = CreateLine("Line_b", Color.yellow).GetComponent<LineRenderer>();

        _distanceLabel = CreateDiegeticText("Distance_Text", "", Color.yellow, 2.5f);
        _missionLabel = CreateDiegeticText("Mission_Text", "Dùng J-L xoay mặt phẳng. Lưu ý: Chỉ đo khi có giao tuyến!", Color.white, 3f);
        _missionLabel.transform.position = new Vector3(0, 2.5f, 0);
    }

    void Update()
    {
        if (Keyboard.current.jKey.isPressed) _rotationX -= 50f * Time.deltaTime;
        if (Keyboard.current.lKey.isPressed) _rotationX += 50f * Time.deltaTime;

        // Giới hạn góc rộng hơn để người dùng có thể xoay đến mức "mất dấu"
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);
        _planeGamma.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        UpdateDynamicIntersections();
    }

    void UpdateDynamicIntersections()
    {
        Vector3 pA = _planeGamma.transform.TransformPoint(_gammaBasePoints[0]);
        Vector3 pD = _planeGamma.transform.TransformPoint(_gammaBasePoints[3]);
        Vector3 pB = _planeGamma.transform.TransformPoint(_gammaBasePoints[1]);
        Vector3 pC = _planeGamma.transform.TransformPoint(_gammaBasePoints[2]);

        // Tính giao điểm kèm theo kiểm tra hợp lệ
        bool hasA = CalculateValidIntersection(pA, pD, pB, pC, 1.0f, out Vector3 startA, out Vector3 endA);
        bool hasB = CalculateValidIntersection(pA, pD, pB, pC, 0.0f, out Vector3 startB, out Vector3 endB);

        // Hiển thị/Ẩn đường thẳng dựa trên kết quả kiểm tra
        _lrA.enabled = hasA;
        _lrB.enabled = hasB;

        if (hasA) { _lrA.SetPosition(0, startA); _lrA.SetPosition(1, endA); }
        if (hasB) { _lrB.SetPosition(0, startB); _lrB.SetPosition(1, endB); }

        // Cập nhật nhãn khoảng cách
        if (hasA && hasB)
        {
            float d = Vector3.Distance(startA, startB);
            _distanceLabel.text = $"d = {d:F2}";
            _distanceLabel.transform.position = (startA + startB) / 2 + Vector3.right * 1.0f;
            
            if (!_isTaskCompleted && Mathf.Abs(_rotationX) > 40f) {
                _isTaskCompleted = true;
                _missionLabel.text = "<color=green>Xác minh thành công!</color>";
            }
        }
        else
        {
            _distanceLabel.text = "Không có giao tuyến";
        }
    }

    // --- HÀM TÍNH TOÁN CÓ KIỂM TRA ĐIỀU KIỆN ---
    bool CalculateValidIntersection(Vector3 leftTop, Vector3 leftBottom, Vector3 rightTop, Vector3 rightBottom, float yTarget, out Vector3 start, out Vector3 end)
    {
        start = Vector3.zero; end = Vector3.zero;

        // Tính tỉ lệ t cho cạnh trái và cạnh phải
        float tLeft = (yTarget - leftTop.y) / (leftBottom.y - leftTop.y);
        float tRight = (yTarget - rightTop.y) / (rightBottom.y - rightTop.y);

        // ĐIỀU KIỆN QUAN TRỌNG: t phải nằm trong [0, 1]
        if (tLeft >= 0 && tLeft <= 1 && tRight >= 0 && tRight <= 1)
        {
            start = Vector3.Lerp(leftTop, leftBottom, tLeft);
            end = Vector3.Lerp(rightTop, rightBottom, tRight);
            return true;
        }
        return false; // Mặt phẳng không cắt ở cao độ này
    }

    // --- CÁC HÀM PHỤ TRỢ GIỮ NGUYÊN ---
    GameObject CreatePlane(string n, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col) {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var ps = go.AddComponent<GeoPlaneEntity>();
        ps.pointA = CreatePoint(n+"A", a, go.transform); ps.pointB = CreatePoint(n+"B", b, go.transform);
        ps.pointC = CreatePoint(n+"C", c, go.transform); ps.pointD = CreatePoint(n+"D", d, go.transform);
        var ren = go.AddComponent<MeshRenderer>(); ren.material = new Material(planeMaterial); ren.material.color = col;
        go.AddComponent<MeshFilter>(); return go;
    }

    Transform CreatePoint(string n, Vector3 pos, Transform parent) {
        GameObject p = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity, parent);
        p.transform.localPosition = pos; p.transform.localScale = Vector3.one * 0.05f; return p.transform;
    }

    GameObject CreateLine(string n, Color col) {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var lr = go.AddComponent<LineRenderer>(); lr.positionCount = 2; lr.startWidth = 0.05f; lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Unlit/Color")); lr.material.color = col; return go;
    }

    TextMeshPro CreateDiegeticText(string n, string content, Color col, float size) {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var tmp = go.AddComponent<TextMeshPro>(); tmp.text = content; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center; go.AddComponent<BillboardEffect>(); return tmp;
    }
}
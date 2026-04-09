using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Module2PureCode : MonoBehaviour
{
    [Header("Cài đặt tài nguyên")]
    public Material planeMaterial; 
    public GameObject pointPrefab; 

    private GameObject _planeAlpha, _planeBeta, _planeGamma;
    private GameObject _lineA, _lineB, _text;
    private bool _isPlaying = false;

    // Lưu trữ tọa độ 4 đỉnh của Gamma để tính toán
    private Vector3[] _gammaPoints = new Vector3[4];

    void Start()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        CreateModule2Geometry();
        ResetModule();
    }

    void CreateModule2Geometry()
    {
        // 1. MẶT PHẲNG ALPHA (Trên)
        _planeAlpha = CreatePlane("Plane_Alpha", 
            new Vector3(-1.5f, 1.0f, -1.0f), new Vector3(1.5f, 1.0f, -1.0f), 
            new Vector3(1.5f, 1.0f, 1.0f),  new Vector3(-1.5f, 1.0f, 1.0f), new Color(0, 0, 1, 0.3f));

        // 2. MẶT PHẲNG BETA (Dưới)
        _planeBeta = CreatePlane("Plane_Beta", 
            new Vector3(-1.5f, 0.0f, -1.0f), new Vector3(1.5f, 0.0f, -1.0f), 
            new Vector3(1.5f, 0.0f, 1.0f),  new Vector3(-1.5f, 0.0f, 1.0f), new Color(0, 0, 1, 0.3f));

        // 3. MẶT PHẲNG GAMMA (Cắt ngang) - Lưu lại tọa độ
        _gammaPoints[0] = new Vector3(-1.0f, 1.8f, 0.5f);  // Gamma A (Top-Left)
        _gammaPoints[1] = new Vector3(1.0f, 1.8f, -0.5f);  // Gamma B (Top-Right)
        _gammaPoints[2] = new Vector3(1.0f, -0.8f, -0.5f); // Gamma C (Bottom-Right)
        _gammaPoints[3] = new Vector3(-1.0f, -0.8f, 0.5f); // Gamma D (Bottom-Left)

        _planeGamma = CreatePlane("Plane_Gamma", _gammaPoints[0], _gammaPoints[1], _gammaPoints[2], _gammaPoints[3], new Color(1, 0, 1, 0.3f));

        // --- CẢI TIẾN: TỰ ĐỘNG TÍNH TOÁN TOÁN HỌC GIAO TUYẾN ---
        
        // Cần tìm giao tuyến ở độ cao y=1 và y=0
        // Tìm vị trí cắt trên 2 cạnh nghiêng AD (trái) và BC (phải) của Gamma
        
        // Giao tuyến a (ở cao độ Alpha y=1.0f)
        Vector3 lineA_start = FindPointAtHeight(_gammaPoints[0], _gammaPoints[3], 1.0f); // Điểm trên cạnh trái AD
        Vector3 lineA_end = FindPointAtHeight(_gammaPoints[1], _gammaPoints[2], 1.0f);   // Điểm trên cạnh phải BC
        _lineA = CreateLine("Line_a", lineA_start, lineA_end);

        // Giao tuyến b (ở cao độ Beta y=0.0f)
        Vector3 lineB_start = FindPointAtHeight(_gammaPoints[0], _gammaPoints[3], 0.0f); // Cạnh trái AD
        Vector3 lineB_end = FindPointAtHeight(_gammaPoints[1], _gammaPoints[2], 0.0f);   // Cạnh phải BC
        _lineB = CreateLine("Line_b", lineB_start, lineB_end);

        // --- CÁC PHẦN KHÁC GIỮ NGUYÊN ---
        GameObject textObj = new GameObject("Parallel_Text");
        textObj.transform.SetParent(this.transform);
        var tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "a // b";
        tmp.fontSize = 2;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.yellow;
        if (!textObj.GetComponent<BillboardEffect>()) textObj.AddComponent<BillboardEffect>();
        _text = textObj;
    }

    // Hàm phụ trợ: Tính toán điểm nằm trên đoạn (p1, p2) có cao độ 'height' cho trước
    Vector3 FindPointAtHeight(Vector3 p1, Vector3 p2, float height)
    {
        // t là tỉ lệ cao độ: 0 ở p1, 1 ở p2
        float t = (height - p1.y) / (p2.y - p1.y);
        // Nội suy tọa độ (x,y,z) dựa trên t
        return p1 + t * (p2 - p1);
    }

    GameObject CreatePlane(string name, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(this.transform);
        go.transform.localPosition = Vector3.zero;
        var planeScript = go.AddComponent<GeoPlaneEntity>();
        
        // Fix điểm dính vào mặt từ bản code trước
        planeScript.pointA = CreatePoint(name + "_A", a, go.transform);
        planeScript.pointB = CreatePoint(name + "_B", b, go.transform);
        planeScript.pointC = CreatePoint(name + "_C", c, go.transform);
        planeScript.pointD = CreatePoint(name + "_D", d, go.transform);

        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = new Material(planeMaterial);
        renderer.material.color = color;
        go.AddComponent<MeshFilter>();
        return go;
    }

    Transform CreatePoint(string n, Vector3 pos, Transform parent)
    {
        GameObject p = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
        p.name = n;
        p.transform.SetParent(parent); 
        p.transform.localPosition = pos; 
        p.transform.localScale = Vector3.one * 0.06f;
        return p.transform;
    }

    GameObject CreateLine(string n, Vector3 start, Vector3 end)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(this.transform);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.04f; lr.endWidth = 0.04f;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = Color.yellow;
        return go;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !_isPlaying) StartCoroutine(PlaySequence());
        if (Keyboard.current.rKey.wasPressedThisFrame) ResetModule();
    }

    void ResetModule()
    {
        StopAllCoroutines();
        _isPlaying = false;
        HardReset(_planeAlpha); HardReset(_planeBeta); HardReset(_planeGamma);
        HardReset(_lineA); HardReset(_lineB); HardReset(_text);
    }

    void HardReset(GameObject obj) {
        if(obj) { obj.SetActive(false); obj.transform.localScale = Vector3.zero; }
    }

    IEnumerator PlaySequence()
    {
        _isPlaying = true;
        _planeAlpha.SetActive(true); _planeBeta.SetActive(true);
        StartCoroutine(Grow(_planeAlpha, 1.0f));
        yield return StartCoroutine(Grow(_planeBeta, 1.0f));
        yield return new WaitForSeconds(0.5f);
        _planeGamma.SetActive(true);
        yield return StartCoroutine(Grow(_planeGamma, 1.2f));
        yield return new WaitForSeconds(0.5f);
        _lineA.SetActive(true); _lineB.SetActive(true);
        StartCoroutine(Grow(_lineA, 0.7f));
        yield return StartCoroutine(Grow(_lineB, 0.7f));
        if(_text) {
            _text.SetActive(true);
            _text.transform.position = (_lineA.transform.position + _lineB.transform.position) / 2 + Vector3.up * 0.2f;
            yield return StartCoroutine(Grow(_text, 0.5f));
        }
        _isPlaying = false;
    }

    IEnumerator Grow(GameObject obj, float duration)
    {
        float e = 0;
        while (e < duration) {
            if (obj == null) yield break;
            e += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, e/duration));
            yield return null;
        }
        if (obj != null) obj.transform.localScale = Vector3.one;
    }
}
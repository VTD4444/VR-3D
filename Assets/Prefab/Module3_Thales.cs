using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Module3ThalesVisualProof : MonoBehaviour
{
    [Header("Tài nguyên")]
    public Material planeMaterial;
    public GameObject pointPrefab;

    private GameObject _p1, _p2, _p3;
    private LineRenderer _lineFixed, _lineMoving;
    private GameObject[] _ptsF = new GameObject[3]; // A, B, C
    private GameObject[] _ptsM = new GameObject[3]; // A', B', C'
    private TextMeshPro _dataDisplay;

    private List<LineRenderer> _fixedTicks = new List<LineRenderer>();
    private List<LineRenderer> _movingTicks = new List<LineRenderer>();

    // Cao độ không cách đều để chứng minh định lý khách quan
    private float y1 = 2.5f, y2 = 1.1f, y3 = 0.0f;
    private float _angle = 20f;
    private float _offset = 1.0f;
    private int _tickCount = 15;

    void Start()
    {
        transform.position = Vector3.zero;
        SetupThalesSpace();
        CreateTicks();
    }

    void SetupThalesSpace()
    {
        // 1. Tạo 3 mặt phẳng song song
        _p1 = CreatePlane("P_Top", y1, new Color(0, 0.4f, 1f, 0.15f));
        _p2 = CreatePlane("P_Mid", y2, new Color(0, 0.4f, 1f, 0.15f));
        _p3 = CreatePlane("P_Bot", y3, new Color(0, 0.4f, 1f, 0.15f));

        // 2. Đoạn thẳng cố định & Nhãn tên A, B, C
        _lineFixed = CreateLR("Fixed_Line", Color.white, 0.03f);
        _lineFixed.SetPosition(0, new Vector3(-1.2f, y1 + 0.5f, 0));
        _lineFixed.SetPosition(1, new Vector3(-1.2f, y3 - 0.5f, 0));

        string[] namesF = { "A", "B", "C" };
        for (int i = 0; i < 3; i++)
        {
            float cy = (i == 0) ? y1 : (i == 1) ? y2 : y3;
            _ptsF[i] = CreateLabeledPoint(namesF[i], new Vector3(-1.2f, cy, 0), Color.red);
        }

        // 3. Đoạn thẳng di động & Nhãn tên A', B', C'
        _lineMoving = CreateLR("Moving_Line", Color.cyan, 0.04f);
        string[] namesM = { "A'", "B'", "C'" };
        for (int i = 0; i < 3; i++)
        {
            _ptsM[i] = CreateLabeledPoint(namesM[i], Vector3.zero, Color.green);
        }

        _dataDisplay = CreateLabel("Data_Display", Color.white, 2.5f);
    }

    // FIX: Hàm tạo nhãn với kích thước hài hòa, không bị "to tổ bố"
    GameObject CreateLabeledPoint(string labelText, Vector3 pos, Color col)
    {
        GameObject p = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
        p.GetComponentInChildren<Renderer>().material.color = col;
        p.transform.localScale = Vector3.one * 0.07f;

        GameObject tObj = new GameObject("Label_" + labelText);
        tObj.transform.SetParent(p.transform);

        // Căn chỉnh vị trí chữ so với quả cầu
        tObj.transform.localPosition = new Vector3(-0.8f, 0.4f, 0);

        var tmp = tObj.AddComponent<TextMeshPro>();
        tmp.text = labelText;
        tmp.fontSize = 2.5f; // Đã sửa từ 20 xuống 2.5 để cân đối
        tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;

        if (!tObj.GetComponent<BillboardEffect>()) tObj.AddComponent<BillboardEffect>();

        return p;
    }

    void CreateTicks()
    {
        float tickWidth = 0.04f;
        for (int i = 0; i < _tickCount; i++)
        {
            _fixedTicks.Add(CreateLR("TickF_" + i, Color.white, tickWidth));
            _movingTicks.Add(CreateLR("TickM_" + i, Color.cyan, tickWidth));
        }
    }

    void Update()
    {
        // Điều khiển phím J-L (xoay) và I-K (tịnh tiến)
        if (Keyboard.current.jKey.isPressed) _angle -= 40f * Time.deltaTime;
        if (Keyboard.current.lKey.isPressed) _angle += 40f * Time.deltaTime;
        if (Keyboard.current.iKey.isPressed) _offset += 1.2f * Time.deltaTime;
        if (Keyboard.current.kKey.isPressed) _offset -= 1.2f * Time.deltaTime;

        UpdateGeometry();
    }

    void UpdateGeometry()
    {
        Vector3 center = new Vector3(_offset, y2, 0);
        Vector3 dir = Quaternion.Euler(0, 0, _angle) * Vector3.up;
        Vector3 sPos = center + dir * 2.2f;
        Vector3 ePos = center - dir * 2.2f;

        _lineMoving.SetPosition(0, sPos);
        _lineMoving.SetPosition(1, ePos);

        bool i1 = GetTargetPoint(sPos, ePos, y1, out Vector3 p1);
        bool i2 = GetTargetPoint(sPos, ePos, y2, out Vector3 p2);
        bool i3 = GetTargetPoint(sPos, ePos, y3, out Vector3 p3);

        _ptsM[0].SetActive(i1); _ptsM[0].transform.position = p1;
        _ptsM[1].SetActive(i2); _ptsM[1].transform.position = p2;
        _ptsM[2].SetActive(i3); _ptsM[2].transform.position = p3;

        // Cập nhật vạch chia (Siêu đậm, siêu dài để nhìn xa vẫn rõ)
        float tLen = 0.35f;
        for (int i = 0; i < _tickCount; i++)
        {
            float ty = y1 - (i * (y1 - y3) / (_tickCount - 1));
            Vector3 posF = new Vector3(-1.2f, ty, 0);
            _fixedTicks[i].SetPosition(0, posF + Vector3.back * tLen);
            _fixedTicks[i].SetPosition(1, posF + Vector3.forward * tLen);

            if (GetTargetPoint(sPos, ePos, ty, out Vector3 posM))
            {
                _movingTicks[i].enabled = true;
                _movingTicks[i].SetPosition(0, posM + Vector3.back * tLen);
                _movingTicks[i].SetPosition(1, posM + Vector3.forward * tLen);
            }
            else { _movingTicks[i].enabled = false; }
        }

        if (i1 && i2 && i3)
        {
            float AB = y1 - y2; float BC = y2 - y3;
            float ApBp = Vector3.Distance(p1, p2); float BpCp = Vector3.Distance(p2, p3);

            _dataDisplay.text = $"<b>THÔNG SỐ ĐO ĐẠC:</b>\n" +
                               $"AB = {AB:F2} | BC = {BC:F2}\n" +
                               $"A'B' = {ApBp:F2} | B'C' = {BpCp:F2}\n" +
                               $"--------------------------\n" +
                               $"<b>TỈ LỆ THALÈS:</b>\n" +
                               $"{AB:F2}/{BC:F2} = <b>{AB / BC:F2}</b>\n" +
                               $"{ApBp:F2}/{BpCp:F2} = <b>{ApBp / BpCp:F2}</b>\n" +
                               $"<color=green>=> TỈ SỐ ĐỒNG NHẤT</color>";
        }
        else
        {
            _dataDisplay.text = "<color=red>CẢNH BÁO: MẤT GIAO ĐIỂM\n(Xoay đoạn thẳng quá mức)</color>";
        }
        _dataDisplay.transform.position = p2 + Vector3.right * 1.5f;
    }

    bool GetTargetPoint(Vector3 s, Vector3 e, float tY, out Vector3 res)
    {
        res = Vector3.zero;
        float t = (tY - s.y) / (e.y - s.y);
        if (t < 0 || t > 1) return false;
        res = Vector3.Lerp(s, e, t);
        return true;
    }

    // --- HELPER METHODS ---
    GameObject CreatePlane(string n, float y, Color col)
    {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var ps = go.AddComponent<GeoPlaneEntity>();
        ps.pointA = CreateAnchor(n + "1", new Vector3(-3.5f, y, -2), go.transform);
        ps.pointB = CreateAnchor(n + "2", new Vector3(3.5f, y, -2), go.transform);
        ps.pointC = CreateAnchor(n + "3", new Vector3(3.5f, y, 2), go.transform);
        ps.pointD = CreateAnchor(n + "4", new Vector3(-3.5f, y, 2), go.transform);
        var ren = go.AddComponent<MeshRenderer>(); ren.material = new Material(planeMaterial);
        ren.material.color = col; go.AddComponent<MeshFilter>(); return go;
    }
    Transform CreateAnchor(string n, Vector3 pos, Transform parent) { GameObject p = new GameObject(n); p.transform.SetParent(parent); p.transform.localPosition = pos; return p.transform; }
    LineRenderer CreateLR(string n, Color col, float w)
    {
        GameObject go = new GameObject(n); go.transform.SetParent(this.transform);
        var lr = go.AddComponent<LineRenderer>(); lr.positionCount = 2; lr.startWidth = w; lr.endWidth = w;
        lr.material = new Material(Shader.Find("Unlit/Color")); lr.material.color = col; return lr;
    }
    TextMeshPro CreateLabel(string n, Color col, float size) { GameObject go = new GameObject(n); go.transform.SetParent(this.transform); var tmp = go.AddComponent<TextMeshPro>(); tmp.fontSize = size; tmp.color = col; tmp.alignment = TextAlignmentOptions.Left; go.AddComponent<BillboardEffect>(); return tmp; }
}
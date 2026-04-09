using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class GeoMeasureLabel : MonoBehaviour
{
    [Header("Cấu hình hiển thị")]
    public Vector3 labelOffset = new Vector3(0, 0.2f, 0); // Đẩy lên 20cm cho rõ
    public float showDistance = 10.0f; // Tăng lên 10m để không bị ẩn quá sớm
    public bool forceShow = true;    // Công tắc này sẽ được GeoAutoSystem điều khiển

    private TextMeshPro _tmp;
    private Camera _mainCam;

    void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        _mainCam = Camera.main;

        _tmp.alignment = TextAlignmentOptions.Center;

        // Tăng fontSize lên để không bị "siêu siêu nhỏ" nữa
        // Minh lưu ý: Scale của Measure_Label trong Prefab nên để là (1, 1, 1)
        _tmp.fontSize = 2.0f;
        _tmp.color = Color.yellow;
    }

    public void UpdateLine(Vector3 p1, Vector3 p2)
    {
        float distance = Vector3.Distance(p1, p2);
        _tmp.text = distance.ToString("F2") + "m";

        // 1. Tính hướng của đường thẳng
        Vector3 lineDir = (p2 - p1).normalized;

        // 2. Tính Vector vuông góc (Pháp tuyến giả định)
        // Dùng tích có hướng giữa hướng đường thẳng và Vector.up (hoặc Vector.right nếu đường thẳng đứng)
        Vector3 sideDir = Vector3.Cross(lineDir, Vector3.up);

        // Nếu đường thẳng trùng với trục đứng (Up), sideDir sẽ bằng 0, ta đổi sang dùng Vector.right
        if (sideDir.sqrMagnitude < 0.001f)
        {
            sideDir = Vector3.Cross(lineDir, Vector3.right);
        }

        // 3. Tính Vector pháp tuyến thực sự vuông góc với đường thẳng và hướng lên trên
        Vector3 normal = Vector3.Cross(sideDir, lineDir).normalized;

        // 4. Đặt vị trí nhãn tại trung điểm và đẩy ra theo hướng pháp tuyến
        float pushDistance = 0.15f; // Đẩy ra 15cm
        transform.position = ((p1 + p2) / 2f) + (normal * pushDistance);

        HandleVisibilityAndBillboard();
    }

    public void UpdatePlane(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        float area = Vector3.Cross(side1, side2).magnitude * 0.5f;

        _tmp.text = "S: " + area.ToString("F2") + "m²";

        Vector3 normal = Vector3.Cross(side1, side2).normalized;
        // Đẩy nhãn ra khỏi mặt phẳng theo hướng pháp tuyến 15cm
        transform.position = ((a + b + c) / 3f) + (normal * 0.15f);

        HandleVisibilityAndBillboard();
    }

    private void HandleVisibilityAndBillboard()
    {
        // 1. Tìm Camera
        if (_mainCam == null) _mainCam = Camera.main;

        // 2. ÉP HIỆN THEO PHÍM L (Bỏ qua khoảng cách để debug)
        // Nếu Minh muốn sau này đứng gần mới hiện thì mới dùng distToCam
        _tmp.enabled = forceShow;

        // 3. Nếu đang bật thì mới xoay mặt về phía người dùng
        if (_tmp.enabled && _mainCam != null)
        {
            transform.LookAt(transform.position + _mainCam.transform.rotation * Vector3.forward,
                             _mainCam.transform.rotation * Vector3.up);
        }
        else if (_mainCam == null)
        {
            // Nếu Console hiện dòng này, Minh phải gán Tag MainCamera cho Camera VR
            Debug.LogWarning("Minh ơi, chưa gán Tag MainCamera cho Camera trong XR Origin rồi!");
        }
    }

    public void ToggleLabel()
    {
        forceShow = !forceShow;
    }
}
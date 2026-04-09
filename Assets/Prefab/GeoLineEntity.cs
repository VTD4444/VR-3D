using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GeoLineEntity : MonoBehaviour 
{
    public Transform pointA;
    public Transform pointB;
    
    [Header("Kéo Object con chứa GeoMeasureLabel vào đây")]
    public GeoMeasureLabel measureLabel;

    private LineRenderer _line;

    void Start() 
    {
        _line = GetComponent<LineRenderer>();
        // Cực kỳ quan trọng để dây không bị lệch khi di chuyển
        _line.useWorldSpace = true; 
    }

    void Update() 
    {
        if (pointA != null && pointB != null) 
        {
            // 1. Cập nhật vị trí dây bám theo quả cầu
            _line.SetPosition(0, pointA.position);
            _line.SetPosition(1, pointB.position);

            // 2. Cập nhật nhãn hiển thị độ dài
            if (measureLabel != null) {
                measureLabel.UpdateLine(pointA.position, pointB.position);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem; // 1. Cần thêm thư viện này

public class GeoDisplayManager : MonoBehaviour 
{
    public static bool ShowLabels = true;

    void Update() 
    {
        // 2. Cách bắt phím L theo hệ thống Input mới
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame) 
        {
            ShowLabels = !ShowLabels;
            
            // Tìm tất cả các nhãn và cập nhật trạng thái
            var allLabels = FindObjectsByType<GeoMeasureLabel>(FindObjectsSortMode.None);
            foreach (var label in allLabels) 
            {
                label.forceShow = ShowLabels;
            }

            Debug.Log("Trạng thái nhãn: " + (ShowLabels ? "Bật" : "Tắt"));
        }
    }
}
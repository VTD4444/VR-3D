using UnityEngine;

public class GeoToggleManager : MonoBehaviour 
{
    // Phím tắt để bật tắt (Minh có thể đổi thành nút trên tay cầm VR sau)
    public KeyCode toggleKey = KeyCode.L; 
    private bool _isAllShowing = true;

    void Update() 
    {
        // 1. Kiểm tra nếu Minh nhấn phím L
        if (Input.GetKeyDown(toggleKey)) 
        {
            _isAllShowing = !_isAllShowing;
            ToggleAllLabels(_isAllShowing);
        }
    }

    public void ToggleAllLabels(bool state) 
    {
        // 2. Tìm TẤT CẢ các Script GeoMeasureLabel đang có trong phòng Lab
        GeoMeasureLabel[] allLabels = FindObjectsByType<GeoMeasureLabel>(FindObjectsSortMode.None);

        foreach (var label in allLabels) 
        {
            // 3. Ép biến forceShow của từng cái theo trạng thái chung
            label.forceShow = state;
        }

        Debug.Log("Hệ thống nhãn: " + (state ? "Đang HIỆN" : "Đang ẨN"));
    }
}
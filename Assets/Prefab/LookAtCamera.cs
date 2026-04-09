using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // Gán biến cho Camera chính để tối ưu hiệu suất (không gọi Camera.main trong Update)
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // Cách 1: Xoay mặt về phía Camera (Chữ có thể bị ngược nếu dùng LookAt trực tiếp)
            // transform.LookAt(mainCameraTransform);

            // Cách 2: Chuẩn nhất cho nhãn tên (Billboard)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCameraTransform.position);
        }
    }
}
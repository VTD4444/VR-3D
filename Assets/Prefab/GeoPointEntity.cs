using UnityEngine;
using TMPro;

public class GeoPointEntity : MonoBehaviour 
{
    private TMP_Text _nameText;

    void Awake() 
    {
        _nameText = GetComponentInChildren<TMP_Text>();

        if (_nameText == null)
        {
            Debug.LogError(gameObject.name + " này chưa có Text (TMP) ở bên trong đâu!");
        }
        else 
        {
            Debug.Log("Đã tự động kết nối với chữ: " + _nameText.name);
        }
    }
    public void SetPointName(string newName) 
    {
        if (_nameText != null) _nameText.text = newName;
    }
}
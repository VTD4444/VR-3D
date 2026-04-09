using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Module1Animator : MonoBehaviour
{
    [Header("Giai đoạn 1: Mặt nền")]
    public GameObject plane1; 

    [Header("Giai đoạn 2: Hai đường thẳng")]
    public GameObject lineA; 
    public GameObject lineB;

    [Header("Giai đoạn 3: Các mặt phẳng còn lại")]
    public List<GameObject> otherPlanes = new List<GameObject>();

    [Header("Cấu hình")]
    public float appearDuration = 1.2f; 
    private bool _isAnimRunning = false;

    void Start() { PrepareObjects(); }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !_isAnimRunning)
        {
            _isAnimRunning = true;
            StartCoroutine(PlaySequence());
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            StopAllCoroutines();
            PrepareObjects();
            _isAnimRunning = false;
            Debug.Log("--- ĐÃ RESET TRIỆT ĐỂ ---");
        }
    }

    void PrepareObjects()
    {
        HardReset(plane1);
        HardReset(lineA);
        HardReset(lineB);
        foreach(var p in otherPlanes) HardReset(p);
    }

    void HardReset(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.localScale = Vector3.zero;
        var lr = obj.GetComponentInChildren<LineRenderer>();
        if (lr) lr.enabled = false;
        obj.SetActive(false); 
    }

    IEnumerator PlaySequence()
    {
        // 1. Hiện mặt phẳng 1
        if(plane1) yield return StartCoroutine(GrowObject(plane1));
        yield return new WaitForSeconds(0.5f);

        // 2. Hiện 2 đường thẳng
        if(lineA) StartCoroutine(GrowObject(lineA));
        if(lineB) yield return StartCoroutine(GrowObject(lineB));
        yield return new WaitForSeconds(0.8f);

        // 3. Hiện các mặt phẳng còn lại lần lượt
        foreach(var p in otherPlanes) {
            if(p) {
                yield return StartCoroutine(GrowObject(p));
                yield return new WaitForSeconds(0.5f);
            }
        }
        _isAnimRunning = false;
    }

    IEnumerator GrowObject(GameObject obj)
    {
        if (obj == null) yield break;
        obj.SetActive(true);
        var lr = obj.GetComponentInChildren<LineRenderer>();
        if (lr) lr.enabled = true;

        float elapsed = 0;
        while (elapsed < appearDuration)
        {
            if (obj == null) yield break; 
            elapsed += Time.deltaTime;
            float percent = Mathf.SmoothStep(0, 1, elapsed / appearDuration);
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, percent);
            yield return null;
        }
        if (obj != null) obj.transform.localScale = Vector3.one;
    }
}
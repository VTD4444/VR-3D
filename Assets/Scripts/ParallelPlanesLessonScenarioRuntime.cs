using UnityEngine;

public class ParallelPlanesLessonScenarioRuntime : MonoBehaviour
{
    [SerializeField] private LessonUIWorldPanel uiPanel;
    [SerializeField] private Transform workspaceRoot;
    [SerializeField] private Vector3 workspaceOffset = new Vector3(1.6f, 0f, 0f);
    [SerializeField] private GameObject[] lessonPrefabs = new GameObject[5];
    [SerializeField] private bool destroyCurrentLessonOnSwitch = true;
    [SerializeField] private float navigationCooldownSeconds = 0.2f;

    private int currentModuleIndex;
    private GameObject currentLessonInstance;
    private bool didBindButtons;
    private float lastNavigationTime = -999f;

    private void Start()
    {
        if (workspaceRoot == null) workspaceRoot = transform;
        if (uiPanel == null) uiPanel = FindFirstObjectByType<LessonUIWorldPanel>();
        if (uiPanel == null)
        {
            Debug.LogError("ParallelPlanesLessonScenarioRuntime: Missing LessonUIWorldPanel reference.");
            return;
        }

        uiPanel.SetHeader(LessonContentCatalog.LessonTitle, LessonContentCatalog.LessonTitles[0]);
        uiPanel.SetModuleTitleLabels(LessonContentCatalog.LessonLabels);
        uiPanel.SetActiveModule(0);
        TryBindButtons();
        uiPanel.SetNavigationState(false, LessonContentCatalog.LessonTitles.Length > 1);
        ShowModule(0);
    }

    private void OnEnable()
    {
        TryBindButtons();
    }

    private void TryBindButtons()
    {
        if (didBindButtons || uiPanel == null) return;
        uiPanel.BindNavigationButtons(OnPreviousButtonPressed, OnNextButtonPressed);
        didBindButtons = true;
    }

    // You can also wire these methods directly from Button OnClick in Inspector.
    public void OnPreviousButtonPressed() => GoToPreviousModule();
    public void OnNextButtonPressed() => GoToNextModule();

    public void GoToPreviousModule()
    {
        if (!CanNavigateNow()) return;
        ShowModule(currentModuleIndex - 1);
    }

    public void GoToNextModule()
    {
        if (!CanNavigateNow()) return;
        ShowModule(currentModuleIndex + 1);
    }

    private bool CanNavigateNow()
    {
        if (Time.unscaledTime - lastNavigationTime < navigationCooldownSeconds) return false;
        lastNavigationTime = Time.unscaledTime;
        return true;
    }

    private void ShowModule(int idx)
    {
        int lessonCount = LessonContentCatalog.LessonTitles.Length;
        int clamped = Mathf.Clamp(idx, 0, lessonCount - 1);
        if (clamped == currentModuleIndex && currentLessonInstance != null) return;
        currentModuleIndex = clamped;
        ClearWorkspace();

        uiPanel.SetHeader(LessonContentCatalog.LessonTitle, LessonContentCatalog.LessonTitles[currentModuleIndex]);
        uiPanel.SetContent(LessonContentCatalog.LessonDescriptions[currentModuleIndex]);
        uiPanel.SetActiveModule(currentModuleIndex);
        uiPanel.SetNavigationState(currentModuleIndex > 0, currentModuleIndex < lessonCount - 1);

        SpawnLessonPrefab(currentModuleIndex);
        Debug.Log($"[LessonMenu] Active lesson index = {currentModuleIndex}, title = {LessonContentCatalog.LessonTitles[currentModuleIndex]}");
    }

    private void SpawnLessonPrefab(int index)
    {
        if (lessonPrefabs == null || index < 0 || index >= lessonPrefabs.Length) return;

        GameObject prefab = lessonPrefabs[index];
        if (prefab == null) return;

        Vector3 spawnPosition = workspaceRoot.position + workspaceOffset;
        Quaternion spawnRotation = workspaceRoot.rotation;
        currentLessonInstance = Instantiate(prefab, spawnPosition, spawnRotation);
        if (workspaceRoot != null)
            currentLessonInstance.transform.SetParent(workspaceRoot, true);
        currentLessonInstance.SetActive(true);
        currentLessonInstance.name = prefab.name + "_Runtime";
    }

    private void ClearWorkspace()
    {
        if (!destroyCurrentLessonOnSwitch) return;
        if (currentLessonInstance != null) Destroy(currentLessonInstance);
        currentLessonInstance = null;
    }
}

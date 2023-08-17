using BepInEx.Logging;
using DR;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SortedCargo;

public class SortedCargoBehaviour : MonoBehaviour
{
    public static string DescribeScene(GameScene scene)
    {
        if (scene == null)
            return "null";
        return $"{scene.SceneName} " +
            $"Diving=[{scene.SceneWithDiving}] " +
            $"Loading=[{(SceneLoader.CurrentSceneData == scene ? SceneLoader.IsSceneLoading : "N/A")}]";
    }

    private UnityAction<Scene, LoadSceneMode> _onSceneLoaded;
    private Il2CppSystem.Action<Il2CppSystem.Object, InputActionChange> _onActionChanged;

    public void OnEnable()
    {
        _onSceneLoaded = new Action<Scene, LoadSceneMode>(OnSceneLoaded);
        SceneManager.sceneLoaded += _onSceneLoaded;

        _onActionChanged = new Action<Il2CppSystem.Object, InputActionChange>(OnActionChange);
        InputSystem.add_onActionChange(_onActionChanged);
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= _onSceneLoaded;
        InputSystem.remove_onActionChange(_onActionChanged);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
            return;
        Reset();
        SortedCargo.Log($"Loading scene {DescribeScene(SceneLoader.CurrentSceneData)}");
        startLoad = DateTimeOffset.UtcNow;
    }

    public void Update()
    {
        if (startLoad != null && !SceneLoader.IsSceneLoading)
        {
            SortedCargo.Log($"Loading scene {SceneLoader.CurrentSceneName} took {DateTimeOffset.UtcNow - startLoad.Value:ss\\.fff}s");
            startLoad = null;
            Setup();
        }
    }
    
    public void OnActionChange(Il2CppSystem.Object obj, InputActionChange change) {
        if (change != InputActionChange.ActionPerformed)
            return;

        var action = obj.Cast<InputAction>();
        if (action.name != "Sorting")
            return;

        Sort();
    }

    private DateTimeOffset? startLoad = null;
    public LootSorter LootSorter = null;

    public void Reset()
    {
        startLoad = null;
        LootSorter = null;
    }

    public void Setup()
    {
        // !SceneWithDiving is not an indication that there isn't diving, e.g. mermaid people town isn't tagged with diving

        var canvas = GameObject.Find("/MainCanvas(Clone)");
        var lootPanel = canvas?.transform?.FindComponentChildrenRecursively<PauseLootBoxScrollPanel>()?[0];
        if (SceneLoader.CurrentSceneData?.SceneWithDiving == true && lootPanel == null)
            SortedCargo.Log(LogLevel.Warning, $"Could not find any {nameof(PauseLootBoxScrollPanel)} component in scene {DescribeScene(SceneLoader.CurrentSceneData)}");

        var label = canvas?.transform?.FindChildrenRecursively("LootBoxLabel")?.GetComponent<TextMeshProUGUI>();
        if (SceneLoader.CurrentSceneData?.SceneWithDiving == true && label == null)
            SortedCargo.Log(LogLevel.Warning, $"Could not find any LootBoxLabel.{nameof(TextMeshProUGUI)} component in scene {DescribeScene(SceneLoader.CurrentSceneData)}");

        if (lootPanel != null && label != null)
            LootSorter = new LootSorter(lootPanel, label);
    }

    public void Sort() {
        if (LootSorter == null)
        {
            Setup();
            if (LootSorter == null)
            {
                if (SceneLoader.CurrentSceneData?.SceneWithDiving == true)
                    SortedCargo.Log(LogLevel.Warning, $"Missing components, not sorting");
                return;
            }
        }

        LootSorter.Sort();
    }
}

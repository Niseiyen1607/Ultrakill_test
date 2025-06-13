using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private SceneField persistenceScene;
    [SerializeField] private SceneField levelScene;

    private List<AsyncOperation> operations = new List<AsyncOperation>();

    public void StartGame()
    {
        operations.Add(SceneManager.LoadSceneAsync(persistenceScene));
        operations.Add(SceneManager.LoadSceneAsync(levelScene, LoadSceneMode.Additive));
    }
}

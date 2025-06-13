using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneLoadTrigger : MonoBehaviour
{
    [Header("Porte à déplacer")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 closedOffset = new Vector3(0, -1, 0);
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease easing = Ease.OutCubic;

    [Header("Scènes")]
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneField[] scenesToUnload;

    [Header("Détection")]
    [SerializeField] private string triggeringTag = "Player";

    private Vector3 openPosition;
    private bool hasTriggered = false;

    private void Start()
    {
        if (doorTransform != null)
            openPosition = doorTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag(triggeringTag))
            return;

        hasTriggered = true;
        StartCoroutine(HandleDoorSequence());
    }

    private IEnumerator HandleDoorSequence()
    {
        doorTransform.DOMove(openPosition + closedOffset, moveDuration).SetEase(easing);
        yield return new WaitForSeconds(moveDuration + 3f); 

        LoadScenes();
        UnloadScenes();

    }

    private void LoadScenes()
    {
        foreach (var scene in scenesToLoad)
        {
            if (!IsSceneLoaded(scene.SceneName))
            {
                SceneManager.LoadSceneAsync(scene.SceneName, LoadSceneMode.Additive);
            }
        }
    }

    private void UnloadScenes()
    {
        foreach (var scene in scenesToUnload)
        {
            if (IsSceneLoaded(scene.SceneName))
            {
                SceneManager.UnloadSceneAsync(scene.SceneName);
            }
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
                return true;
        }
        return false;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelRestarter : MonoBehaviour
{
    public static LevelRestarter Instance { get; private set; }

    [SerializeField] private SceneField levelSceneName;
    [SerializeField] private SceneField persistanceScene;
    [SerializeField] private string playerTag = "Player";

    private string currentLevelName;

    private void Awake()
    {
        // Déjà une instance ?
        if (Instance != null && Instance != this)
        {
            // Si c’est une autre scène de niveau, on remplace l’ancienne
            if (Instance.levelSceneName.SceneName != this.levelSceneName.SceneName)
            {
                Destroy(Instance.gameObject);
                Instance = this;
                DontDestroyOnLoad(gameObject);
                currentLevelName = levelSceneName.SceneName;
            }
            else
            {
                Destroy(gameObject); // Même niveau => cette instance est en trop
            }
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentLevelName = levelSceneName.SceneName;
    }

    public void RestartLevel()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        Debug.Log("LevelRestarter: RestartRoutine started");

        SceneManager.LoadScene(persistanceScene);
        SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);

        yield return new WaitUntil(() => SceneManager.GetSceneByName(levelSceneName).isLoaded);

        GameObject player = GameObject.FindWithTag(playerTag);
        GameObject playerParent = player != null ? player.transform.parent?.gameObject : null;

        GameObject respawnObj = GameObject.FindWithTag("Respawn");

        if (player != null && respawnObj != null && playerParent != null)
        {
            Debug.Log("Respawning player...");
            playerParent.transform.position = respawnObj.transform.position;
            Rigidbody rb = playerParent.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("Player or Respawn point not found after scene reload.");
        }
    }

    public static void ClearInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}

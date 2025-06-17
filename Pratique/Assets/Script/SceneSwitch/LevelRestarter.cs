using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelRestarter : MonoBehaviour
{
    [SerializeField] private SceneField levelSceneName;
    [SerializeField] private SceneField persistanceScene;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float delayBeforeRestart = 1f;

    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            RestartLevel();
        }
    }

    public void RestartLevel()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        SceneManager.LoadScene(persistanceScene);
        SceneManager.LoadScene(levelSceneName, LoadSceneMode.Additive);

        yield return new WaitUntil(() => SceneManager.GetSceneByName(levelSceneName).isLoaded);

        GameObject player = GameObject.FindWithTag(playerTag);
        if (player && respawnPoint)
        {
            player.transform.position = respawnPoint.position;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}

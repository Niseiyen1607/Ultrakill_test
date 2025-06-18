using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (LevelRestarter.Instance != null)
            {
                LevelRestarter.Instance.RestartLevel();
            }
            else
            {
                Debug.LogWarning("No LevelManager instance found!");
            }
        }
    }
}

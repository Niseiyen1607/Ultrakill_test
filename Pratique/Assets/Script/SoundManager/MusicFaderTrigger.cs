using UnityEngine;

public class MusicFaderTrigger : MonoBehaviour
{
    public enum TriggerFadeMode { FadeIn, FadeOut }

    public TriggerFadeMode mode = TriggerFadeMode.FadeIn;
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (MusicFaderManager.Instance == null) return;

        switch (mode)
        {
            case TriggerFadeMode.FadeIn:
                MusicFaderManager.Instance.StartPlaylist();
                break;
            case TriggerFadeMode.FadeOut:
                MusicFaderManager.Instance.StopPlaylist();
                break;
        }
    }
}

using UnityEngine;
using System.Collections;

public class S_Door_Teleport : MonoBehaviour
{
    public Transform player;
    public Transform destination;
    public GameObject playerg;
    public S_Fade_black fader;
    
    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    private IEnumerator TeleportRoutine()
    {
        S_Player_movement movement =
            player.GetComponent<S_Player_movement>();

        movement.enabled = false;

        yield return fader.FadeOut();

        player.position = destination.position;

        yield return new WaitForSeconds(0.25f);

        yield return fader.FadeIn();

        movement.enabled = true;
    }
}
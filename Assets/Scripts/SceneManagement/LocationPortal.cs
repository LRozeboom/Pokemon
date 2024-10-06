using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Teleports the player to a different position without switching scenes
/// </summary>
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] DestinationIdentifier destinationPortal;

    PlayerController player;
    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    IEnumerator Teleport()
    {
        GameController.Instance.PauseGame(pause: true);

        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.spawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(pause: false);
    }

    public Transform SpawnPoint => spawnPoint;
}

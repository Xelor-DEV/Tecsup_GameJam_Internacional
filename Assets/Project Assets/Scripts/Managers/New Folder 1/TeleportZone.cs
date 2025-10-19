using UnityEngine;
using System.Collections;

public class TeleportZone : MonoBehaviour
{
    [Header("Teleport Configuration")]
    [SerializeField] private Entity targetEntity = Entity.Player;
    [SerializeField] private Transform teleportDestination;
    [SerializeField] private int targetCameraIndex = 0;

    [Header("Wave Check")]
    [SerializeField] private WavesCombatData wavesData;
    [SerializeField] private string waveName = "None";

    [Header("Destination Teleport")]
    [SerializeField] private TeleportZone destinationTeleport;

    private bool isTransitioning = false;
    private Collider2D zoneCollider;
    private bool isManuallyDisabled = false;
    private WaveCombat currentWave;

    private void Start()
    {
        zoneCollider = GetComponent<Collider2D>();

        // Find the current wave if specified
        if (wavesData != null && waveName != "None")
        {
            foreach (WaveCombat wave in wavesData.waves)
            {
                if (wave.waveName == waveName)
                {
                    currentWave = wave;
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || isManuallyDisabled) return;

        EntityIdentifier entity = other.GetComponent<EntityIdentifier>();
        if (entity != null && entity.Entity == targetEntity)
        {
            // Check if wave is completed (if this teleport requires a wave)
            if (waveName != "None" && currentWave != null && currentWave.completed)
            {
                // If wave is completed, block teleportation
                return;
            }

            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    private IEnumerator TeleportSequence(Transform playerTransform)
    {
        isTransitioning = true;

        // Disable collider temporarily during transition
        if (zoneCollider != null)
            zoneCollider.enabled = false;

        Time.timeScale = 0f;

        PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        yield return CameraManager.Instance.StartCoroutine(CameraManager.Instance.Fade(0f, 1f));

        yield return new WaitForSecondsRealtime(0.5f);

        playerTransform.position = teleportDestination.position;

        if (targetCameraIndex >= 0)
            CameraManager.Instance.SwitchCamera(targetCameraIndex);

        yield return new WaitForSecondsRealtime(0.3f);

        yield return CameraManager.Instance.StartCoroutine(CameraManager.Instance.Fade(1f, 0f));

        if (playerMovement != null)
            playerMovement.enabled = true;

        Time.timeScale = 1f;

        yield return new WaitForEndOfFrame();

        // Only re-enable this teleport if it doesn't require manual reactivation
        if (zoneCollider != null && !isManuallyDisabled)
        {
            zoneCollider.enabled = true;
        }

        isTransitioning = false;

        // If this teleport has a wave name (and we're teleporting, so wave is not completed yet),
        // then disable the destination teleport until manually reactivated
        if (waveName != "None" && destinationTeleport != null)
        {
            destinationTeleport.ManualDisable();
        }
    }

    public void ManualDisable()
    {
        isManuallyDisabled = true;
        if (zoneCollider != null)
            zoneCollider.enabled = false;
    }

    public void ManualEnable()
    {
        isManuallyDisabled = false;
        if (zoneCollider != null)
            zoneCollider.enabled = true;
    }

    // Draw Gizmos to show the connection to the destination
    private void OnDrawGizmos()
    {
        if (teleportDestination != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, teleportDestination.position);

            // Draw arrow direction
            Vector3 direction = (teleportDestination.position - transform.position).normalized;
            float arrowHeadLength = 0.5f;
            float arrowHeadAngle = 20.0f;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

            Gizmos.DrawRay(transform.position + direction * 0.9f, right * arrowHeadLength);
            Gizmos.DrawRay(transform.position + direction * 0.9f, left * arrowHeadLength);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (teleportDestination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, teleportDestination.position);

            // Draw sphere at both ends
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawWireSphere(teleportDestination.position, 0.3f);
        }
    }
}
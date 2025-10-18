using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float visionRadius = 5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private Entity targetEntity = Entity.Player;

    [Header("References")]
    [SerializeField] private Transform visionPoint;

    public bool CanSeeTarget { get; private set; }
    public Transform Target { get; private set; }

    private void Update()
    {
        DetectTarget();
    }

    private void DetectTarget()
    {
        CanSeeTarget = false;
        Target = null;

        Collider2D[] targetsInView = Physics2D.OverlapCircleAll(visionPoint.position, visionRadius, targetLayers);

        foreach (Collider2D target in targetsInView)
        {
            EntityIdentifier entity = target.GetComponent<EntityIdentifier>();
            if (entity != null && entity.Entity == targetEntity && entity.IsTargetable)
            {
                // Check for obstacles
                Vector2 directionToTarget = (target.transform.position - visionPoint.position).normalized;
                float distanceToTarget = Vector2.Distance(visionPoint.position, target.transform.position);

                RaycastHit2D hit = Physics2D.Raycast(visionPoint.position, directionToTarget, distanceToTarget, obstacleLayers);
                if (hit.collider == null)
                {
                    CanSeeTarget = true;
                    Target = target.transform;
                    break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (visionPoint != null)
        {
            Gizmos.color = CanSeeTarget ? Color.green : Color.red;
            Gizmos.DrawWireSphere(visionPoint.position, visionRadius);
        }
    }
}
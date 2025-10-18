using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GrabManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private int maxDetectionCount = 10;
    [SerializeField] private LayerMask detectionLayers = 1;
    [SerializeField] private Entity targetEntity = Entity.Item;

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 1.5f;
    [SerializeField] private float dropAnimationDuration = 0.6f;
    [SerializeField] private float dropArcHeight = 1f;
    [SerializeField] private Ease dropEase = Ease.OutBack;

    [Header("References")]
    [SerializeField] private Transform detectionPoint;
    [SerializeField] private InventoryDatabase inventoryDatabase;
    [SerializeField] private PlayerMovement player;

    [Header("Gizmos Settings")]
    [SerializeField] private Color noDetectionColor = Color.green;
    [SerializeField] private Color detectionColor = Color.red;
    [SerializeField] private bool showGizmos = true;

    private Collider2D[] detectionResults;
    private int detectionCount = 0;
    private bool isDetecting = false;
    private EntityIdentifier currentDetectedItem;

    private void Awake()
    {
        detectionResults = new Collider2D[maxDetectionCount];
    }

    void Update()
    {
        PerformDetection();
    }

    private void PerformDetection()
    {
        if (detectionPoint == null)
        {
            Debug.LogError("Detection Point no asignado!");
            return;
        }

        detectionCount = Physics2D.OverlapCircleNonAlloc(
            detectionPoint.position,
            detectionRadius,
            detectionResults,
            detectionLayers
        );

        isDetecting = false;
        currentDetectedItem = null;

        EntityIdentifier closestItem = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < detectionCount; i++)
        {
            Collider2D collider = detectionResults[i];
            if (collider != null)
            {
                EntityIdentifier entityIdentifier = collider.GetComponent<EntityIdentifier>();
                if (entityIdentifier != null && entityIdentifier.Entity == targetEntity)
                {
                    float distance = Vector2.Distance(detectionPoint.position, collider.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestItem = entityIdentifier;
                    }
                }
            }
        }

        if (closestItem != null)
        {
            currentDetectedItem = closestItem;
            isDetecting = true;
        }
    }

    public void OnGrabInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentDetectedItem != null)
            {
                ProcessItem(currentDetectedItem);
            }
        }
    }

    public void OnDropInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DropLastItem();
        }
    }

    private void ProcessItem(EntityIdentifier entityIdentifier)
    {
        ItemManager itemManager = entityIdentifier.GetComponent<ItemManager>();
        if (itemManager != null && itemManager.GetItem() != null)
        {
            bool itemAdded = inventoryDatabase.AddItem(itemManager.GetItem());

            if (itemAdded)
            {
                Destroy(entityIdentifier.gameObject);
                currentDetectedItem = null;
                isDetecting = false;
            }
        }
    }

    private void DropLastItem()
    {
        int lastItemIndex = -1;

        for (int i = inventoryDatabase.ArraySize - 1; i >= 0; i--)
        {
            if (inventoryDatabase.GetItem(i) != null)
            {
                lastItemIndex = i;
                break;
            }
        }

        if (lastItemIndex != -1)
        {
            DropItemAtIndex(lastItemIndex);
        }
        else
        {
            Debug.Log("No hay objetos en el inventario para dropear");
        }
    }

    private void DropItemAtIndex(int index)
    {
        InventoryItem itemToDrop = inventoryDatabase.GetItem(index);

        if (itemToDrop != null && itemToDrop.prefab != null)
        {
            // Calcular posición de drop (frente al jugador)
            Vector2 dropDirection = GetPlayerFacingDirection();
            Vector3 dropPosition = player.transform.position + (Vector3)dropDirection * dropDistance;

            // Instanciar el objeto en la posición del jugador
            GameObject droppedObject = Instantiate(itemToDrop.prefab, player.transform.position, Quaternion.identity);

            // Asegurar que el objeto dropeado tenga los componentes necesarios
            ItemManager itemManager = droppedObject.GetComponent<ItemManager>();
            itemManager.SetItem(itemToDrop);

            // Animación suave con DOTween - movimiento en arco
            AnimateDrop(droppedObject, dropPosition);

            // Remover del inventario
            inventoryDatabase.RemoveItem(index);

            Debug.Log($"Objeto dropeado: {itemToDrop.itemName}");
        }
    }

    private void AnimateDrop(GameObject droppedObject, Vector3 targetPosition)
    {
        // Calcular punto medio para el arco
        Vector3 startPosition = droppedObject.transform.position;
        Vector3 midPoint = (startPosition + targetPosition) / 2;
        midPoint.y += dropArcHeight;

        // Crear secuencia de animación
        Sequence dropSequence = DOTween.Sequence();

        // Animación de escala (aparece suavemente)
        droppedObject.transform.localScale = Vector3.zero;
        dropSequence.Append(droppedObject.transform.DOScale(Vector3.one, dropAnimationDuration * 0.3f).SetEase(Ease.OutBack));

        // Animación de movimiento en arco usando Path (más suave que DOJump)
        Vector3[] path = new Vector3[] { startPosition, midPoint, targetPosition };
        dropSequence.Join(droppedObject.transform.DOPath(path, dropAnimationDuration, PathType.CatmullRom)
            .SetEase(dropEase));

        // Efecto de "rebote" al final
        dropSequence.Append(droppedObject.transform.DOScale(Vector3.one * 1.1f, 0.1f));
        dropSequence.Append(droppedObject.transform.DOScale(Vector3.one, 0.1f));
    }

    private Vector2 GetPlayerFacingDirection()
    {
        if (player != null)
        {
            Vector2 moveDirection = player.GetMovementDirection();
            if (moveDirection.magnitude > 0.1f)
            {
                return moveDirection.normalized;
            }
        }

        // Por defecto, usar la dirección a la que mira el sprite
        SpriteRenderer spriteRenderer = player.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.flipX)
        {
            return Vector2.right;
        }
        return Vector2.left;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || detectionPoint == null) return;

        Gizmos.color = isDetecting ? detectionColor : noDetectionColor;
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(detectionPoint.position, 0.1f);

        // Dibujar dirección de drop y arco
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Vector2 dropDirection = GetPlayerFacingDirection();
            Vector3 dropEndPosition = player.transform.position + (Vector3)dropDirection * dropDistance;

            // Dibujar arco de drop
            Vector3 startPosition = player.transform.position;
            Vector3 midPoint = (startPosition + dropEndPosition) / 2;
            midPoint.y += dropArcHeight;

            Gizmos.DrawLine(startPosition, midPoint);
            Gizmos.DrawLine(midPoint, dropEndPosition);
            Gizmos.DrawWireSphere(dropEndPosition, 0.2f);
        }
    }

    public void SetDetectionRadius(float newRadius)
    {
        detectionRadius = Mathf.Max(0.1f, newRadius);
    }

    public void SetMaxDetectionCount(int newMaxCount)
    {
        maxDetectionCount = Mathf.Max(1, newMaxCount);

        if (Application.isPlaying)
        {
            detectionResults = new Collider2D[maxDetectionCount];
        }
    }

    public void SetDetectionPoint(Transform newPoint)
    {
        detectionPoint = newPoint;
    }

    public void SetTargetEntity(Entity newEntity)
    {
        targetEntity = newEntity;
    }

    public bool IsDetecting => isDetecting;
    public int CurrentDetectionCount => detectionCount;
}
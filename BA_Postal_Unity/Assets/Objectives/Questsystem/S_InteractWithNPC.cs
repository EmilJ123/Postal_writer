using UnityEngine;

public class S_PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;

    private S_IInteractable currentInteractable;

    private void Update()
    {
        FindInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F))
        {
            if (currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
            }
        }
    }

    private void FindInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            interactionRange
        );

        S_IInteractable closestInteractable = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in colliders)
        {
            S_IInteractable interactable =
                collider.GetComponent<S_IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector3.Distance(
                    transform.position,
                    collider.transform.position
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        currentInteractable = closestInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 2f;
    public LayerMask interactableLayer;
    public float scanInterval = 0.05f;

    public float promptHeightOffset = 1f;

    private GameObject _currentActivePrompt;
    private IInteractable _bestTarget = null;
    private float _timer = 0f;

    private readonly Collider[] _scanResults = new Collider[10];

    private InputAction _interactAction;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            var playerInput = PlayerController.Instance.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                _interactAction = playerInput.actions["Interact"];
                _interactAction.performed += OnInteract;
            }
        }
    }

    private void OnDestroy()
    {
        if (_interactAction != null)
        {
            _interactAction.performed -= OnInteract;
        }
        ClearCurrentPrompt();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= scanInterval)
        {
            _timer = 0f;
            ScanForBestInteractable();
        }
    }

    private void ScanForBestInteractable()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactRadius, _scanResults, interactableLayer, QueryTriggerInteraction.Collide);

        IInteractable closestInteractable = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 myPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _scanResults[i];
            if (col.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactable is MonoBehaviour mb && (!mb.gameObject.activeInHierarchy || !mb.enabled))
                    continue;

                float distSqr = (col.transform.position - myPos).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != _bestTarget)
        {
            _bestTarget = closestInteractable;

            ClearCurrentPrompt();
            if (_bestTarget != null)
            {
                RefreshCurrentPrompt(_bestTarget);
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_bestTarget != null)
        {
            _bestTarget.Interact();

            ScanForBestInteractable();
        }
    }

    private void ClearCurrentPrompt()
    {
        if (_currentActivePrompt != null)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.HideInteractPrompt(_currentActivePrompt);
            }
            _currentActivePrompt = null;
        }
    }

    private void RefreshCurrentPrompt(IInteractable target)
    {
        if (target is MonoBehaviour mb && PopupManager.Instance != null)
        {
            _currentActivePrompt = PopupManager.Instance.ShowInteractPrompt(mb.transform.position, promptHeightOffset);
            if (_currentActivePrompt != null && _currentActivePrompt.TryGetComponent<InteractPrompt>(out var promptScript))
            {
                promptScript.Init(mb.transform, promptHeightOffset, target.ActionName);
            }
        }
    }
}
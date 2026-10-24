using UnityEngine;

public class PlayerMiningInteraction : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float interactionRange = 1.8f;
    [SerializeField] private MiningInteractionPrompt prompt;
    [SerializeField] private PickaxeMiningAnimation pickaxe;
    [SerializeField] private InventoryController inventory;
    [SerializeField] private Camera worldCamera;

    private MiningTarget currentTarget;
    private bool interactHeld;  
    private bool requireRelease;
    private float elapsed;
    private float completedUntil;

    public MiningTarget CurrentTarget => currentTarget;
    public float Progress => currentTarget == null ? 0f : Mathf.Clamp01(elapsed / currentTarget.HoldDuration);
    public bool IsHolding => interactHeld && !requireRelease && currentTarget != null && elapsed > 0f;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    public void SetInteractHeld(bool held)
    {
        if (held && !interactHeld)
            completedUntil = 0f;
        interactHeld = held;
        if (!held)
        {
            requireRelease = false;
            ResetProgress();
        }
    }

    private void Update()
    {
        MiningTarget nearest = inventory != null && inventory.isOpening ? null : FindNearestTarget();
        if (nearest != currentTarget)
        {
            if (currentTarget != null && interactHeld)
                requireRelease = true;
            ResetProgress();
            completedUntil = 0f;
            currentTarget = nearest;
        }

        if (currentTarget == null)
        {
            if (prompt != null)
                prompt.Hide();
            return;
        }

        if (interactHeld && !requireRelease)
        {
            elapsed += Time.deltaTime;
            if (pickaxe != null)
                pickaxe.SetMining(true, currentTarget.ClosestPoint(transform.position));

            if (elapsed >= currentTarget.HoldDuration)
            {
                MiningTarget completedTarget = currentTarget;
                requireRelease = true;
                completedUntil = Time.time + 0.75f;
                if (pickaxe != null)
                    pickaxe.SetMining(false, Vector3.zero); 
                completedTarget.CompleteMining();
                // Reward callbacks may disable or destroy the target/player.
                if (!isActiveAndEnabled || currentTarget == null || !currentTarget.isActiveAndEnabled)
                {
                    if (prompt != null)
                        prompt.Hide();
                    return;
                }
            }
        }

        if (prompt != null)
        {
            bool completed = Time.time < completedUntil;
            Vector3 promptPosition = currentTarget.PromptPosition;
            prompt.Show(worldCamera, promptPosition, completed ? 1f : Progress, IsHolding, completed);
        }
    }

    private MiningTarget FindNearestTarget()
    {
        MiningTarget nearest = null;
        float nearestDistance = interactionRange * interactionRange;
        foreach (MiningTarget candidate in MiningTarget.ActiveTargets)
        {
            if (candidate == null || !candidate.isActiveAndEnabled)
                continue;

            float distance = (candidate.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }
        return nearest;
    }

    private void ResetProgress()
    {
        elapsed = 0f;
        if (pickaxe != null)
            pickaxe.SetMining(false, Vector3.zero);
    }

    private void OnDisable()
    {
        SetInteractHeld(false);
        currentTarget = null;
        completedUntil = 0f;
        if (prompt != null)
            prompt.Hide();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SetInteractHeld(false);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SetInteractHeld(false);
    }
}

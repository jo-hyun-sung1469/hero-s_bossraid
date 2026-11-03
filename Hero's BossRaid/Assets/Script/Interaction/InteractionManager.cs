using System.Collections.Generic;
using UnityEngine;

// Trigger에 들어온 후보만 관리. 대상 선택, E 입력, 홀드 시간, 안내 UI를 조율.
[DisallowMultipleComponent]
public class InteractionManager : MonoBehaviour
{
    //매니저 오브젝트와 별도로 상호작용을 수행하는 플레이어
    [SerializeField] private PlayerInputHandler player;
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private InventoryController inventory;
    [SerializeField, Min(0f)] private float completedDisplayTime = 0.75f;

    //상호작용이 가능한 오브젝트들의 콜라이더를 모은 딕셔너리
    private readonly Dictionary<InteractionObject, HashSet<Collider2D>> contacts = new Dictionary<InteractionObject, HashSet<Collider2D>>();
    private readonly List<InteractionObject> staleTargets = new List<InteractionObject>();
    private InteractionObject holdingTarget;
    private InteractionObject modalTarget;
    private bool inputHeld;
    private bool requireRelease;
    private bool inputBlocked;
    private float elapsed;
    private float completedUntil;
    public static InteractionManager Instance { get; private set; }

    public PlayerInputHandler Player => player;
    public InteractionObject CurrentTarget { get; private set; }
    public int CandidateCount => contacts.Count;
    public bool HasModalInteraction => modalTarget != null && modalTarget.BlocksOtherInteractions;
    public bool IsBlocked => player == null || !player.isActiveAndEnabled || inputBlocked || (inventory != null && inventory.isOpening) || HasModalInteraction;
    public bool IsHolding => holdingTarget != null && inputHeld && !requireRelease;
    public float Progress => Time.time < completedUntil ? 1f : holdingTarget != null ? Mathf.Clamp01(elapsed / holdingTarget.HoldDuration) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //씬이 바뀌면 기존 매니저에 새 플레이어와 UI 참조를 전달
            Instance.ApplySceneReferences(this);
            Destroy(this);
            return;
        }

        Instance = this;
        if (worldCamera == null)
            worldCamera = Camera.main;
        DontDestroyOnLoad(gameObject);
    }

    private void ApplySceneReferences(InteractionManager sceneManager)
    {
        ClearInteractions();
        player = sceneManager.player;
        interactionUI = sceneManager.interactionUI;
        worldCamera = sceneManager.worldCamera != null ? sceneManager.worldCamera : Camera.main;
        inventory = sceneManager.inventory;
        completedDisplayTime = sceneManager.completedDisplayTime;
        inputBlocked = false;
    }

    public void Register(InteractionObject target, Collider2D playerCollider)
    {
        if (target == null || playerCollider == null || !isActiveAndEnabled)
            return;
        if (player == null || playerCollider.GetComponentInParent<PlayerInputHandler>() != player)
            return;
        if (!contacts.TryGetValue(target, out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            contacts.Add(target, colliders);
        }
        colliders.Add(playerCollider);
    }

    public bool HasContacts(InteractionObject target)
    {
        return target != null && contacts.ContainsKey(target);
    }

    public void Unregister(InteractionObject target, Collider2D playerCollider)
    {
        if (!contacts.TryGetValue(target, out HashSet<Collider2D> colliders))
            return;
        colliders.Remove(playerCollider);
        if (colliders.Count == 0)
            UnregisterObject(target);
    }

    public void UnregisterObject(InteractionObject target)
    {
        contacts.Remove(target);
        if (holdingTarget == target)
            CancelHold(true);
        if (CurrentTarget == target)
        {
            CurrentTarget = null;
            completedUntil = 0f;
            HideUI();
        }
    }

    public void SetBlocked(bool blocked)
    {
        inputBlocked = blocked;
        if (IsBlocked)
        {
            CancelHold(true);
            completedUntil = 0f;
            CurrentTarget = null;
            HideUI();
        }
    }

    public void SetInteractHeld(bool held)
    {
        if (held == inputHeld)
            return;
        inputHeld = held;
        if (!held)
        {
            requireRelease = false;
            CancelHold(false);
            return;
        }

        completedUntil = 0f;
        RefreshCandidates();
        SelectTarget();
        if (IsBlocked || CurrentTarget == null)
        {
            requireRelease = true;
            return;
        }
        if (CurrentTarget.RequiresHold)
        {
            holdingTarget = CurrentTarget;
            holdingTarget.SetHolding(this, true);
        }
        else
        {
            requireRelease = true;
            Execute(CurrentTarget);
        }
    }

    private void Update()
    {
        if (Instance != this)
            return;
        RefreshCandidates();
        if (IsBlocked)
        {
            CancelHold(true);
            completedUntil = 0f;
            CurrentTarget = null;
            HideUI();
            return;
        }
        SelectTarget();
        if (IsHolding)
        {
            holdingTarget.SetHolding(this, true);
            elapsed += Time.deltaTime;
            if (elapsed >= holdingTarget.HoldDuration)
            {
                InteractionObject completed = holdingTarget;
                CancelHold(false);
                requireRelease = true;
                completedUntil = Time.time + completedDisplayTime;
                Execute(completed);
                if (this == null || !isActiveAndEnabled)
                    return;
            }
        }
        if (IsBlocked || CurrentTarget == null || !CurrentTarget.IsAvailable)
            HideUI();
        else if (interactionUI != null)
            interactionUI.Show(worldCamera, CurrentTarget, Progress, IsHolding, Time.time < completedUntil);
    }

    private void RefreshCandidates()
    {
        staleTargets.Clear();
        foreach (var pair in contacts)
        {
            pair.Value.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
            if (pair.Key == null || !pair.Key.IsAvailable || pair.Value.Count == 0)
                staleTargets.Add(pair.Key);
        }
        foreach (InteractionObject target in staleTargets)
            UnregisterObject(target);
    }

    private void SelectTarget()
    {
        if (IsBlocked)
        {
            CurrentTarget = null;
            return;
        }
        // 홀드 도중 다른 대상이 가까워져도 실행 대상을 바꾸지 않음.
        if (inputHeld && (holdingTarget != null || requireRelease) && CurrentTarget != null && contacts.ContainsKey(CurrentTarget) && CurrentTarget.IsAvailable)
            return;
        InteractionObject nearest = null;
        float bestDistance = float.PositiveInfinity;
        if (CurrentTarget != null && contacts.ContainsKey(CurrentTarget) && CurrentTarget.IsAvailable)
        {
            nearest = CurrentTarget;
            bestDistance = ((Vector2)(nearest.ClosestPoint(player.transform.position) - player.transform.position)).sqrMagnitude;
        }
        foreach (InteractionObject candidate in contacts.Keys)
        {
            if (candidate == null || !candidate.IsAvailable)
                continue;
            float distance = ((Vector2)(candidate.ClosestPoint(player.transform.position) - player.transform.position)).sqrMagnitude;
            if (distance < bestDistance)
            {
                nearest = candidate;
                bestDistance = distance;
            }
        }
        if (CurrentTarget != nearest)
            completedUntil = 0f;
        CurrentTarget = nearest;
    }

    private void Execute(InteractionObject target)
    {
        target.Interact(this);
        if (this == null || !isActiveAndEnabled)
            return;
        if (target != null && target.BlocksOtherInteractions)
        {
            modalTarget = target;
            HideUI();
        }
    }

    private void CancelHold(bool needsRelease)
    {
        InteractionObject previous = holdingTarget;
        holdingTarget = null;
        elapsed = 0f;
        if (needsRelease && inputHeld)
            requireRelease = true;
        if (previous != null)
            previous.SetHolding(this, false);
    }

    private void HideUI() { if (interactionUI != null) interactionUI.Hide(); }

    private void OnDisable()
    {
        if (Instance == this)
            ClearInteractions();
    }

    private void ClearInteractions()
    {
        SetInteractHeld(false);
        foreach (InteractionObject target in contacts.Keys)
            if (target != null) target.ForgetPlayer(this);
        contacts.Clear();
        CurrentTarget = null;
        modalTarget = null;
        completedUntil = 0f;
        HideUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationFocus(bool focused) { if (!focused) SetInteractHeld(false); }
    private void OnApplicationPause(bool paused) { if (paused) SetInteractHeld(false); }
}

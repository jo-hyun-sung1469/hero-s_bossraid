using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 감지 영역에 붙이는 공통 상호작용 컴포넌트. 간단한 기능은 On Interacted에 연결.
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InteractionObject : MonoBehaviour
{
    [Header("안내 UI")]
    [SerializeField] protected string actionText = "상호작용";
    [SerializeField] protected string holdingText = "상호작용 중";
    [SerializeField] protected string completedText = "상호작용 완료";
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float promptHeight = 0.5f;
    [Header("실행 방식")]
    [Tooltip("0이면 한 번 누르기, 0보다 크면 지정 시간 동안 홀드")]
    [SerializeField, Min(0f)] private float holdDuration;
    [SerializeField] private UnityEvent onInteracted = new UnityEvent();

    private BoxCollider2D triggerCollider;
    private readonly HashSet<InteractionManager> nearbyPlayers = new HashSet<InteractionManager>();

    public string ActionText => actionText;
    public string HoldingText => holdingText;
    public string CompletedText => completedText;
    public virtual float HoldDuration => holdDuration;
    public bool RequiresHold => HoldDuration > 0f;
    public virtual bool BlocksOtherInteractions => false;
    public BoxCollider2D TriggerCollider => triggerCollider != null ? triggerCollider : GetComponent<BoxCollider2D>();
    public virtual Vector3 PromptPosition => (interactionPoint != null ? interactionPoint.position : transform.position) + Vector3.up * promptHeight;
    public virtual bool IsAvailable => isActiveAndEnabled && TriggerCollider.enabled && gameObject.activeInHierarchy;

    protected virtual void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    protected virtual void Awake()
    {
        triggerCollider = GetComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
    }

    public virtual Vector3 ClosestPoint(Vector3 playerPosition)
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }

    public virtual void Interact(InteractionManager interactor)
    {
        onInteracted.Invoke();
    }

    public virtual void SetHolding(InteractionManager interactor, bool holding) { }

    private void OnTriggerEnter2D(Collider2D other) { Register(other); }
    private void OnTriggerStay2D(Collider2D other) { Register(other); }

    private void Register(Collider2D other)
    {
        // Unity는 비활성 Behaviour에도 Trigger 메시지를 보낼 수 있으므로 명시적으로 검사.
        if (!isActiveAndEnabled)
            return;
        PlayerInputHandler player = other.GetComponentInParent<PlayerInputHandler>();
        InteractionManager manager = player != null ? player.Interactions : null;
        if (manager == null || !manager.isActiveAndEnabled || manager.Player != player)
            return;
        nearbyPlayers.Add(manager);
        manager.Register(this, other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInputHandler player = other.GetComponentInParent<PlayerInputHandler>();
        InteractionManager manager = player != null ? player.Interactions : null;
        if (manager == null)
            return;
        manager.Unregister(this, other);
        if (!manager.HasContacts(this))
            nearbyPlayers.Remove(manager);
    }

    internal void ForgetPlayer(InteractionManager manager) { nearbyPlayers.Remove(manager); }

    protected virtual void OnDisable()
    {
        foreach (InteractionManager manager in nearbyPlayers)
            if (manager != null)
                manager.UnregisterObject(this);
        nearbyPlayers.Clear();
    }
}

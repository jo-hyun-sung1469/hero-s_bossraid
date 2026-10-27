using UnityEngine;

// 검의 스프라이트와 손잡이 위치를 바꾸고, 공통 휘두르기 애니메이션을 재생
public class SwordSwingAnimation : MonoBehaviour
{
    private static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private static readonly int SwingState = Animator.StringToHash("Base Layer.Swing");

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer bladeRenderer;
    [SerializeField] private Sprite normalSword;
    [SerializeField] private Sprite ironSword;
    [SerializeField] private Sprite goldSword;
    [SerializeField] private Sprite rubySword;
    [SerializeField] private Sprite diamondSword;
    [SerializeField, Min(0.1f)] private float bladeSize = 1.35f;
    [SerializeField, Min(0f)] private float handDistance = 0.45f;

    private PlayerMove playerMove;
    private SpriteRenderer playerRenderer;
    private Vector2 facingDirection = Vector2.right;

    private void Awake()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player != null)
            playerRenderer = player.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerMove != null && playerMove.Vector2.sqrMagnitude > 0.001f)
            facingDirection = playerMove.Vector2.normalized;
    }

    public void Play(Weapon weapon, Vector2 direction)
    {
        if (animator == null || bladeRenderer == null)
            return;

        if (!SetWeapon(weapon))
        {
            animator.Play(IdleState, 0, 0f);
            animator.Update(0f);
            bladeRenderer.enabled = false;
            return;
        }

        if (direction.sqrMagnitude < 0.001f)
            direction = facingDirection;
        direction.Normalize();
        facingDirection = direction;

        // 감지한 적을 향하고, 대상이 없으면 마지막 이동 방향으로 휘두름
        transform.localPosition = new Vector3(direction.x * handDistance,
            direction.y * handDistance - 0.1f, 0f);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if (playerRenderer != null)
        {
            bladeRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            bladeRenderer.sortingOrder = playerRenderer.sortingOrder + (direction.y > 0.1f ? -1 : 1);
        }

        animator.Play(SwingState, 0, 0f);
        animator.Update(0f);
    }

    private bool SetWeapon(Weapon weapon)
    {
        Sprite sprite;
        Vector2 grip;
        switch (weapon)
        {
            case Weapon.None:
                sprite = normalSword;
                grip = new Vector2(0.25f, 0.13f);
                break;
            case Weapon.IronSword:
                sprite = ironSword;
                grip = new Vector2(0.12f, 0.10f);
                break;
            case Weapon.GoldSword:
                sprite = goldSword;
                grip = new Vector2(0.13f, 0.10f);
                break;
            case Weapon.RubySword:
                sprite = rubySword;
                grip = new Vector2(0.15f, 0.12f);
                break;
            case Weapon.DiamondSword:
                sprite = diamondSword;
                grip = new Vector2(0.16f, 0.12f);
                break;
            default:
                bladeRenderer.sprite = null;
                return false;
        }

        bladeRenderer.sprite = sprite;
        if (sprite == null)
            return false;

        // 원본 아이콘의 중앙 피벗을 유지하면서, 실제 회전축은 손잡이에 맞춤
        float scale = bladeSize / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        Vector2 gripOffset = (Vector2.Scale(sprite.rect.size, grip) - sprite.pivot) / sprite.pixelsPerUnit;
        Quaternion rotation = Quaternion.Euler(0f, 0f, -45f);
        bladeRenderer.transform.localRotation = rotation;
        bladeRenderer.transform.localScale = Vector3.one * scale;
        bladeRenderer.transform.localPosition = -(rotation * (Vector3)(gripOffset * scale));
        return true;
    }

    private void OnDisable()
    {
        if (bladeRenderer != null)
            bladeRenderer.enabled = false;
    }
}

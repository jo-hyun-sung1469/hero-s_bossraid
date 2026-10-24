using UnityEngine;

public class PickaxeMiningAnimation : MonoBehaviour
{
    private static readonly int IsMining = Animator.StringToHash("IsMining");

    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 restScale;

    private void Awake()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;
        restScale = transform.localScale;
    }

    public void SetMining(bool mining, Vector3 targetPosition)
    {
        if (animator == null)
            return;

        animator.SetBool(IsMining, mining);
        if (mining && player != null)
        {
            Vector2 direction = (Vector2)(targetPosition - player.position);
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector2.right;
            direction.Normalize();
            transform.localPosition = new Vector3(direction.x * 0.65f, direction.y * 0.65f - 0.1f, 0f);
            float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle * (direction.x < 0f ? -1f : 1f));
            transform.localScale = new Vector3(direction.x < 0f ? -1f : 1f, 1f, 1f);
        }
        else
        {
            animator.Play("Idle", 0, 0f);
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            transform.localScale = restScale;
        }
    }
}

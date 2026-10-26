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

    //광질 에니메이션 실행
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
            //광질 시 벽이 곡괭이와 다른 방향에 있다면 벽이 있는 방향으로 회전 및 이동
            transform.localPosition = new Vector3(direction.x * 0.65f, direction.y * 0.65f - 0.1f, 0f);
            float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle * (direction.x < 0f ? -1f : 1f));
            transform.localScale = new Vector3(direction.x < 0f ? -1f : 1f, 1f, 1f);
        }
        else
        {
            //아니라면 그냥 일반상태 유지
            animator.Play("Idle", 0, 0f);
            transform.localPosition = restPosition;
            transform.localRotation = restRotation;
            transform.localScale = restScale;
        }
    }
}

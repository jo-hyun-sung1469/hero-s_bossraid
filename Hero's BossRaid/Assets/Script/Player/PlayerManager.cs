using UnityEngine;

using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
// 주변 상황을 감지하고 플레이어의 상태 조율하는 Manager
public class PlayerManager : MonoBehaviour
{
    // 지정하지 않으면 플레이어 위치를 공격 범위의 중심으로 사용
    [SerializeField] private Transform attackPoint;
    [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
    [SerializeField] private LayerMask targetLayers = ~0;
    // 플레이 중 Inspector에서 감지 여부 확인
    [SerializeField] private bool hasTargetInRange;

    private PlayerController _playerController;
    private readonly List<Collider2D> rangeHits = new List<Collider2D>();

    public bool HasTargetInRange => hasTargetInRange;
    public IDamageable CurrentTarget { get; private set; }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    // 공격 요청 시 범위 안의 가장 가까운 대상에게 한 번 피해를 줌
    public void CheckAttackRange()
    {
        CurrentTarget = null;
        hasTargetInRange = false;

        Vector2 origin = attackPoint != null ? attackPoint.position : transform.position;
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(targetLayers);
        rangeHits.Clear();
        Physics2D.OverlapBox(origin, new Vector2(attackRange, attackRange*2),90, filter, rangeHits);

        float nearestDistance = float.PositiveInfinity;
        Vector2 attackDirection = Vector2.zero;
        foreach (Collider2D hit in rangeHits)
        {
            // 플레이어 본체와 자식 콜라이더는 감지 대상에서 제외
            if (hit.transform.IsChildOf(_playerController.transform))
                continue;

            // 적의 자식에 콜라이더가 있어도 부모의 IDamageable을 찾음
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, _playerController))
                continue;
            if (target is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                continue;

            float distance = (hit.ClosestPoint(origin) - origin).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                CurrentTarget = target;
                hasTargetInRange = true;
                attackDirection = hit.bounds.center - transform.position;
            }
        }

        // 빗나간 공격도 애니메이션을 재생하고, 대상이 있을 때만 피해를 전달
        float damage = _playerController.Attack(attackDirection);
        if (CurrentTarget != null)
            CurrentTarget.TakeDamage(damage);
    }

    private void OnDisable()
    {
        CurrentTarget = null;
        hasTargetInRange = false;
        rangeHits.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.color = hasTargetInRange ? Color.red : Color.yellow;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackRange, attackRange * 2f, 0f));
        Gizmos.matrix = previousMatrix;
    }
}

using UnityEngine;

// 플레이어의 채굴 애니메이션만 담당. 감지와 홀드 진행은 InteractionManager에서 관리.
public class PlayerMiningInteraction : MonoBehaviour
{
    //광질 애니메이션을 위한 스크립트
    [SerializeField] private PickaxeMiningAnimation pickaxe;

    //광질 에니메이션 실행 및 종료
    public void SetMining(bool mining, Vector3 targetPosition)
    {
        if (pickaxe != null)
            pickaxe.SetMining(mining, targetPosition);
    }

    private void OnDisable()
    {
        //에니메이션 종료
        SetMining(false, Vector3.zero);
    }
}

using UnityEngine;

// 광석의 Trigger 영역에 부착. 광석/보상 데이터는 기존 MiningTarget을 사용.
public class MiningInteractionObject : InteractionObject
{
    [SerializeField] private MiningTarget miningTarget;

    public override float HoldDuration => miningTarget != null ? miningTarget.HoldDuration : 0f;
    public override Vector3 PromptPosition => miningTarget != null ? miningTarget.PromptPosition : base.PromptPosition;
    public override bool IsAvailable => base.IsAvailable && miningTarget != null && miningTarget.isActiveAndEnabled;

    protected override void Reset()
    {
        base.Reset();
        miningTarget = GetComponentInParent<MiningTarget>();
        actionText = "광석 캐기";
        holdingText = "광석 캐는 중";
        completedText = "채굴 완료";
    }

    public override Vector3 ClosestPoint(Vector3 playerPosition)
    {
        return miningTarget != null ? miningTarget.ClosestPoint(playerPosition) : base.ClosestPoint(playerPosition);
    }

    public override void SetHolding(InteractionManager interactor, bool holding)
    {
        if (interactor.Player == null)
            return;
        PlayerMiningInteraction mining = interactor.Player.GetComponent<PlayerMiningInteraction>();
        if (mining != null)
            mining.SetMining(holding, holding ? ClosestPoint(interactor.Player.transform.position) : Vector3.zero);
    }

    public override void Interact(InteractionManager interactor)
    {
        //렌덤값으로 얻을 광물 설정
        if (miningTarget != null)
            miningTarget.CompleteMining(Random.Range(1f, 100f));
    }
}

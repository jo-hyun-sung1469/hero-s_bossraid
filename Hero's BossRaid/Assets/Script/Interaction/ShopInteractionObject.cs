using UnityEngine;

// 상점/NPC의 Trigger 영역에 부착. 상점 UI와 거래 로직은 ShopManager에 위임.
public class ShopInteractionObject : InteractionObject
{
    //상점 NPC의 ShopManager를 연결
    [SerializeField] private ShopManager shopManager;

    public override float HoldDuration => 0f;
    public override bool IsAvailable => base.IsAvailable && shopManager != null && shopManager.isActiveAndEnabled && !shopManager.IsOpen;
    public override bool BlocksOtherInteractions => shopManager != null && shopManager.IsOpen;

    protected override void Reset()
    {
        base.Reset();
        actionText = "상점 열기";
        completedText = "상점 열림";
    }

    public override void Interact(InteractionManager interactor)
    {
        if (shopManager != null && interactor.Player != null)
            shopManager.OpenShop(interactor.Player.transform);
    }
}

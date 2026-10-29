using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private InventoryController inventory;
    [SerializeField] private GameObject shopObject;
    // 이 스크립트는 상점 UI가 아니라 월드의 상점/NPC에 부착
    [SerializeField, Min(0.1f)] private float interactionRange = 1.8f;

    public bool IsOpen => shopObject != null && shopObject.activeSelf;

    public void BuySword(string weaponName)
    {
        
    }

    public bool TryOpenShop(Transform interactor)
    {
        if (!isActiveAndEnabled || interactor == null)
            return false;

        Vector2 offset = interactor.position - transform.position;
        if (offset.sqrMagnitude > interactionRange * interactionRange)
            return false;

        OpenShop();
        return IsOpen;
    }

    public void OpenShop()
    {//상점 UI를 켜는 기능 (이동하는 애니메이션 X)
        if (shopObject == null)
        {
            Debug.LogWarning("ShopManager의 Shop Object에 상점 UI를 연결해주세요.", this);
            return;
        }

        shopObject.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopObject != null)
            shopObject.SetActive(false);
    }
}

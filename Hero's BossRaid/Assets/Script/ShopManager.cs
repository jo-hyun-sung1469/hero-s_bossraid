using System.Collections.Generic;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum OreItem
{
    None = 0,
    Iron = 200,
    Gold = 2500,
    Ruby = 20000,
    Diamond = 100000,
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private InventoryController inventory;
    [SerializeField] private Image shopObject;
    
    //상점 Image들
    [Header("상점 Image")]
    [SerializeField] private Image sellImage;
    [SerializeField] private Image buyImage;

    [Header("수량 지정 판매 UI")]
    // 전체 UI
    [SerializeField] private GameObject countSelectUI;
    // 선택된 수량 Text
    [SerializeField] private TextMeshProUGUI countText;
    // 수량 증가 버튼
    [SerializeField] private Button increaseCountButton;
    // 수량 감소 버튼
    [SerializeField] private Button decreaseCountButton;
    // 판매 버튼
    [SerializeField] private Button countSellButton;
    // 선택된 판매 품목
    private string selectedSellItem;
    // 선택된 판매 수량
    private int selectedSellCount;

    [Header("재화 부족 안내 UI")]
    [SerializeField] private GameObject insufficientResourceUI;

    //상점에서 구매할 아이템의 정보 Object
    [Header("상점 아이템 Info")]
    [SerializeField] private Dictionary<string, GameObject> infoUI = new Dictionary<string, GameObject>();
    //검에 사용될 재화 Dictionary
    [SerializeField] private Dictionary<string, Dictionary<string, int>> swordCost = new Dictionary<string, Dictionary<string, int>>();
    public bool IsOpen => shopObject != null && shopObject.gameObject.activeSelf;
    public bool IsCountSelectUIOpen => countSelectUI != null && countSelectUI.activeInHierarchy;
    public bool IsInsufficientResourceUIOpen => insufficientResourceUI != null && insufficientResourceUI.activeInHierarchy;

    private void Awake()
    {
        OnCloseCountSelectUI();
        OnCloseInsufficientResourceUI();
    }

    public void OpenShop(Transform interactor)
    {
        if (!isActiveAndEnabled || interactor == null || IsOpen)
            return;

        if (shopObject == null)
        {
            Debug.LogWarning("ShopManager의 Shop Object에 상점 UI를 연결해주세요.", this);
            return;
        }

        OnCloseInsufficientResourceUI();
        shopObject.gameObject.SetActive(true);
        if (inventory != null)
        {
            inventory.gameObject.SetActive(true);
            inventory.Open();
        }
    }

    public void OnCloseShop()
    {
        OnCloseCountSelectUI();
        OnCloseInsufficientResourceUI();
        if (shopObject != null)
            shopObject.gameObject.SetActive(false);
        if (inventory != null && inventory.gameObject.activeInHierarchy)
            inventory.Close();
    }
    
    // 구입 & 판매 변경시 사용할 기능
    public void OnChangeShop(string shopName)
    {
        OnCloseCountSelectUI();
        OnCloseInsufficientResourceUI();
        if (shopName == "Sell")
        {
            sellImage.gameObject.SetActive(true);
            buyImage.gameObject.SetActive(false);
        }
        else if (shopName == "Buy")
        {
            sellImage.gameObject.SetActive(false);
            buyImage.gameObject.SetActive(true);
        }
        Debug.Log("실행완료");
    }
    
    //상점 구매 기능 종류
    //무기 구매 기능
    public void OnBuySword(string weaponName)
    {
        // 2026-11-04
        if (inventory == null || inventory.Player == null || inventory.Player.HasWeapon(weaponName))
            return;
        switch (weaponName)
        {
            case "Iron":
            case "Gold":
            case "Ruby":
            case "Diamond":
                // 살려는 무기가 Dictionary에 존재하는지 확인
                if (!swordCost.TryGetValue(weaponName, out Dictionary<string, int> cost))
                    return;
                //필요한 아이템을 모두 사용할 수 있을 때만 무기 지급
                if (!UseItemList(cost))
                {
                    OnOpenInsufficientResourceUI();
                    return;
                }
                OnCloseInsufficientResourceUI();
                inventory.GetSword(weaponName);
                break;
        }
    }

    //재화 부족시 사용하는 함수
    public void OnOpenInsufficientResourceUI()
    {
        if (insufficientResourceUI == null)
            return;
        insufficientResourceUI.transform.SetAsLastSibling();
        insufficientResourceUI.SetActive(true);
    }

    //재화 부족 UI 닫기 함수
    public void OnCloseInsufficientResourceUI()
    {
        if (insufficientResourceUI != null)
            insufficientResourceUI.SetActive(false);
    }

    //사용될 아이템이 보유한 아이템보다 적은지 확인하는 함수
    private bool UseItemList(Dictionary<string, int> itemList)
    {
        if (inventory == null || itemList == null)
            return false;

        //하나라도 부족하면 아무 아이템도 차감하지 않고 구매 실패
        foreach (var cost in itemList)
        {
            if (cost.Value < 0)
                return false;

            long ownedCount;
            switch (cost.Key)
            {
                case "Iron": ownedCount = inventory.IronCount; break;
                case "Gold": ownedCount = inventory.GoldCount; break;
                case "Ruby": ownedCount = inventory.RubyCount; break;
                case "Diamond": ownedCount = inventory.DiamondCount; break;
                case "Money": ownedCount = inventory.Money; break;
                default: return false;
            }

            if (ownedCount < cost.Value)
                return false;
        }

        //전체 재료가 충분한 것을 확인한 뒤 한 번에 차감
        foreach (var cost in itemList)
        {
            inventory.UseItem(cost.Key, cost.Value);
        }

        return true;
    }

    //무기에 관한 정보를 보여주는 UI를 활성화하는 함수
    public void OnOpenSwordInfo(string itemName)
    {
        //구매 버튼의 Iron과 정보 버튼의 IronSword를 모두 같은 정보창에 연결
        if (itemName != null && !infoUI.ContainsKey(itemName))
            itemName += "Sword";
        foreach (string item in infoUI.Keys)
        {
            if (item == itemName)
                infoUI[item].SetActive(true);
            else
                infoUI[item].SetActive(false);
        }
    }

    //무기 정보창 닫는 기능
    //이 기능에서 SetActive가 지정된 UI만 닫는 이유는 열었을떄 이미 그 UI만 열었을 뿐만 아니라
    //Info오브젝트도 SetActive를 false로 만들기 때문
    //또한 다시 오브젝트를 열때 열 UI만 true가 되고 나머지는 false가 되기 때문
    public void OnCloseSwordInfo()
    {
        foreach (string item in infoUI.Keys)
        {
                infoUI[item].SetActive(false);
        }
    }

    //상점 판매 기능 종류
    //수량 지정 판매 버튼에서 광석 이름을 전달해 선택창 열기
    public void OnOpenCountSelectUI(string itemName)
    {
        if (inventory == null || countSelectUI == null)
            return;

        selectedSellItem = itemName;
        selectedSellCount = GetOwnedItemCount(itemName) > 0 ? 1 : 0;
        countSelectUI.transform.SetAsLastSibling();
        countSelectUI.SetActive(true);
        UpdateCountSelectUI();
    }

    //판매 수량 증가시 사용하는 함수
    public void OnIncreaseSellCount()
    {
        if (countSelectUI == null || !countSelectUI.activeInHierarchy)
            return;
        if (selectedSellCount < GetOwnedItemCount(selectedSellItem))
            selectedSellCount++;
        UpdateCountSelectUI();
    }

    //판매 수량 감소시 사용하는 함수
    public void OnDecreaseSellCount()
    {
        if (countSelectUI == null || !countSelectUI.activeInHierarchy)
            return;
        if (selectedSellCount > 1)
            selectedSellCount--;
        UpdateCountSelectUI();
    }

    //선택창의 판매 버튼에서 최종 수량으로 실제 판매 요청 함수
    public void OnConfirmCountSell()
    {
        if (countSelectUI == null || !countSelectUI.activeInHierarchy)
            return;
        //창을 연 뒤 보유 수량이 바뀐 경우에도 초과 판매하지 않음
        if (selectedSellCount <= 0 || selectedSellCount > GetOwnedItemCount(selectedSellItem))
        {
            UpdateCountSelectUI();
            return;
        }
        
        //실제 판매 함수 실행
        OnSell(selectedSellItem, selectedSellCount);
        OnCloseCountSelectUI();
    }

    //판매 수량 선택 UI 닫는 함수
    public void OnCloseCountSelectUI()
    {
        if (countSelectUI != null)
            countSelectUI.SetActive(false);
        selectedSellItem = null;
        selectedSellCount = 0;
    }
    
    //판매 수량이 달라질때 마다 / 판매할때 마다 사용되는 함수 
    private void UpdateCountSelectUI()
    {
        int owned = GetOwnedItemCount(selectedSellItem);
        selectedSellCount = owned > 0 ? Mathf.Clamp(selectedSellCount, 1, owned) : 0;
        
        if (countText != null)
            countText.text = selectedSellCount.ToString();
        //판매할 아이템이 소유한 수량보다 많으면 수량 증가 버튼 입력 종료
        if (increaseCountButton != null)
            increaseCountButton.interactable = selectedSellCount < owned;
        //판매할 아이템의 수량이 1보다 작으면 수량 감소 버튼 입력 정지
        if (decreaseCountButton != null)
            decreaseCountButton.interactable = selectedSellCount > 1;
        // 판매 수량이 0보다 작으면 판매 버튼 입력 정지
        if (countSellButton != null)
            countSellButton.interactable = selectedSellCount > 0;
    }

    //현재 가지고 있는 재화 수량 리턴 함수
    private int GetOwnedItemCount(string itemName)
    {
        if (inventory == null)
            return 0;
        switch (itemName)
        {
            case "Iron": return inventory.IronCount;
            case "Gold": return inventory.GoldCount;
            case "Ruby": return inventory.RubyCount;
            case "Diamond": return inventory.DiamondCount;
            default: return 0;
        }
    }

    //광물 판매 기능
    //수량 지정 가능한 판매 기능
    public void OnSell(string itemName, int itemCount)
    {
        if (inventory == null || itemCount <= 0)
            return;
        OreItem ore = OreItem.None;
        switch (itemName)
        {
            case "Iron": ore = itemCount > inventory.IronCount ? OreItem.None : OreItem.Iron; break;
            case "Gold": ore = itemCount > inventory.GoldCount ? OreItem.None : OreItem.Gold;  break;
            case "Ruby": ore = itemCount > inventory.RubyCount ? OreItem.None : OreItem.Ruby;  break;
            case "Diamond": ore = itemCount > inventory.DiamondCount ? OreItem.None : OreItem.Diamond;  break;
        }

        if (ore != OreItem.None)
        {
            inventory.UseItem(itemName, itemCount);
            inventory.GetMoney((long)ore * itemCount);
        }
    }
    //단일 판매 기능
    public void OnSell(string itemName)
    {
        OreItem ore = OreItem.None;
        switch (itemName)
        {
            case "Iron": ore = inventory.IronCount <= 0 ? OreItem.None : OreItem.Iron; break;
            case "Gold": ore = inventory.GoldCount <= 0 ? OreItem.None : OreItem.Gold;  break;
            case "Ruby": ore = inventory.RubyCount <= 0 ? OreItem.None : OreItem.Ruby;  break;
            case "Diamond": ore = inventory.DiamondCount <= 0 ? OreItem.None : OreItem.Diamond;  break;
        }

        if (ore != OreItem.None)
        {
            inventory.UseItem(itemName, 1);
            inventory.GetMoney((int)ore);
        }
    }
    //전체 판매 기능
    public void OnAllSell(string itemName)
    {
        int oreCount = 0;
        OreItem ore = OreItem.None;
        switch (itemName)
        {
            case "Iron": ore = OreItem.Iron;
                oreCount = inventory.IronCount; break;
            case "Gold": ore = OreItem.Gold;
                oreCount = inventory.GoldCount; break;
            case "Ruby": ore = OreItem.Ruby;
                oreCount = inventory.RubyCount; break;
            case "Diamond": ore = OreItem.Diamond;
                oreCount = inventory.DiamondCount; break;
        }

        if (ore != OreItem.None)
        {
            inventory.UseItem(itemName, oreCount);
            inventory.GetMoney((int)ore * oreCount);
        }
    }
}

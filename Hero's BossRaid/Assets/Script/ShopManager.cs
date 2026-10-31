using System.Collections.Generic;
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
    // 매니저가 별도 오브젝트에 있으면 월드의 Shop/NPC를 연결
    [SerializeField] private Transform shopPoint;
    [SerializeField, Min(0.1f)] private float interactionRange = 1.8f;
    
    //상점 Image들
    [Header("상점 Image")]
    [SerializeField] private Image sellImage;
    [SerializeField] private Image buyImage;
    
    //상점에서 구매할 아이템의 정보 Object
    [Header("상점 아이템 Info")]
    [SerializeField] private Dictionary<string, GameObject> infoUI = new Dictionary<string, GameObject>();
    public bool IsOpen => shopObject != null && shopObject.gameObject.activeSelf;

    public void OpenShop(Transform interactor)
    {
        if (!isActiveAndEnabled || interactor == null || IsOpen)
            return;

        Vector3 shopPosition = shopPoint != null ? shopPoint.position : transform.position;
        Vector2 offset = interactor.position - shopPosition;
        if (offset.sqrMagnitude > interactionRange * interactionRange)
            return;

        if (shopObject == null)
        {
            Debug.LogWarning("ShopManager의 Shop Object에 상점 UI를 연결해주세요.", this);
            return;
        }

        shopObject.gameObject.SetActive(true);
        if (inventory != null)
        {
            inventory.gameObject.SetActive(true);
            inventory.Open();
        }
    }

    public void OnCloseShop()
    {
        if (shopObject != null)
            shopObject.gameObject.SetActive(false);
        if (inventory != null && inventory.gameObject.activeInHierarchy)
            inventory.Close();
    }
    
    // 구입 & 판매 변경시 사용할 기능
    public void OnChangeShop(string shopName)
    {
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
    }
    
    //상점 구매 기능 종류
    //무기 구매 기능
    public void OnBuySword(string weaponName)
    {
        switch (weaponName)
        {
            case "Iron": inventory.GetSword("Iron"); break;
            case "Gold": inventory.GetSword("Gold"); break;
            case "Ruby": inventory.GetSword("Ruby"); break;
            case "Diamond": inventory.GetSword("Diamond"); break;
        }
    }

    public void OnOpenSwordInfo(string itemName)
    {
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
    //광물 판매 기능
    //소유하고 있는 아이템보다 많이 팔 수 있는 문제가 존재함(수정 필수!)
    public void OnSell(string itemName, int itemCount)
    {
        OreItem ore = OreItem.None;
        switch (itemName)
        {
            case "Iron": ore = OreItem.Iron; break;
            case "Gold": ore = OreItem.Gold; break;
            case "Ruby": ore = OreItem.Ruby; break;
            case "Diamond": ore = OreItem.Diamond; break;
        }

        if (ore != OreItem.None)
        {
            inventory.UseItem(itemName, itemCount);
            inventory.GetMoney((int)ore * itemCount);
        }
    }

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

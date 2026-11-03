using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class InventoryController : MonoBehaviour
{
    private RectTransform _inventoryPos;
    [SerializeField] private float speed = 20f;
    [HideInInspector] public bool isOpening;

    [Header("무기 장착")]
    [SerializeField] private PlayerController playerController;

    [Header("보유 골드 Text")]
    [SerializeField] private TextMeshProUGUI moneyText;
    
    [Header("광석 Text")]
    [SerializeField] private TextMeshProUGUI ironText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI rubyText;
    [SerializeField] private TextMeshProUGUI diamondText;

    //광석 갯수
    private int _ironCount;
    public int  IronCount { get => _ironCount; set => _ironCount = value; }
    private int _goldCount;
    public  int  GoldCount { get => _goldCount; set => _goldCount = value; }
    private int _rubyCount;
    public int  RubyCount { get => _rubyCount; set => _rubyCount = value; }
    private int _diamondCount;
    public int  DiamondCount { get => _diamondCount; set => _diamondCount = value; }

    //원래는 Gold로 할 생각이였지만 같은 이름의 광석 때문에 변경
    private long _money;
    public long Money => _money;

    private void Awake()
    {
        _inventoryPos = GetComponent<RectTransform>();
        UpdateMoneyUI();
    }
    
    public void Open()
    {
        StopAllCoroutines();
        isOpening = true;
        StartCoroutine(OpenCorutine());
    }

    public void Close()
    {
        StopAllCoroutines();
        isOpening = false;
        StartCoroutine(CloseCorutine());
    }

    public void GetItem(OreType oreType)
    {
        switch (oreType)
        {
            case OreType.Iron: _ironCount++;
                ironText.text = $"철광석 개수: {_ironCount}";
                break;
            case OreType.Gold: _goldCount++; 
                goldText.text = $"금광석 개수:  {_goldCount}";
                break;
            case OreType.Ruby: _rubyCount++;
                rubyText.text = $"루비 개수: {_rubyCount}";
                break;
            case OreType.Diamond: _diamondCount++;
                diamondText.text = $"다이아몬드 개수: {_diamondCount}";
                break;
        }
    }

    //Gold용 GetItem함수
    public void GetMoney(int money)
    {
        GetMoney((long)money);
    }

    public void GetMoney(long money)
    {
        this._money += money;
        UpdateMoneyUI();
    }

    //보유 골드가 바뀌면 인벤토리 왼쪽 상단의 표시 갱신
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"골드: {_money:N0}";
    }

    public void GetSword(string sword)
    {
        //상점에서 재화를 소모하면 얻을 수 있음
    }

    // 검 슬롯의 Button.onClick에서 무기 이름을 전달
    public void EquipSword(string weaponName)
    {
        if (playerController == null)
        {
            Debug.LogWarning("InventoryController에 PlayerController를 연결해주세요.", this);
            return;
        }

        playerController.ChangeWeapon(weaponName);
    }

    public void UseItem(string itemName, int itemcnt)
    {
        switch(itemName)
        {
            case "Iron": _ironCount -= itemcnt; break;
            case "Gold": _goldCount -= itemcnt; break;
            case "Ruby": _rubyCount -= itemcnt; break;
            case "Diamond": _diamondCount -= itemcnt; break;
            case "Money": _money -= itemcnt;
                UpdateMoneyUI(); break;
        }
        UpdateOreUI();
    }

    //판매 및 구매 재료 사용 후 실제 보유 수량 표시
    private void UpdateOreUI()
    {
        if (ironText != null)
            ironText.text = $"철광석 개수: {_ironCount}";
        if (goldText != null)
            goldText.text = $"금광석 개수: {_goldCount}";
        if (rubyText != null)
            rubyText.text = $"루비 개수: {_rubyCount}";
        if (diamondText != null)
            diamondText.text = $"다이아몬드 개수: {_diamondCount}";
    }

    public IEnumerator OpenCorutine()
    {
        while (_inventoryPos.position.x <= 50f)
        {
            _inventoryPos.position += Vector3.right * (speed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator CloseCorutine()
    {
        while (_inventoryPos.position.x >= -1000f)
        {
            _inventoryPos.position += Vector3.left * (speed * Time.deltaTime);
            yield return null;
        }

    }
}

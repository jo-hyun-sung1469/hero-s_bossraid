using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private RectTransform _inventoryPos;
    [SerializeField] private float speed = 20f;
    [HideInInspector] public bool isOpening;
    
    [Header("광석 Text")]
    [SerializeField] private TextMeshProUGUI ironText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI rubyText;
    [SerializeField] private TextMeshProUGUI diamondText;

    //광석 갯수
    private int _ironCount;
    public int IronCount { get; set; }
    private int _goldCount;
    public int GoldCount { get; set; }
    private int _rubyCount;
    public int RubyCount { get; set; }
    private int _diamondCount;
    public int DiamondCount { get; set; }

    private void Awake()
    {
        _inventoryPos = GetComponent<RectTransform>();
    }
    
    public void Open()
    {
        StartCoroutine(OpenCorutine());
    }

    public void Close()
    {
        StartCoroutine(CloseCorutine());
    }

    public void GetItem(OreType oreType)
    {
        switch (oreType)
        {
            case OreType.Iron: _ironCount++;
                ironText.text = _ironCount.ToString();
                break;
            case OreType.Gold: _goldCount++; 
                goldText.text = _goldCount.ToString();
                break;
            case OreType.Ruby: _rubyCount++;
                rubyText.text = _rubyCount.ToString();
                break;
            case OreType.Diamond: _diamondCount++;
                diamondText.text = _diamondCount.ToString();
                break;
        }
    }

    public void GetSword(string sword)
    {
        //대장간에서 재화를 소모하면 얻을 수 있음
    }

    public void UseItem(string itemName, int itemcnt)
    {
        //재화 사용시 사용할 함수
    }

    public IEnumerator OpenCorutine()
    {
        while (_inventoryPos.position.x <= 50f)
        {
            Debug.Log("실행 1");
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

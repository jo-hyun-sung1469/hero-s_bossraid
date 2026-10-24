using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private RectTransform _inventoryPos;
    [SerializeField] private float speed = 20f;
    [HideInInspector] public bool isOpening;

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

    public void GetItem(string itemName, int itemcnt)
    {
        //아이템 획득시 사용할 함수
        //아이템은 몇개 없고 증가 감소만 하면 된다
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

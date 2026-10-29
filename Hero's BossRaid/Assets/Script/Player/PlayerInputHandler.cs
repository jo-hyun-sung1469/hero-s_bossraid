using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerMove))]
public class PlayerInputHandler : MonoBehaviour
{   
    //스크립트 참조
    private PlayerMove _playerMove;
    private PlayerMiningInteraction _miningInteraction;
    private bool isTransitioning = false;
    [SerializeField] private InventoryController inventoryController; 
    [SerializeField] private PlayerManager playerManager;
    // MainScene에서 상점 NPC의 ShopManager를 연결
    [SerializeField] private ShopManager shopManager;

    void Start()
    {
        _playerMove = GetComponent<PlayerMove>();
        _miningInteraction = GetComponent<PlayerMiningInteraction>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            _playerMove.Vector2 = context.ReadValue<Vector2>();
        }
        else if(context.canceled)
        {
            _playerMove.Vector2 = Vector2.zero;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // 키를 떼면 씬의 상호작용 종류와 관계없이 채굴 홀딩을 해제
        if (context.canceled)
        {
            if (_miningInteraction != null)
                _miningInteraction.SetInteractHeld(false);
            return;
        }

        if (isTransitioning || (inventoryController != null && inventoryController.isOpening))
            return;

        if (SceneManager.GetActiveScene().name == "MiningScene")
        {
            // 채광 씬: 누르고 있는 동안 채굴
            if (_miningInteraction != null)
                _miningInteraction.SetInteractHeld(context.ReadValueAsButton());
        }
        else if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (_miningInteraction != null)
                _miningInteraction.SetInteractHeld(false);
            // 메인 씬: 키를 처음 눌렀을 때만 거리를 확인하고 상점을 엶
            if (context.started && shopManager != null)
                shopManager.TryOpenShop(transform);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        // UI를 클릭할 때 월드 공격이 함께 실행되지 않도록 차단
        if (isTransitioning || (inventoryController != null && inventoryController.isOpening)
            || (shopManager != null && shopManager.IsOpen))
            return;

        if (context.phase == InputActionPhase.Started)
        {
            playerManager.CheckAttackRange();
        }
    }
    
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started || isTransitioning)
            return;

        StartCoroutine(ChangeInventory(!inventoryController.isOpening));
    }

    private IEnumerator ChangeInventory(bool open)
    {
        isTransitioning = true;

        if (open)
        {
            inventoryController.gameObject.SetActive(true);

            // 열기 이동이 끝날 때까지 기다림
            yield return inventoryController.OpenCorutine();
        }
        else
        {
            // 닫기 이동이 끝날 때까지 기다림
            yield return inventoryController.CloseCorutine();

            inventoryController.gameObject.SetActive(false);
        }

        inventoryController.isOpening = open;
        isTransitioning = false;
    }

}

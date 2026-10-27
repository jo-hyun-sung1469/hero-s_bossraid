using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
public class PlayerInputHandler : MonoBehaviour
{   
    //스크립트 참조
    private PlayerMove _playerMove;
    private PlayerMiningInteraction _miningInteraction;
    private bool isTransitioning = false;
    [SerializeField] private InventoryController inventoryController; 
    [SerializeField] private PlayerManager playerManager;

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
    {//광석에 상호작용시 작동하는 함수(NPC와의 상호작용도 포함할 예정)
        if (_miningInteraction != null)
            _miningInteraction.SetInteractHeld(!context.canceled && context.ReadValueAsButton());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
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

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
public class PlayerInputHandler : MonoBehaviour
{   
    //스크립트 참조
    private PlayerMove _playerMove;
    //별도 Manager 오브젝트의 상호작용 매니저를 연결
    [SerializeField] private InteractionManager _interactionManager;
    private bool isTransitioning = false;
    [SerializeField] private InventoryController inventoryController; 
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private ShopManager shopManager;

    //씬 이동 후에도 유지되는 매니저를 우선 사용
    public InteractionManager Interactions => InteractionManager.Instance != null ? InteractionManager.Instance : _interactionManager;

    private void Awake()
    {
        _playerMove = GetComponent<PlayerMove>();
        if (shopManager == null)
            shopManager = FindFirstObjectByType<ShopManager>();
    }

    private void Update()
    {
        if (Interactions != null)
            Interactions.SetBlocked(isTransitioning);
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
        if (Interactions == null)
            return;
        Interactions.SetBlocked(isTransitioning);
        //키를 떼면 상호작용 종류와 관계없이 홀딩을 해제
        Interactions.SetInteractHeld(!context.canceled && context.ReadValueAsButton());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        // UI를 클릭할 때 월드 공격이 함께 실행되지 않도록 차단
        if (isTransitioning || (inventoryController != null && inventoryController.isOpening)
            || (Interactions != null && Interactions.IsBlocked))
            return;

        if (context.phase == InputActionPhase.Started)
        {
            playerManager.CheckAttackRange();
        }
    }
    
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Started)
            return;

        //탭 키는 맨 위의 수량 선택 창부터 닫음
        if (shopManager != null && shopManager.IsCountSelectUIOpen)
        {
            shopManager.OnCloseCountSelectUI();
            return;
        }

        //수량 선택 창이 없으면 상점과 인벤토리를 함께 닫음
        if (shopManager != null && shopManager.IsOpen)
        {
            shopManager.OnCloseShop();
            return;
        }

        if (inventoryController == null || isTransitioning || (Interactions != null && Interactions.HasModalInteraction))
            return;

        StartCoroutine(ChangeInventory(!inventoryController.isOpening));
    }

    private void OnDisable()
    {
        if (Interactions != null && Interactions.Player == this)
        {
            //플레이어가 비활성화되면 진행 중인 홀딩도 종료
            Interactions.SetInteractHeld(false);
            Interactions.SetBlocked(true);
        }
    }

    private IEnumerator ChangeInventory(bool open)
    {
        isTransitioning = true;
        //상점에서 시작한 인벤토리 이동과 겹치지 않도록 종료
        inventoryController.StopAllCoroutines();

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

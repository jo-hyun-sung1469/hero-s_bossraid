using UnityEngine;

public class PlayerMiningInteraction : MonoBehaviour
{
    //채굴 상호작용이 가능한 최대거리
    [SerializeField, Min(0.1f)] private float interactionRange = 1.8f;
    //벽과 가까워지면 상호작용할 UI 생성하기 위한 스크립트
    [SerializeField] private MiningInteractionPrompt prompt;
    //광질 애니메이션을 위한 스크립트
    [SerializeField] private PickaxeMiningAnimation pickaxe;
    //인벤토리가 열려 있는지만 확인하는 변수
    [SerializeField] private InventoryController inventory;
    //카메라
    [SerializeField] private Camera worldCamera;

    //광질할 타겟
    private MiningTarget currentTarget;
    //입력유지가 되는지 알려주는 변수
    private bool interactHeld;  
    //입력 유지를 하는지 알려주는 변수(false는 홀딩, 아니면 true)
    private bool requireRelease;
    //입력 유지가 지속된 시간
    private float elapsed;
    //완료 UI가 끝나는 시각
    private float completedUntil;

    public MiningTarget CurrentTarget => currentTarget;
    //채굴 진행상황
    public float Progress => currentTarget == null ? 0f : Mathf.Clamp01(elapsed / currentTarget.HoldDuration);
    //입력이 계속 유지되는지 확인하는 프로퍼티
    public bool IsHolding => interactHeld && !requireRelease && currentTarget != null && elapsed > 0f;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    //Handler와 직접 연결되는 함수, 입력이 유지되는지 확인함
    public void SetInteractHeld(bool held)
    {
        if (held && !interactHeld)
            completedUntil = 0f;
        interactHeld = held;
        if (!held)
        {
            requireRelease = false;
            ResetProgress();
        }
    }

    private void Update()
    {
        //가장 가까운 광질 오브젝트를 찾음 + 인벤토리 오브젝트가 참조되어 있는지 아님 열려있는지 확인
        MiningTarget nearest = inventory != null && inventory.isOpening ? null : FindNearestTarget();
        
        //만약 가까운 광질 오브젝트가 currentTarget 오브젝트가 아니면
        if (nearest != currentTarget)
        {
            if (currentTarget != null && interactHeld)
                requireRelease = true;
            ResetProgress();
            completedUntil = 0f;
            //가장 가까운 오브젝트로 변경
            currentTarget = nearest;
        }
        
        if (currentTarget == null)
        {
            if (prompt != null)
                //광질 UI비활성화
                prompt.Hide();
            return;
        }

        if (interactHeld && !requireRelease)
        {
            //홀딩 시간 증가
            elapsed += Time.deltaTime;
            if (pickaxe != null)
                //광질 에니메이션 실행
                pickaxe.SetMining(true, currentTarget.ClosestPoint(transform.position));
            
            if (elapsed >= currentTarget.HoldDuration)
            {
                //완료된 타겟으로 지정
                MiningTarget completedTarget = currentTarget;
                //홀딩 종료
                requireRelease = true;
                //UI종료 시간 0.75초 추가
                completedUntil = Time.time + 0.75f;
                if (pickaxe != null)
                    //에니메이션 종료
                    pickaxe.SetMining(false, Vector3.zero);
                //렌덤값으로 얻을 광물 설정
                float random = Random.Range(1f, 100f);
                completedTarget.CompleteMining(random);
                
                // Reward callbacks may disable or destroy the target/player.
                if (!isActiveAndEnabled || currentTarget == null || !currentTarget.isActiveAndEnabled)
                {
                    if (prompt != null)
                        prompt.Hide();
                    return;
                }
            }
        }

        if (prompt != null)
        {
            bool completed = Time.time < completedUntil;
            Vector3 promptPosition = currentTarget.PromptPosition;
            prompt.Show(worldCamera, promptPosition, completed ? 1f : Progress, IsHolding, completed);
        }
    }

    //가장 가까운 광질 상호작용 오브젝트를 찾는 함수
    private MiningTarget FindNearestTarget()
    {
        MiningTarget nearest = null;
        float nearestDistance = interactionRange * interactionRange;
        //광질 상호작용 오브젝트가 모인 리스트에서 확인
        foreach (MiningTarget candidate in MiningTarget.ActiveTargets)
        {
            //MiningTarget이 비어있거나 활성화되어 있지않으면
            if (candidate == null || !candidate.isActiveAndEnabled)
                continue;

            //플레이어와 광질 상호작용 오브젝트와의 거리(제곱)
            float distance = (candidate.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            //1.8^2보다 가깝다면
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                //가장 가까운 광질 상호작용 오브젝트로 설정
                nearest = candidate;
            }
        }
        return nearest;
    }

    //광질 작업 초기화
    private void ResetProgress()
    {
        elapsed = 0f;
        if (pickaxe != null)
            pickaxe.SetMining(false, Vector3.zero);
    }

    private void OnDisable()
    {
        //입력 받지 않음
        SetInteractHeld(false);
        currentTarget = null;
        completedUntil = 0f;
        if (prompt != null)
            prompt.Hide();
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SetInteractHeld(false);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SetInteractHeld(false);
    }
}

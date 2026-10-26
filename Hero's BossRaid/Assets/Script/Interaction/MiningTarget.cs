using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class MiningTarget : MonoBehaviour
{
    //캘수 있는 오브젝트 리스트
    private static readonly List<MiningTarget> activeTargets = new List<MiningTarget>();

    //광석 벽 이미지
    [SerializeField] private SpriteRenderer oreRenderer;
    //채굴 시간
    [SerializeField, Min(0.1f)] private float holdDuration = 1.2f;
    //상호작용 UI 세로 위치 오프셋
    [SerializeField] private float promptHeightOffset = 0.5f;
    //광석 채굴 완료후 호출할 함수 이벤트
    [SerializeField] private UnityEvent<OreType> onMined = new UnityEvent<OreType>();
    //SO 추가(얻을 광석을 결정함)
    [SerializeField] private OreProbabilitySO oreProbabilitySO;
    
    //getset
    public static IReadOnlyList<MiningTarget> ActiveTargets => activeTargets;
    public float HoldDuration => holdDuration;
    
    //UI를 위로 조금 올림
    public Vector3 PromptPosition => oreRenderer.bounds.center + Vector3.up * promptHeightOffset;
    public int CompletedCount { get; private set; }

    private void Awake()
    {
        if (oreRenderer == null)
            oreRenderer = GetComponent<SpriteRenderer>();
    }
    
    //활설화시 리스트에 없다면 추가
    private void OnEnable()
    {
        if (!activeTargets.Contains(this))
            activeTargets.Add(this);
    }

    //비활성화시 리스트에 제거
    private void OnDisable()
    {
        activeTargets.Remove(this);
    }

    //플레이어와 광석벽과 가까울떄 실행하는 함수
    public Vector3 ClosestPoint(Vector3 playerPosition)
    {
        Bounds bounds = oreRenderer.bounds;
        //전달한 좌표에서 bounds 영역까지 가장 가까운 점을 반환하는 함수야.
        Vector3 point = bounds.ClosestPoint(new Vector3(playerPosition.x, playerPosition.y, bounds.center.z));
        point.z = playerPosition.z;
        return point;
    }

    //광석 캐기가 완료되었을 경우 실행
    public void CompleteMining(float random)
    {
        if (oreProbabilitySO == null)
        {
            Debug.LogError("MiningTarget에 OreProbability SO를 연결해주세요.", this);
            return;
        }

        CompletedCount++;

        onMined.Invoke(oreProbabilitySO.GetOre(random));
    }
}

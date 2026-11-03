using TMPro;
using UnityEngine;

// 대상의 안내 문구, 키 힌트, 홀드 진행도를 표시. 감지와 실제 실행은 담당하지 않음.
[RequireComponent(typeof(CanvasGroup))]
public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private TMP_Text hintLabel;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private string interactionKey = "E";
    [SerializeField] private UnityEngine.UI.Image progressFill;
    [SerializeField] private GameObject progressTrack;
    [SerializeField] private UnityEngine.UI.Image keyBackground;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    //채굴 등 상호작용시 사용할 UI를 설정 & 보여주는 함수
    public void Show(Camera worldCamera, InteractionObject target, float progress, bool holding, bool completed)
    {
        if (target == null || worldCamera == null || canvas == null || parentRect == null)
        {
            Hide();
            return;
        }
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(target.PromptPosition);
        if (screenPosition.z <= 0f)
        {
            Hide();
            return;
        }
        //E 버튼과 진행도 막대를 오브젝트 위에 배치하는 코드
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out Vector2 localPoint);
        //UI 위치 조정
        Vector2 halfSize = rectTransform.rect.size * 0.5f;
        Rect bounds = parentRect.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x);
        localPoint.y = Mathf.Clamp(localPoint.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y);
        //설정된 위치로 이동
        rectTransform.anchoredPosition = localPoint;

        //text 변경
        if (actionLabel != null)
            actionLabel.text = completed ? target.CompletedText : holding ? target.HoldingText : target.ActionText;
        if (hintLabel != null)
            hintLabel.text = completed ? "손을 떼고 다시 누르세요" : interactionKey + (target.RequiresHold ? " 키를 꾹 누르세요" : " 키를 누르세요");
        if (keyLabel != null)
            keyLabel.text = interactionKey;
        if (progressFill != null)
        {
            progressFill.gameObject.SetActive(target.RequiresHold);
            progressFill.fillAmount = Mathf.Clamp01(progress);
        }
        if (progressTrack != null)
            progressTrack.SetActive(target.RequiresHold);
        if (keyBackground != null)
            keyBackground.color = completed ? new Color(0.78f, 1f, 0.85f) : Color.white;
        //플레이어가 가까우면
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    //플레이어가 멀어지거나 상호작용 안내가 필요 없을 때 발동
    public void Hide()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

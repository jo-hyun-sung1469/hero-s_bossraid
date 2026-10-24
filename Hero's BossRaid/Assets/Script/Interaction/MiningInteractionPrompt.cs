using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MiningInteractionPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private TMP_Text hintLabel;
    [SerializeField] private UnityEngine.UI.Image progressFill;
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

    public void Show(Camera worldCamera, Vector3 worldPosition, float progress, bool holding, bool completed)
    {
        if (worldCamera == null || canvas == null || parentRect == null)
        {
            Hide();
            return;
        }

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            Hide();
            return;
        }

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out Vector2 localPoint);
        Vector2 halfSize = rectTransform.rect.size * 0.5f;
        Rect bounds = parentRect.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, bounds.xMin + halfSize.x, bounds.xMax - halfSize.x);
        localPoint.y = Mathf.Clamp(localPoint.y, bounds.yMin + halfSize.y, bounds.yMax - halfSize.y);
        rectTransform.anchoredPosition = localPoint;

        actionLabel.text = completed ? "채굴 완료" : holding ? "광석 캐는 중" : "광석 캐기";
        hintLabel.text = completed ? "손을 떼고 다시 누르세요" : "E 키를 꾹 누르세요";
        progressFill.fillAmount = Mathf.Clamp01(progress);
        keyBackground.color = completed ? new Color(0.78f, 1f, 0.85f) : Color.white;
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

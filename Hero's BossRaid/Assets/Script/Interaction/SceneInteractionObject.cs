using UnityEngine;
using UnityEngine.SceneManagement;

// 2026-11-05
public class SceneInteractionObject : InteractionObject
{
    [SerializeField] private string sceneName;
    [SerializeField, Min(0.1f)] private float sceneHoldDuration = 1.2f;

    private bool isLoadingScene;

    public override float HoldDuration => sceneHoldDuration;
    public override bool IsAvailable => base.IsAvailable && !isLoadingScene && !string.IsNullOrWhiteSpace(sceneName);
    public override bool BlocksOtherInteractions => isLoadingScene;

    protected override void Reset()
    {
        base.Reset();
        actionText = "입장하기";
        holdingText = "입장 준비 중";
        completedText = "이동 중";
    }

    public override void Interact(InteractionManager interactor)
    {
        if (!IsAvailable || interactor == null || interactor.Player == null)
            return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"이동할 씬을 Build Settings에 추가해주세요: {sceneName}", this);
            return;
        }

        isLoadingScene = true;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class MiningTarget : MonoBehaviour
{
    private static readonly List<MiningTarget> activeTargets = new List<MiningTarget>();

    [SerializeField] private SpriteRenderer oreRenderer;
    [SerializeField, Min(0.1f)] private float holdDuration = 1.2f;
    [SerializeField] private float promptHeightOffset = 0.5f;
    [SerializeField] private UnityEvent onMined = new UnityEvent();

    public static IReadOnlyList<MiningTarget> ActiveTargets => activeTargets;
    public float HoldDuration => holdDuration;
    public Vector3 PromptPosition => oreRenderer.bounds.center + Vector3.up * promptHeightOffset;
    public int CompletedCount { get; private set; }

    private void Awake()
    {
        if (oreRenderer == null)
            oreRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!activeTargets.Contains(this))
            activeTargets.Add(this);
    }

    private void OnDisable()
    {
        activeTargets.Remove(this);
    }

    public Vector3 ClosestPoint(Vector3 playerPosition)
    {
        Bounds bounds = oreRenderer.bounds;
        Vector3 point = bounds.ClosestPoint(new Vector3(playerPosition.x, playerPosition.y, bounds.center.z));
        point.z = playerPosition.z;
        return point;
    }

    public void CompleteMining()
    {
        CompletedCount++;
        onMined.Invoke();
    }
}
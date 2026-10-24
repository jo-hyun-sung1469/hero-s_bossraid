using System;
using System.Collections.Generic;
using UnityEngine;

public enum OreType
{
    Iron = 0,
    Gold = 1,
    Ruby = 2,
    Diamond = 3
}

[Serializable]
public struct OreChance
{
    [SerializeField] private OreType oreType;
    [SerializeField, Min(0)] private int weight;

    public OreType OreType => oreType;
    public int Weight => weight;

    public OreChance(OreType oreType, int weight)
    {
        this.oreType = oreType;
        this.weight = weight;
    }
}

[CreateAssetMenu(fileName = "OreProbability", menuName = "Scriptable Mining/OreProbability")]
public class OreProbability : ScriptableObject
{
    [Header("광석 추첨 가중치")]
    [Tooltip("광석 종류별 가중치를 추가하세요. 실제 확률은 해당 가중치 / 전체 가중치입니다. 0인 광석은 나오지 않습니다.")]
    [SerializeField] private List<OreChance> oreChances = new List<OreChance>
    {
        new OreChance(OreType.Iron, 75),
        new OreChance(OreType.Gold, 25),
        new OreChance(OreType.Ruby, 0),
        new OreChance(OreType.Diamond, 0)
    };

    public IReadOnlyList<OreChance> Probabilities => oreChances.AsReadOnly();

    /// <summary>종류에 관계없이 현재 실제 확률(%)을 확인합니다.</summary>
    public float GetProbability(OreType oreType)
    {
        int totalWeight = GetTotalWeight(oreChances);
        foreach (OreChance chance in oreChances)
            if (chance.OreType == oreType)
                return chance.Weight * 100f / totalWeight;

        return 0f;
    }

    /// <summary>한 광석의 가중치를 변경합니다. 목록에 없으면 추가합니다.</summary>
    public void SetWeight(OreType oreType, int weight)
    {
        var updated = new List<OreChance>(oreChances);
        int index = updated.FindIndex(chance => chance.OreType == oreType);
        var entry = new OreChance(oreType, weight);
        if (index >= 0)
            updated[index] = entry;
        else
            updated.Add(entry);

        GetTotalWeight(updated);
        oreChances = updated;
    }

    /// <summary>모든 광석의 비율을 한 번에 교체합니다. 검증 실패 시 기존 비율을 유지합니다.</summary>
    public void SetProbabilities(params OreChance[] probabilities)
    {
        if (probabilities == null)
            throw new ArgumentNullException(nameof(probabilities));

        var updated = new List<OreChance>(probabilities);
        GetTotalWeight(updated);
        oreChances = updated;
    }

    public OreType GetOre()
    {
        int roll = UnityEngine.Random.Range(0, GetTotalWeight(oreChances));
        foreach (OreChance chance in oreChances)
        {
            if (roll < chance.Weight)
                return chance.OreType;

            roll -= chance.Weight;
        }

        throw new InvalidOperationException("광석 추첨에 실패했습니다. 가중치 목록을 확인해주세요.");
    }

    private static int GetTotalWeight(IReadOnlyList<OreChance> chances)
    {
        if (chances == null || chances.Count == 0)
            throw new ArgumentException("광석 확률 목록이 비어 있습니다.");

        long total = 0;
        var oreTypes = new HashSet<OreType>();
        foreach (OreChance chance in chances)
        {
            if (chance.Weight < 0)
                throw new ArgumentException("광석 가중치는 0 이상이어야 합니다.");
            if (!oreTypes.Add(chance.OreType))
                throw new ArgumentException("같은 광석 종류를 중복 등록할 수 없습니다.");

            total += chance.Weight;
        }

        if (total <= 0 || total > int.MaxValue)
            throw new ArgumentException("전체 광석 가중치 합계는 1 이상, int 최댓값 이하여야 합니다.");

        return (int)total;
    }
}

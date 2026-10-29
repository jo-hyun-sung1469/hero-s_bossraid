using System;
using System.Collections.Generic;
using UnityEngine;

public enum OreType
{//광석 종류
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
public class OreProbabilitySO : ScriptableObject
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

    public OreType GetOre(float random)
    {
        float probability = 0;
        OreType ore = OreType.Iron;
        foreach (OreChance oreChance in oreChances)
        {
            probability += oreChance.Weight;
            if (probability > random)
            {
                ore = oreChance.OreType; 
            }
        }

        return ore;
    }

    public void UpgradeOreProbability(int level)
    {
        //광석 확률 레벨업 로직
    }
}

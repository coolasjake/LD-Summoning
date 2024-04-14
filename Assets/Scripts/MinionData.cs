using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinionData", menuName = "ScriptableObjects/MinionData", order = 1)]
public class MinionData : ScriptableObject
{
    [Header("Summon Stats")]
    public string runeCombo = "0000";
    public static int bloodCost;
    public static int candlesCost;
    public static int soulsCost;
    public static int sandwichesCost;

    [Header("Minion Stats")]
    public ResourceType workType = ResourceType.Blood;
    public int resourcesPerWorkTick = 1;

    public GameObject minionPrefab;
    public Color testColor = Color.white;
}

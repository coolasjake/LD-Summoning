using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinionData", menuName = "ScriptableObjects/MinionData", order = 1)]
public class MinionData : ScriptableObject
{
    [Header("Summon Stats")]
    public string runeCombo = "0000";
    public int bloodCost;
    public int candlesCost;
    public int soulsCost;
    public int fleshCost;

    [Header("Minion Stats")]
    public bool discovered = false;
    public ResourceType workType = ResourceType.Blood;
    public int resourcesPerWorkTick = 1;
    public float moveSpeed = 2f;

    public Sprite minionSprite;
    public Color testColor = Color.white;

    public int Cost(int resourceIndex)
    {
        switch(resourceIndex)
        {
            case 0:
                return bloodCost;
            case 1:
                return candlesCost;
            case 2:
                return soulsCost;
            case 3:
                return fleshCost;
        }

        return 0;
    }
}

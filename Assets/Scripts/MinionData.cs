using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinionData", menuName = "ScriptableObjects/MinionData", order = 1)]
public class MinionData : ScriptableObject
{
    [Header("Circle Stats")]
    public int inspirationCost = 1;
    public int chalkCost = 1;
    public int bloodCost = 0;

    [Header("Minion Stats")]
    public float drawSpeed = 0.1f;
    public float mineSpeed = 0.1f;
    public float researchSpeed = 0.1f;
    public int sacrificePower = 1;

    public Sprite circleSprite;
    public GameObject minionPrefab;
}

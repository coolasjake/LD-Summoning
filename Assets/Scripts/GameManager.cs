using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int[] resources = new int[4];

    public static int blood;
    public static int candles;
    public static int souls;
    public static int meat;

    private static GameManager singleton;

    public static void SetCombo(string runeCombo)
    {

    }

    public static void Summon()
    {
        singleton.TrySummon();
    }


    public Circle circle;
    public List<MinionData> minionsOptions = new List<MinionData>();
    public List<Minion> minions = new List<Minion>();

    public float workInterval = 1f;

    public List<ResourceGenerator> generators = new List<ResourceGenerator>();

    public TMP_Text bloodCost;
    public TMP_Text candlesCost;
    public TMP_Text soulsCost;
    public TMP_Text meatCost;

    private float _lastWork = 0;
    private MinionData _currentSummon = null;

    void Awake()
    {
        singleton = this;
    }

    void Start()
    {
        
    }

    private void TrySummon()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time > _lastWork + workInterval)
        {
            WorkTick();
            _lastWork += workInterval;
        }
    }

    private void WorkTick()
    {

        int[] work = new int[4];

        foreach (Minion minion in minions)
        {
            if (minion.data.workType == ResourceType.Circle)
                continue;

            work[(int)minion.data.workType] += minion.data.resourcesPerWorkTick;
        }

        for (int i = 0; i < 4; ++i)
        {
            resources[i] += work[i];
        }

        foreach (ResourceGenerator generator in generators)
        {
            if (work[(int)generator.type] > 0)
            {
                generator.WorkedAnimation();
            }
        }
    }
}
public enum ResourceType
{
    Blood,
    Candles,
    Souls,
    Meat,
    Circle,
}

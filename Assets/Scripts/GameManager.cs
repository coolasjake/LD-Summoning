using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using System;

public class GameManager : MonoBehaviour
{
    public static long[] resources = new long[4];
    public static long[] workSpeed = new long[4];
    public static long[] manualWork = { 1, 1, 1, 1 };

    private static GameManager singleton;

    public static void SetRunes(string runeCombo)
    {
        singleton.SetSummon(runeCombo);
    }

    public static bool CanSummon()
    {
        return singleton._canSummon;
    }

    public static void Summon()
    {
        singleton.TrySummon();
    }

    public static void ManuallyGenerate(ResourceType type)
    {
        //DEBUG CODE TO TEST LARGE NUMBERS
        if (type == ResourceType.Multiplier)
        {
            for (int i = 0; i < 4; ++i)
            {
                resources[i] += Math.Max(resources[i], 1000);
            }
            singleton.UpdateResourceCounts();
            singleton.CheckCanSummon();
        }
        //END DEBUG CODE

        if ((int)type >= 4)
            return;

        resources[(int)type] += manualWork[(int)type];
        singleton.UpdateResourceCounts();
        singleton.CheckCanSummon();
    }

    [Header("References")]
    public Circle circle;
    public string minionDataFolder = "Data/AbilityData";
    public Minion minionPrefab;
    public List<ResourceGenerator> generators = new List<ResourceGenerator>();

    [Header("Settings")]
    public float workInterval = 1f;

    [Header("Costs")]
    public TMP_Text minionName;
    public TMP_Text loopsCost;
    public TMP_Text minionWorkType;

    public Color cantAffordCol = Color.black;
    public Color[] resourceCols = { Color.red, Color.yellow, Color.cyan, Color.gray };
    public TMP_Text bloodCost;
    public TMP_Text candlesCost;
    public TMP_Text soulsCost;
    public TMP_Text fleshCost;

    public UnityEvent StartMusic = new UnityEvent();
    public UnityEvent EndMusic = new UnityEvent();
    public UnityEvent MusicIntensity1 = new UnityEvent();
    public UnityEvent MusicIntensity2 = new UnityEvent();
    public UnityEvent MusicIntensity3 = new UnityEvent();
    public UnityEvent MusicIntensity4 = new UnityEvent();

    private MinionData[] minionsOptions;
    private List<MinionWorkData> workData = new List<MinionWorkData>();
    private List<Minion> minions = new List<Minion>();

    private float _lastWork = 0;
    private MinionWorkData _currentSummon = null;
    private bool _canSummon = false;
    private int _highestTier = 1;

    void Awake()
    {
        singleton = this;

        LoadMinions();

        foreach (ResourceGenerator generator in generators)
        {
            if ((int)generator.type >= 4)
                continue;
            generator.display.color = resourceCols[(int)generator.type];
        }

        StartMusic.Invoke();
        MusicIntensity1.Invoke();
    }

    void Start()
    {
        
    }

    private void LoadMinions()
    {
        if (minionsOptions == null || minionsOptions.Length == 0)
            minionsOptions = Resources.LoadAll<MinionData>(minionDataFolder);
        if (minionsOptions.Length == 0)
            Debug.LogError("No minions found at path: " + minionDataFolder);
        else
        {
            foreach (MinionData summon in minionsOptions)
            {
                summon.discovered = false;
                workData.Add(new MinionWorkData(summon));
            }

            _currentSummon = workData[0];
            SetSummon("1111");
            _currentSummon.data.discovered = true;
            SetSummon("0000");
            _currentSummon.data.discovered = true;
            CheckCanSummon();
        }    
    }

    private void SetSummon(string runeCombo)
    {
        _currentSummon = workData[0];

        foreach (MinionWorkData summon in workData)
        {
            if (summon.data.runeCombo == runeCombo)
                _currentSummon = summon;
        }

        circle.SetupSummon(_currentSummon.data.drawTime, _currentSummon.data.numLoops);
        CheckCanSummon();
    }

    private void TrySummon()
    {
        if (_canSummon)
        {
            Debug.Log("Summoning " + _currentSummon.data.name);

            for (int i = 0; i < 4; ++i)
            {
                resources[i] -= _currentSummon.data.Cost(i);
            }

            _currentSummon.data.discovered = true;

            bool isTempMinion = _currentSummon.Summon();
            
            if (_currentSummon.data.workType == ResourceType.All)
            {
                for (int i = 0; i < 4; ++i)
                {
                    Minion newMinion = Instantiate<Minion>(minionPrefab, circle.transform.position, Quaternion.identity, transform);
                    newMinion.Setup(_currentSummon.data, generators[i], isTempMinion);
                }
            }
            else
            {
                Minion newMinion = Instantiate<Minion>(minionPrefab, circle.transform.position, Quaternion.identity, transform);
                newMinion.Setup(_currentSummon.data, generators[(int)_currentSummon.data.workType], isTempMinion);
            }

            if (_currentSummon.data.tier > _highestTier)
            {
                _highestTier = _currentSummon.data.tier;
                SetMusicIntensity(_highestTier);
            }
        }

        CheckCanSummon();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time > _lastWork + workInterval)
        {
            WorkTick();
            _lastWork += workInterval;
        }
        CheckCanSummon();
        CircleWorkTick();
    }

    private void WorkTick()
    {
        int[] work = new int[4];
        float multiplier = 100f;

        //Calculate values
        foreach (MinionWorkData minion in workData)
        {
            if ((int)minion.WorkType < 4)
                work[(int)minion.WorkType] += minion.WorkSpeed;
            else if (minion.WorkType == ResourceType.All)
            {
                for (int i = 0; i < 4; ++i)
                    work[i] += minion.WorkSpeed;
            }
            else if (minion.WorkType == ResourceType.Multiplier)
                multiplier += minion.WorkSpeed;
        }

        //Apply bonus
        for (int i = 0; i < 4; ++i)
            work[i] = Mathf.RoundToInt(work[i] * (multiplier * 0.01f));

        //Add work to resources
        for (int i = 0; i < 4; ++i)
        {
            resources[i] += work[i];
        }

        //Play worked animations
        foreach (ResourceGenerator generator in generators)
        {
            if ((int)generator.type >= 4)
                continue;

            if (work[(int)generator.type] > 0)
            {
                generator.WorkedAnimation();
            }
        }


        for (int i = 0; i < 4; ++i)
        {
            workSpeed[i] = work[i];
        }

        //Calculate manual work value
        for (int i = 0; i < 4; ++i)
        {
            manualWork[i] = Mathf.Max(1, work[i]);
        }

        UpdateResourceCounts();
    }

    private void CircleWorkTick()
    {
        //Calculate values
        int totalDrawSpeed = 0;
        foreach (MinionWorkData minion in workData)
        {
            if (minion.WorkType == ResourceType.Circle)
                totalDrawSpeed += minion.WorkSpeed;
        }
        circle.AutoDrawCircle(totalDrawSpeed * Time.fixedDeltaTime, _canSummon);
    }

    private void UpdateResourceCounts()
    {
        foreach (ResourceGenerator generator in generators)
        {
            int type = (int)generator.type;
            if (type >= 4)
                continue;
            generator.display.color = resourceCols[type];
            generator.display.text = resources[type].Shorthand() + "\n" + workSpeed[type].Shorthand() + "/s";
            //generator.display.text = "<sprite name=\"" + generator.type.ToString() + "\">" + resources[type].Shorthand() + "\n" + workSpeed[type].Shorthand() + "/s";
        }
    }

    private void CheckCanSummon()
    {
        _canSummon = true;

        int bCost = GetCost(ResourceType.Blood);
        int cCost = GetCost(ResourceType.Candles);
        int sCost = GetCost(ResourceType.Souls);
        int fCost = GetCost(ResourceType.Flesh);

        bloodCost.text = resources[(int)ResourceType.Blood].Shorthand() + " / " + bCost.Shorthand();
        candlesCost.text = resources[(int)ResourceType.Candles].Shorthand()  + " / " + cCost.Shorthand();
        soulsCost.text = resources[(int)ResourceType.Souls].Shorthand() + " / " + sCost.Shorthand();
        fleshCost.text = resources[(int)ResourceType.Flesh].Shorthand() + " / " + fCost.Shorthand();
        /*
        bloodCost.text = "<sprite name=\"Blood\">" + bCost.Shorthand() + " / " + resources[(int)ResourceType.Blood].Shorthand();
        candlesCost.text = "<sprite name=\"Candles\">" + cCost.Shorthand() + " / " + resources[(int)ResourceType.Candles].Shorthand();
        soulsCost.text = "<sprite name=\"Souls\">" + sCost.Shorthand() + " / " + resources[(int)ResourceType.Souls].Shorthand();
        fleshCost.text = "<sprite name=\"Flesh\">" + fCost.Shorthand() + " / " + resources[(int)ResourceType.Flesh].Shorthand();
        */

        minionWorkType.text = "<sprite name=\"" + _currentSummon.WorkType + "\">";
        loopsCost.text = _currentSummon.data.numLoops + "<sprite name=\"Circle\">";

        //Blood
        if (bCost <= 0)
        {
            bloodCost.text = "";
        }
        else if (bCost > resources[(int)ResourceType.Blood])
        {
            _canSummon = false;
            bloodCost.color = cantAffordCol;
        }
        else
            bloodCost.color = resourceCols[(int)ResourceType.Blood];

        //Candles
        if (cCost <= 0)
        {
            candlesCost.text = "";
        }
        else if (cCost > resources[(int)ResourceType.Candles])
        {
            _canSummon = false;
            candlesCost.color = cantAffordCol;
        }
        else
            candlesCost.color = resourceCols[(int)ResourceType.Candles];

        //Souls
        if (sCost <= 0)
        {
            soulsCost.text = "";
        }
        else if (sCost > resources[(int)ResourceType.Souls])
        {
            _canSummon = false;
            soulsCost.color = cantAffordCol;
        }
        else
            soulsCost.color = resourceCols[(int)ResourceType.Souls];

        //Flesh
        if (fCost <= 0)
        {
            fleshCost.text = "";
        }
        else if (fCost > resources[(int)ResourceType.Flesh])
        {
            _canSummon = false;
            fleshCost.color = cantAffordCol;
        }
        else
            fleshCost.color = resourceCols[(int)ResourceType.Flesh];

        if (_canSummon)
            minionName.color = Color.white;
        else
            minionName.color = Color.red;

        if (_currentSummon.data.discovered)
            minionName.text = _currentSummon.data.name;
        else
            minionName.text = "???";
    }



    public void SetMusicIntensity(int intensity)
    {
        switch (intensity)
        {
            case 1:
                MusicIntensity1.Invoke();
                break;
            case 2:
                MusicIntensity2.Invoke();
                break;
            case 3:
                MusicIntensity3.Invoke();
                break;
            case 4:
                MusicIntensity4.Invoke();
                break;
        }
    }

    public void DEBUG_SummonWorldEater()
    {
        EndMusic.Invoke();
    }

    private int GetCost(ResourceType resourceType)
    {
        if (_currentSummon == null)
            return 1;

        if (resourceType == ResourceType.Circle)
            return Mathf.CeilToInt(_currentSummon.data.drawTime * _currentSummon.costMult);

        int type = (int)resourceType;
        if (type < 4)
            return Mathf.CeilToInt(_currentSummon.data.Cost(type) * _currentSummon.costMult);

        return 0;
    }

    private class MinionWorkData
    {
        public MinionData data;
        public float costMult = 1f;
        public int count = 0;
        public int level = 1;

        public int WorkSpeed => data.resourcesPerWorkTick * level * count;
        public ResourceType WorkType => data.workType;

        public MinionWorkData(MinionData summonData)
        {
            data = summonData;
        }

        public bool Summon()
        {
            costMult += costMult * 0.1f;
            if (count < 3)
            {
                count += 1;
                return false;
            }
            else
                level += 1;
            return true;
        }
    }
}

public static class Utility
{
    public static string Shorthand(this long number)
    {
        return Shorthand((double)number);
    }
    public static string Shorthand(this int number)
    {
        return Shorthand((double)number);
    }
    public static string Shorthand(this float number)
    {
        return Shorthand((double)number);
    }

    public static string Shorthand(this double number)
    {
        //0-999
        if (number < 1000)
            return number.DecimalPlaces(0).ToString();
        //1k-999k
        if (number < 1000000)
            return (number / 1000f).DecimalPlaces(0) + "K";
        //1m-999m
        if (number < 1000000000)
            return (number / 1000000f).DecimalPlaces(0) + "M";
        //1b-999b
        if (number < 1000000000000)
            return (number / 1000000000f).DecimalPlaces(0) + "B";
        //>1t
        return (number / 1000000000000f).DecimalPlaces(0) + "T";
    }

    public static double DecimalPlaces(this double value, int numPlaces)
    {
        numPlaces = Mathf.Clamp(numPlaces, 0, 10);
        float multiplier = Mathf.Pow(10, numPlaces);
        return Math.Round(value * multiplier) / multiplier;
    }
}

public enum ResourceType
{
    Blood,
    Candles,
    Souls,
    Flesh,
    Circle,
    Multiplier,
    All
}

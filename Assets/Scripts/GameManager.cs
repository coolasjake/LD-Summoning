using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static int[] resources = new int[4];
    public static int[] workSpeeds = { 1, 1, 1, 1 };

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
        if (type == ResourceType.Circle)
            return;

        resources[(int)type] += workSpeeds[(int)type];
        Debug.Log(resources[(int)type] + ", added " + workSpeeds[(int)type]);
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

    public Color cantAffordCol = Color.black;
    public Color[] resourceCols = { Color.red, Color.yellow, Color.cyan, Color.gray };
    public TMP_Text bloodCost;
    //public Color bloodCol = Color.red;
    public TMP_Text candlesCost;
    //public Color candlesCol = Color.yellow;
    public TMP_Text soulsCost;
    //public Color soulsCol = Color.cyan;
    public TMP_Text fleshCost;
    //public Color fleshCol = Color.grey;

    public UnityEvent StartMusic = new UnityEvent();
    public UnityEvent EndMusic = new UnityEvent();
    public UnityEvent MusicIntensity1 = new UnityEvent();
    public UnityEvent MusicIntensity2 = new UnityEvent();
    public UnityEvent MusicIntensity3 = new UnityEvent();
    public UnityEvent MusicIntensity4 = new UnityEvent();

    public AudioMixerSnapshot snapshotMain;
    public AudioMixerSnapshot snapshotEnd;

    private MinionData[] minionsOptions;
    private List<Minion> minions = new List<Minion>();

    private float _lastWork = 0;
    private MinionData _currentSummon = null;
    private bool _canSummon = false;

    void Awake()
    {
        singleton = this;

        LoadMinions();

        foreach (ResourceGenerator generator in generators)
        {
            if (generator.type == ResourceType.Circle)
                continue;
            generator.display.color = resourceCols[(int)generator.type];
        }

        snapshotMain.TransitionTo(0.1f);
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
                summon.discovered = false;

            _currentSummon = minionsOptions[0];
            SetSummon("0000");
            CheckCanSummon();
        }    
    }

    private void SetSummon(string runeCombo)
    {
        _currentSummon = minionsOptions[0];

        foreach (MinionData summon in minionsOptions)
        {
            if (summon.runeCombo == runeCombo)
                _currentSummon = summon;
        }

        CheckCanSummon();
    }

    private void TrySummon()
    {
        if (_canSummon)
        {
            for (int i = 0; i < 4; ++i)
            {
                resources[i] -= _currentSummon.Cost(i);
            }

            _currentSummon.discovered = true;

            Minion newMinion = Instantiate<Minion>(minionPrefab, circle.transform.position, Quaternion.identity, transform);
            newMinion.data = _currentSummon;
            newMinion.SR.sprite = _currentSummon.minionSprite;
            newMinion.SR.color = _currentSummon.testColor;
            newMinion.SetGenerator(generators[(int)newMinion.data.workType]);
            minions.Add(newMinion);
        }

        CheckCanSummon();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time > _lastWork + workInterval)
        {
            WorkTick();
            CheckCanSummon();
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
            print((ResourceType)i + " working for " + work[i] + " currently " + resources[i]);
            resources[i] += work[i];
        }

        foreach (ResourceGenerator generator in generators)
        {
            if (generator.type == ResourceType.Circle)
                continue;

            if (work[(int)generator.type] > 0)
            {
                generator.WorkedAnimation();
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            work[i] = Mathf.Max(1, work[i]);
        }

        workSpeeds = work;

        UpdateResourceCounts();
    }

    private void UpdateResourceCounts()
    {
        foreach (ResourceGenerator generator in generators)
        {
            if (generator.type == ResourceType.Circle)
                continue;
            generator.display.color = resourceCols[(int)generator.type];
            generator.display.text = generator.type.ToString() + ":\n" + resources[(int)generator.type];
        }
    }

    private void CheckCanSummon()
    {
        _canSummon = true;

        if (_currentSummon.bloodCost > resources[(int)ResourceType.Blood])
        {
            _canSummon = false;
            bloodCost.color = cantAffordCol;
        }
        else
            bloodCost.color = resourceCols[(int)ResourceType.Blood];

        if (_currentSummon.candlesCost > resources[(int)ResourceType.Candles])
        {
            _canSummon = false;
            candlesCost.color = cantAffordCol;
        }
        else
            candlesCost.color = resourceCols[(int)ResourceType.Candles];

        if (_currentSummon.soulsCost > resources[(int)ResourceType.Souls])
        {
            _canSummon = false;
            soulsCost.color = cantAffordCol;
        }
        else
            soulsCost.color = resourceCols[(int)ResourceType.Souls];

        if (_currentSummon.fleshCost > resources[(int)ResourceType.Flesh])
        {
            _canSummon = false;
            fleshCost.color = cantAffordCol;
        }
        else
            fleshCost.color = resourceCols[(int)ResourceType.Flesh];

        bloodCost.text = "Blood:\n" + _currentSummon.bloodCost + " / " + resources[(int)ResourceType.Blood];
        candlesCost.text = "Candles:\n" + _currentSummon.candlesCost + " / " + resources[(int)ResourceType.Candles];
        soulsCost.text = "Souls:\n" + _currentSummon.soulsCost + " / " + resources[(int)ResourceType.Souls];
        fleshCost.text = "Flesh:\n" + _currentSummon.fleshCost + " / " + resources[(int)ResourceType.Flesh];

        if (_canSummon)
            minionName.color = Color.white;
        else
            minionName.color = Color.red;

        if (_currentSummon.discovered)
            minionName.text = _currentSummon.name;
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
        snapshotEnd.TransitionTo(0.1f);
        EndMusic.Invoke();
    }
}

public enum ResourceType
{
    Blood,
    Candles,
    Souls,
    Flesh,
    Circle,
}

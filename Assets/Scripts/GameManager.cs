using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int chalk;
    public static int inspiration;

    public static Minion GetSacrificeMinion()
    {
        //Get minion from prison
        return null;
    }

    private static GameManager singleton;

    public List<Circle> circles = new List<Circle>();
    public ResourceGenerator mine;
    public ResourceGenerator library;
    public Prison prison;
    public List<Minion> minions = new List<Minion>();

    void Awake()
    {
        singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Minion minion in minions)
        {
            minion.Work();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prison : Workable
{
    public List<Minion> inmates = new List<Minion>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void DoWork(float workSpeed)
    {
        throw new System.NotImplementedException();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    public MinionData data;
    public Workable assignedWorkstation = null;

    public void Work()
    {
        float workSpeed = 1f;

        switch(assignedWorkstation.type)
        {
            //case Workable.WorkType.Chalk:

        }

        assignedWorkstation.DoWork(workSpeed);
    }
}

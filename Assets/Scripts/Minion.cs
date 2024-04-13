using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    public MinionData data;
    public Workable assignedWorkstation = null;

    public void Work()
    {
        if (assignedWorkstation == null)
            return;

        float workSpeed = 1f;

        switch(assignedWorkstation.type)
        {
            case Workable.WorkType.Chalk:
                workSpeed = data.mineSpeed;
                break;
            case Workable.WorkType.Inspiration:
                workSpeed = data.researchSpeed;
                break;
            case Workable.WorkType.Circle:
                workSpeed = data.mineSpeed;
                break;
            case Workable.WorkType.Blood:
                //workSpeed = data.sacrificePower;
                return;
            case Workable.WorkType.None:
                return;
        }

        assignedWorkstation.DoWork(workSpeed);
    }
}

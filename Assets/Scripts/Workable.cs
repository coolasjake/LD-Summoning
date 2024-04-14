using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Workable : MonoBehaviour
{

    /// <summary> Work this station. Return false if no work could be done </summary>
    /// <returns></returns>
    public abstract void DoWork(float workSpeed);
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceGenerator: MonoBehaviour
{
    public ResourceType type;
    public TMP_Text display;
    public float workRadius = 2f;
    public float orbitSpeed = 0.01f;
    [HideInInspector]
    public int numMinions = 0;
    private int _minionsGivenAPos = 0;

    private void OnMouseDown()
    {
        GameManager.ManuallyGenerate(type);
        print("MINING!!! " + type.ToString());
    }

    public void WorkedAnimation()
    {

    }

    private void Update()
    {
        _minionsGivenAPos = 0;
    }

    public Vector3 MinionWorkPos()
    {
        float angle = (_minionsGivenAPos / (float)numMinions) * 360f;
        angle = (angle + Time.time * orbitSpeed) % 360f;
        Vector3 orbitPos = (Quaternion.Euler(0f, 0f, angle) * Vector3.up) * workRadius;
        Vector3 target = transform.position + orbitPos;

        _minionsGivenAPos += 1;

        return target;
    }
}

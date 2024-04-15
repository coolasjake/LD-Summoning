using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResourceGenerator: MonoBehaviour
{
    public ResourceType type;
    public TMP_Text display;
    public float workRadius = 2f;
    public float orbitSpeed = 0.01f;
    [HideInInspector]
    public List<Minion> minions = new List<Minion>();
    private int _minionsGivenAPos = 0;

    public UnityEvent ManualCollect = new UnityEvent();
    public UnityEvent ManualCollectBig = new UnityEvent();

    private void OnMouseDown()
    {
        GameManager.ManuallyGenerate(type);
        ManualCollect.Invoke();
    }

    public void WorkedAnimation()
    {

    }

    private void Update()
    {
        _minionsGivenAPos = 0;
    }

    public void UpdateLevels()
    {

    }

    public Vector3 MinionWorkPos()
    {
        float angle = 0;
        if (minions.Count > 0)
            angle = (_minionsGivenAPos / (float)minions.Count) * 360f;
        angle = (angle + Time.time * orbitSpeed) % 360f;
        Vector3 orbitPos = (Quaternion.Euler(0f, 0f, angle) * Vector3.up) * workRadius;
        Vector3 target = transform.position + orbitPos;

        _minionsGivenAPos += 1;

        return target;
    }
}

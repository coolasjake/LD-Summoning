using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    public SpriteRenderer SR;
    public MinionData data;
    public ResourceGenerator generator;

    public void SetGenerator(ResourceGenerator gen)
    {
        generator = gen;
        generator.numMinions += 1;
    }

    private void Update()
    {
        if (generator == null)
            transform.position = Vector3.MoveTowards(transform.position, Vector3.one * 100f, data.moveSpeed * Time.deltaTime);
        else
            transform.position = Vector3.MoveTowards(transform.position, generator.MinionWorkPos(), data.moveSpeed * Time.deltaTime);
    }
}

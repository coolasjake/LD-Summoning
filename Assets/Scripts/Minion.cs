using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion : MonoBehaviour
{
    private bool isTemp = true;
    public SpriteRenderer SR;
    public MinionData data;
    public ResourceGenerator generator;

    public void Setup(MinionData minData, ResourceGenerator gen, bool temp)
    {
        data = minData;
        isTemp = temp;
        SR.sprite = data.minionSprite;
        SR.color = data.testColor;
        generator = gen;
        if (isTemp == false)
            generator.minions.Add(this);
    }

    private void Update()
    {
        Vector3 target = Vector3.one * 100f;
        if (generator != null)
            target = generator.MinionWorkPos();
        
        transform.position = Vector3.MoveTowards(transform.position, target, data.moveSpeed * Time.deltaTime);
        if (isTemp && transform.position == target)
            AbsorbAndLevel();
    }

    private void AbsorbAndLevel()
    {
        Destroy(gameObject);
    }
}

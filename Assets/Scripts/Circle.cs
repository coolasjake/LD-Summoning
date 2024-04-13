using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Workable
{
    private Material _mat;

    [Range(0f, 1f)]
    public float fill = 1f;

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        _mat = new Material(SR.sharedMaterial);
        SR.material = _mat;
    }

    // Update is called once per frame
    void Update()
    {
        _mat.SetFloat("_Fill", fill);
    }

    public override void DoWork(float workSpeed)
    {
        throw new System.NotImplementedException();
    }
}

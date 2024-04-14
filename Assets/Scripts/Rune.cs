using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune : MonoBehaviour
{
    public delegate void VoidEvent();
    public VoidEvent runePressed;

    public SpriteRenderer SR;
    public Collider2D clickCollider;

    public bool isOn = false;

    void Start()
    {
        if (SR == null)
            SR = GetComponent<SpriteRenderer>();
        if (clickCollider == null)
            clickCollider = GetComponent<Collider2D>();

        SR.enabled = isOn;
    }

    private void OnMouseDown()
    {
        isOn = !isOn;

        SR.enabled = isOn;

        runePressed?.Invoke();
    }

    public bool ColliderOn
    {
        set
        {
            clickCollider.enabled = value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Rune : MonoBehaviour
{
    public delegate void VoidEvent();
    public VoidEvent runePressed;

    public SpriteRenderer SR;
    public Collider2D clickCollider;

    public bool isOn = false;

    public UnityEvent RuneOn = new UnityEvent();
    public UnityEvent RuneOff = new UnityEvent();

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
    }

    private void ToggleRune()
    {
        isOn = !isOn;
        SR.enabled = isOn;
        runePressed?.Invoke();

        if (isOn)
            RuneOn.Invoke();
        else
            RuneOff.Invoke();
    }

    public bool ColliderOn
    {
        set
        {
            clickCollider.enabled = value;
        }
    }
}

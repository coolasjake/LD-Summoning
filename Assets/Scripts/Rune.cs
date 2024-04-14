using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune : MonoBehaviour
{
    public delegate void VoidEvent();
    public VoidEvent runePressed;

    public SpriteRenderer SR;

    public bool isOn = false;

    private void OnMouseDown()
    {
        isOn = !isOn;

        SR.enabled = isOn;

        runePressed?.Invoke();
    }
}

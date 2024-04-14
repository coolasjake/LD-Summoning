using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public SpriteRenderer outerCircle;
    public SpriteRenderer innerCircle;

    public List<Rune> runes = new List<Rune>();

    private Material _outMat;
    private Material _inMat;

    [Range(0f, 1f)]
    public float fill = 1f;

    private float _startAngle = 0f;
    private bool _drawing = false;

    private Vector2 _lastDir = Vector2.zero;

    public static Vector3 MousePos => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    // Start is called before the first frame update
    void Start()
    {
        _outMat = new Material(outerCircle.sharedMaterial);
        outerCircle.material = _outMat;

        _inMat = new Material(innerCircle.sharedMaterial);
        innerCircle.material = _inMat;

        foreach (Rune rune in runes)
        {
            rune.runePressed += UpdateRunes;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_drawing == false)
        {
            fill = Mathf.Max(fill - Time.deltaTime * 0.2f, 0f);
            ApplyFill();
        }
    }

    private void ApplyFill()
    {
        _outMat.SetFloat("_Fill", fill);
        _inMat.SetFloat("_Fill", fill);
    }

    private void UpdateRunes()
    {

    }

    private void OnMouseDown()
    {
        StartDrawingCircle();
    }

    private void OnMouseExit()
    {
        _drawing = false;
        fill = 0;
    }

    private void StartDrawingCircle()
    {
        if (_drawing == false)
        {
            _lastDir = MousePos - transform.position;
            _startAngle = Vector2.SignedAngle(_lastDir, Vector2.up);
            _outMat.SetFloat("_StartAngle", _startAngle);
            _drawing = true;
        }
    }

    private void OnMouseDrag()
    {
        DrawCircle();
    }

    private void DrawCircle()
    {
        if (_drawing == false)
            return;

        Vector2 direction = MousePos - transform.position;
        float deltaAngle = Vector2.SignedAngle(direction,_lastDir);
        if (deltaAngle > 0)
            fill += deltaAngle / 360f;//Mathf.Max(fill, (angle - _startAngle) / 360f);
        _lastDir = direction;
        if (fill >= 1)
        {
            fill = 1f;
            FinishCircle();
        }
        ApplyFill();
    }

    private void FinishCircle()
    {

    }

    private float CircleAngle()
    {
        Vector2 direction = MousePos - transform.position;
        return Vector2.SignedAngle(Vector2.up, direction);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_lastDir);

    }
}

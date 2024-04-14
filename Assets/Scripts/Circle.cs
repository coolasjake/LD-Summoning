using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public SpriteRenderer outerCircle;
    public SpriteRenderer innerCircle;

    public float flashDuration = 0.5f;

    public List<Rune> runes = new List<Rune>();

    private Material _outMat;
    private Material _inMat;

    [Range(0f, 1f)]
    public float fill = 1f;

    private float _startAngle = 0f;
    private bool _startedDrawing = false;
    private bool _doingAnimation = false;
    private bool _drawing = false;
    private float _opacity = 1f;

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
        if (_startedDrawing && _drawing == false && _doingAnimation == false)
        {
            _opacity = Mathf.Max(_opacity - Time.deltaTime, 0);
            if (_opacity == 0)
            {
                fill = 0;
                ApplyFill();
                _opacity = 1f;
                _startedDrawing = false;
            }
            _outMat.SetFloat("_Opacity", _opacity);
            _inMat.SetFloat("_Opacity", _opacity);
        }
    }

    private void ApplyFill()
    {
        _outMat.SetFloat("_Fill", fill);
        _inMat.SetFloat("_Fill", fill);
    }

    private void UpdateRunes()
    {
        string combo = "";
        foreach (Rune rune in runes)
        {
            combo += rune.isOn ? "1" : "0";
        }

        GameManager.SetRunes(combo);
    }

    private void OnMouseDown()
    {
        StartDrawingCircle();
    }

    private void OnMouseExit()
    {
        if (_drawing && !_doingAnimation)
            StopDrawing();
    }

    private void OnMouseDrag()
    {
        DrawCircle();
    }

    private void StartDrawingCircle()
    {
        if (_doingAnimation)
            return;

        if (_startedDrawing == false)
        {
            _lastDir = MousePos - transform.position;
            _startAngle = Vector2.SignedAngle(_lastDir, Vector2.up);
            _outMat.SetFloat("_StartAngle", _startAngle);
            _startedDrawing = true;

            foreach (Rune rune in runes)
                rune.ColliderOn = false;
        }

        _drawing = true;
        ResetOpacity();
    }

    private void DrawCircle()
    {
        if (_drawing == false || _doingAnimation)
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

    private void StopDrawing()
    {
        _drawing = false;
        foreach (Rune rune in runes)
            rune.ColliderOn = true;
    }

    private void FinishCircle()
    {
        if (_finishCircleCo != null)
            return;

        _startedDrawing = false;
        if (GameManager.CanSummon())
        {
            _finishCircleCo = StartCoroutine(SummonAnimation());
        }
        else
        {
            _finishCircleCo = StartCoroutine(SummonFailedAnimation());
        }
    }

    private Coroutine _finishCircleCo = null;
    private IEnumerator SummonAnimation()
    {
        _doingAnimation = true;

        if (flashDuration > 0)
        {
            float startTime = Time.time;
            _opacity = 1;
            while (Time.time < startTime + flashDuration)
            {
                _opacity += (Time.deltaTime / flashDuration) * 2f;
                _outMat.SetFloat("_Opacity", _opacity);
                _inMat.SetFloat("_Opacity", _opacity);

                yield return new WaitForEndOfFrame();
            }
        }
        ResetOpacity();

        fill = 0;
        ApplyFill();

        _doingAnimation = false;
        StopDrawing();

        GameManager.Summon();

        _finishCircleCo = null;
    }
    private IEnumerator SummonFailedAnimation()
    {
        _doingAnimation = true;

        if (flashDuration > 0)
        {
            float startTime = Time.time;
            _opacity = 1;
            while (Time.time < startTime + flashDuration)
            {
                _opacity -= Time.deltaTime / flashDuration;
                _outMat.SetFloat("_Opacity", _opacity);
                _inMat.SetFloat("_Opacity", _opacity);

                yield return new WaitForEndOfFrame();
            }
        }
        ResetOpacity();

        fill = 0;
        ApplyFill();

        _doingAnimation = false;
        StopDrawing();

        _finishCircleCo = null;
    }

    private void ResetOpacity()
    {
        _opacity = 1;
        _outMat.SetFloat("_Opacity", _opacity);
        _inMat.SetFloat("_Opacity", _opacity);
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

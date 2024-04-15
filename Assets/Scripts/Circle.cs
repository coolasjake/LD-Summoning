using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Circle : MonoBehaviour
{
    public SpriteRenderer outerCircle;
    public SpriteRenderer innerCircle;

    public float flashDuration = 0.5f;
    public float maxAnglePerSecond = 720f;

    public List<Rune> runes = new List<Rune>();

    public UnityEvent StartDrawing = new UnityEvent();
    public UnityEvent CancelDrawing = new UnityEvent();
    public UnityEvent SummonSucceed = new UnityEvent();
    public UnityEvent SummonFail = new UnityEvent();

    private Material _outMat;
    private Material _inMat;

    [Range(0f, 1f)]
    public float fill = 1f;

    private float _startAngle = 0f;
    private float _lastAngle = 0;
    private bool _startedDrawing = false;
    private bool _doingAnimation = false;
    private bool _playerDrawing = false;
    private bool _minionsDrawing = false;
    private float _opacity = 1f;
    private bool _initialized = false;

    private Vector2 _startDir = Vector2.zero;
    private Vector2 _lastDir = Vector2.zero;

    public static Vector3 MousePos => Camera.main.ScreenToWorldPoint(Input.mousePosition);

    private int _drawCost = 1;
    private int _numLoops = 1;
    private float _currentDrawValue = 0;

    public void SetupSummon(int drawCost, int numLoops)
    {
        Initialize();
        _drawCost = drawCost;
        _numLoops = numLoops;
        UpdateDrawValueAndAngle();
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        if (_initialized)
            return;

        _outMat = new Material(outerCircle.sharedMaterial);
        outerCircle.material = _outMat;

        _inMat = new Material(innerCircle.sharedMaterial);
        innerCircle.material = _inMat;

        foreach (Rune rune in runes)
        {
            rune.runePressed += UpdateRunes;
        }

        _initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_startedDrawing && _playerDrawing == false && _minionsDrawing == false && _doingAnimation == false)
        {
            _opacity = Mathf.Max(_opacity - Time.deltaTime, 0);
            if (_opacity == 0)
            {
                _currentDrawValue = 0;
                UpdateDrawValueAndAngle();
                _opacity = 1f;
                _startedDrawing = false;
            }
            _outMat.SetFloat("_Opacity", _opacity);
            _inMat.SetFloat("_Opacity", _opacity);
        }
    }

    private void UpdateDrawValueAndAngle()
    {
        if (_drawCost <= 0)
            _drawCost = 1;
        if (_numLoops <= 0)
            _numLoops = 1;

        fill = _currentDrawValue / ((float)_drawCost / _numLoops);
        _lastAngle = _startAngle + (fill * 360f) % 360f;

        _outMat.SetFloat("_Fill", fill);
        _inMat.SetFloat("_Fill", fill);

        if (_currentDrawValue >= _drawCost)
        {
            fill = _numLoops;
            FinishCircle();
        }
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
        StartDrawingPlayer();
    }

    private void OnMouseExit()
    {
        if (_playerDrawing && !_doingAnimation)
            PlayerStopDrawing();
    }

    private void OnMouseDrag()
    {
        PlayerDrawCircle();
    }

    private void StartDrawingPlayer()
    {
        if (_doingAnimation)
            return;

        foreach (Rune rune in runes)
            rune.ColliderOn = false;

        if (_startedDrawing == false)
        {
            _startDir = MousePos - transform.position;
            _lastDir = _startDir;
            _startAngle = Vector2.SignedAngle(_startDir, Vector2.up);
            _outMat.SetFloat("_StartAngle", _startAngle);

            StartDrawing.Invoke();
            ResetOpacity();

            _startedDrawing = true;
        }

        _playerDrawing = true;
        _lastDir = MousePos - transform.position;
    }

    private void StartDrawingMinions()
    {
        if (_doingAnimation)
            return;

        if (_startedDrawing == false)
        {
            _startDir = Vector2.up;
            _lastDir = _startDir;
            _startAngle = Vector2.SignedAngle(_startDir, Vector2.up);
            _outMat.SetFloat("_StartAngle", _startAngle);

            StartDrawing.Invoke();
            ResetOpacity();

            _startedDrawing = true;
        }
    }

    /// <summary> Call in fixed update with work value multiplied by fixed delta time. </summary>
    public void AutoDrawCircle(float workValue, bool costsMet)
    {
        _minionsDrawing = false;

        if (costsMet == false || _doingAnimation)
        {
            if (_startedDrawing && _playerDrawing)
                CancelDrawing.Invoke();
            return;
        }

        if (workValue > 0)
        {
            if (_startedDrawing == false)
                StartDrawingMinions();

            _minionsDrawing = true;
            AddToDrawing(workValue);
        }
    }

    private void PlayerDrawCircle()
    {
        if (_playerDrawing == false || _doingAnimation)
            return;

        Vector2 direction = MousePos - transform.position;
        float delta = Vector2.SignedAngle(direction,_lastDir);
        if (delta > 0 && delta < maxAnglePerSecond * Time.deltaTime)
        {
            _lastDir = direction;
            AddToDrawing((delta / 360f) * (_drawCost / _numLoops));
        }
    }

    private void AddToDrawing(float value)
    {
        _currentDrawValue += value;
        UpdateDrawValueAndAngle();
    }

    private void PlayerStopDrawing()
    {
        if (_minionsDrawing == false)
            CancelDrawing.Invoke();
        _playerDrawing = false;
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
            SummonSucceed.Invoke();
            _finishCircleCo = StartCoroutine(SummonAnimation());
        }
        else
        {
            SummonFail.Invoke();
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

        _currentDrawValue = 0;
        UpdateDrawValueAndAngle();

        _doingAnimation = false;
        PlayerStopDrawing();

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

        _currentDrawValue = 0;
        UpdateDrawValueAndAngle();

        _doingAnimation = false;
        PlayerStopDrawing();

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
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_startDir);
        Gizmos.DrawWireSphere(transform.position, _lastAngle);
    }
}

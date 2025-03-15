using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    public float colorChangeSpeed = 0.1f;
    
    private Material _material;
    private float _offset;
    private readonly Color[] _colors = {
        Color.blue,
        Color.red,
        Color.green,
        Color.magenta,
        Color.cyan
    };
    
    private int _currentColorIndex;
    private Color _currentColor;
    private Color _targetColor;
    private float _colorLerpTime;

    private void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _currentColor = _colors[0];
        _targetColor = _colors[1];
    }

    private void Update()
    {
        // Scroll background
        _offset += Time.deltaTime * scrollSpeed;
        _material.mainTextureOffset = new Vector2(0, _offset);

        // Change color
        _colorLerpTime += Time.deltaTime * colorChangeSpeed;
        if (_colorLerpTime >= 1f)
        {
            _colorLerpTime = 0f;
            _currentColor = _targetColor;
            _currentColorIndex = (_currentColorIndex + 1) % _colors.Length;
            _targetColor = _colors[_currentColorIndex];
        }

        _material.color = Color.Lerp(_currentColor, _targetColor, _colorLerpTime);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[System.Serializable]
public class GUIGrid
{
    public delegate void RepaintDelegate();

    protected const float CLIP_OFFSET = 4f;
    protected const float SCROLL_SENSITIVITY = 40f;
    protected const float FOCUS_EDGE_OFFSET_PRECENT = 0.1f;

    [SerializeField]
    protected int _selectedIndex;
    [SerializeField]
    protected int _elementCount;
    [SerializeField]
    protected float _scrollPosition;
    [SerializeField]
    protected bool _useBestFit;
    [SerializeField]
    protected Vector2 _elementSize;
    [SerializeField]
    protected Vector2 _viewSize;
    [SerializeField]
    private Rect _viewRect;
    [SerializeField]
    protected Vector2 _elementPadding;
    [SerializeField]
    protected Vector2 _contentPadding;

    private RepaintDelegate _onRepaint;
    private Vector2 _elementTrueSize;
    private AnimFloat _focusScrollTarget;
    private Point _size;
    protected float _bestFitSpace;


    public GUIGrid()
    {
        _contentPadding = new Vector2(15f, 15f);
        _elementPadding = new Vector2(10f, 10f);
        _focusScrollTarget = new AnimFloat(0f, Repaint);
        _focusScrollTarget.speed = 1f;
    }

    /// <summary>
    /// Gets or sets the index of the element that is selected.
    /// </summary>
    public int SelectedIndex
    {
        get { return _selectedIndex; }
        set { _selectedIndex = value; }
    }

    /// <summary>
    /// If true elements will resize to take up the full width of the screen instead
    /// of leaving space.
    /// </summary>
    public bool UseBestFit
    {
        get { return _useBestFit; }
        set { _useBestFit = value; }
    }

    /// <summary>
    /// Gives us a this editor the ability to call repaint on it's
    /// owner whenever a value changes that effects the layout.
    /// </summary>
    public event RepaintDelegate OnRepaint
    {
        add
        {
            _onRepaint += value;
        }
        remove
        {
            _onRepaint -= value;
        }
    }

    /// <summary>
    /// Returns back the size of the grid that is generated based
    /// on best fit. 
    /// </summary>
    public Point Size
    {
        get { return _size; }
    }

    /// <summary>
    /// Returns back the full size of the content view
    /// </summary>
    public Vector2 ViewSize
    {
        get { return _viewSize; }
    }

    /// <summary>
    /// Gets or sets the number of elements we should draw.
    /// </summary>
    public int ElementCount
    {
        get { return _elementCount; }
        set { _elementCount = value; }
    }

    /// <summary>
    /// Gets or sets the width of the elements
    /// </summary>
    public float ElementWidth
    {
        get { return _size.x; }
        set
        {
            if (value < 1f) value = 1f;
            _elementSize.x = value;
        }
    }

    /// <summary>
    /// Gets or sets the height of the elements
    /// </summary>
    public float ElementHeight
    {
        get { return _size.y; }
        set
        {
            if (value < 1f) value = 1f;
            _elementSize.y = value;
        }
    }

    /// <summary>
    /// Gets or sets the size of each element to be drawn. The minimum size
    /// is 1f.
    /// </summary>
    public Vector2 ElementSize
    {
        get { return _elementSize; }
        set
        {
            if (value.x < 1f) value.x = 1f;
            if (value.y < 1f) value.y = 1f;
            _elementSize = value;
        }
    }

    /// <summary>
    /// Returns back the final size of each element this includes the
    /// element size, padding, and best fit.
    /// </summary>
    public Vector2 ElementTrueSize
    {
        get
        {
            return _elementTrueSize;
        }
    }

    /// <summary>
    /// Invoked in OnGUI to draw the grid.
    /// </summary>
    /// <param name="screenRect">The rectangle that the grid should be contained within</param>
    public virtual void DoLayout(Rect screenRect)
    {
        GUI.BeginClip(screenRect);
        {
            Event current = Event.current;

            switch (current.type)
            {
                case EventType.scrollWheel:
                    _scrollPosition += current.delta.y * SCROLL_SENSITIVITY;
                    Repaint();
                    // They used the scroll wheel so we stop focusing.
                    StopFucus();
                    break;
                case EventType.KeyDown:
                    switch (current.keyCode)
                    {
                        case KeyCode.DownArrow:
                            if (_selectedIndex + _size.x < _elementCount)
                            {
                                _selectedIndex += _size.x;
                                Repaint();
                                FocusElement(_selectedIndex);
                            }
                            break;
                        case KeyCode.UpArrow:
                            if (_selectedIndex > _size.x)
                            {
                                _selectedIndex -= _size.x;
                                Repaint();
                                FocusElement(_selectedIndex);
                            }
                            break;
                        case KeyCode.LeftArrow:
                            if (_selectedIndex > 0)
                            {
                                _selectedIndex--;
                                Repaint();
                                FocusElement(_selectedIndex);
                            }
                            break;
                        case KeyCode.RightArrow:
                            if (_selectedIndex < _elementCount - 1)
                            {
                                _selectedIndex++;
                                Repaint();
                                FocusElement(_selectedIndex);
                            }
                            break;
                    }
                    break;
            }

            // If we are trying to focus we animate towards the target
            if (_focusScrollTarget.isAnimating)
            {
                _scrollPosition = _focusScrollTarget.value;
            }

            GUI.Box(_viewRect, GUIContent.none);

            // This values should only be kept during the repaint event.
            if (Event.current.type == EventType.Repaint)
            {
                _viewRect = screenRect;
                _viewRect.x = 0;
                _viewRect.y = 0;
                _bestFitSpace = 0f;
                _viewSize.x = _viewRect.width - (_contentPadding.x * 2f);
                _size.x = (int)(_viewSize.x / (_elementSize.x + _elementPadding.x));
                if (_useBestFit)
                {
                    _bestFitSpace = _viewSize.x - (_size.x * (_elementSize.x + _elementPadding.x));
                    _bestFitSpace /= _size.x;
                }
                _size.y = Mathf.CeilToInt((float)_elementCount / _size.x);
                _viewSize.y = (_elementTrueSize.y * _size.y) + (_contentPadding.y * 2f);
                _elementTrueSize.x = _elementSize.x + _elementPadding.x + _bestFitSpace;
                _elementTrueSize.y = _elementSize.y + _elementPadding.y + _bestFitSpace;
            }

            Rect scrollRect = _viewRect;
            scrollRect.width = 20;

            EditorGUI.BeginChangeCheck();
            {
                _scrollPosition = GUI.VerticalScrollbar(scrollRect, _scrollPosition, Mathf.Min(_viewRect.height, _viewSize.y), 0f, _viewSize.y);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // If the user uses the scroll bar we want to force stop focus
                StopFucus();
            }

            Rect elementRect = new Rect();
            // Clamp their width and height to our screen width.
            elementRect.width = Mathf.Clamp(_elementSize.x + _bestFitSpace, 0f, _viewSize.x);
            elementRect.height = Mathf.Clamp(_elementSize.y + _bestFitSpace, 0f, _viewSize.x);

            for (int y = 0; y < _size.y; y++)
            {
                elementRect.y = (y * _elementTrueSize.y) + _viewRect.y - _scrollPosition + _contentPadding.y;

                if (elementRect.y + elementRect.height < -CLIP_OFFSET)
                {
                    // We are outside the view. So here we can calculate how far we can skip ahead
                    float jumpAmount = _scrollPosition / (elementRect.height + _elementPadding.y);
                    y += Mathf.FloorToInt(jumpAmount - 1);
                    continue;
                }
                else if (elementRect.y + CLIP_OFFSET > screenRect.y + screenRect.height)
                {
                    // We are below our view so we can stop drawing.
                    break;
                }

                for (int x = 0; x < _size.x; x++)
                {
                    elementRect.x = (x * _elementTrueSize.x) + screenRect.x + _contentPadding.x + 10f;
                    int elementIndex = x + (y * _size.x);
                    // Check for input
                    if (elementRect.Contains(current.mousePosition))
                    {
                        switch (current.type)
                        {
                            case EventType.MouseDown:
                                _selectedIndex = elementIndex;
                                current.Use();
                                break;
                            case EventType.ContextClick:
                                OnElementContextClicked(elementIndex);
                                break;
                        }
                    }

                    if (elementIndex < _elementCount)
                    {
                        OnDrawElement(elementRect, elementIndex);
                    }
                }
            }
        }
        GUI.EndClip();

        // Handling Focusing off screen

    }

    /// <summary>
    /// If the scroll view is currently trying to focus an element
    /// this will be canceled.
    /// </summary>
    public void StopFucus()
    {
        _focusScrollTarget.value = _focusScrollTarget.target;
    }

    /// <summary>
    /// Makes sure the element at index is in view. If the index is out
    /// of range this function has no effect.
    /// </summary>
    public void FocusElement(int index)
    {
        if (index < 0 || index >= _elementCount)
        {
            // We are out of range
            return;
        }
        // Get the height of our selection
        float selectionHeight = _selectedIndex / _size.x;
        // Multiply our to get our height in pixels
        selectionHeight *= _elementTrueSize.y;
        // Check if we are out of view on the bottom
        if (selectionHeight < _scrollPosition)
        {
            _focusScrollTarget.value = _scrollPosition;
            _focusScrollTarget.target = selectionHeight - (_viewRect.height * FOCUS_EDGE_OFFSET_PRECENT);
        }
        // Check if we are out of view at the top
        else if (selectionHeight > _viewRect.height + _scrollPosition - _elementSize.y)
        {
            _focusScrollTarget.value = _scrollPosition;
            _focusScrollTarget.target = selectionHeight - _viewRect.height + _elementTrueSize.y + (_viewRect.height * FOCUS_EDGE_OFFSET_PRECENT);
        }
    }

    protected void Repaint()
    {
        if (_onRepaint != null)
        {
            _onRepaint();
        }
    }

    protected virtual void OnElementContextClicked(int index)
    {

    }

    protected virtual void OnDrawElement(Rect rect, int index)
    {

    }
}

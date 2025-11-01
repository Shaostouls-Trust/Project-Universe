using ProjectUniverse.PowerSystem;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionView : VisualElement
{
    private NodeView _startNode;
    private NodeView _endNode;
    private bool _isPreview;
    private Vector2 _previewEndPosition;
    private int _outputPortIndex;
    private int _inputPortIndex;
    private Color _connectionColor;
    private float _connectionThickness;

    public int GetOutputPortIndex() => _outputPortIndex;
    public int GetInputPortIndex() => _inputPortIndex;
    public NodeView GetStartNode() => _startNode;
    public NodeView GetEndNode() => _endNode;

    public ConnectionView(NodeView startNode, NodeView endNode, bool isPreview = false,
                         int outputPortIndex = 0, int inputPortIndex = 0,
                         Color? connectionColor = null, float connectionThickness = 2f)
    {
        _startNode = startNode;
        _endNode = endNode;
        _isPreview = isPreview;
        _outputPortIndex = outputPortIndex;
        _inputPortIndex = inputPortIndex;
        _connectionColor = connectionColor ?? Color.white;
        _connectionThickness = connectionThickness;

        AddToClassList("connection");
        if (_isPreview)
            AddToClassList("connection-preview");

        style.position = Position.Absolute;

        // Apply color and thickness
        style.backgroundColor = _connectionColor;
        style.height = _connectionThickness;

        RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
    }

    private void OnAttachedToPanel(AttachToPanelEvent evt)
    {
        UpdatePosition();
        UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
    }

    public void UpdateConnectionStyle(Color color, float thickness)
    {
        _connectionColor = color;
        _connectionThickness = thickness;
        style.backgroundColor = _connectionColor;
        style.height = _connectionThickness;
    }

    public void UpdatePosition()
    {
        if (_startNode == null || parent == null) return;

        // Use the stored port indices
        Vector2 startPos = _startNode.GetOutputPortPosition(_outputPortIndex);
        Vector2 endPos;

        if (_isPreview)
        {
            endPos = _previewEndPosition;
        }
        else if (_endNode != null)
        {
            endPos = _endNode.GetInputPortPosition(_inputPortIndex);
        }
        else
        {
            return;
        }

        // Convert world positions to local space
        startPos = parent.WorldToLocal(startPos);
        if (!_isPreview && _endNode != null)
            endPos = parent.WorldToLocal(endPos);

        // Calculate line angle and length
        float length = Vector2.Distance(startPos, endPos);

        // Avoid zero-length lines which cause rotation issues
        if (float.IsNaN(length) || length < 0.001f)
        {
            style.display = DisplayStyle.None;
            return;
        }
        else
        {
            style.display = DisplayStyle.Flex;
        }

        float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;

        // Position and rotate the line
        style.left = startPos.x;
        style.top = startPos.y - (_connectionThickness / 2f); // Center the line vertically
        style.width = length;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdatePreviewEndPosition(Vector2 position)
    {
        _previewEndPosition = position;
        UpdatePosition();
    }

}
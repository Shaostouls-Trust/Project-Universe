using UnityEngine;
using UnityEngine.UIElements;

public class NetworkGraphController : MonoBehaviour
{
    public NetworkGraph Graph;
    public UIDocument UIDocument;
    public NetworkGraphView _graphView;

    private void Awake()
    {
        if (Graph == null)
        {
            Debug.LogError("A default NetworkGraph was created.");
            Graph = ScriptableObject.CreateInstance<NetworkGraph>();
        }
    }

    private void OnEnable()
    {
        //_graphView = gameObject.AddComponent<NetworkGraphView>();
        _graphView.Graph = Graph;
    }
}
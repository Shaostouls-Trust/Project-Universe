using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Network Graph", menuName = "Electrical System/Network Graph")]
public class NetworkGraph : ScriptableObject
{
    public List<NetworkNode> Nodes = new List<NetworkNode>();
    public List<SerializableConnection> Connections = new List<SerializableConnection>();

    private void OnEnable()
    {
        // Restore connections when the ScriptableObject is loaded
        RestoreConnections();
    }

    public NetworkNode CreateNode(string name, Vector2 position)
    {
        var node = new NetworkNode
        {
            Name = name,
            Position = position
        };

        Nodes.Add(node);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        return node;
    }

    public void Connect(NetworkNode outputNode, NetworkNode inputNode, int outputPortIndex = 0, int inputPortIndex = 0)
    {
        if (outputNode == null || inputNode == null) return;

        // Remove any existing connection at these specific ports
        RemoveConnection(outputNode.Id, outputPortIndex, inputNode.Id, inputPortIndex);

        // Add the new connection
        var connection = new SerializableConnection(outputNode.Id, inputNode.Id, outputPortIndex, inputPortIndex);

        Connections.Add(connection);

        // Update the runtime connection lists
        EnsureConnectionListSize(outputNode.OutputConnections, outputPortIndex + 1);
        EnsureConnectionListSize(inputNode.InputConnections, inputPortIndex + 1);
        EnsureConnectionListSize(outputNode.OutputConnectionIds, outputPortIndex + 1);
        EnsureConnectionListSize(inputNode.InputConnectionIds, inputPortIndex + 1);
        EnsureConnectionListSize(outputNode.OutputConnectionPortIndices, outputPortIndex + 1);
        EnsureConnectionListSize(inputNode.InputConnectionPortIndices, inputPortIndex + 1);

        outputNode.OutputConnections[outputPortIndex] = inputNode;
        inputNode.InputConnections[inputPortIndex] = outputNode;
        outputNode.OutputConnectionIds[outputPortIndex] = inputNode.Id;
        inputNode.InputConnectionIds[inputPortIndex] = outputNode.Id;
        outputNode.OutputConnectionPortIndices[outputPortIndex] = inputPortIndex;
        inputNode.InputConnectionPortIndices[inputPortIndex] = outputPortIndex;

        // Capture PowerNode states
        outputNode.CapturePowerNodeState();
        inputNode.CapturePowerNodeState();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void EnsureConnectionListSize<T>(List<T> list, int requiredSize)
    {
        while (list.Count < requiredSize)
        {
            list.Add(default(T));
        }
    }

    public void RemoveConnection(string outputNodeId, int outputPortIndex, string inputNodeId, int inputPortIndex)
    {
        Connections.RemoveAll(c =>
            c.outputNodeId == outputNodeId && c.outputPortIndex == outputPortIndex &&
            c.inputNodeId == inputNodeId && c.inputPortIndex == inputPortIndex);

        // Update runtime connections
        var outputNode = Nodes.FirstOrDefault(n => n.Id == outputNodeId);
        var inputNode = Nodes.FirstOrDefault(n => n.Id == inputNodeId);

        if (outputNode != null && outputPortIndex < outputNode.OutputConnections.Count)
        {
            outputNode.OutputConnections[outputPortIndex] = null;
            if (outputPortIndex < outputNode.OutputConnectionIds.Count)
                outputNode.OutputConnectionIds[outputPortIndex] = null;
        }

        if (inputNode != null && inputPortIndex < inputNode.InputConnections.Count)
        {
            inputNode.InputConnections[inputPortIndex] = null;
            if (inputPortIndex < inputNode.InputConnectionIds.Count)
                inputNode.InputConnectionIds[inputPortIndex] = null;
        }
    }

    public void RestoreConnections()
    {
        // First, restore all GameObject references
        foreach (var node in Nodes)
        {
            node.RestoreGameObjectReferences();
        }

        // Clear runtime connections
        foreach (var node in Nodes)
        {
            node.InputConnections.Clear();
            node.OutputConnections.Clear();
        }

        // Rebuild runtime connections from serialized data
        foreach (var connection in Connections)
        {
            var outputNode = Nodes.FirstOrDefault(n => n.Id == connection.outputNodeId);
            var inputNode = Nodes.FirstOrDefault(n => n.Id == connection.inputNodeId);

            if (outputNode != null && inputNode != null)
            {
                EnsureConnectionListSize(outputNode.OutputConnections, connection.outputPortIndex + 1);
                EnsureConnectionListSize(inputNode.InputConnections, connection.inputPortIndex + 1);

                outputNode.OutputConnections[connection.outputPortIndex] = inputNode;
                inputNode.InputConnections[connection.inputPortIndex] = outputNode;
            }
        }

        // Restore PowerNode states
        foreach (var node in Nodes)
        {
            node.RestorePowerNodeState();
        }

        // Refresh room network data
        foreach (var node in Nodes)
        {
            if (node.RoomNetwork != null)
            {
                node.RefreshRoomNetworkData();
            }
        }
    }

    public void Disconnect(NetworkNode node)
    {
        if (node == null) return;

        // Remove all connections involving this node
        Connections.RemoveAll(c => c.outputNodeId == node.Id || c.inputNodeId == node.Id);

        // Remove from runtime connections
        foreach (var otherNode in Nodes)
        {
            otherNode.InputConnections.Remove(node);
            otherNode.OutputConnections.Remove(node);
        }

        node.InputConnections.Clear();
        node.OutputConnections.Clear();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void DeleteNode(NetworkNode node)
    {
        if (node == null) return;

        Disconnect(node);
        Nodes.Remove(node);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void SaveAllNodeStates()
    {
        foreach (var node in Nodes)
        {
            node.CapturePowerNodeState();
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
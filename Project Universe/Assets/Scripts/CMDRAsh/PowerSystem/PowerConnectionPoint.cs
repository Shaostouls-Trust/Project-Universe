using UnityEngine;

namespace ProjectUniverse.PowerSystem
{

    [System.Serializable]
    public class PowerConnectionPoint
    {
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localDirection = Vector3.forward;
        public float connectionRadius = 2f;
        public string name = "Connection";
        public ConnectionType connectionType = ConnectionType.Both;

        public enum ConnectionType
        {
            Input,
            Output,
            Both
        }

        [HideInInspector]
        public Component ownerComponent;

        [HideInInspector]
        public bool isConnected = false;

        public PowerConnectionPoint()
        {
            name = "Connection";
            localPosition = Vector3.zero;
            localDirection = Vector3.forward;
            connectionRadius = 2f;
            connectionType = ConnectionType.Both;
        }

        public PowerConnectionPoint(string name, Vector3 position, ConnectionType type)
        {
            this.name = name;
            this.localPosition = position;
            this.connectionType = type;
            this.localDirection = Vector3.forward;
            this.connectionRadius = 2f;
        }

        public Vector3 GetWorldPosition()
        {
            if (ownerComponent == null) return Vector3.zero;
            return ownerComponent.transform.TransformPoint(localPosition);
        }

        public Vector3 GetWorldDirection()
        {
            if (ownerComponent == null) return Vector3.forward;
            return ownerComponent.transform.TransformDirection(localDirection);
        }

        public bool IsNearPosition(Vector3 worldPos)
        {
            return Vector3.Distance(GetWorldPosition(), worldPos) <= connectionRadius;
        }

        public bool CanConnectFrom()
        {
            return connectionType == ConnectionType.Output || connectionType == ConnectionType.Both;
        }

        public bool CanConnectTo()
        {
            return connectionType == ConnectionType.Input || connectionType == ConnectionType.Both;
        }
    }
}
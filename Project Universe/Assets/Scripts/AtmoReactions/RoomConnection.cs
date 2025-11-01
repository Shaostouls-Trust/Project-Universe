using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Environment.Gas;
using ProjectUniverse.Environment.Volumes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectUniverse.Environment.Hazards
{
    /// <summary>
    /// Represents a connection between two rooms for mobile ignition source pathfinding
    /// </summary>
    [System.Serializable]
    public class RoomConnection
    {
        public enum ConnectionType
        {
            Door,
            PipeVent,
            Breach
        }

        public ConnectionType Type { get; private set; }
        public VolumeAtmosphereController TargetRoom { get; private set; }
        public Vector3 ConnectionPoint { get; private set; }
        public float BaseWeight { get; set; }
        public float CurrentWeight { get; set; }
        public bool IsPassable { get; private set; }
        public DoorAnimator AssociatedDoor { get; private set; }
        public PipeSection AssociatedPipeSection { get; private set; }
        public int QueuedSourceCount { get; private set; }

        // For doors
        public RoomConnection(DoorAnimator door, VolumeAtmosphereController targetRoom, Vector3 point)
        {
            Type = ConnectionType.Door;
            AssociatedDoor = door;
            TargetRoom = targetRoom;
            ConnectionPoint = point;
            BaseWeight = 0.4f;
            UpdatePassability();
        }

        // For pipe vents
        public RoomConnection(PipeSection pipeSection, VolumeAtmosphereController targetRoom, Vector3 point)
        {
            Type = ConnectionType.PipeVent;
            AssociatedPipeSection = pipeSection;
            TargetRoom = targetRoom;
            ConnectionPoint = point;
            BaseWeight = 0.3f;
            IsPassable = true; // Vents are always passable if they exist
        }

        // For breaches
        public RoomConnection(VolumeAtmosphereController targetRoom, Vector3 point)
        {
            Type = ConnectionType.Breach;
            TargetRoom = targetRoom;
            ConnectionPoint = point;
            BaseWeight = 0.35f;
            IsPassable = true;
        }

        public void UpdatePassability()
        {
            if (Type == ConnectionType.Door && AssociatedDoor != null)
            {
                IsPassable = AssociatedDoor.OpenOrOpening() || AssociatedDoor.IsRuptured;
                // Closed doors still allow some seepage
                if (!IsPassable)
                {
                    BaseWeight = 0.1f; // Seepage weight
                }
                else
                {
                    BaseWeight = 0.4f; // Open door weight
                }
            }
        }

        public void CalculateWeight(float pressureDifferential)
        {
            CurrentWeight = BaseWeight;

            // Apply pressure modifier with tanh to prevent extreme values
            float pressureModifier = 1.0f + 0.3f * (float) Math.Tanh(pressureDifferential);
            CurrentWeight *= pressureModifier;

            // Apply queue penalty if sources are waiting
            if (QueuedSourceCount > 0)
            {
                CurrentWeight *= Mathf.Max(0.3f, 1.0f - (QueuedSourceCount * 0.1f));
            }
        }

        public void AddToQueue()
        {
            QueuedSourceCount++;
        }

        public void RemoveFromQueue()
        {
            QueuedSourceCount = Mathf.Max(0, QueuedSourceCount - 1);
        }
    }
}
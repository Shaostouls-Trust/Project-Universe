using UnityEngine;
using System.Collections.Generic;

namespace ProjectUniverse.PowerSystem
{
    [System.Serializable]
    public class CableAssignment
    {
        public string pathId;
        public CableSize cableSize;

        public CableAssignment(string pathId, CableSize cableSize)
        {
            this.pathId = pathId;
            this.cableSize = cableSize;
        }
    }

    [CreateAssetMenu(fileName = "CableAssignmentData", menuName = "Electrical System/Cable Assignment Data")]
    public class CableAssignmentData : ScriptableObject
    {
        [SerializeField]
        private List<CableAssignment> assignments = new();

        private Dictionary<string, CableSize> assignmentCache;

        private void OnEnable()
        {
            RebuildCache();
        }

        private void RebuildCache()
        {
            assignmentCache = new Dictionary<string, CableSize>();
            foreach (var assignment in assignments)
            {
                assignmentCache[assignment.pathId] = assignment.cableSize;
            }
        }

        public void SetAssignment(string pathId, CableSize cableSize)
        {
            // Remove existing assignment if any
            assignments.RemoveAll(a => a.pathId == pathId);

            // Add new assignment
            assignments.Add(new CableAssignment(pathId, cableSize));

            // Update cache
            assignmentCache[pathId] = cableSize;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveAssignment(string pathId)
        {
            assignments.RemoveAll(a => a.pathId == pathId);

            assignmentCache?.Remove(pathId);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public bool TryGetAssignment(string pathId, out CableSize cableSize)
        {
            if (assignmentCache == null)
                RebuildCache();

            return assignmentCache.TryGetValue(pathId, out cableSize);
        }

        public void ClearAllAssignments()
        {
            assignments.Clear();
            assignmentCache?.Clear();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public List<CableAssignment> GetAllAssignments()
        {
            return new List<CableAssignment>(assignments);
        }
    }
}
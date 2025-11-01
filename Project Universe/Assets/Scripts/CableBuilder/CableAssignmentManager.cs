using UnityEngine;

namespace ProjectUniverse.PowerSystem
{
    public class CableAssignmentManager : MonoBehaviour
    {
        private static CableAssignmentManager instance;
        public static CableAssignmentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CableAssignmentManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("CableAssignmentManager");
                        instance = go.AddComponent<CableAssignmentManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private CableAssignmentData assignmentData;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Try to load the assignment data if not set
            if (assignmentData == null)
            {
                assignmentData = Resources.Load<CableAssignmentData>("CableAssignmentData");

#if UNITY_EDITOR
                // Create one if it doesn't exist
                if (assignmentData == null)
                {
                    assignmentData = ScriptableObject.CreateInstance<CableAssignmentData>();

                    // Create Resources folder if it doesn't exist
                    if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    UnityEditor.AssetDatabase.CreateAsset(assignmentData, "Assets/Resources/CableAssignmentData.asset");
                    UnityEditor.AssetDatabase.SaveAssets();
                }
#endif
            }
        }

        public void AssignCableSize(string pathId, CableSize size)
        {
            if (assignmentData != null)
                assignmentData.SetAssignment(pathId, size);
        }

        public void UnassignCableSize(string pathId)
        {
            if (assignmentData != null)
                assignmentData.RemoveAssignment(pathId);
        }

        public bool TryGetAssignedCableSize(string pathId, out CableSize size)
        {
            if (assignmentData != null)
                return assignmentData.TryGetAssignment(pathId, out size);

            size = default;
            return false;
        }
    }
}
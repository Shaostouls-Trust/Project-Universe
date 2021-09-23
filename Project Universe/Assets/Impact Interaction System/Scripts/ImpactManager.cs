using Impact.Interactions;
using Impact.Materials;
using Impact.Objects;
using Impact.Utility.ObjectPool;
using System.Collections.Generic;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Singleton class for managing Impact configuration, material mapping, and handling interaction processing.
    /// </summary>
    [AddComponentMenu("Impact/Impact Manager")]
    public class ImpactManager : MonoBehaviour
    {
        [SerializeField]
        private int _physicsInteractionsLimit = 0;
        [SerializeField]
        private int _interactionResultBufferSize = 8;
        [SerializeField]
        private int _activeContinuousInteractions = 16;
        [SerializeField]
        private int _materialCompositionBufferSize = 8;

        [SerializeField]
        private bool _usePhysicMaterialMapping = false;

        [SerializeField]
        private bool _dontDestroyOnLoad = true;
        [SerializeField]
        private bool _setAsActiveInstance = true;

        [SerializeField]
        private List<ImpactPhysicMaterialMapping> _physicMaterialMap = new List<ImpactPhysicMaterialMapping>();
        [SerializeField]
        private List<ImpactPhysicsMaterial2DMapping> _physicsMaterial2DMap = new List<ImpactPhysicsMaterial2DMapping>();

        //Combined dictonary of both 3D and 2D material maps
        private Dictionary<int, IImpactMaterial> materialMapDictionary;
        private Dictionary<string, ClassPool> interactionResultPools;

        private int physicsInteractionsCounter;
        private IContinuousInteractionResult[] activeContinuousInteractionResults;
        private HashSet<long> activeContinuousInteractionKeys;
        private int currentActiveContinuousInteractionCount = 0;

        /// <summary>
        /// Shared buffer for retrieving interaction results from Impact Materials.
        /// </summary>
        public IInteractionResult[] InteractionResultBuffer { get; private set; }

        /// <summary>
        /// Shared buffer for retrieving material composition.
        /// </summary>
        public ImpactMaterialComposition[] MaterialCompositionBuffer { get; private set; }

        /// <summary>
        /// Is the material mapping feature enabled in the configuration?
        /// </summary>
        public bool UseMaterialMapping
        {
            get { return _usePhysicMaterialMapping; }
            set { _usePhysicMaterialMapping = value; }
        }

        private void Awake()
        {
            if (_setAsActiveInstance)
                ImpactManagerInstance.SetInstance(this);

            Initialize();
        }

        private void OnDestroy()
        {
            //Dispose of any active continuous interactions
            for (int i = 0; i < activeContinuousInteractionResults.Length; i++)
            {
                IContinuousInteractionResult c = activeContinuousInteractionResults[i];

                if (c != null)
                    c.Dispose();
            }

            //Clear the instance
            //This won't do anything if the active instance is not this
            ImpactManagerInstance.ClearInstance(this);
        }

        private void FixedUpdate()
        {
            physicsInteractionsCounter = 0;

            for (int i = 0; i < activeContinuousInteractionResults.Length; i++)
            {
                IContinuousInteractionResult c = activeContinuousInteractionResults[i];

                if (c == null)
                    continue;

                if (!c.IsAlive)
                {
                    c.Dispose();
                    activeContinuousInteractionResults[i] = null;
                    activeContinuousInteractionKeys.Remove(c.Key);
                    currentActiveContinuousInteractionCount = Mathf.Clamp(currentActiveContinuousInteractionCount - 1, 0, _activeContinuousInteractions);
                }
                else
                    c.FixedUpdate();
            }
        }

        /// <summary>
        /// Set up all needed manager data including the buffer arrays and material mapping data.
        /// </summary>
        public void Initialize()
        {
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            InteractionResultBuffer = new IInteractionResult[_interactionResultBufferSize];
            MaterialCompositionBuffer = new ImpactMaterialComposition[_materialCompositionBufferSize];
            activeContinuousInteractionResults = new IContinuousInteractionResult[_activeContinuousInteractions];

            materialMapDictionary = new Dictionary<int, IImpactMaterial>();

            for (int i = 0; i < _physicMaterialMap.Count; i++)
            {
                if (_physicMaterialMap[i].PhysicMaterial != null)
                {
                    int id = _physicMaterialMap[i].PhysicMaterial.GetInstanceID();

                    if (!materialMapDictionary.ContainsKey(id))
                        materialMapDictionary.Add(id, _physicMaterialMap[i].ImpactMaterial);
                }

            }

            for (int i = 0; i < _physicsMaterial2DMap.Count; i++)
            {
                if (_physicsMaterial2DMap[i].PhysicsMaterial2D != null)
                {
                    int id = _physicsMaterial2DMap[i].PhysicsMaterial2D.GetInstanceID();

                    if (!materialMapDictionary.ContainsKey(id))
                        materialMapDictionary.Add(id, _physicsMaterial2DMap[i].ImpactMaterial);
                }
            }

            activeContinuousInteractionKeys = new HashSet<long>();
            interactionResultPools = new Dictionary<string, ClassPool>();
        }

        /// <summary>
        /// Checks if the current physics interaction count has been reached.
        /// </summary>
        /// <returns>True if the limit has been reached, false otherwise.</returns>
        public bool HasReachedPhysicsInteractionsLimit()
        {
            if (_physicsInteractionsLimit <= 0)
                return false;

            return physicsInteractionsCounter > _physicsInteractionsLimit;
        }

        /// <summary>
        /// Increments the physics interaction count.
        /// </summary>
        public void IncrementPhysicsInteractionsLimit()
        {
            physicsInteractionsCounter++;
        }

        /// <summary>
        /// Has the maximum number of active continuous interactions been reached?
        /// </summary>
        /// <returns>True if the limit has been reached, False otherwise.</returns>
        public bool HasReachedContinuousInteractionLimit()
        {
            return currentActiveContinuousInteractionCount >= _activeContinuousInteractions;
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Collider.
        /// </summary>
        /// <param name="collider">The collider that has the Physic Material to get a mapping for.</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public bool TryGetImpactMaterialFromMapping(Collider collider, out IImpactMaterial impactMaterial)
        {
            if (!_usePhysicMaterialMapping || collider.sharedMaterial == null)
            {
                impactMaterial = null;
                return false;
            }

            return materialMapDictionary.TryGetValue(collider.sharedMaterial.GetInstanceID(), out impactMaterial);
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Collider2D.
        /// </summary>
        /// <param name="collider2d">The collider that has the Physics Material 2D to get a mapping for.</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public bool TryGetImpactMaterialFromMapping(Collider2D collider2d, out IImpactMaterial impactMaterial)
        {
            if (!_usePhysicMaterialMapping || collider2d.sharedMaterial == null)
            {
                impactMaterial = null;
                return false;
            }

            return materialMapDictionary.TryGetValue(collider2d.sharedMaterial.GetInstanceID(), out impactMaterial);
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Physics Material instance ID.
        /// </summary>
        /// <param name="physicsMaterialInstanceId">The instance ID of the physics material (3D or 2D).</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public bool TryGetImpactMaterialFromMapping(int physicsMaterialInstanceId, out IImpactMaterial impactMaterial)
        {
            if (!_usePhysicMaterialMapping)
            {
                impactMaterial = null;
                return false;
            }

            return materialMapDictionary.TryGetValue(physicsMaterialInstanceId, out impactMaterial);
        }

        /// <summary>
        /// Process interaction data using the Impact Material and an optional Impact Object that the interaction originated from.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactMaterial">The Impact Material to get interaction results from.</param>
        /// <param name="impactObject">An optional Impact Object that the interaction originated from.</param>
        public void ProcessInteraction<T>(T interactionData, IImpactMaterial impactMaterial, IImpactObject impactObject) where T : IInteractionData
        {
            int count = impactMaterial.GetInteractionResultsNonAlloc(interactionData, InteractionResultBuffer);
            for (int i = 0; i < count; i++)
            {
                InteractionResultBuffer[i].Process(impactObject);
            }
        }

        /// <summary>
        /// Process a continuous interaction using the interaction data and the given Impact Object. The primary material at the interaction point will be used.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactObject">The Impact Object that an Impact Material will be retrieved from.</param>
        public void ProcessContinuousInteraction<T>(T interactionData, IImpactObject impactObject) where T : IInteractionData
        {
            ProcessContinuousInteraction(interactionData, impactObject.GetPrimaryMaterial(interactionData.Point), impactObject);
        }

        /// <summary>
        /// Process a continuous interaction using the interaction data, an Impact Material, and an optional Impact Object that the interaction originated from.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactMaterial">The Impact Material to get interaction results from.</param>
        /// <param name="impactObject">An optional Impact Object that the interaction originated from.</param>
        public void ProcessContinuousInteraction<T>(T interactionData, IImpactMaterial material, IImpactObject impactObject) where T : IInteractionData
        {
            int resultCount = material.GetInteractionResultsNonAlloc(interactionData, InteractionResultBuffer);

            for (int i = 0; i < resultCount; i++)
            {
                IContinuousInteractionResult result = InteractionResultBuffer[i] as IContinuousInteractionResult;

                //result is not a continuous interaction, so simply Process it.
                if (result == null)
                    InteractionResultBuffer[i].Process(impactObject);
                //Otherwise update an existing continuous interaction or add a new one.
                else
                    AddOrUpdateContinuousInteractionResult(impactObject, result);
            }
        }

        /// <summary>
        /// Adds or updates the given continuous interaction result to the active continuous interaction results, if able.
        /// </summary>
        /// <param name="impactObject">The impact object the result is being sent from.</param>
        /// <param name="result">The new continuous interaction result.</param>
        public void AddOrUpdateContinuousInteractionResult(IImpactObject impactObject, IContinuousInteractionResult result)
        {
            IContinuousInteractionResult existing = findContinuousInteractionResult(result.Key);

            if (existing != null)
            {
                existing.KeepAlive(result);
                result.Dispose();
            }
            else
                addContinuousInteractionResult(impactObject, result);
        }

        public bool HasActiveContinuousInteractionWithKey(long key)
        {
            return activeContinuousInteractionKeys.Contains(key);
        }

        private IContinuousInteractionResult findContinuousInteractionResult(long key)
        {
            for (int i = 0; i < activeContinuousInteractionResults.Length; i++)
            {
                if (activeContinuousInteractionResults[i] != null && activeContinuousInteractionResults[i].Key.Equals(key))
                    return activeContinuousInteractionResults[i];
            }

            return null;
        }

        private void addContinuousInteractionResult(IImpactObject impactObject, IContinuousInteractionResult continuousInteractionResult)
        {
            for (int i = 0; i < activeContinuousInteractionResults.Length; i++)
            {
                if (activeContinuousInteractionResults[i] == null)
                {
                    activeContinuousInteractionResults[i] = continuousInteractionResult;
                    currentActiveContinuousInteractionCount++;
                    activeContinuousInteractionKeys.Add(continuousInteractionResult.Key);
                    continuousInteractionResult.Process(impactObject);
                    return;
                }
            }

            continuousInteractionResult.Dispose();
        }

        /// <summary>
        /// Creates a new Interaction Result pool with the given key and size.
        /// </summary>
        /// <typeparam name="T">The IPoolable type to use.</typeparam>
        /// <param name="key">The name of the pool, used to retrieve objects.</param>
        /// <returns>The newly created pool, or the existing pool if there is one with the same key.</returns>
        public ClassPool CreateInteractionResultPool<T>(string key) where T : class, IPoolable, new()
        {
            if (interactionResultPools.ContainsKey(key))
                return interactionResultPools[key];

            ClassPool pool = new ClassPool();
            interactionResultPools.Add(key, pool);

            return pool;
        }

        /// <summary>
        /// Attempts to get an available empty interaction result from the pool with the specified key.
        /// </summary>
        /// <typeparam name="T">The IPoolable type to get.</typeparam>
        /// <param name="key">The name of the pool.</param>
        /// <param name="obj">The returned result, if one was found.</param>
        /// <returns>True if an available result was found. False otherwise.</returns>
        public bool TryGetInteractionResultFromPool<T>(string key, out T obj) where T : class, IPoolable, new()
        {
            if (!interactionResultPools.ContainsKey(key))
                CreateInteractionResultPool<T>(key);

            ClassPool pool;

            if (interactionResultPools.TryGetValue(key, out pool))
            {
                obj = pool.GetObject<T>();
                return true;
            }

            obj = default;
            return false;
        }
    }
}


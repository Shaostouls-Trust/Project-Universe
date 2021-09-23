using System;
using Impact.Interactions;
using Impact.Materials;
using Impact.Objects;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact
{
    /// <summary>
    /// Static class that handles the reference for the currently active Impact Manager instance.
    /// </summary>
    public static class ImpactManagerInstance
    {
        private static ImpactManager instance;
        private static bool hasInstance;

        /// <summary>
        /// Shared buffer for retrieving interaction results from Impact Materials.
        /// </summary>
        public static IInteractionResult[] InteractionResultBuffer
        {
            get
            {
                ImpactManager instance = GetInstance();
                return instance.InteractionResultBuffer;
            }
        }

        /// <summary>
        /// Shared buffer for retrieving material composition.
        /// </summary>
        public static ImpactMaterialComposition[] MaterialCompositionBuffer
        {
            get
            {
                ImpactManager instance = GetInstance();
                return instance.MaterialCompositionBuffer;
            }
        }

        /// <summary>
        /// Sets the ImpactManager instance that all static methods will use.
        /// </summary>
        /// <param name="inst">The new instance.</param>
        public static void SetInstance(ImpactManager inst)
        {
            instance = inst;
            hasInstance = instance != null;
        }

        /// <summary>
        /// Gets the current ImpactManager instance.
        /// </summary>
        /// <param name="createIfNull">Should an instance be created if one does not exist?</param>
        /// <returns>The current ImpactManager instance, or null if one does not exist and createIfNull is false.</returns>
        public static ImpactManager GetInstance(bool createIfNull = true)
        {
            if (createIfNull && !hasInstance)
            {
                GameObject g = new GameObject("Impact Manager");
                instance = g.AddComponent<ImpactManager>();
            }

            return instance;
        }

        /// <summary>
        /// Clears the current ImpactManager instance, if it is the same as the provided instance.
        /// </summary>
        /// <param name="inst">The instance to clear.</param>
        public static void ClearInstance(ImpactManager inst)
        {
            if (instance == inst)
            {
                instance = null;
                hasInstance = false;
            }
        }

        /// <summary>
        /// Is the material mapping feature enabled in the configuration?
        /// </summary>
        public static bool UseMaterialMapping
        {
            get
            {
                ImpactManager instance = GetInstance();
                return instance.UseMaterialMapping;
            }
            set
            {
                ImpactManager instance = GetInstance();
                instance.UseMaterialMapping = value;
            }
        }

        /// <summary>
        /// Checks if the current physics interaction count has been reached.
        /// </summary>
        /// <returns>True if the limit has been reached, false otherwise.</returns>
        public static bool HasReachedPhysicsInteractionsLimit()
        {
            ImpactManager instance = GetInstance();
            return instance.HasReachedPhysicsInteractionsLimit();
        }

        /// <summary>
        /// Increments the physics interaction count.
        /// </summary>
        public static void IncrementPhysicsInteractionsLimit()
        {
            ImpactManager instance = GetInstance();
            instance.IncrementPhysicsInteractionsLimit();
        }

        /// <summary>
        /// Has the maximum number of active continuous interactions been reached?
        /// </summary>
        /// <returns>True if the limit has been reached, False otherwise.</returns>
        public static bool HasReachedContinuousInteractionLimit()
        {
            ImpactManager instance = GetInstance();
            return instance.HasReachedContinuousInteractionLimit();
        }

        public static bool HasActiveContinuousInteractionWithKey(long key)
        {
            ImpactManager instance = GetInstance();
            return instance.HasActiveContinuousInteractionWithKey(key);
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Collider.
        /// </summary>
        /// <param name="collider">The collider that has the Physic Material to get a mapping for.</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public static bool TryGetImpactMaterialFromMapping(Collider collider, out IImpactMaterial impactMaterial)
        {
            ImpactManager instance = GetInstance();
            return instance.TryGetImpactMaterialFromMapping(collider, out impactMaterial);
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Collider2D.
        /// </summary>
        /// <param name="collider2d">The collider that has the Physics Material 2D to get a mapping for.</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public static bool TryGetImpactMaterialFromMapping(Collider2D collider2d, out IImpactMaterial impactMaterial)
        {
            ImpactManager instance = GetInstance();
            return instance.TryGetImpactMaterialFromMapping(collider2d, out impactMaterial);
        }

        /// <summary>
        /// Try to get an Impact Material from the Material Mapping using the given Physics Material instance ID.
        /// </summary>
        /// <param name="physicsMaterialInstanceId">The instance ID of the physics material (3D or 2D).</param>
        /// <param name="impactMaterial">The material that was found in the mapping, if one was found. Null otherwise.</param>
        /// <returns>True if a matching map was found, False otherwise.</returns>
        public static bool TryGetImpactMaterialFromMapping(int physicsMaterialInstanceId, out IImpactMaterial impactMaterial)
        {
            ImpactManager instance = GetInstance();
            return instance.TryGetImpactMaterialFromMapping(physicsMaterialInstanceId, out impactMaterial);
        }

        /// <summary>
        /// Process interaction data using the given Impact Object. The primary material at the interaction point will be used.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactObject">The Impact Object that the primary Impact Material will be retrieved from.</param>
        public static void ProcessInteraction<T>(T interactionData, IImpactObject impactObject) where T : IInteractionData
        {
            if (impactObject == null)
                return;

            ProcessInteraction(interactionData, impactObject.GetPrimaryMaterial(interactionData.Point), impactObject);
        }

        /// <summary>
        /// Process interaction data using the Impact Material and an optional Impact Object that the interaction originated from.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactMaterial">The Impact Material to get interaction results from.</param>
        /// <param name="impactObject">An optional Impact Object that the interaction originated from.</param>
        public static void ProcessInteraction<T>(T interactionData, IImpactMaterial impactMaterial, IImpactObject impactObject) where T : IInteractionData
        {
            ImpactManager instance = GetInstance();
            instance.ProcessInteraction(interactionData, impactMaterial, impactObject);
        }

        /// <summary>
        /// Process a continuous interaction using the interaction data and the given Impact Object. The primary material at the interaction point will be used.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactObject">The Impact Object that an Impact Material will be retrieved from.</param>
        public static void ProcessContinuousInteraction<T>(T interactionData, IImpactObject impactObject) where T : IInteractionData
        {
            ImpactManager instance = GetInstance();
            instance.ProcessContinuousInteraction(interactionData, impactObject.GetPrimaryMaterial(interactionData.Point), impactObject);
        }

        /// <summary>
        /// Process a continuous interaction using the interaction data, an Impact Material, and an optional Impact Object that the interaction originated from.
        /// </summary>
        /// <param name="interactionData">The interaction data to process.</param>
        /// <param name="impactMaterial">The Impact Material to get interaction results from.</param>
        /// <param name="impactObject">An optional Impact Object that the interaction originated from.</param>
        public static void ProcessContinuousInteraction<T>(T interactionData, IImpactMaterial material, IImpactObject impactObject) where T : IInteractionData
        {
            ImpactManager instance = GetInstance();
            instance.ProcessContinuousInteraction(interactionData, material, impactObject);
        }

        /// <summary>
        /// Adds or updates the given continuous interaction result to the active continuous interaction results, if able.
        /// </summary>
        /// <param name="impactObject">The impact object the result is being sent from.</param>
        /// <param name="result">The new continuous interaction result.</param>
        public static void AddOrUpdateContinuousInteractionResult(IImpactObject impactObject, IContinuousInteractionResult result)
        {
            ImpactManager instance = GetInstance();
            instance.AddOrUpdateContinuousInteractionResult(impactObject, result);
        }

        /// <summary>
        /// Creates a new Interaction Result pool with the given key and size.
        /// </summary>
        /// <typeparam name="T">The IPoolable type to use.</typeparam>
        /// <param name="key">The name of the pool, used to retrieve objects.</param>
        /// <returns>The newly created pool, or the existing pool if there is one with the same key.</returns>
        public static void CreateInteractionResultPool<T>(string key) where T : class, IPoolable, new()
        {
            ImpactManager instance = GetInstance();
            instance.CreateInteractionResultPool<T>(key);
        }

        /// <summary>
        /// Attempts to get an available empty interaction result from the pool with the specified key.
        /// </summary>
        /// <typeparam name="T">The IPoolable type to get.</typeparam>
        /// <param name="key">The name of the pool.</param>
        /// <param name="obj">The returned result, if one was found.</param>
        /// <returns>True if an available result was found. False otherwise.</returns>
        public static bool TryGetInteractionResultFromPool<T>(string key, out T obj) where T : class, IPoolable, new()
        {
            ImpactManager instance = GetInstance();
            return instance.TryGetInteractionResultFromPool(key, out obj);
        }
    }
}

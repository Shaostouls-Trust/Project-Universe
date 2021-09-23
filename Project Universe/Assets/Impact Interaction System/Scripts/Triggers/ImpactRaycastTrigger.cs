using Impact.Materials;
using Impact.Objects;
using UnityEngine;

namespace Impact.Triggers
{
    /// <summary>
    /// Helper class for handling interactions based on raycasts.
    /// </summary>
    public static class ImpactRaycastTrigger
    {
        /// <summary>
        /// Trigger an interaction from a raycast. The interaction will be processed on the object that was hit by the raycast.
        /// </summary>
        /// <param name="hit">Raycast hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="tagMask">The tag mask to use for the interaction.</param>
        /// <param name="velocity">The velocity of the interaction.</param>
        /// <param name="fromObject">The object the interaction is originating from.</param>
        /// <param name="interactionType">The type of interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger(ImpactTagMask tagMask, RaycastHit hit, Vector3 velocity, GameObject fromObject, int interactionType, bool useMaterialComposition)
        {
            InteractionData interactionData = new InteractionData();

            interactionData.TagMask = tagMask;
            interactionData.InteractionType = interactionType;
            interactionData.Velocity = velocity;
            interactionData.ThisObject = fromObject;

            Trigger(interactionData, hit, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a raycast. The interaction will be processed on the object that was hit by the raycast.
        /// </summary>
        /// <param name="hit">Raycast hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="interactionData">The original interaction data. Velocity, TagMask, ThisObject, and InteractionType should already be set.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger<T>(T interactionData, RaycastHit hit, bool useMaterialComposition) where T : IInteractionData
        {
            interactionData.OtherObject = hit.collider.gameObject;
            interactionData.Point = hit.point;
            interactionData.Normal = hit.normal;
            interactionData.CompositionValue = 1;

            IImpactObject otherObject = hit.collider.GetComponentInParent<IImpactObject>();
            int physicsMaterialId = 0;

            if (ImpactManagerInstance.UseMaterialMapping && hit.collider.sharedMaterial != null)
                physicsMaterialId = hit.collider.sharedMaterial.GetInstanceID();

            triggerOnHitObject(interactionData, otherObject, physicsMaterialId, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a 2D raycast. The interaction will be processed on the object that was hit by the raycast.
        /// </summary>
        /// <param name="hit">Raycast 2D hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="tagMask">The tag mask to use for the interaction.</param>
        /// <param name="velocity">The velocity of the interaction.</param>
        /// <param name="fromObject">The object the interaction is originating from.</param>
        /// <param name="interactionType">The type of interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger2D(ImpactTagMask tagMask, RaycastHit2D hit, Vector3 velocity, GameObject fromObject, int interactionType, bool useMaterialComposition)
        {
            InteractionData interactionData = new InteractionData();

            interactionData.TagMask = tagMask;
            interactionData.InteractionType = interactionType;
            interactionData.Velocity = velocity;
            interactionData.ThisObject = fromObject;

            Trigger2D(interactionData, hit, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a 2D raycast. The interaction will be processed on the object that was hit by the raycast.
        /// </summary>
        /// <param name="hit">Raycast 2D hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="interactionData">The original interaction data. Velocity, TagMask, ThisObject, and InteractionType should already be set.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger2D<T>(T interactionData, RaycastHit2D hit, bool useMaterialComposition) where T : IInteractionData
        {
            interactionData.OtherObject = hit.collider.gameObject;
            interactionData.Point = hit.point;
            interactionData.Normal = hit.normal;
            interactionData.CompositionValue = 1;

            IImpactObject otherObject = hit.collider.GetComponentInParent<IImpactObject>();
            int physicsMaterialId = 0;

            if (ImpactManagerInstance.UseMaterialMapping && hit.collider.sharedMaterial != null)
                physicsMaterialId = hit.collider.sharedMaterial.GetInstanceID();

            triggerOnHitObject(interactionData, otherObject, physicsMaterialId, useMaterialComposition);
        }

        private static void triggerOnHitObject<T>(T interactionData, IImpactObject otherObject, int physicsMaterialId, bool useMaterialComposition) where T : IInteractionData
        {
            if (otherObject != null)
            {
                if (useMaterialComposition)
                {
                    int count = otherObject.GetMaterialCompositionNonAlloc(interactionData.Point, ImpactManagerInstance.MaterialCompositionBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        ImpactMaterialComposition comp = ImpactManagerInstance.MaterialCompositionBuffer[i];
                        if (comp.CompositionValue > 0)
                        {
                            IInteractionData newInteractionData = interactionData.Clone();
                            newInteractionData.CompositionValue = comp.CompositionValue;
                            ImpactManagerInstance.ProcessInteraction(newInteractionData, comp.Material, otherObject);
                        }
                    }
                }
                else
                    ImpactManagerInstance.ProcessInteraction(interactionData, otherObject);
            }
            else if (ImpactManagerInstance.UseMaterialMapping)
            {
                IImpactMaterial m;
                if (ImpactManagerInstance.TryGetImpactMaterialFromMapping(physicsMaterialId, out m))
                {
                    ImpactManagerInstance.ProcessInteraction(interactionData, m, null);
                }
            }
        }

        /// <summary>
        /// Trigger an interaction from a raycast. Impact Material data will be retrieved from the object that was hit by the raycast. The interaction will be processed using the given IImpactObject.
        /// </summary>
        /// <param name="hit">Raycast hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="impactObject">The Impact Object the resulting interaction will be processed on.</param>
        /// <param name="velocity">The velocity of the interaction.</param>
        /// <param name="interactionType">The type of interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger(IImpactObject impactObject, RaycastHit hit, Vector3 velocity, int interactionType, bool useMaterialComposition)
        {
            InteractionData interactionData = new InteractionData();

            interactionData.InteractionType = interactionType;
            interactionData.Velocity = velocity;
            interactionData.ThisObject = impactObject.GameObject;

            Trigger(interactionData, hit, impactObject, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a raycast. Impact Material data will be retrieved from the object that was hit by the raycast. The interaction will be processed using the given IImpactObject.
        /// </summary>
        /// <param name="hit">Raycast hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="interactionData">The original interaction data. Velocity, ThisObject, and InteractionType should already be set.</param>
        /// <param name="impactObject">The IImpactObject that will process the interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger<T>(T interactionData, RaycastHit hit, IImpactObject impactObject, bool useMaterialComposition) where T : IInteractionData
        {
            interactionData.Point = hit.point;
            interactionData.Normal = hit.normal;
            interactionData.OtherObject = hit.collider.gameObject;
            interactionData.CompositionValue = 1;

            IImpactObject otherObject = hit.collider.GetComponentInParent<IImpactObject>();
            int physicsMaterialId = 0;

            if (ImpactManagerInstance.UseMaterialMapping && hit.collider.sharedMaterial != null)
                physicsMaterialId = hit.collider.sharedMaterial.GetInstanceID();

            triggerOnRaycastingObject(interactionData, impactObject, otherObject, physicsMaterialId, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a 2D raycast. Impact Material data will be retrieved from the object that was hit by the raycast. The interaction will be processed using the given IImpactObject.
        /// </summary>
        /// <param name="hit">Raycast 2D hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="impactObject">The Impact Object the resulting interaction will be processed on.</param>
        /// <param name="velocity">The velocity of the interaction.</param>
        /// <param name="interactionType">The type of interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger2D(IImpactObject impactObject, RaycastHit2D hit, Vector3 velocity, int interactionType, bool useMaterialComposition)
        {
            InteractionData interactionData = new InteractionData();

            interactionData.InteractionType = interactionType;
            interactionData.Velocity = velocity;
            interactionData.ThisObject = impactObject.GameObject;

            Trigger2D(interactionData, hit, impactObject, useMaterialComposition);
        }

        /// <summary>
        /// Trigger an interaction from a 2D raycast. Impact Material data will be retrieved from the object that was hit by the raycast. The interaction will be processed using the given IImpactObject.
        /// </summary>
        /// <param name="hit">Raycast 2D hit data. The contact point, normal, and the object that was hit will be retrived from this.</param>
        /// <param name="interactionData">The original interaction data. Velocity, ThisObject, and InteractionType should already be set.</param>
        /// <param name="impactObject">The IImpactObject that will process the interaction.</param>
        /// <param name="useMaterialComposition">Should this trigger use the material composition of the object it hit by the raycast?</param>
        public static void Trigger2D<T>(T interactionData, RaycastHit2D hit, IImpactObject impactObject, bool useMaterialComposition) where T : IInteractionData
        {
            interactionData.Point = hit.point;
            interactionData.Normal = hit.normal;
            interactionData.OtherObject = hit.collider.gameObject;
            interactionData.CompositionValue = 1;

            IImpactObject otherObject = hit.collider.GetComponentInParent<IImpactObject>();
            int physicsMaterialId = 0;

            if (ImpactManagerInstance.UseMaterialMapping && hit.collider.sharedMaterial != null)
                physicsMaterialId = hit.collider.sharedMaterial.GetInstanceID();

            triggerOnRaycastingObject(interactionData, impactObject, otherObject, physicsMaterialId, useMaterialComposition);
        }

        private static void triggerOnRaycastingObject<T>(T interactionData, IImpactObject impactObject, IImpactObject otherObject, int physicsMaterialId, bool useMaterialComposition) where T : IInteractionData
        {
            if (otherObject != null)
            {
                if (useMaterialComposition)
                {
                    int count = otherObject.GetMaterialCompositionNonAlloc(interactionData.Point, ImpactManagerInstance.MaterialCompositionBuffer);
                    for (int i = 0; i < count; i++)
                    {
                        ImpactMaterialComposition comp = ImpactManagerInstance.MaterialCompositionBuffer[i];
                        if (comp.CompositionValue > 0)
                        {
                            IInteractionData newInteractionData = interactionData.Clone();
                            newInteractionData.CompositionValue = comp.CompositionValue;
                            newInteractionData.TagMask = comp.Material.MaterialTagsMask;
                            ImpactManagerInstance.ProcessInteraction(newInteractionData, impactObject);
                        }
                    }
                }
                else
                {
                    IImpactMaterial material = otherObject.GetPrimaryMaterial(interactionData.Point);
                    if (material != null || (ImpactManagerInstance.UseMaterialMapping && ImpactManagerInstance.TryGetImpactMaterialFromMapping(physicsMaterialId, out material)))
                        interactionData.TagMask = material.MaterialTagsMask;

                    ImpactManagerInstance.ProcessInteraction(interactionData, impactObject);
                }
            }
            else if (ImpactManagerInstance.UseMaterialMapping)
            {
                IImpactMaterial material;
                if (ImpactManagerInstance.TryGetImpactMaterialFromMapping(physicsMaterialId, out material))
                    interactionData.TagMask = material.MaterialTagsMask;

                ImpactManagerInstance.ProcessInteraction(interactionData, impactObject);
            }
        }
    }
}


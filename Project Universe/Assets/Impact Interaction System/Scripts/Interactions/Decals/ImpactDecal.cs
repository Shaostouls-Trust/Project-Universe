using Impact.Data;
using Impact.Utility;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// Standard implementation of ImpactDecalBase.
    /// </summary>
    [AddComponentMenu("Impact/Impact Decal")]
    public class ImpactDecal : ImpactDecalBase
    {
        /// <summary>
        /// Modes for setting how a decal should be rotated.
        /// </summary>
        public enum DecalRotationMode
        {
            /// <summary>
            /// Randomly rotate the decal on the surface normal axis.
            /// </summary>
            AlignToNormalRandom = 0,
            /// <summary>
            /// Rotate the decal to match the interaction velocity.
            /// </summary>
            AlignToNormalAndVelocity = 1,
            /// <summary>
            /// Only rotate to match the interaction normal.
            /// </summary>
            AlignToNormalOnly = 2
        }

        [SerializeField]
        private float _decalDistance = 0.01f;
        [SerializeField]
        private DecalRotationMode _rotationMode = DecalRotationMode.AlignToNormalRandom;
        [SerializeField]
        private AlignmentAxis _axis = AlignmentAxis.ZDown;
        [SerializeField]
        private bool _parentToObject = true;

        [SerializeField]
        private int _poolSize = 50;
        [SerializeField]
        private ObjectPoolFallbackMode _poolFallbackMode = ObjectPoolFallbackMode.Oldest;

        public override int PoolSize
        {
            get { return _poolSize; }
            set { _poolSize = value; }
        }

        public override ObjectPoolFallbackMode PoolFallbackMode
        {
            get { return _poolFallbackMode; }
            set { _poolFallbackMode = value; }
        }

        /// <summary>
        /// The distance the decal should be placed from the surface.
        /// </summary>
        public float DecalDistance
        {
            get { return _decalDistance; }
            set { _decalDistance = value; }
        }

        /// <summary>
        /// How the decal should be rotated.
        /// </summary>
        public DecalRotationMode RotationMode
        {
            get { return _rotationMode; }
            set { _rotationMode = value; }
        }

        /// <summary>
        /// Which axis should be pointed towards the surface.
        /// </summary>
        public AlignmentAxis Axis
        {
            get { return _axis; }
            set { _axis = value; }
        }

        /// <summary>
        /// Should the decal be parented to the object it is placed on?
        /// </summary>
        public bool ParentToObject
        {
            get { return _parentToObject; }
            set { _parentToObject = value; }
        }

        private DestroyMessenger parentObject;

        public override void SetupDecal(DecalInteractionResult interactionResult, Vector3 point, Vector3 normal)
        {
            transform.position = point + normal * DecalDistance;

            if (RotationMode == DecalRotationMode.AlignToNormalRandom)
            {
                transform.rotation = AlignmentAxisUtilities.GetRotationForAlignment(Axis, normal);
                rotateRandom();
            }
            else if (RotationMode == DecalRotationMode.AlignToNormalAndVelocity)
            {
                transform.rotation = AlignmentAxisUtilities.GetVelocityRotation(Axis, normal, interactionResult.OriginalData.Velocity);
            }
            else
            {
                transform.rotation = AlignmentAxisUtilities.GetRotationForAlignment(Axis, normal);
            }

            if (ParentToObject && interactionResult.OriginalData.OtherObject)
            {
                transform.SetParent(interactionResult.OriginalData.OtherObject.transform);

                parentObject = interactionResult.OriginalData.OtherObject.GetOrAddComponent<DestroyMessenger>();
                parentObject.OnDestroyed += onParentDestroyed;
            }
            else
                transform.SetParent(OriginalParent, true);
        }

        private void rotateRandom()
        {
            if (Axis == AlignmentAxis.ZDown || Axis == AlignmentAxis.ZUp)
                transform.Rotate(new Vector3(0, 0, Random.value * 360f), Space.Self);
            else
                transform.Rotate(new Vector3(0, Random.value * 360f, 0), Space.Self);
        }

        private void onParentDestroyed()
        {
            MakeAvailable();
        }

        public override void Retrieve(int priority)
        {
            if (parentObject)
                parentObject.OnDestroyed -= onParentDestroyed;
            parentObject = null;

            base.Retrieve(priority);
        }

        public override void MakeAvailable()
        {
            if (parentObject)
                parentObject.OnDestroyed -= onParentDestroyed;
            parentObject = null;

            base.MakeAvailable();
        }
    }
}

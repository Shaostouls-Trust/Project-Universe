using Impact.Materials;
using System.Collections.Generic;
using UnityEngine;
using Impact.Utility;
using System;

namespace Impact.Objects
{
    /// <summary>
    /// Component for Impact Objects attached to a terrain.
    /// </summary>
    [AddComponentMenu("Impact/Impact Terrain", 0)]
    public class ImpactTerrain : ImpactObjectBase
    {
        [SerializeField]
        private Terrain _terrain;
        [SerializeField]
        private List<ImpactMaterialBase> _terrainMaterials = new List<ImpactMaterialBase>();

        private float[,,] cachedAlphamaps;
        private ImpactMaterialComposition[] compositionBuffer;
        private bool hasTerrain;

        /// <summary>
        /// The terrain associated with this object.
        /// </summary>
        public Terrain Terrain
        {
            get { return _terrain; }
            set
            {
                _terrain = value;
                hasTerrain = _terrain != null;
            }
        }

        /// <summary>
        /// The terrain data associated with this object.
        /// </summary>
        public TerrainData TerrainData
        {
            get
            {
                if (Terrain != null)
                    return Terrain.terrainData;

                return null;
            }
        }

        /// <summary>
        /// The list of materials that correspond to the terrain's terrain layers.
        /// </summary>
        public List<ImpactMaterialBase> TerrainMaterials
        {
            get { return _terrainMaterials; }
        }

        private void Awake()
        {
            hasTerrain = Terrain != null;
            RefreshCachedAlphamaps();
        }

        private void Reset()
        {
            Terrain = GetComponent<Terrain>();
        }

        /// <summary>
        /// Refresh the cached alphamaps/splatmaps. 
        /// Use this if your alphamaps can change during runtime.
        /// </summary>
        public void RefreshCachedAlphamaps()
        {
            if (!hasTerrain)
            {
                Debug.LogError($"Cannot refresh cached alphamaps for ImpactTerrain {gameObject.name} because it has no TerrainData.");
                return;
            }

            cachedAlphamaps = TerrainData.GetAlphamaps(0, 0, TerrainData.alphamapResolution, TerrainData.alphamapResolution);
            compositionBuffer = new ImpactMaterialComposition[TerrainData.terrainLayers.Length];
        }

        /// <summary>
        /// Syncs the terrain layers and materials list so they are the same length.
        /// </summary>
        public void SyncTerrainLayersAndMaterialsList()
        {
            if (TerrainData == null)
            {
                Debug.LogError($"Cannot sync terrain layers and materials for ImpactTerrain {gameObject.name} because it has no TerrainData.");
                return;
            }

            TerrainLayer[] terrainLayers = TerrainData.terrainLayers;

            int terrainLayerCount = terrainLayers.Length;
            int terrainMaterialTypesCount = TerrainMaterials.Count;
            int diff = terrainLayerCount - terrainMaterialTypesCount;

            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    TerrainMaterials.Add(null);
                }
            }
            else if (diff < 0)
            {
                TerrainMaterials.RemoveRange(terrainLayers.Length, -diff);
            }
        }

        public override int GetMaterialCompositionNonAlloc(Vector3 point, ImpactMaterialComposition[] results)
        {
            if (!hasTerrain)
            {
                Debug.LogError($"Cannot get material composition for ImpactTerrain {gameObject.name} because it has no TerrainData.");
                return 0;
            }

            Vector2Int alphamapIndices = getAlphamapIndicesAtPoint(point);
            int finalLength = Mathf.Min(results.Length, compositionBuffer.Length);

            int count = 0;
            float compositionValueTotal = 0;

            //Clear influence buffer
            for (int i = 0; i < compositionBuffer.Length; i++)
            {
                compositionBuffer[i].CompositionValue = 0;
                compositionBuffer[i].Material = null;
            }

            //Get the composition of all impact materials, combining when needed (since you can have multiple textures mapped to the same impact material)
            for (int i = 0; i < compositionBuffer.Length; i++)
            {
                IImpactMaterial m = TerrainMaterials[i];
                float comp = cachedAlphamaps[alphamapIndices.y, alphamapIndices.x, i];

                int existingIndex = compositionBuffer.IndexOf(p => p.Material == m);
                if (existingIndex > -1)
                {
                    compositionBuffer[existingIndex].CompositionValue += comp;
                }
                else
                {
                    compositionBuffer[i].CompositionValue = comp;
                    compositionBuffer[i].Material = m;
                }

                if (count < finalLength)
                {
                    compositionValueTotal += comp;
                    count++;
                }
            }

            //Sort composition buffer by composition value
            Array.Sort(compositionBuffer, (a, b) => { return b.CompositionValue.CompareTo(a.CompositionValue); });

            //Populate final composition results
            for (int i = 0; i < finalLength; i++)
            {
                //Adjust composition value so results will always add up to 1, this is for cases where results.length < compositionBuffer.Length
                compositionBuffer[i].CompositionValue = Mathf.Clamp01(compositionBuffer[i].CompositionValue / compositionValueTotal);
                results[i] = compositionBuffer[i];
            }

            return finalLength;
        }

        public override IImpactMaterial GetPrimaryMaterial(Vector3 point)
        {
            if (!hasTerrain)
            {
                Debug.LogError($"Cannot get primary material for ImpactTerrain {gameObject.name} because it has no TerrainData.");
                return null;
            }

            Vector2Int alphamapIndices = getAlphamapIndicesAtPoint(point);

            float max = 0;
            int maxIndex = -1;

            for (int i = 0; i < TerrainMaterials.Count; i++)
            {
                if (cachedAlphamaps[alphamapIndices.y, alphamapIndices.x, i] > max)
                {
                    max = cachedAlphamaps[alphamapIndices.y, alphamapIndices.x, i];
                    maxIndex = i;
                }
            }

            return TerrainMaterials[maxIndex];
        }

        public override IImpactMaterial GetPrimaryMaterial()
        {
            if (TerrainMaterials.Count > 0)
                return TerrainMaterials[0];

            return null;
        }

        private Vector2Int getAlphamapIndicesAtPoint(Vector3 point)
        {
            Vector3 terrainPosition = _terrain.transform.position;
            Vector2Int v = new Vector2Int();

            v.x = (int)(((point.x - terrainPosition.x) / TerrainData.size.x) * TerrainData.alphamapWidth);
            v.y = (int)(((point.z - terrainPosition.z) / TerrainData.size.z) * TerrainData.alphamapHeight);

            return v;
        }
    }
}
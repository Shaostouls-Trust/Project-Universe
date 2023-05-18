using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using UnityEngine.Rendering;
[Serializable]
public struct FlareState
{
    public Vector3 sourceCoordinate;
    public Vector3[] flareWorldPosCenter;
    public float fadeoutScale;
    public int fadeState;    //0:normal, 1:fade in, 2: fade out, 3:unrendered
}

[ExecuteInEditMode]
public class MFLensFlare : MonoBehaviour
{
    public float extendNearPlane = 1;
    public float maxFadeOutScale = 1;
    public bool HDRP_Mode = false;

    public bool DebugMode;
    [Space(10)]
    public List<MFFlareLauncher> lightSource;
    public Material material;
    public float fadeoutTime;
    public List<FlareState> flareDatas;
    private Camera _camera;
    private Vector2 _halfScreen;
    private MaterialPropertyBlock _propertyBlock;
    private List<Mesh> _totalMesh;
    private List<Vector3> _totalVert;
    private List<Vector2> _totalUv;
    private List<Color> _totalColor;
    private List<int> _totalTriangle;
    private List<Vector4> _screenpos;

    private static readonly int STATIC_FLARESCREENPOS = Shader.PropertyToID("_FlareScreenPos");
    private static readonly int STATIC_BaseMap = Shader.PropertyToID("_MainTex");
    private static readonly float DISTANCE = 1f;

    private void Awake()
    {
        _totalVert = new List<Vector3>();
        _totalUv = new List<Vector2>();
        _totalColor = new List<Color>();
        _totalTriangle = new List<int>();
        _totalMesh = new List<Mesh>();
        _screenpos = new List<Vector4>();
        
        _camera = GetComponent<Camera>();
        if (flareDatas == null)
        {
            flareDatas = new List<FlareState>();
        }
        _propertyBlock = new MaterialPropertyBlock();
    }
    private FlareState InitFlareData(MFFlareLauncher mfFlareLauncher)
    {
        FlareState state = new FlareState
        {
            sourceCoordinate = Vector3.zero,
            flareWorldPosCenter = new Vector3[mfFlareLauncher.assetModel.spriteBlocks.Count],
            // edgeScale = 1,
            fadeoutScale = 0,
            fadeState = 1
        };
        return state;
    }

    public void AddLight(MFFlareLauncher mfFlareLauncher)
    {
        if (DebugMode)
        {
            Debug.Log("Add Light " + mfFlareLauncher.gameObject.name + " to FlareList");
        }
        lightSource.Add(mfFlareLauncher);
        flareDatas.Add(InitFlareData(mfFlareLauncher));
        if (_totalMesh == null)
        {
            _totalMesh = new List<Mesh>();
        }
        _totalMesh.Add(new Mesh());
    }

    // public void RemoveLight(URPFlareLauncher urpFlareLauncher)
    // {
    //     if(DebugMode)Debug.Log("Remove Light " + urpFlareLauncher.gameObject.name + " from FlareList");
    //     int t = lightSource.IndexOf(urpFlareLauncher);
    //     lightSource.RemoveAt(t);
    //     flareDatas.RemoveAt(t);
    //     while (_totalMesh.Count > lightSource.Count)
    //     {
    //         _totalMesh.RemoveAt(0);
    //     }
    // }
    private void Update()
    {
        if (!Application.isPlaying)
        {
            Awake();
        }

        _halfScreen = new Vector2(_camera.scaledPixelWidth / 2 + _camera.pixelRect.xMin, _camera.scaledPixelHeight / 2 + _camera.pixelRect.yMin);
        _totalMesh.Clear();
        _screenpos.Clear();
        for (int i = 0; i < lightSource.Count; i++)
        {
            _totalMesh.Add(new Mesh());
            FlareState flareState = flareDatas[i];
            GetSourceCoordinate(ref flareState, i);
            bool isIn = CheckIn(ref flareState, i);
            if (flareState.fadeoutScale > 0)
            {
                // if (flareData.fadeState == 3)
                // {
                    // _totalMesh.Add(new Mesh());
                // }
                if (!isIn)
                {
                    flareState.fadeState = 2;
                }
            }
            if(flareState.fadeoutScale < 1)
            {
                if (isIn)
                {
                    flareState.fadeState = 1;
                }
            }
            if (!isIn && flareState.fadeoutScale <=0 )
            {
                if (flareState.fadeState != 3)
                {
                    flareState.fadeState = 3;
                }
            }
            else
            {
                CalculateMeshData(ref flareState, i);
            }

            switch (flareState.fadeState)
            {
                case 1:
                    flareState.fadeoutScale += Time.deltaTime / fadeoutTime;
                    flareState.fadeoutScale = Mathf.Clamp(flareState.fadeoutScale, 0, maxFadeOutScale);
                    flareDatas[i] = flareState;
                    break;
                case 2:
                    flareState.fadeoutScale -= Time.deltaTime / fadeoutTime;
                    flareState.fadeoutScale = Mathf.Clamp(flareState.fadeoutScale, 0, maxFadeOutScale);
                    flareDatas[i] = flareState;
                    break;
                case 3:
                    // RemoveLight(lightSource[i]);
                    break;
                default: flareDatas[i] = flareState;
                    break;
            }
        }
        CreateMesh();

        if (DebugMode)
        {
            Debug.Log("Lens Flare : " + lightSource.Count + " lights");

            for (int i = 0; i < lightSource.Count; i++)
            {
                DebugDrawMeshPos(i);
            }
            
        }
    }

    bool CheckIn(ref FlareState state, int lightIndex)
    {
        if (state.sourceCoordinate.x <  _camera.pixelRect.xMin || state.sourceCoordinate.y < _camera.pixelRect.yMin 
            || state.sourceCoordinate.x > _camera.pixelRect.xMax || state.sourceCoordinate.y > _camera.pixelRect.yMax
            || Vector3.Dot(lightSource[lightIndex].directionalLight ? -lightSource[lightIndex].transform.forward : lightSource[lightIndex].transform.position - _camera.transform.position, _camera.transform.forward) < 0.25f)
        {
            _screenpos.Add(Vector4.zero);
            return false;
        }
        else
        {
            // var camPos = _camera.transform.position;
            // var targetPos = lightSource[lightIndex].directionalLight
            //     ? -lightSource[lightIndex].transform.forward * 10000f
            //     : lightSource[lightIndex].transform.position;
            // Ray ray = new Ray(camPos, targetPos - camPos );
            // RaycastHit hit;
            // Physics.Raycast(ray, out hit);
            // if (Vector3.Distance(hit.point, camPos) < Vector3.Distance(targetPos, camPos))
            // {
            //     if (hit.point == Vector3.zero) return true;
            //     return false;
            // }
            Vector4 screenUV = state.sourceCoordinate;
            screenUV.x = screenUV.x / _camera.pixelWidth;
            screenUV.y = screenUV.y / _camera.pixelHeight;
            screenUV.w = lightSource[lightIndex].directionalLight ? 1 : 0;
            _screenpos.Add(screenUV);
            return true;
        }
    }

    void GetSourceCoordinate(ref FlareState state, int lightIndex)
    {
        //v1.0.8c
        //Vector3 sourceScreenPos = _camera.WorldToScreenPoint(
        //    lightSource[lightIndex].directionalLight 
        //    ?_camera.transform.position - lightSource[lightIndex].transform.forward * 10000
        //    :lightSource[lightIndex].transform.position
        //    );
        Vector3 sourceScreenPos = WorldToScreenPoint(
            lightSource[lightIndex].directionalLight
            ? _camera.transform.position - lightSource[lightIndex].transform.forward * 10000
            : lightSource[lightIndex].transform.position
            );
        state.sourceCoordinate = sourceScreenPos;
    }

    //v1.0.8c
    public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {               
        Matrix4x4 projectMatrix = _camera.projectionMatrix;
        double x = screenPosition.x / Screen.width;  x = 2f * (x - 0.5f);
        double y = screenPosition.y / Screen.height; y = 2f * (y - 0.5f);
        double z = -(screenPosition.z + projectMatrix.m23) / projectMatrix.m22;
        double w = screenPosition.z; x *= w; y *= w;
        
        Matrix4x4 inverseMatrix = _camera.projectionMatrix.inverse;
        double xM = (inverseMatrix.m00 * x + inverseMatrix.m01 * y + inverseMatrix.m02 * z + inverseMatrix.m03 * w);
        double yM = (inverseMatrix.m10 * x + inverseMatrix.m11 * y + inverseMatrix.m12 * z + inverseMatrix.m13 * w);
        double zM = (inverseMatrix.m20 * x + inverseMatrix.m21 * y + inverseMatrix.m22 * z + inverseMatrix.m23 * w);
        
        Matrix4x4 inverseMatrixM = _camera.worldToCameraMatrix.inverse;
        double xA = (inverseMatrixM.m00 * xM + inverseMatrixM.m01 * yM + inverseMatrixM.m02 * zM + inverseMatrixM.m03);
        double yA = (inverseMatrixM.m10 * xM + inverseMatrixM.m11 * yM + inverseMatrixM.m12 * zM + inverseMatrixM.m13);
        double zA = (inverseMatrixM.m20 * xM + inverseMatrixM.m21 * yM + inverseMatrixM.m22 * zM + inverseMatrixM.m23);
        return new Vector3((float)(xA), (float)(yA), (float)(zA));
    }
    public Vector3 WorldToScreenPoint(Vector3 worldPosition)
    {        
        Matrix4x4 matrix = _camera.worldToCameraMatrix;
        double x = (matrix.m00 * worldPosition.x + matrix.m01 * worldPosition.y + matrix.m02 * worldPosition.z + matrix.m03);
        double y = (matrix.m10 * worldPosition.x + matrix.m11 * worldPosition.y + matrix.m12 * worldPosition.z + matrix.m13);
        double z = (matrix.m20 * worldPosition.x + matrix.m21 * worldPosition.y + matrix.m22 * worldPosition.z + matrix.m23);
        
        Matrix4x4 matrixM = _camera.projectionMatrix;
        double xM = (matrixM.m00 * x + matrixM.m01 * y + matrixM.m02 * z + matrixM.m03);
        double yM = (matrixM.m10 * x + matrixM.m11 * y + matrixM.m12 * z + matrixM.m13);
        double zM = (matrixM.m20 * x + matrixM.m21 * y + matrixM.m22 * z + matrixM.m23);
        double wM = (matrixM.m30 * x + matrixM.m31 * y + matrixM.m32 * z + matrixM.m33);
       
        double xA = xM / wM; double yA = yM / wM;

        xA = 0.5 + (xA * 0.5); yA = 0.5 + (yA * 0.5);
        return new Vector3((float)(xA * Screen.width), (float)(yA * Screen.height), (float)(-z));
    }

    void CalculateMeshData(ref FlareState state, int lightIndex)
    {
        Vector3[] oneFlareLine = new Vector3[lightSource[lightIndex].assetModel.spriteBlocks.Count];
        float[] useLightColor = new float[lightSource[lightIndex].assetModel.spriteBlocks.Count];
        for (int i = 0; i < lightSource[lightIndex].assetModel.spriteBlocks.Count; i++)
        {
            Vector2 realSourceCoordinateOffset = new Vector2(state.sourceCoordinate.x - _halfScreen.x, state.sourceCoordinate.y - _halfScreen.y);
            Vector2 realOffset = realSourceCoordinateOffset * lightSource[lightIndex].assetModel.spriteBlocks[i].offset;
            oneFlareLine[i] = new Vector3(_halfScreen.x + realOffset.x, _halfScreen.y + realOffset.y, DISTANCE);
            useLightColor[i] = lightSource[lightIndex].assetModel.spriteBlocks[i].useLightColor;
        }
        state.flareWorldPosCenter = oneFlareLine;
    }

    void CreateMesh()
    {
        if (_totalMesh.Count <= 0){ Debug.Log("Not enough source");return;}
        
        List<Vector3> vertList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        List<int> tri = new List<int>();
        List<Color> vertColors = new List<Color>();

        int count = 0;
        var transform1 = _camera.transform;
        var position = transform1.position;
        var center = position + transform1.forward * 0.1f;
        for (int lightIndex = 0; lightIndex < lightSource.Count; lightIndex++)
        {
            _totalColor.Clear();
            _totalTriangle.Clear();
            _totalVert.Clear();
            _totalUv.Clear();
            vertList.Clear();
            uvList.Clear();
            tri.Clear();
            vertColors.Clear();
            if (flareDatas[lightIndex].fadeoutScale > 0)
            {
                MFFlareLauncher observer = lightSource[lightIndex];
                Texture2D tex = observer.assetModel.flareSprite;//observer.asset.flareSprite;
                float angle = (45 +Vector2.SignedAngle(Vector2.up, new Vector2(flareDatas[lightIndex].sourceCoordinate.x - _halfScreen.x, flareDatas[lightIndex].sourceCoordinate.y - _halfScreen.y))) / 180 * Mathf.PI;
                for (int i = 0; i < lightSource[lightIndex].assetModel.spriteBlocks.Count; i++)
                {
                    Rect rect = observer.assetModel.spriteBlocks[i].block;
                    Vector2 halfSize = new Vector2(
                        tex.width * rect.width / 2 * observer.assetModel.spriteBlocks[i].scale * (observer.assetModel.fadeWithScale ? ( flareDatas[lightIndex].fadeoutScale * 0.5f + 0.5f) : 1), 
                        tex.height * rect.height / 2 * observer.assetModel.spriteBlocks[i].scale * (observer.assetModel.fadeWithScale ? ( flareDatas[lightIndex].fadeoutScale * 0.5f + 0.5f) : 1));
                    Vector3 flarePos = flareDatas[lightIndex].flareWorldPosCenter[i];
                    if (observer.assetModel.spriteBlocks[i].useRotation)
                    {
                        float magnitude = Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
                        float cos = magnitude * Mathf.Cos(angle);
                        float sin = magnitude * Mathf.Sin(angle);
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x - sin, flarePos.y + cos, flarePos.z * extendNearPlane )) - center); //v1.0.8c _camera.ScreenToWorldPoint
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x - cos, flarePos.y - sin, flarePos.z * extendNearPlane)) - center);
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x + cos, flarePos.y + sin, flarePos.z * extendNearPlane)) - center);
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x + sin, flarePos.y - cos, flarePos.z * extendNearPlane)) - center);
                    }
                    else
                    {
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y + halfSize.y, flarePos.z * extendNearPlane)) - center); //v1.0.8c
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y - halfSize.y, flarePos.z * extendNearPlane)) - center);
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y + halfSize.y, flarePos.z * extendNearPlane)) - center);
                        vertList.Add(ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y - halfSize.y, flarePos.z * extendNearPlane)) - center);

                    }
                    uvList.Add(rect.position);
                    uvList.Add(rect.position + new Vector2(rect.width, 0));
                    uvList.Add(rect.position + new Vector2(0, rect.height));
                    uvList.Add(rect.position + rect.size);
                    
                    tri.Add(i * 4);
                    tri.Add(i * 4 + 3);
                    tri.Add(i * 4 + 1);
                    tri.Add(i * 4);
                    tri.Add(i * 4 + 2);
                    tri.Add(i * 4 + 3);
                    
                    Color vertexAddColor = observer.assetModel.spriteBlocks[i].color;
                    Color lightColor = default;
                    Light source = observer.GetComponent<Light>();
                    lightColor.r = Mathf.Lerp(1, source.color.r,
                        observer.assetModel.spriteBlocks[i].useLightColor);
                    lightColor.g = Mathf.Lerp(1, source.color.g,
                        observer.assetModel.spriteBlocks[i].useLightColor);
                    lightColor.b = Mathf.Lerp(1, source.color.b,
                        observer.assetModel.spriteBlocks[i].useLightColor);
                    lightColor.a = 1;
                    lightColor *= observer.useLightIntensity ? source.intensity : 1;
                    
                   

                    if (HDRP_Mode)
                    {
                        vertexAddColor *= new Vector4(lightColor.r, lightColor.g, lightColor.b,
                       (1.5f - Mathf.Abs(observer.assetModel.spriteBlocks[i].offset)) / 1.5f
                       * (1 - Mathf.Min(1, new Vector2(flarePos.x - _halfScreen.x, flarePos.y - _halfScreen.y).magnitude / new Vector2(_halfScreen.x, _halfScreen.y).magnitude))
                        ) * 1;
                    }
                    else
                    {
                        vertexAddColor *= new Vector4(lightColor.r, lightColor.g, lightColor.b,
                       (1.5f - Mathf.Abs(observer.assetModel.spriteBlocks[i].offset)) / 1.5f
                       * (1 - Mathf.Min(1, new Vector2(flarePos.x - _halfScreen.x, flarePos.y - _halfScreen.y).magnitude / new Vector2(_halfScreen.x, _halfScreen.y).magnitude))
                        ) * ((observer.assetModel.fadeWithAlpha ? flareDatas[lightIndex].fadeoutScale : 1));
                    }


                    vertexAddColor = vertexAddColor.linear;
                    vertColors.Add(vertexAddColor);
                    vertColors.Add(vertexAddColor);
                    vertColors.Add(vertexAddColor);
                    vertColors.Add(vertexAddColor);
                }
                _totalVert.AddRange(vertList);
                _totalUv.AddRange(uvList);
                _totalTriangle.AddRange(tri);
                _totalColor.AddRange(vertColors);
                _totalMesh[count].vertices = _totalVert.ToArray();
                _totalMesh[count].uv = _totalUv.ToArray();
                _totalMesh[count].triangles = _totalTriangle.ToArray();
                _totalMesh[count].colors = _totalColor.ToArray();
                _propertyBlock.SetTexture(STATIC_BaseMap, observer.assetModel.flareSprite);
                _propertyBlock.SetVector(STATIC_FLARESCREENPOS, _screenpos[count]);
                Graphics.DrawMesh(_totalMesh[count], center, Quaternion.identity, material, 0, _camera, 0, _propertyBlock);
                count++;
            }
        }
    }
    void DebugDrawMeshPos(int lightIndex)
    {
        for (int i = 0; i < lightSource[lightIndex].assetModel.spriteBlocks.Count; i++)
        {
            Debug.DrawLine(_camera.transform.position, _camera.ScreenToWorldPoint(flareDatas[lightIndex].flareWorldPosCenter[i]));
        }
    }
}

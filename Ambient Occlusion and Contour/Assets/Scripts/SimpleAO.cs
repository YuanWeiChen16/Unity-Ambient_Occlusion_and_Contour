using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleAO : MonoBehaviour
{
    public int samplesNum = 256;
    public GameObject[] objs;

    List<MeshFilter> meshs = new List<MeshFilter>();
    int[] tempLayer;
    LayerMask AOLayer;
    Bounds allBounds;
    ShadowCastingMode[] tempShadowMode;

    Vector3[] rayDir;

    Camera AOcam;
    public RenderTexture AOrt1;
    public RenderTexture AOrt2;
    Texture2D vTex;

    Material AOmat;
    Material[] tempMat;
    int vertexNum = 0;
    int rtwidth = 256;
    float radSurface;

    private void OnEnable()
    {
        AOLayer = 1 << LayerMask.NameToLayer("AO");

        vertexNum = 0;
        objs = GameObject.FindGameObjectsWithTag("AOV");
        meshs.Clear();
        for(int i=0;i<objs.Length;i++)
        {
            MeshFilter[] mfs = objs[i].GetComponentsInChildren<MeshFilter>();
            meshs.AddRange(mfs);
        }
        List<MeshFilter> temp = new List<MeshFilter>(meshs.Count);
        for (int i=0;i<meshs.Count;i++)
        {
            if(meshs[i].gameObject.GetComponent<MeshRenderer>() != null)
            {
                vertexNum += meshs[i].mesh.vertices.Length;
                temp.Add(meshs[i]);
            }
        }
        meshs = temp;

        InitSamples();
        SetAOCamera();
        ImplementAO();
    }

    void InitSamples()
    {
        // set bound
        tempLayer = new int[meshs.Count];
        tempShadowMode = new ShadowCastingMode[meshs.Count];

        for(int i=0;i<meshs.Count;i++)
        {
            MeshRenderer renderer = meshs[i].gameObject.GetComponent<MeshRenderer>();

            if (i != 0)
                allBounds.Encapsulate(renderer.bounds);
            else allBounds = renderer.bounds;

            tempLayer[i] = meshs[i].gameObject.layer;
            tempShadowMode[i] = renderer.shadowCastingMode;

            renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        }

        radSurface = Mathf.Max(allBounds.extents.x, Mathf.Max(allBounds.extents.y, allBounds.extents.z));
        rayDir = new Vector3[samplesNum];

        float angle = Mathf.PI * (3 - Mathf.Sqrt(5));
        float start = (1 - 1.0f / samplesNum);
        float step = (1.0f / samplesNum - 1) - start;
        for(int i=0;i<samplesNum;i++)
        {
            float theta = angle * i;
            float n = start + i * step / samplesNum;
            float r = Mathf.Sqrt(1 - n * n);
            rayDir[i].x = r * Mathf.Cos(theta);
            rayDir[i].y = r * Mathf.Sin(theta);
            rayDir[i].z = n;
            rayDir[i] = allBounds.center + rayDir[i] * radSurface;
        }
    }

    void SetAOCamera()
    {
        AOcam = gameObject.AddComponent<Camera>();

        AOcam = gameObject.GetComponent<Camera>();
        AOcam.enabled = false;

        AOcam.cullingMask = 1 << LayerMask.NameToLayer("AO");
        AOcam.clearFlags = CameraClearFlags.Depth;
        AOcam.nearClipPlane = 0.01f;
        AOcam.farClipPlane = radSurface * 2;
        AOcam.orthographic = true;
        AOcam.allowHDR = false;
        AOcam.allowMSAA = false;
        AOcam.allowDynamicResolution = false;
        AOcam.depthTextureMode = DepthTextureMode.Depth;
        AOcam.orthographicSize = radSurface;
        AOcam.aspect = 1f;

        AOmat = new Material(Shader.Find("GeoAO/VertexAO"));

        int height = (int)Mathf.Ceil(vertexNum / (float)rtwidth);

        AOrt1 = new RenderTexture(rtwidth, height, 0, RenderTextureFormat.ARGBHalf);
        AOrt1.anisoLevel = 0;
        AOrt1.filterMode = FilterMode.Point;
        AOrt2 = new RenderTexture(rtwidth, height, 0, RenderTextureFormat.ARGBHalf);
        AOrt2.anisoLevel = 0;
        AOrt2.filterMode = FilterMode.Point;

        vTex = new Texture2D(rtwidth, height, TextureFormat.RGBAFloat, false);
        vTex.anisoLevel = 0;
        vTex.filterMode = FilterMode.Point;

        int id = 0;
        int size = vTex.width * vTex.height;
        Color[] vColor = new Color[size];
        for(int i=0;i<meshs.Count;i++)
        {
            Transform cur = meshs[i].gameObject.transform;
            Vector3[] v = meshs[i].mesh.vertices;
            for(int j=0;j<v.Length;j++)
            {
                Vector3 p = cur.TransformPoint(v[j]);
                vColor[id].r = p.x;
                vColor[id].g = p.y;
                vColor[id].b = p.z;
                id++;
            }
        }
        vTex.SetPixels(vColor);
        vTex.Apply(false, false);

    }

    void ImplementAO()
    {
        AOmat.SetInt("_uCount", samplesNum);
        AOmat.SetTexture("_AOTex", AOrt1);
        AOmat.SetTexture("_AOTex2", AOrt2);
        AOmat.SetTexture("_uVertex", vTex);

        for (int i = 0; i < meshs.Count; i++)
            meshs[i].gameObject.layer = LayerMask.NameToLayer("AO");

        for (int i = 0; i < samplesNum; i++)
        {
            //set camera visible for each vertex
            AOcam.transform.position = rayDir[i];
            AOcam.transform.LookAt(allBounds.center);

            Matrix4x4 ViewMat = AOcam.worldToCameraMatrix;
            Matrix4x4 ProjectionMat = AOcam.projectionMatrix;

            bool direct3D = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
            if (direct3D)
            {
                // Invert Y for rendering to a render texture
                for (int a = 0; a < 4; a++)
                {
                    ProjectionMat[1, a] = -ProjectionMat[1, a];
                }
                // Scale and bias from OpenGL -> D3D depth range
                for (int a = 0; a < 4; a++)
                {
                    ProjectionMat[2, a] = ProjectionMat[2, a] * 0.5f + ProjectionMat[3, a] * 0.5f;
                }
            }

            AOmat.SetMatrix("_VP", (ProjectionMat * ViewMat));
            AOcam.Render();
        }
        for(int i=0; i<meshs.Count; i++)
        {
            meshs[i].gameObject.layer = tempLayer[i];
            meshs[i].gameObject.GetComponent<MeshRenderer>().shadowCastingMode = tempShadowMode[i];
        }

        // Create a texture containing AO information read by the mesh shader
        List<Vector2[]> alluv = new List<Vector2[]>(meshs.Count);

        Material matShow = new Material(Shader.Find("GeoAO/VertAOOpti"));
        matShow.SetTexture("_AOTex", AOrt1);
        float width = (float)(AOrt2.width - 1);
        float height = (float)(AOrt2.height - 1);
        int id = 0;
        tempMat = new Material[meshs.Count];
        for (int i = 0; i < meshs.Count; i++)
        {
            Vector3[] v = meshs[i].mesh.vertices;
            alluv.Add(new Vector2[v.Length]);
            for (int j = 0; j < v.Length; j++)
            {
                alluv[i][j] = new Vector2((id % rtwidth) / width, id / rtwidth / height);
                id++;
            }
            meshs[i].mesh.uv2 = alluv[i];
            tempMat[i] = meshs[i].gameObject.GetComponent<Renderer>().material;
            meshs[i].gameObject.GetComponent<Renderer>().material = matShow;
        }

    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var matrix = AOcam.cameraToWorldMatrix;
        AOmat.SetMatrix("_InverseView", matrix);
        Graphics.Blit(null, AOrt1, AOmat);
        AOcam.targetTexture = null;
        Graphics.Blit(AOrt1, AOrt2);
    }

    private void OnDisable()
    {
        DestroyImmediate(gameObject.GetComponent<Camera>());
        for (int i = 0; i < meshs.Count; i++)
        {
            meshs[i].gameObject.GetComponent<Renderer>().material = tempMat[i];
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class loadScene : EditorWindow
{
    public TextAsset LoadFile;
    List<GameObject> objs = new List<GameObject>();
    Dictionary<string, int> scriptPlace = new Dictionary<string, int>();
    Dictionary<string, int> objPlace = new Dictionary<string, int>();

    [MenuItem("Tool/Load Scene")]
    public static void MenuOpen()
    {
        EditorWindow.GetWindow(typeof(loadScene), false, "Load Scene", true);
    }

    // Update is called once per frame
    void OnGUI()
    {
       
        LoadFile = (TextAsset)EditorGUILayout.ObjectField("LoadFile: ", LoadFile, typeof(TextAsset), true);
        if (GUILayout.Button("Load Text"))
        {
            RenderTexture renderTex = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets/Textures/lightLook.renderTexture", typeof(RenderTexture));
            Cubemap cubeTex = (Cubemap)AssetDatabase.LoadAssetAtPath("Assets/Textures/CubeT.cubemap", typeof(Cubemap));
            if (load())
            {  
                // copy obj
                objs.Add(new GameObject());
                objPlace["PassPrefab"] = objs.Count - 1;
                DestroyImmediate(objs[objPlace["PassPrefab"]]);
                objs[objPlace["PassPrefab"]] = Instantiate(objs[objPlace["prefab"]]);
                objs[objs.Count - 1].name = "PassPrefab";
                objs[objs.Count - 1].tag = "Untagged";
                Material silhoutte = new Material(Shader.Find("Outlined/Silhouette Only"));
                if (objs[objPlace["PassPrefab"]].GetComponentInChildren<MeshRenderer>() != null)
                {
                    MeshRenderer[] renderers = objs[objPlace["PassPrefab"]].GetComponentsInChildren<MeshRenderer>();
                    for(int i=0;i<renderers.Length;i++)
                    {
                        renderers[i].material = silhoutte;
                    }
                }
            }

        }
    }

    bool load()
    {
        string light_type = "Point Light";
        if (LoadFile == null)
            Debug.LogError("Cannot open the file!");
        else
        {
            string[] lineText = LoadFile.text.Split('\n');
            int size = 0;
            objs.Clear();
            scriptPlace.Clear();
            objPlace.Clear();
            int condition = 0; // 0: init, 1: cam, 2: light, 3: prefab, 4: parameters
            for (int i = 0; i < lineText.Length; i++)
            {
                if (lineText[i].EndsWith("\r"))
                {
                    lineText[i] = lineText[i].Remove(lineText[i].Length - 1);
                }

                string[] data = lineText[i].Split(' ');
                if (data.Length == 1)
                {
                    if (data[0] == "camera")
                    {
                        condition = 1;
                        objs.Add(new GameObject());
                        size = objs.Count;
                        objs[size - 1].AddComponent<Camera>();
                        objs[size - 1].name = "Main Camera";
                        objs[size - 1].tag = "MainCamera";
                        objPlace["camera"] = size - 1;
                    }
                    else if (data[0] == "light")
                    {
                        condition = 2;
                        objs.Add(new GameObject());
                        size = objs.Count;
                        objs[size - 1].AddComponent<Light>();

                        objPlace["light"] = size - 1;
                        objs[size - 1].name = "light";
                    }
                    else if (data[0] == "prefab")
                    {
                        condition = 3;
                        objs.Add(new GameObject());
                        size = objs.Count;
                        objPlace["prefab"] = size - 1;
                    }
                    else if (data[0] == "parameters")
                    {
                        condition = 4;
                    }
                    else if (data[0].Length == 0) { }
                    else return false;
                }
                else if (data.Length > 0)
                {
                    if (data[0] == "p")
                        objs[size - 1].transform.position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                    else if (data[0] == "r")
                        objs[size - 1].transform.Rotate(new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])));
                    else if (data[0] == "s")
                        objs[size - 1].transform.localScale = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                    else if (data[0].Length == 0) { }

                    else if (condition == 1)
                    {
                        if (data[0] == "depth")
                            objs[size - 1].GetComponent<Camera>().depth = -1;
                        else if (data[0] == "c" && data[1] == "AOVCameraScript")
                        {
                            objs[size - 1].AddComponent<AOVCameraScript>();
                            scriptPlace["AOVCameraScript"] = size - 1;
                        }
                        else if (data[0] == "c" && data[1] == "SmoothMouseLook")
                        {
                            objs[size - 1].AddComponent<SmoothMouseLook>();
                            scriptPlace["SmoothMouseLook"] = size - 1;
                        }
                        else if (data[0] == "c" && data[1] == "AmplifyOcclusionEffect")
                        {
                            objs[size - 1].AddComponent<AmplifyOcclusionEffect>();
                            objs[size - 1].GetComponent<AmplifyOcclusionEffect>().enabled = false;
                            scriptPlace["AmplifyOcclusionEffect"] = size - 1;
                        }
                        else if (data[0].Length == 0) { }
                        else return false;
                    }
                    else if (condition == 2)
                    {
                        if (data[0] == "t" && int.Parse(data[1]) == 0)
                        {
                            objs[size - 1].GetComponent<Light>().type = LightType.Spot;
                            light_type = "Spot Light";
                        }
                        else if (data[0] == "t" && int.Parse(data[1]) == 1)
                        {
                            objs[size - 1].GetComponent<Light>().type = LightType.Point;
                            light_type = "Point Light";
                        }
                        else if (data[0] == "t" && int.Parse(data[1]) == 2)
                        {
                            objs[size - 1].GetComponent<Light>().type = LightType.Directional;
                            light_type = "Directional Light";
                        }

                        else if (data[0] == "range")
                            objs[size - 1].GetComponent<Light>().range = float.Parse(data[1]);
                        else if (data[0] == "color")
                            objs[size - 1].GetComponent<Light>().color = new Color(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                        else if (data[0] == "intensity")
                            objs[size - 1].GetComponent<Light>().intensity = float.Parse(data[1]);
                        else if (data[0] == "angle")
                            objs[size - 1].GetComponent<Light>().spotAngle = float.Parse(data[1]);
                        else if (data[0].Length == 0) { }
                        else return false;
                    }
                    else if (condition == 3)
                    {
                        if (data[0] == "path")
                        {
                            DestroyImmediate(objs[size - 1]);
                            objs[size - 1] = Instantiate((GameObject)AssetDatabase.LoadAssetAtPath(data[1], typeof(GameObject)));
                            objs[size - 1].name = objs[size - 1].name.Remove(objs[size - 1].name.Length - 7);
                            objs[size - 1].tag = "AOV";
                        }
                        else if (data[0].Length == 0) { }
                        else return false;
                    }
                    else if (condition == 4)
                    {
                        
                    }
                }
            }
        }
        return true;
    }
}

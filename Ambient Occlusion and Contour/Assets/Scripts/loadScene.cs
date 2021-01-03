using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class loadScene : MonoBehaviour
{
    public static TextAsset Load_File;
    public static List<GameObject> objsList = new List<GameObject>();
    public static Dictionary<string, int> scriptPlace = new Dictionary<string, int>();
    public static Dictionary<string, int> objPlace = new Dictionary<string, int>();
    
    //[MenuItem("Tool/Load Scene")]
    //public static void MenuOpen()
    //{
    //    EditorWindow.GetWindow(typeof(loadScene), false, "Load Scene", true);
    //}

    //// Update is called once per frame
    //void OnGUI()
    //{

    //    Load_File = (TextAsset)EditorGUILayout.ObjectField("LoadFile: ", Load_File, typeof(TextAsset), true);
    //    if (GUILayout.Button("Load Text"))
    //    {
    //        RenderTexture renderTex = (RenderTexture)AssetDatabase.LoadAssetAtPath("Assets/Textures/lightLook.renderTexture", typeof(RenderTexture));
    //        Cubemap cubeTex = (Cubemap)AssetDatabase.LoadAssetAtPath("Assets/Textures/CubeT.cubemap", typeof(Cubemap));
    //        if (load(Load_File))
    //        {
    //            // copy obj
    //            copyFile();
    //        }

    //    }
    //}
    public static void DefaultRendering()
    {
        GameObject SAO = GameObject.FindGameObjectWithTag("SimpleAO");
        if (SAO != null)
            SAO.GetComponent<SimpleAO>().enabled = false;
        GameObject AO = GameObject.FindGameObjectWithTag("MainCamera");
        if (AO != null)
        {
            AO.GetComponent<AmplifyOcclusionEffect>().enabled = false;
            AO.GetComponent<AOVCameraScript>().enabled = false;
        }
    }
    public static bool load(TextAsset LoadFile)
    {
        DefaultRendering();
        clear();
        string light_type = "Point Light";
        if (LoadFile == null)
            Debug.LogError("Cannot open the file!");
        else
        {
            string[] lineText = LoadFile.text.Split('\n');
            int size = 0;
            objsList.Clear();
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
                        objsList.Add(new GameObject());
                        size = objsList.Count;
                        objsList[size - 1].AddComponent<Camera>();
                        objsList[size - 1].name = "Main Camera";
                        objsList[size - 1].tag = "MainCamera";
                        objPlace["camera"] = size - 1;
                    }
                    else if (data[0] == "light")
                    {
                        condition = 2;
                        objsList.Add(new GameObject());
                        size = objsList.Count;
                        objsList[size - 1].AddComponent<Light>();

                        objPlace["light"] = size - 1;
                        objsList[size - 1].name = "light";
                    }
                    else if (data[0] == "prefab")
                    {
                        condition = 3;
                        //objsList.Add(new GameObject());
                        //size = objsList.Count;
                        objPlace["prefab"] = size;
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
                        objsList[size - 1].transform.position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                    else if (data[0] == "r")
                        objsList[size - 1].transform.Rotate(new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])));
                    else if (data[0] == "s")
                        objsList[size - 1].transform.localScale = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                    else if (data[0].Length == 0) { }

                    else if (condition == 1)
                    {
                        if (data[0] == "depth")
                            objsList[size - 1].GetComponent<Camera>().depth = -1;
                        else if (data[0] == "c" && data[1] == "AOVCameraScript")
                        {
                            objsList[size - 1].AddComponent<AOVCameraScript>();
                            objsList[size - 1].GetComponent<AOVCameraScript>().enabled = false;
                            scriptPlace["AOVCameraScript"] = size - 1;
                        }
                        else if (data[0] == "c" && data[1] == "SmoothMouseLook")
                        {
                            objsList[size - 1].AddComponent<SmoothMouseLook>();
                            scriptPlace["SmoothMouseLook"] = size - 1;
                        }
                        else if (data[0] == "c" && data[1] == "AmplifyOcclusionEffect")
                        {
                            objsList[size - 1].AddComponent<AmplifyOcclusionEffect>();
                            objsList[size - 1].GetComponent<AmplifyOcclusionEffect>().enabled = false;
                            scriptPlace["AmplifyOcclusionEffect"] = size - 1;
                        }
                        else if (data[0].Length == 0) { }
                        else return false;
                    }
                    else if (condition == 2)
                    {
                        if (data[0] == "t" && int.Parse(data[1]) == 0)
                        {
                            objsList[size - 1].GetComponent<Light>().type = LightType.Spot;
                            light_type = "Spot Light";
                        }
                        else if (data[0] == "t" && int.Parse(data[1]) == 1)
                        {
                            objsList[size - 1].GetComponent<Light>().type = LightType.Point;
                            light_type = "Point Light";
                        }
                        else if (data[0] == "t" && int.Parse(data[1]) == 2)
                        {
                            objsList[size - 1].GetComponent<Light>().type = LightType.Directional;
                            light_type = "Directional Light";
                        }

                        else if (data[0] == "range")
                            objsList[size - 1].GetComponent<Light>().range = float.Parse(data[1]);
                        else if (data[0] == "color")
                            objsList[size - 1].GetComponent<Light>().color = new Color(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
                        else if (data[0] == "intensity")
                            objsList[size - 1].GetComponent<Light>().intensity = float.Parse(data[1]);
                        else if (data[0] == "angle")
                            objsList[size - 1].GetComponent<Light>().spotAngle = float.Parse(data[1]);
                        else if (data[0].Length == 0) { }
                        else return false;
                    }
                    else if (condition == 3)
                    {
                        if (data[0] == "path")
                        {
                            //DestroyImmediate(objsList[size - 1]);
                            objsList.Add(Instantiate((GameObject)AssetDatabase.LoadAssetAtPath(data[1], typeof(GameObject))));
                            size = objsList.Count;
                            objsList[size - 1].name = objsList[size - 1].name.Remove(objsList[size - 1].name.Length - 7);
                            objsList[size - 1].tag = "AOV";
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
        copyFile();
        return true;
    }
    public static void copyFile()
    {
        objsList.Add(new GameObject());
        objPlace["PassPrefab"] = objsList.Count - 1;
        DestroyImmediate(objsList[objPlace["PassPrefab"]]);
        objsList[objPlace["PassPrefab"]] = Instantiate(objsList[objPlace["prefab"]]);
        objsList[objsList.Count - 1].name = "PassPrefab";
        objsList[objsList.Count - 1].tag = "Silhouette";
        Material silhoutte = new Material(Shader.Find("Outlined/Silhouette Only"));
        if (objsList[objPlace["PassPrefab"]].GetComponentInChildren<MeshRenderer>() != null)
        {
            MeshRenderer[] renderers = objsList[objPlace["PassPrefab"]].GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = silhoutte;
            }
        }
        
    }
    public static void clear()
    {
        scriptPlace.Clear();
        objPlace.Clear();
        for(int i=0;i<objsList.Count;i++)
        {
            DestroyImmediate(objsList[i]);
        }
        objsList.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class UISetting : MonoBehaviour
{
    public RawImage menu;
    public Slider maxObSlider;
    public Text maxObText;
    public Slider falloffSlider;
    public Text falloffText;
    public Slider debugMixSlider;
    public Text debugMixText;
    public Toggle debugShow;
    public Toggle Silhouette;
    bool debugstate = false;
    bool silhouetteState = false;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 size = menu.rectTransform.sizeDelta / 2;
        menu.rectTransform.anchoredPosition = new Vector2(Screen.width - size.x, Screen.height - size.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Canvas>().enabled)
        {
            // maxOb value
            if(Mathf.Abs(maxObSlider.value - float.Parse(maxObText.text)) > 0.001)
            {
                maxObText.text = maxObSlider.value.ToString();
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                if(camera.GetComponent<AOVCameraScript>() != null)
                {
                    camera.GetComponent<AOVCameraScript>().maxObscuranceDistance = maxObSlider.value;
                }
            }
            // falloff value
            if (Mathf.Abs(falloffSlider.value - float.Parse(falloffText.text)) > 0.001)
            {
                falloffText.text = falloffSlider.value.ToString();
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                if (camera.GetComponent<AOVCameraScript>() != null)
                {
                    camera.GetComponent<AOVCameraScript>().falloffExponent = falloffSlider.value;
                }
            }
            // debugMix value
            if (Mathf.Abs(debugMixSlider.value - float.Parse(debugMixText.text)) > 0.001)
            {
                debugMixText.text = debugMixSlider.value.ToString();
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                if (camera.GetComponent<AOVCameraScript>() != null)
                {
                    camera.GetComponent<AOVCameraScript>().debugMix = debugMixSlider.value;
                }
            }
            if(debugShow.isOn != debugstate)
            {
                debugstate = debugShow.isOn;
                GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                if (camera.GetComponent<AOVCameraScript>() != null)
                {
                    camera.GetComponent<AOVCameraScript>().debugShow = debugShow.isOn;
                }
            }
            if (Silhouette.isOn != silhouetteState)
            {
                silhouetteState = Silhouette.isOn;
                GameObject passPrefab = GameObject.FindGameObjectWithTag("Silhouette");
                if (passPrefab != null)
                {
                    Transform[] obj = passPrefab.GetComponentsInChildren<Transform>();
                    for(int i=1;i<obj.Length;i++)
                    {
                        obj[i].gameObject.GetComponent<MeshRenderer>().enabled = (silhouetteState);
                    }
                }
            }
        }
    }

    public void onClick()
    {
        string path = EditorUtility.OpenFilePanel("Load Scene", "", "txt");
        path = path.Replace('\\', '/');
        path = FileUtil.GetProjectRelativePath(path);
        Debug.Log(path);
        TextAsset settingfile = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
        loadScene.load(settingfile);
    }
}

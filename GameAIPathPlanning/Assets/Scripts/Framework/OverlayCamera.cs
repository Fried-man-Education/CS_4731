using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OverlayCamera : MonoBehaviour
{
    Camera cam;
    public Material Material { get; private set; }

    //public Material hackMaterial;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if(cam == null)
        {
            Debug.Log("No camera!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(cam.targetTexture != null)
        {
            ////TODO temp hack
            //Material = hackMaterial;
            //return;

            cam.targetTexture.Release();
            cam.targetTexture = null;
        }

        //cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGB32);
        cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);

        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        //Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        //Material mat = new Material(Shader.Find("Unlit/Transparent Cutout"));
        mat.SetTexture("_MainTex", cam.targetTexture);
        Material = mat;

        //Disable to allow manual calls to cam.Render() as needed
        //cam.enabled = false;

        this.gameObject.SetActive(false);
    }


}

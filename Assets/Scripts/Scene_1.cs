using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_1 : MonoBehaviour
{
    private bool isLoad = false;

    private GameObject gameObjectCubeRed;
    private GameObject gameObjectCubeBlue;
    private GameObject gameObjectCubeGreen;

    private Action action; 

    private IEnumerator Start()
    {
        yield return AssetBundleManager.Instance.CoLoadAssetBundleAll<Material>("Materials");

        Debug.Log("Bundle Load Finished.");

        isLoad = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (gameObjectCubeRed != null)
                gameObjectCubeRed = null;

            //StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Red.prefab", (GameObject x) => { gameObjectCubeRed = x; }));

            StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Red.prefab", (GameObject x) => { SetCubeRed(x); }));
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (gameObjectCubeBlue != null)
                gameObjectCubeBlue = null;

            //StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Blue.prefab", (GameObject x) => { gameObjectCubeBlue = x; }));

            StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Blue.prefab", (GameObject x) => { SetCubeBlue(x); }));
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (gameObjectCubeGreen != null)
                gameObjectCubeGreen = null;

            //StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Green.prefab", (GameObject x) => { gameObjectCubeGreen = x; }));

            StartCoroutine(AssetBundleManager.Instance.CoInstantiateAssetBundle("Assets/AssetBundle/Cube_Green.prefab", (GameObject x) => { SetCubeGreen(x); }));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void SetCubeRed(GameObject gameObject)
    {
        gameObjectCubeRed = gameObject;

        gameObjectCubeRed.transform.position = Vector3.zero;
    }

    private void SetCubeBlue(GameObject gameObject)
    {
        gameObjectCubeBlue = gameObject;

        gameObjectCubeBlue.transform.position -= new Vector3(5.0f, 0.0f, 0.0f);
    }

    private void SetCubeGreen(GameObject gameObject)
    {
        gameObjectCubeGreen = gameObject;

        gameObjectCubeGreen.transform.position -= new Vector3(-5.0f, 0.0f, 0.0f);
    }
}
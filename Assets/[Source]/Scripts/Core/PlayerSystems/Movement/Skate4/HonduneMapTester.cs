using System;
using System.IO;
using UnityEngine;

public class HonduneMapTester : MonoBehaviour
{
	private bool showUI = true;

	private bool disableUI;

	private Vector2 scrollPosition = Vector2.zero;

	private string[] assetFiles;

	private GameObject prefab;

	public Material splineMat;

	public HonduneMapTester()
	{
	}

	private void LoadAssetBundle(int selection)
	{
		MonoBehaviour.print(string.Concat("Loading ", this.assetFiles[selection]));
		AssetBundle assetBundle = AssetBundle.LoadFromFile(this.assetFiles[selection]);
		if (assetBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!");
			return;
		}
		Application.LoadLevel(Path.GetFileNameWithoutExtension(assetBundle.GetAllScenePaths()[0]));
		base.Invoke("SetUpGrinds", 1f);
	}

	private void OnGUI()
	{
		if (Input.GetKeyDown("m"))
		{
			this.showUI = true;
		}
		if (!this.showUI)
		{
			return;
		}
		GUI.Box(new Rect(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y, 10f, 10f), "");
		GUIStyle gUIStyle = new GUIStyle()
		{
			fontSize = 35
		};
		GUI.Label(new Rect((float)(Screen.width / 3), 0f, (float)(Screen.width / 3), 100f), "Hondune's Skater XL Map Importer", gUIStyle);
		GUI.Label(new Rect((float)(Screen.width / 3), 50f, (float)(Screen.width / 3), 100f), "Brought to you by hondune.com");
		if (this.assetFiles == null)
		{
			return;
		}
		this.scrollPosition = GUI.BeginScrollView(new Rect((float)(Screen.width / 3), 100f, (float)(Screen.width / 3), (float)(Screen.height - 200)), this.scrollPosition, new Rect(0f, 0f, (float)(Screen.width / 3 - 20), (float)(25 * (int)this.assetFiles.Length)));
		int num = 0;
		for (int i = 0; i < (int)this.assetFiles.Length; i++)
		{
			if (!Path.GetFileName(this.assetFiles[i]).Contains(".") && Path.GetFileName(this.assetFiles[i]) != "AssetBundles")
			{
				if (GUI.Button(new Rect(10f, (float)(25 * num), (float)(Screen.width / 3 - 30), 20f), string.Concat("Load ", Path.GetFileName(this.assetFiles[i]))))
				{
					this.LoadAssetBundle(i);
					this.showUI = false;
				}
				num++;
			}
		}
		GUI.EndScrollView();
		if (num == 0)
		{
			GUI.Label(new Rect((float)(Screen.width / 3), 100f, (float)(Screen.width / 3), 100f), "No bundles found in AssetBundles folder!");
		}
	}

	private void SetUpGrinds()
	{
		this.prefab = GameObject.Find("Grinds");
		if (this.prefab == null)
		{
			if (this.prefab == null)
			{
				base.Invoke("SetUpGrinds", 0.5f);
			}
			return;
		}
		Transform gameObject = (new GameObject("Grind Triggers And Splines")).transform;
		Transform[] componentsInChildren = this.prefab.GetComponentsInChildren<Transform>();
		for (int i = 0; i < (int)componentsInChildren.Length; i++)
		{
			Transform transforms = componentsInChildren[i];
			if (transforms.name.Contains("GrindSpline"))
			{
				Transform gameObject1 = (new GameObject(string.Concat(transforms.name, "SplineDisplay"))).transform;
				gameObject1.parent = gameObject;
				Vector3[] child = new Vector3[transforms.childCount];
				for (int j = 0; j < (int)child.Length; j++)
				{
					child[j] = transforms.GetChild(j).position;
				}
				for (int k = 0; k < (int)child.Length - 1; k++)
				{
					GameObject gameObject2 = new GameObject(string.Concat("RailCol", k))
					{
						layer = 12
					};
					gameObject2.transform.position = child[k];
					gameObject2.transform.LookAt(child[k + 1]);
					float single = Vector3.Distance(child[k], child[k + 1]);
					GameObject vector3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
					vector3.transform.parent = gameObject2.transform;
					vector3.transform.localRotation = Quaternion.identity;
					vector3.transform.localScale = new Vector3(0.08f, 0.08f, single);
					vector3.transform.localPosition = (Vector3.forward * single) / 2f;
					vector3.GetComponent<Renderer>().material = this.splineMat;
					gameObject2.transform.parent = gameObject1;
					Debug.DrawLine(child[k], child[k + 1], Color.red);
				}
			}
		}
	}

	private void Start()
	{
		this.splineMat.SetFloat("_BlendMode", 1f);
		this.splineMat.SetFloat("_SurfaceType", 1f);
		this.splineMat.SetColor("_UnlitColor", new Color(1f, 0f, 0f, 1f));
		this.splineMat.enableInstancing = true;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		this.assetFiles = Directory.GetFiles(string.Concat(Application.dataPath, "\\AssetBundles"));
	}
}
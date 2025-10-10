using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelManager
{
	public static readonly string[] DontClosePanel = new string[] { "StageLoadingPanel" , "TipPanel"}; // 这两个面板不会被关闭和销毁
	//Layer
	public enum Layer
	{
		Panel,
		Tip,
	}
	//层级列表， 在这里层级只有panel 和 tip
	private static Dictionary<Layer, Transform> layers = new Dictionary<Layer, Transform>();
	//面板列表
	public static Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
	//结构
	public static Transform root;
	public static Transform canvas;
	//初始化
	public static void Init()
	{	
		// 找到场景中的root， root下有canvas， canvas下有panel和tip
		root = GameObject.Find("Root").transform;
		canvas = root.Find("Canvas"); 
		Transform panel = canvas.Find("Panel");
		Transform tip = canvas.Find("Tip");
		layers[Layer.Panel] = panel;
		layers[Layer.Tip] = tip;
		// layers.Add(Layer.Panel, panel);
		// layers.Add(Layer.Tip, tip);
		GameObject.DontDestroyOnLoad(root);
	}

	//打开面板
	public static void Open<T>(params object[] para) where T : BasePanel
	{
		//已经打开
		string name = typeof(T).ToString();
		Debug.Log(">>>>>>>>>>>>>>>>>>>>>> openUI: " + name);
		if (panels.ContainsKey(name))
		{
			return;
		}
		//组件
		BasePanel panel = root.gameObject.AddComponent<T>();
		panel.OnInit();
		panel.Init();
		//父容器
		Transform layer = layers[panel.layer];
		panel.skin.transform.SetParent(layer, false);
		//列表
		panels.Add(name, panel);
		//OnShow
		panel.OnShow(para);
	}

	// 获取面板
	public static T Get<T>() where T : BasePanel
	{
		string name = typeof(T).ToString();
		if (panels.ContainsKey(name))
		{
			return panels[name] as T;
		}
		else
		{
			Debug.LogError("Panel not found: " + name);
			return null;
		}
	}

	public static void Close<T>() where T : BasePanel
	{
		string name = typeof(T).ToString();
		Close(name);
	}

	//关闭面板
	public static void Close(string name)
	{
		//没有打开
		if (!panels.ContainsKey(name))
		{
			return;
		}
		BasePanel panel = panels[name];

		//OnClose
		panel.OnClose();
		//列表
		panels.Remove(name);
		//销毁
		GameObject.Destroy(panel.skin);
		Component.Destroy(panel);
	}

	public static void Clear()
	{
		HashSet<string> dontCloseSet = new HashSet<string>(DontClosePanel);
		foreach (string name in new List<string>(panels.Keys))
		{
			if (dontCloseSet.Contains(name))
			{
				continue;
			}

			Close(name);
		}
	}
}

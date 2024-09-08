using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PanelManager
{
    public enum Layer
    {
        Panel,
        Tip
    }

    private static Dictionary<Layer, Transform> layers = new Dictionary<Layer, Transform>();
    private static Dictionary<string, PanelBase> panels = new Dictionary<string, PanelBase>();
    private static Transform root;
    private static Transform canvas;

    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas").transform;
        layers.Add(Layer.Panel, canvas.Find("Panel"));
        layers.Add(Layer.Tip, canvas.Find("Tip"));
    }

    public static void Open<T>(params object[] objects) where T : PanelBase
    {
        string panelName = typeof(T).ToString();
        if (panels.ContainsKey(panelName)) return;
        PanelBase newPanel = root.AddComponent<T>();
        newPanel.OnInit();
        newPanel.Init();

        Transform layer = layers[newPanel.layer];
        newPanel.resGO.transform.SetParent(layer);
        newPanel.resGO.transform.localPosition = Vector3.zero;
        panels.Add(panelName, newPanel);
        newPanel.OnShow(objects);
    }
    
    public static void Update<T>() where T : PanelBase
    {
        string panelName = typeof(T).ToString();
        if (panels.TryGetValue(panelName, out PanelBase panel))
        {
            panel.OnUpdate();
        }
    }

    public static void Close(string name)
    {
        if (!panels.ContainsKey(name)) return;
        PanelBase panel = panels[name];
        panel.OnClose();
        panels.Remove(name);
        GameObject.Destroy(panel.resGO);
        GameObject.Destroy(panel);
    }
}

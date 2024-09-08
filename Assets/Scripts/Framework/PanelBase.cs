using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PanelBase : MonoBehaviour
{
    public readonly string RES_PREFABS = "Prefabs/";
    
    protected string resPath;

    public GameObject resGO;
    public PanelManager.Layer layer = PanelManager.Layer.Panel;

    public void Init()
    {
        resGO = Instantiate(Resources.Load<GameObject>(resPath));
        resGO.transform.localPosition = Vector3.zero;
    }

    public virtual void OnInit()
    {
        
    }

    public virtual void OnShow(params object[] objects)
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }

    public virtual void OnClose()
    {
        
    }

    public void Close()
    {
        string name = GetType().ToString();
        PanelManager.Close(name);
    }
}

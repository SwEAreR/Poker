using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TipPanel : PanelBase
{
    private const string SCRIPTNAME = "TipPanel";
    
    private Button Raycast;
    private Transform TipBg;
    private Text TipText;

    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Tip;
    }

    public override void OnShow(params object[] objects)
    {
        //Raycast = resGO.transform.Find("Raycast").GetComponent<Button>();
        TipBg = resGO.transform.Find("TipBg");
        TipText = TipBg.Find("TipText").GetComponent<Text>();

       // Raycast.onClick.AddListener(OnRaycastClick);

        if (objects.Length >= 1)
        {
            TipText.text = objects[0].ToString();
        }
        Fading();
    }

    private void OnRaycastClick()
    {
        //Close();
    }


    public override void OnClose()
    {
        base.OnClose();
    }
    

    public void Fading()
    {
        resGO.transform.localPosition = Vector3.zero;
        
        DOTween.To(() => TipBg.localPosition,
            it => TipBg.localPosition = it, TipBg.localPosition+Vector3.up * 100, 2f);
        DOTween.To(() => TipBg.transform.GetComponent<CanvasGroup>().alpha,
            it => TipBg.transform.GetComponent<CanvasGroup>().alpha = it, 0, 2f).onComplete = () =>
        {
            Destroy(resGO);
        };

    }
}

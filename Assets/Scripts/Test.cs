// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Text;
// using UnityEditor;
// using UnityEngine;
//
// public class Test : ScriptableWizard
// {
//     public List<int> ranks = new List<int>();
//     public List<Card> cards = new List<Card>();
//     [MenuItem("ScriptableWizard/Test")]
//     public static void CreateWizard()
//     {
//         DisplayWizard<Test>("MyTestMenu", "确定","应用");
//     }
//     void Start()
//     {
//         // string ip = "127.0.0.1";
//         // int port = 8888;
//         // NetManager.SocketConnect(ip, port);
//         //
//         // PanelManager.Init();
//         // PanelManager.Open<TipPanel>("Hello World!");
//     }
//
//     private void Update()
//     {
//         // NetManager.NetManagerUpdate();
//     }
//
//     private void OnWizardCreate()
//     {
//         cards.Clear();
//         foreach (int rank in ranks)
//         {
//             cards.Add(new Card(1, rank));
//         }
//         Debug.Log(CardManager.GetCardType(cards));
//     }
//
//     private void OnWizardOtherButton()
//     {
//         cards.Clear();
//         foreach (int rank in ranks)
//         {
//             cards.Add(new Card(1, rank));
//         }
//         Debug.Log(CardManager.GetCardType(cards));
//     }
// }

using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }
    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }
}
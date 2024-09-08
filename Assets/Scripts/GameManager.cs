using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string id = "";
    public static bool isHost;
    public static PlayerState playerState = PlayerState.None;

    public static List<Card> cards = new List<Card>();
    public static List<Card> headCards = new List<Card>();

    public static string LHO = "";
    public static string RHO = "";
    
    public static int LHOBean;
    public static int RHOBean;
    public static int MyselfBean;

    public static GameObject LHOobj;
    public static GameObject RHOobj;
    public static GameObject Myselfobj;

    public static string landLordId = "";

    public static bool isPressing;
    public static List<Card> selectedCards = new List<Card>();

    public static Animator character0;  // 地主
    public static Animator character1;
    public static Animator character2;
    public static Animator character3;

    public enum PlayerState
    {
        None,
        Call,
        GiveUp,
        Play
    }
    
    private void Start()
    {
        NetManager.AddMessageListener("MessageKick", OnMessageKick);
        NetManager.AddStateEvent(NetManager.NetEvent.Close, OnConnectClose);
        PanelManager.Init();
        PanelManager.Open<LoginPanel>();
        
        CardManager.Init();

        character0 = GameObject.Find("CharacterCamera0").transform.Find("character").GetComponent<Animator>();
        character1 = GameObject.Find("CharacterCamera1").transform.Find("character").GetComponent<Animator>();
        character2 = GameObject.Find("CharacterCamera2").transform.Find("character").GetComponent<Animator>();
        character3 = GameObject.Find("CharacterCamera3").transform.Find("character").GetComponent<Animator>();
    }

    private void Update()
    {
        NetManager.NetManagerUpdate();
    }

    public void OnMessageKick(MessageBase messageBase)
    {
        PanelManager.Open<TipPanel>("当前账号在另一端上线");
    }

    public void OnConnectClose(string str)
    {
        PanelManager.Open<TipPanel>("断开连接");
    }

    public static void ShowSyncMessage(string id, string path)
    {
        GameObject go = Resources.Load<GameObject>(path);
        ClearSyncMessage(id);
        Transform MsgTrs = null;
        if (LHO == id)
        {
            MsgTrs = LHOobj.transform.Find("Message");
        }
        else if (RHO == id)
        {
            MsgTrs = RHOobj.transform.Find("Message");
        }
        else if (GameManager.id == id)
        {
            MsgTrs = Myselfobj.transform.Find("Message");
        }
        if (MsgTrs != null)
            Instantiate(go, MsgTrs);
    }

    public static void ClearSyncMessage(string id)
    {
        Transform MsgTrs = null;
        if (LHO == id)
        {
            MsgTrs = LHOobj.transform.Find("Message");
            
        }
        else if (RHO == id)
        {
            MsgTrs = RHOobj.transform.Find("Message");
        }
        else if (GameManager.id == id)
        {
            MsgTrs = Myselfobj.transform.Find("Message");
        }
        if (MsgTrs != null)
            for (int i = 0; i < MsgTrs.childCount; i++)
                Destroy(MsgTrs.GetChild(i).gameObject);
    }
    
    public static void ShowSyncCards(string id, Card[] cards)
    {
        ClearSyncCards(id);
        for (int i = 0; i < cards.Length - 1; i++)
        {
            for (int j = i + 1; j < cards.Length; j++)
            {
                if (cards[i].cardRank > cards[j].cardRank)
                {
                    (cards[i], cards[j]) = (cards[j], cards[i]);
                }
                else if (cards[i].cardRank == cards[j].cardRank && cards[i].cardSuit < cards[j].cardSuit)
                {
                    (cards[i], cards[j]) = (cards[j], cards[i]);
                }
            }
        }
        Transform CardsTrs = null;
        if (LHO == id)
        {
            CardsTrs = LHOobj.transform.Find("Cards");
        }
        else if (RHO == id)
        {
            CardsTrs = RHOobj.transform.Find("Cards");
        }
        else if (GameManager.id == id)
        {
            CardsTrs = Myselfobj.transform.Find("ShowCards");
        }
        GenerateCard(cards, CardsTrs, 0.8f);
    }

    public static void ClearSyncCards(string id)
    {
        Transform CardsTrs = null;
        if (LHO == id)
        {
            CardsTrs = LHOobj.transform.Find("Cards");
            
        }
        else if (RHO == id)
        {
            CardsTrs = RHOobj.transform.Find("Cards");
        }
        else if (GameManager.id == id)
        {
            CardsTrs = Myselfobj.transform.Find("ShowCards");
        }
        if (CardsTrs != null)
            for (int i = 1; i < CardsTrs.childCount; i++)
                Destroy(CardsTrs.GetChild(i).gameObject);
    }
    
    public static void GenerateCard(Card[] cards, Transform transform, float scale = 1)
    {
        GameObject Card = transform.Find("Card").gameObject;
        if (Card == null) return;
        for (int i = 0; i < cards.Length; i++)
        {
            string name = CardManager.GetName(cards[i]);
            Transform newCard = GameObject.Instantiate(Card, transform).transform;
            newCard.gameObject.SetActive(true);
            newCard.name = name;
            UnityEngine.UI.Image img = newCard.GetComponent<UnityEngine.UI.Image>();
            // Sprite sprite = await ExtensionsResources`.LoadResourcesAsync<Sprite>("Card/" + name);
            Sprite sprite = Resources.Load<Sprite>(ResourcesPath.Card + name);
            img.sprite = sprite;
            img.transform.localScale = Vector3.one * scale;

            newCard.AddComponent<CardUI>();
        }
    }
    
    public static void DestroyCards()
    {
        Transform trs = Myselfobj.transform.Find("Cards");
        for (int i = 1; i < trs.childCount; i++)
        {
            Destroy(trs.GetChild(i).gameObject);
        }
    }
}

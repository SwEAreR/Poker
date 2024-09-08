using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PokerPanel : PanelBase
{
    private readonly string SCRIPTNAME = "PokerPanel";
    private Transform Myself;
    private Text UserName;
    private Text BeanCount;
    private GameObject Card;
    private Transform HeadCards;

    private Transform PickRole;
    private Button CallBtn;
    private Button RobBtn;
    private Button NotRob;
    private Button NotCall;
    
    private Transform Play;
    private Button ShowBtn;
    private Button PassBtn;

    private int LHOCardNum;
    private int RHOCardNum;

    private Transform Mask;
    
    private static readonly int IsPlay = Animator.StringToHash("isPlay");

    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] objects)
    {
        Myself = resGO.transform.Find("Myself");
        Card = Myself.Find("Cards/Card").gameObject;
        HeadCards = resGO.transform.Find("HeadCards");

        PickRole = resGO.transform.Find("Buttons/PickRole");
        CallBtn = PickRole.Find("CallBtn").GetComponent<Button>();
        RobBtn = PickRole.Find("RobBtn").GetComponent<Button>();
        NotRob = PickRole.Find("NotRob").GetComponent<Button>();
        NotCall = PickRole.Find("NotCall").GetComponent<Button>();
        
        Play = resGO.transform.Find("Buttons/Play");
        ShowBtn = Play.Find("ShowBtn").GetComponent<Button>();
        PassBtn = Play.Find("PassBtn").GetComponent<Button>();

        Mask = resGO.transform.Find("Mask");
        
        GameManager.LHOobj = resGO.transform.Find("LHO").gameObject;
        GameManager.RHOobj = resGO.transform.Find("RHO").gameObject;
        GameManager.Myselfobj = resGO.transform.Find("Myself").gameObject;
        
        CallBtn.onClick.AddListener(OnCallBtnClick);
        RobBtn.onClick.AddListener(OnRobBtnClick);
        NotRob.onClick.AddListener(OnNotRobBtnClick);
        NotCall.onClick.AddListener(OnNotCallBtnClick);
        ShowBtn.onClick.AddListener(OnShowBtnClick);
        PassBtn.onClick.AddListener(OnPassBtnClick);
        Mask.Find("ReturnBtn").GetComponent<Button>().onClick.AddListener(OnReturnRoom);
        
        NetManager.AddMessageListener("MessageCardList", OnMessageCardList);
        NetManager.AddMessageListener("MessageGetStartPlayer", OnMessageGetStartPlayer);
        NetManager.AddMessageListener("MessageSwitchPickUp", OnMessageSwitchPickUp);
        NetManager.AddMessageListener("MessageRoomPlayer", OnMessageRoomPlayer);
        NetManager.AddMessageListener("MessagePokerCall", OnMessagePokerCall);
        NetManager.AddMessageListener("MessagePokerRestart", OnMessagePokerRestart);
        NetManager.AddMessageListener("MessageShowCards", OnMessageShowCards);
        NetManager.AddMessageListener("MessagePokerResult", OnMessagePokerResult);
        
        NetManager.Send(new MessageCardList());
        NetManager.Send(new MessageGetStartPlayer());
        NetManager.Send(new MessageRoomPlayer());
        
        AudioManager.Instance.Play_BGM(AudioManager.BGMType.Normal);
    }

    private void InitPlayerInfo()
    {
        GameManager.Myselfobj.transform.Find("UserName").GetComponent<Text>().text = GameManager.id;
        GameManager.LHOobj.transform.Find("UserName").GetComponent<Text>().text = GameManager.LHO;
        GameManager.RHOobj.transform.Find("UserName").GetComponent<Text>().text = GameManager.RHO;
        
        GameManager.Myselfobj.transform.Find("BeanImg/BeanNum").GetComponent<Text>().text = GameManager.MyselfBean.ToString();
        GameManager.LHOobj.transform.Find("BeanImg/BeanNum").GetComponent<Text>().text = GameManager.LHOBean.ToString();
        GameManager.RHOobj.transform.Find("BeanImg/BeanNum").GetComponent<Text>().text = GameManager.RHOBean.ToString();
    }

    private void OnCallBtnClick()
    {
        GameManager.playerState = GameManager.PlayerState.Call;
        MessagePokerCall msg = new MessagePokerCall()
        {
            call = true
        };
        NetManager.Send(msg);
    }
    
    private void OnRobBtnClick()
    {
        GameManager.playerState = GameManager.PlayerState.Call;
        MessagePokerCall msg = new MessagePokerCall()
        {
            call = true
        };
        NetManager.Send(msg);
    }

    private void OnNotRobBtnClick()
    {
        GameManager.playerState = GameManager.PlayerState.GiveUp;
        MessagePokerCall msg = new MessagePokerCall()
        {
            call = false
        };
        NetManager.Send(msg);
    }

    private void OnNotCallBtnClick()
    {
        GameManager.playerState = GameManager.PlayerState.GiveUp;
        MessagePokerCall msg = new MessagePokerCall()
        {
            call = false
        };
        NetManager.Send(msg);
    }
    
    private void OnShowBtnClick()
    {
        if (GameManager.selectedCards.Count == 0)
        {
            PanelManager.Open<TipPanel>("请出牌！");
            return;
        }
        MessageShowCards msg = new MessageShowCards()
        {
            cards = CardManager.CardToData(GameManager.selectedCards).ToArray()
        };
        NetManager.Send(msg);
        // GameManager.selectedCards.Clear();
    }
    
    private void OnPassBtnClick()
    {
        MessageShowCards msg = new MessageShowCards();
        NetManager.Send(msg);
        // InitCardPosition();
    }

    public override void OnClose()
    {
        NetManager.RemoveMessageListener("MessageCardList", OnMessageCardList);
        NetManager.RemoveMessageListener("MessageGetStartPlayer", OnMessageGetStartPlayer);
        NetManager.RemoveMessageListener("MessageSwitchPickUp", OnMessageSwitchPickUp);
        NetManager.RemoveMessageListener("MessageRoomPlayer", OnMessageRoomPlayer);
        NetManager.RemoveMessageListener("MessagePokerCall", OnMessagePokerCall);
        NetManager.RemoveMessageListener("MessagePokerRestart", OnMessagePokerRestart);
        NetManager.RemoveMessageListener("MessageShowCards", OnMessageShowCards);
        NetManager.RemoveMessageListener("MessagePokerResult", OnMessagePokerResult);


    }
    
    public void OnMessageCardList(MessageBase messageBase)
    {
        MessageCardList msg = messageBase as MessageCardList;
        for (int i = 0; i < msg.cardData.Length; i++)
        {
            Card card = new Card(msg.cardData[i].cardSuit, msg.cardData[i].cardRank);
            GameManager.cards.Add(card);
        }

        for (int i = 0; i < msg.headCardData.Length; i++)
        {
            Card card = new Card(msg.headCardData[i].cardSuit, msg.headCardData[i].cardRank);
            GameManager.headCards.Add(card);
        }

        GameManager.GenerateCard(GameManager.cards.ToArray(), Card.transform.parent);
        SortCard();

        RHOCardNum = LHOCardNum = GameManager.cards.Count;
        SetPlayerCardNum();
    }

   
    private void SortCard()
    {
        Transform cardParentTrs = Card.transform.parent;
        for (int i = 2; i < cardParentTrs.childCount; i++)
        {
            int currentRank = (int)CardManager.GetCard(cardParentTrs.GetChild(i).name).cardRank;
            int currentSuit = (int)CardManager.GetCard(cardParentTrs.GetChild(i).name).cardSuit;
            for (int j = 1; j < i; j++)
            {
                int rank = (int)CardManager.GetCard(cardParentTrs.GetChild(j).name).cardRank;
                int suit = (int)CardManager.GetCard(cardParentTrs.GetChild(j).name).cardSuit;
                if (currentRank > rank)
                {
                    cardParentTrs.GetChild(i).SetSiblingIndex(j);
                    break;
                }
                else if (currentRank == rank && currentSuit > suit)
                {
                    cardParentTrs.GetChild(i).SetSiblingIndex(j);
                    break;
                }
            }
        }
    }

    private void OnMessageGetStartPlayer(MessageBase messagebase)
    {
        MessageGetStartPlayer msg = messagebase as MessageGetStartPlayer;
        if (msg.id != GameManager.id)
        {
            GameManager.ShowSyncMessage(msg.id, ResourcesPath.Message_Clock);
            return;
        }
        CallBtn.gameObject.SetActive(true);
        NotCall.gameObject.SetActive(true);
    }

    private void OnMessageRoomPlayer(MessageBase messagebase)
    {
        MessageRoomPlayer msg = messagebase as MessageRoomPlayer;
        GameManager.MyselfBean = msg.MyselfBean;
        GameManager.RHO = msg.RHO;
        GameManager.RHOBean = msg.RHOBean;
        GameManager.LHO = msg.LHO;
        GameManager.LHOBean = msg.LHOBean;
        InitPlayerInfo();
    }
    
    private void OnMessageSwitchPickUp(MessageBase messagebase)
    {
        MessageSwitchPickUp msg = messagebase as MessageSwitchPickUp;

        for (int i = 0; i < PickRole.childCount; i++)
        {
            PickRole.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < Play.childCount; i++)
        {
            Play.GetChild(i).gameObject.SetActive(false);
        }
        
        GameManager.ClearSyncMessage(msg.id);
        
        if (msg.id != GameManager.id)
        {
            GameManager.ShowSyncMessage(msg.id, ResourcesPath.Message_Clock);
            GameManager.ClearSyncCards(msg.id);
            return;
        }
        
        switch (GameManager.playerState)
        {
            case GameManager.PlayerState.Call:
                RobBtn.gameObject.SetActive(true);
                NotRob.gameObject.SetActive(true);
                break;
            case GameManager.PlayerState.GiveUp:
                MessagePokerCall messagePokerCall = new MessagePokerCall()
                {
                    call = false
                };
                NetManager.Send(messagePokerCall);
                break;
            case GameManager.PlayerState.None:
                if (msg.isCall && GameManager.landLordId == "")
                {
                    RobBtn.gameObject.SetActive(true);
                    NotRob.gameObject.SetActive(true);
                }
                else
                {
                    CallBtn.gameObject.SetActive(true);
                    NotCall.gameObject.SetActive(true);
                }
                break;
            case GameManager.PlayerState.Play:
                ShowBtn.gameObject.SetActive(true);
                PassBtn.gameObject.SetActive(msg.canPass);
                GameManager.ClearSyncCards(GameManager.id);
                break;
        }
    }
    
    private void OnMessagePokerCall(MessageBase messagebase)
    {
        MessagePokerCall msg = messagebase as MessagePokerCall;
        GameManager.ClearSyncMessage(msg.id);

        switch (msg.result)
        {
            case 0:
                GameManager.ShowSyncMessage(msg.id, ResourcesPath.Message_Call);
                if (msg.id == GameManager.id)
                    NetManager.Send(new MessageSwitchPickUp());
                AudioManager.Instance.PlaySound_Call();
                break;
            case 1:
                //rob
                GameManager.ShowSyncMessage(msg.id,  ResourcesPath.Message_Rob);
                if (msg.id == GameManager.id)
                    NetManager.Send(new MessageSwitchPickUp());
                AudioManager.Instance.PlaySound_Rob();
                break;
            case 2:
                if (GameManager.isHost)
                    NetManager.Send(new MessagePokerRestart());
                break;
            case 3:
                BecomeLandLord(msg.id);
                ShowHeadCards();
                GameManager.playerState = GameManager.PlayerState.Play;
                if (msg.id == GameManager.id)
                    NetManager.Send(new MessageSwitchPickUp());
                break;
            case 4:
                GameManager.ShowSyncMessage(msg.id,  ResourcesPath.Message_NotCall);
                if (msg.id == GameManager.id)
                    NetManager.Send(new MessageSwitchPickUp());
                AudioManager.Instance.PlaySound_NotCall();
                break;
            case 5:
                GameManager.ShowSyncMessage(msg.id,  ResourcesPath.Message_NotRob);
                if (msg.id == GameManager.id)
                    NetManager.Send(new MessageSwitchPickUp());
                AudioManager.Instance.PlaySound_Pass();
                break;
        }
    }
    
    private void OnMessagePokerRestart(MessageBase messagebase)
    {
        MessagePokerRestart msg = messagebase as MessagePokerRestart;
        GameManager.ClearSyncMessage(GameManager.id);
        GameManager.ClearSyncMessage(GameManager.LHO);
        GameManager.ClearSyncMessage(GameManager.RHO);
        for (int i = 0; i < PickRole.childCount; i++)
        {
            PickRole.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < Card.transform.parent.childCount; i++)
        {
            GameObject go = Card.transform.parent.GetChild(i).gameObject;
            if (go == Card) continue;
            Destroy(go);
        }

        GameManager.playerState = GameManager.PlayerState.None;
        GameManager.cards.Clear();
        GameManager.headCards.Clear();
        NetManager.Send(new MessageCardList());
        NetManager.Send(new MessageGetStartPlayer());
        
        Mask.gameObject.SetActive(false);
        Transform trs = GameManager.Myselfobj.transform.Find("ShowCards");
        for (int i = 1; i < trs.childCount; i++)
        {
            Destroy(trs.GetChild(i).gameObject);
        }
        trs = GameManager.LHOobj.transform.Find("Cards");
        for (int i = 1; i < trs.childCount; i++)
        {
            Destroy(trs.GetChild(i).gameObject);
        }
        trs = GameManager.RHOobj.transform.Find("Cards");
        for (int i = 1; i < trs.childCount; i++)
        {
            Destroy(trs.GetChild(i).gameObject);
        }
        for (int i = 0; i < 3; i++)
        {
            Image childImg = HeadCards.GetChild(i).GetComponent<Image>();
            childImg.sprite = HeadCards.GetChild(3).GetComponent<Image>().sprite;
        }
    }

    public void BecomeLandLord(string id)
    {
        Transform ts = null;
        GameManager.landLordId = id;
        if (GameManager.id == id)
        {
            ts = GameManager.Myselfobj.transform;
            GameManager.character1 = GameManager.character0;
        }
        else if (GameManager.LHO == id)
        {
            ts = GameManager.LHOobj.transform;
            GameManager.character2 = GameManager.character0;
        }
        else if (GameManager.RHO == id)
        {
            ts = GameManager.RHOobj.transform;
            GameManager.character3 = GameManager.character0;
        }
        // ts.Find("Head").GetComponent<Image>().sprite =
        //     Resources.Load<Sprite>("Images/LandLord");
        ts.Find("RawHead").gameObject.SetActive(false);
        ts.Find("RawHeadDizhu").gameObject.SetActive(true);
        
        for (int i = 0; i < PickRole.childCount; i++)
        {
            PickRole.GetChild(i).gameObject.SetActive(false);
        }

        GameManager.ClearSyncMessage(GameManager.id);
        GameManager.ClearSyncMessage(GameManager.LHO);
        GameManager.ClearSyncMessage(GameManager.RHO);
    }

    private void ShowHeadCards()
    {
        for (int i = 0; i < 3; i++)
        {
            Image childImg = HeadCards.GetChild(i).GetComponent<Image>();
            childImg.sprite = Resources.Load<Sprite>("Card/" + CardManager.GetName(GameManager.headCards[i]));
        }

        if (GameManager.landLordId == GameManager.id)
        {
            GameManager.cards.AddRange(GameManager.headCards);
            GameManager.GenerateCard(GameManager.headCards.ToArray(), Card.transform.parent);
            SortCard();
        }

        RHOCardNum += GameManager.landLordId == GameManager.RHO
            ? GameManager.headCards.Count
            : 0;
        LHOCardNum += GameManager.landLordId == GameManager.LHO
            ? GameManager.headCards.Count
            : 0;
        SetPlayerCardNum();
    }

    private void InitCardPosition()
    {
        Transform trs = Card.transform.parent;
        Vector3 pos = trs.GetChild(0).localPosition;
        for (int i = 1; i < trs.childCount; i++)
        {
            Vector3 p = trs.GetChild(i).localPosition;
            trs.GetChild(i).localPosition -= Vector3.up * p.y;
            trs.GetChild(i).localPosition += Vector3.up * pos.y;
        }
    }
    
    private void SetPlayerCardNum()
    {
        GameManager.RHOobj.transform.Find("CardNumBg/CardNum").GetComponent<Text>().text = RHOCardNum.ToString();
        GameManager.LHOobj.transform.Find("CardNumBg/CardNum").GetComponent<Text>().text = LHOCardNum.ToString();
    }
    
    private void OnMessageShowCards(MessageBase messagebase)
    {
        MessageShowCards msg = messagebase as MessageShowCards;
        if (!msg.result)
        {
            PanelManager.Open<TipPanel>(msg.cardType == 0 ? "牌型不合适" : "该牌大不过");
            return;
        }
        if (msg.cards.Length == 0)
        {
            GameManager.ShowSyncMessage(msg.id, ResourcesPath.Message_Pass);
            AudioManager.Instance.PlaySound_Pass();
        }
        else
        {
            GameManager.selectedCards.Clear();
            GameManager.ClearSyncMessage(msg.id);
            var cards = CardManager.DataToCard(msg.cards.ToList());
            GameManager.ShowSyncCards(msg.id, cards.ToArray());
            if (GameManager.id == msg.id)
            {
                CardManager.RemovePlayCards(cards);
                GameManager.DestroyCards();
                GameManager.GenerateCard(GameManager.cards.ToArray(), Card.transform.parent);
                SortCard();
                CardManager.InitCardsPosition();
            }

            AudioManager.Instance.PlaySound_PlayCard();

            if (msg.id == GameManager.id)
            {
                GameManager.character1.SetTrigger(IsPlay);
            }
            
            if (msg.id == GameManager.LHO)
            {
                GameManager.character2.SetTrigger(IsPlay);
            }
            
            if (msg.id == GameManager.RHO)
            {
                GameManager.character3.SetTrigger(IsPlay);
            }
        }

        if (msg.id == GameManager.id)
            NetManager.Send(GameManager.cards.Count != 0 ? new MessageSwitchPickUp() : new MessagePokerResult());

        RHOCardNum -= msg.id == GameManager.RHO ? msg.cards.Length : 0;
        LHOCardNum -= msg.id == GameManager.LHO ? msg.cards.Length : 0;
        SetPlayerCardNum();
    }
    
    private void OnMessagePokerResult(MessageBase messagebase)
    {
        MessagePokerResult msg = messagebase as MessagePokerResult;
        if (msg.id == GameManager.landLordId)
        {
            GameManager.MyselfBean += 200;
            GameManager.LHOBean -= 100;
            GameManager.RHOBean -= 100;
            PanelManager.Open<TipPanel>($"地主 {msg.id} 获得胜利");
        }
        else
        {
            if (GameManager.id == GameManager.landLordId)
            {
                GameManager.MyselfBean -= 200;
                GameManager.LHOBean += 100;
                GameManager.RHOBean += 100;
                PanelManager.Open<TipPanel>($"农民 {GameManager.LHO}、{GameManager.RHO} 获得胜利");
            }
            else
            {
                var anotherPlay = GameManager.LHO == GameManager.landLordId ? GameManager.RHO : GameManager.LHO;
                GameManager.MyselfBean += 100;
                if (GameManager.LHO == GameManager.landLordId)
                {
                    GameManager.LHOBean -= 200;
                    GameManager.RHOBean += 100;
                }
                else if (GameManager.RHO == GameManager.landLordId)
                {
                    GameManager.LHOBean += 100;
                    GameManager.RHOBean -= 200;
                }
                PanelManager.Open<TipPanel>($"农民 {GameManager.id}、{anotherPlay} 获得胜利");
            }
        }
        InitPlayerInfo();
        for (int i = 0; i < Play.childCount; i++)
        {
            Play.GetChild(i).gameObject.SetActive(false);
        }
        bool isWin = msg.id == GameManager.id || msg.id != GameManager.landLordId;
        AudioManager.Instance.PlayBGM_PokerResult(isWin);
        Mask.gameObject.SetActive(true);
    }
    
    private void OnReturnRoom()
    {
        NetManager.Send(new MessagePokerRestart());
    }
}

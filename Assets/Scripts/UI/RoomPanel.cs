using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : PanelBase
{
    private readonly string SCRIPTNAME = "RoomPanel";
    private string ownId;
    
    
    private Transform Content;
    private Transform Player;
    private Button StartBtn;
    private Button PrepareBtn;
    private Button QuitBtn;
    private Text roomTitle;

    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] objects)
    {
        roomTitle = resGO.transform.Find("PlayerList/Title").GetComponent<Text>();
        Content = resGO.transform.Find("PlayerList/ScrollView/Viewport/Content");
        Player = Content.Find("Player");
        StartBtn = resGO.transform.Find("StartBtn").GetComponent<Button>();
        PrepareBtn = resGO.transform.Find("PrepareBtn").GetComponent<Button>();
        QuitBtn = resGO.transform.Find("QuitBtn").GetComponent<Button>();

        StartBtn.onClick.AddListener(OnStartBtnClick);
        PrepareBtn.onClick.AddListener(OnPrepareBtnClick);
        QuitBtn.onClick.AddListener(OnQuitBtnClick);

        NetManager.AddMessageListener("MessageRoomData", OnMessageRoomData);
        NetManager.AddMessageListener("MessageExitRoom", OnMessageExitRoom);
        NetManager.AddMessageListener("MessagePlayerData", OnMessagePlayerData);
        NetManager.AddMessageListener("MessagePlayerPrepare", OnMessagePlayerPrepare);
        NetManager.AddMessageListener("MessageStartPoker", OnMessageStartPoker);
        
        MessagePlayerData messagePlayerData = new MessagePlayerData();
        NetManager.Send(messagePlayerData);
        MessageRoomData messageRoomData = new MessageRoomData();
        NetManager.Send(messageRoomData);
    }

    private void OnStartBtnClick()
    {
        MessageStartPoker msg = new MessageStartPoker();
        NetManager.Send(msg);
    }

    private void OnPrepareBtnClick()
    {
        MessagePlayerPrepare msg = new MessagePlayerPrepare();
        NetManager.Send(msg);
    }

    private void OnQuitBtnClick()
    {
        MessageExitRoom msg = new MessageExitRoom();
        NetManager.Send(msg);
    }
    
    public override void OnClose()
    {
        NetManager.RemoveMessageListener("MessageRoomData", OnMessageRoomData);
        NetManager.RemoveMessageListener("MessageExitRoom", OnMessageExitRoom);
        NetManager.RemoveMessageListener("MessagePlayerData", OnMessagePlayerData);
        NetManager.RemoveMessageListener("MessagePlayerPrepare", OnMessagePlayerPrepare);
        NetManager.RemoveMessageListener("MessageStartPoker", OnMessageStartPoker);
    }
    
    private void OnMessageRoomData(MessageBase messagebase)
    {
        MessageRoomData msg = messagebase as MessageRoomData;
        for (int i = Content.childCount-1; i >0; i--)
        {
            Destroy(Content.GetChild(i).gameObject);
        }
        
        if (msg.playerDatas == null)
        {
            return;
        }

        bool startFlag = true;
        int count = 0;
        foreach (PlayerData playerData in msg.playerDatas)
        {
            GeneratePlayer(playerData);

            if (ownId == playerData.id)
            {
                GameManager.isHost = playerData.isHost;
                PrepareBtn.GetComponentInChildren<Text>().text = playerData.isPrepare ? "取消准备" : "准备";
            }

            startFlag &= playerData.isPrepare;
            count++;
        }

        StartBtn.gameObject.SetActive(startFlag && GameManager.isHost && count == 3);

        roomTitle.text = "房间 " + msg.roomId;
    }
    
    private void GeneratePlayer(PlayerData playerData)
    {
        GameObject newPlayer = Instantiate(Player, Content).gameObject;
        newPlayer.SetActive(true);

        Transform newPlayerTrs = newPlayer.transform;
        newPlayerTrs.localPosition = Vector3.zero;
        newPlayerTrs.localScale = Vector3.one;

        Transform Prepare = newPlayerTrs.Find("Prepare");
        Transform Host = newPlayerTrs.Find("Host");
        Text Id = newPlayerTrs.Find("Id").GetComponent<Text>();
        Text Count = newPlayerTrs.Find("Count").GetComponent<Text>();
        Id.text = playerData.id;
        Count.text = Common.BeanCountSet(playerData.bean);
        if (playerData.isPrepare)
        {
            Prepare.gameObject.SetActive(true);
        }
        else
        {
            Prepare.gameObject.SetActive(false);
        }
        Host.gameObject.SetActive(playerData.isHost);
    }
    
    private void OnMessageExitRoom(MessageBase messagebase)
    {
        MessageExitRoom msg = messagebase as MessageExitRoom;
        if (msg.result)
        {
            PanelManager.Open<TipPanel>("退出房间");
            PanelManager.Open<RoomListPanel>();
            PanelManager.Update<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("退出房间失败");
        }
    }
    
    private void OnMessagePlayerData(MessageBase messagebase)
    {
        MessagePlayerData msg = messagebase as MessagePlayerData;
        ownId = msg.id;
    }
    
    private void OnMessagePlayerPrepare(MessageBase messagebase)
    {
        MessagePlayerPrepare msg = messagebase as MessagePlayerPrepare;
        PrepareBtn.GetComponentInChildren<Text>().text = msg.IsPrepare ? "取消准备" : "准备";
    }
    
    private void OnMessageStartPoker(MessageBase messagebase)
    {
        MessageStartPoker msg = messagebase as MessageStartPoker;

        PanelManager.Open<TipPanel>(msg.result ? "开始游戏！" : "开始游戏失败");

        if (msg.result)
        {
            PanelManager.Open<PokerPanel>();
            PanelManager.Close("RoomPanel");
        }
    }
}
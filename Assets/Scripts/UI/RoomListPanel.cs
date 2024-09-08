using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : PanelBase
{
    private readonly string SCRIPTNAME = "RoomListPanel";

    private Transform Head;
    private Text UserName;
    private Text BeanNum;
    
    private Transform RoomList;
    private Transform Content;
    private Transform Room;
    
    private Transform SecondPanel;
    private Button CreateBtn;
    private Button RefreshBtn;

    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] objects)
    {
        Head = resGO.transform.Find("Head");
        UserName = Head.Find("UserName").GetComponent<Text>();
        BeanNum = Head.Find("BeanImg/BeanNum").GetComponent<Text>();
        
        RoomList = resGO.transform.Find("RoomList");
        Content = RoomList.transform.Find("ScrollView/Viewport/Content");
        Room = Content.Find("Room");
        
        SecondPanel = resGO.transform.Find("SecondPanel");
        CreateBtn = SecondPanel.Find("CreateBtn").GetComponent<Button>();
        RefreshBtn = SecondPanel.Find("RefreshBtn").GetComponent<Button>();
        
        CreateBtn.onClick.AddListener(OnCreateBtnClick);
        RefreshBtn.onClick.AddListener(OnRefreshBtnClick);
        
         NetManager.AddMessageListener("MessagePlayerData", OnMessagePlayerData);
         NetManager.AddMessageListener("MessageCreateRoom", OnMessageCreateRoom);
         NetManager.AddMessageListener("MessageRoomList", OnMessageRoomList);
         NetManager.AddMessageListener("MessageEnterRoom", OnMessageEnterRoom);

         OnUpdate();
    }
    private void OnCreateBtnClick()
    {
        MessageCreateRoom msg = new MessageCreateRoom();
        NetManager.Send(msg);
    }

    private void OnRefreshBtnClick()
    {
        MessageRoomList msg = new MessageRoomList();
        NetManager.Send(msg);
    }

    public override void OnUpdate()
    {
        MessageRoomList messageRoomList = new MessageRoomList();
        NetManager.Send(messageRoomList);
        MessagePlayerData messagePlayerData = new MessagePlayerData();
        NetManager.Send(messagePlayerData);
    }

    public override void OnClose()
    {
        NetManager.RemoveMessageListener("MessagePlayerData", OnMessagePlayerData);
        NetManager.RemoveMessageListener("MessageCreateRoom", OnMessageCreateRoom);
        NetManager.RemoveMessageListener("MessageRoomList", OnMessageRoomList);
        NetManager.RemoveMessageListener("MessageEnterRoom", OnMessageEnterRoom);
    }

    private void GenerateRoom(RoomData roomData)
    {
        GameObject newRoom = Instantiate(Room, Content).gameObject;
        newRoom.SetActive(true);

        Transform newRoomTrs = newRoom.transform;
        newRoomTrs.localPosition = Vector3.zero;
        newRoomTrs.localScale = Vector3.one;

        Transform Prepare = newRoomTrs.Find("Prepare");
        Text Id = newRoomTrs.Find("Id").GetComponent<Text>();
        Text Count = newRoomTrs.Find("Count").GetComponent<Text>();
        Id.text = roomData.id.ToString();
        Count.text = $"{roomData.count}/3";
        if (roomData.isPrepare)
        {
            Prepare.gameObject.SetActive(false);
        }
        else
        {
            Prepare.gameObject.SetActive(true);
        }

        newRoomTrs.GetComponent<Button>().onClick.AddListener(() => OnRoomClick(roomData.id));
    }
    
    private void OnRoomClick(int roomId)
    {
        MessageEnterRoom msg = new MessageEnterRoom()
        {
            roomId = roomId
        };
        NetManager.Send(msg);
    }
    
    private void OnMessagePlayerData(MessageBase messagebase)
    {
        MessagePlayerData msg = messagebase as MessagePlayerData;
        BeanNum.text = msg.bean.ToString();
        UserName.text = msg.id;
    }
    
    private void OnMessageRoomList(MessageBase messagebase)
    {
        MessageRoomList msg = messagebase as MessageRoomList;
        for (int i = Content.childCount - 1; i > 0; i--)
        {
            Destroy(Content.GetChild(i).gameObject);
        }

        if (msg.roomInfos == null)
        {
            return;
        }

        foreach (RoomData t in msg.roomInfos)
        {
            GenerateRoom(t);
        }
    }
    
    private void OnMessageEnterRoom(MessageBase messagebase)
    {
        MessageEnterRoom msg= messagebase as MessageEnterRoom;
        if (msg.result)
        {
            PanelManager.Open<RoomPanel>();
        }
        else
        {
            PanelManager.Open<TipPanel>("进入房间失败");
        }
    }

    private void OnMessageCreateRoom(MessageBase messagebase)
    {
        MessageCreateRoom msg = messagebase as MessageCreateRoom;
        if (msg.result)
        {
            PanelManager.Open<TipPanel>("创建成功");
            PanelManager.Open<RoomPanel>();
        }
        else
        {
            PanelManager.Open<TipPanel>("创建失败");
        }
    }

}
    

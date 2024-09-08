using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginPanel : PanelBase
{
    private readonly string SCRIPTNAME = "LoginPanel";
    private readonly string PROTONAME = "MessageLogin";
    
    private Transform Login;

    private InputField userNameInput;
    private InputField passwordInput;
    private Button loginBtn;
    private Button registerBtn;
    private Button quitBtn;
    
    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] objects)
    {
        Login = resGO.transform.Find("Login");
        userNameInput = Login.Find("UserNameInput").GetComponent<InputField>();
        passwordInput = Login.Find("PasswordInput").GetComponent<InputField>();
        loginBtn = Login.Find("LoginBtn").GetComponent<Button>();
        registerBtn = Login.Find("RegisterBtn").GetComponent<Button>();
        quitBtn = Login.Find("QuitBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginBtnClick);
        registerBtn.onClick.AddListener(OnRegisterBtnClick);
        quitBtn.onClick.AddListener(OnQuitBtnClick);
        
        NetManager.AddMessageListener(PROTONAME, OnMessageLogin);
        NetManager.AddStateEvent(NetManager.NetEvent.ConnectSuccess, OnConnectSuccess);
        NetManager.AddStateEvent(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.SocketConnect("127.0.0.1", 8888);

        AudioManager.Instance.Play_BGM(AudioManager.BGMType.Welcome);
    }
    
    private void OnConnectSuccess(string str)
    {
        Debug.Log("连接成功：" + str);
    }

    private void OnConnectFail(string str)
    {
        Debug.Log("连接失败：" + str);
        PanelManager.Open<TipPanel>(str);
    }

    private void OnLoginBtnClick()
    {
        if (userNameInput.text == "" || passwordInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }

        MessageLogin messageLogin = new MessageLogin()
        {
            id = userNameInput.text,
            pw = passwordInput.text
        };
        NetManager.Send(messageLogin);
    }
    
    private void OnRegisterBtnClick()
    {
        PanelManager.Open<RegisterPanel>();
    }

    private void OnQuitBtnClick()
    {
        Application.Quit();
    }
    
    private void OnMessageLogin(MessageBase messagebase)
    {
        MessageLogin messageLogin = messagebase as MessageLogin;
        if (messageLogin.result)
        {
            GameManager.id = messageLogin.id;
            PanelManager.Open<TipPanel>("登录成功");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("登录失败");
        }
    }

    public override void OnClose()
    {
        NetManager.RemoveMessageListener(PROTONAME, OnMessageLogin);
        NetManager.RemoveStateEvent(NetManager.NetEvent.ConnectSuccess, OnConnectSuccess);
        NetManager.RemoveStateEvent(NetManager.NetEvent.ConnectFail, OnConnectFail);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : PanelBase
{
    private readonly string SCRIPTNAME = "RegisterPanel";
    private readonly string PROTONAME = "MessageRegister";
    
    private Transform Register;
    private InputField userNameInput;
    private InputField passwordInput;
    private InputField passwordConfirmInput;
    private Button confirmBtn;
    private Button backBtn;
    
    public override void OnInit()
    {
        resPath = RES_PREFABS + SCRIPTNAME;
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] objects)
    {
        Register = resGO.transform.Find("Register");
        userNameInput = Register.Find("UserNameInput").GetComponent<InputField>();
        passwordInput = Register.Find("PasswordInput").GetComponent<InputField>();
        passwordConfirmInput = Register.Find("PasswordConfirmInput").GetComponent<InputField>();
        confirmBtn = Register.Find("ConfirmBtn").GetComponent<Button>();
        backBtn = Register.Find("BackBtn").GetComponent<Button>();

        confirmBtn.onClick.AddListener(OnRegisterClick);
        backBtn.onClick.AddListener(OnBackClick);
        
        NetManager.AddMessageListener(PROTONAME, OnMessageRegister);
    }

    private void OnRegisterClick()
    {
        if (userNameInput.text == "" || passwordInput.text == "")
        {
            PanelManager.Open<TipPanel>("用户名和密码不能为空");
            return;
        }

        if (passwordInput.text != passwordConfirmInput.text)
        {
            PanelManager.Open<TipPanel>("两次密码不一致");
            return;
        }

        MessageRegister messageRegister = new MessageRegister()
        {
            id = userNameInput.text,
            pw = passwordInput.text
        };
        NetManager.Send(messageRegister);
    }
    
    private void OnBackClick()
    {
        Close();
    }
    
    private void OnMessageRegister(MessageBase messagebase)
    {
        MessageRegister messageRegister = messagebase as MessageRegister;
        if (messageRegister.result)
        {
            PanelManager.Open<TipPanel>("注册成功");
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("注册失败");
        }
    }
    
    public override void OnClose()
    {
        NetManager.RemoveMessageListener(PROTONAME, OnMessageRegister);
    }
}

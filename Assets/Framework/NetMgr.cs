using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;


public class NetMgr : MgrBase
{
    public static NetMgr It;
    void Awake() { It = this; }

    public enum EnNetState
    {
        Lost = 0,
        Connecting,
        Connected
    }

    private EnNetState m_State;
    private XTcpClient m_Client;

    public override void Init()
    {
        m_State = EnNetState.Lost;
        m_Client = new XTcpClient("127.0.0.1", 1000);
        m_Client.OnConnectedHandler += OnConnected;
        m_Client.OnDisconnectedHandler += OnDisconnected;
        m_Client.OnErrorHandler += OnError;

        OnConnectStart();
        m_Client.ConnectAsync();
    }

    public override void UnInit()
    {
        m_Client.Close();
    }

    public bool IsConnected()
    {
        return m_Client.IsConnected();
    }

    // 开始连接服务器
    void OnConnectStart()
    {

    }

    void OnError(SocketException ex)
	{
        //Debug.LogWarning("NetMgr:OnError");
        //Globals.It.UIMgr.StopWaiting();
        //Globals.It.UIMgr.ShowMsgBox("连接服务器失败，请稍后再试");
        //Globals.It.LogicMgr.OnServerError();
	}

    void OnDisconnected()
	{
        m_State = EnNetState.Lost;
		//Debug.LogWarning("NetMgr:OnDisconnected");
        //Globals.It.UIMgr.ShowMsgBox("连接服务器失败，请稍后再试", "为什么");
        //Globals.It.LogicMgr.OnServerDisconnected();
	}

    void OnConnected()
	{
        m_State = EnNetState.Connected;
        //Debug.Log("NetMgr:OnConnected.." + IsConnected());
        //Globals.It.UIMgr.StopWaiting();
        //Globals.It.LogicMgr.OnServerConnected();
	}
	
	void FixedUpdate()
	{
        if (IsConnected()) 
		{
            m_Client.MainLoop();
		}
	}

    public void AddHandle(string name, Network.PtoHandle handle)
    {
        m_Client.Net.AddHandle(name, handle);
    }

    public void DelHandle(string name, Network.PtoHandle handle)
    {
        m_Client.Net.DelHandle(name, handle);
    }

    private void SendPto(ref Network.PtoBase data)
	{
        if (m_State != EnNetState.Connected) return;
        m_Client.Write(ref data);
	}

    IEnumerator CoConnect()
    {
        m_State = EnNetState.Connecting;
        //Globals.It.UIMgr.StartWaiting();
        m_Client.ConnectAsync();
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            switch (m_State)
            {
                case EnNetState.Connected:
                    //Globals.It.UIMgr.StopWaiting();
                    /*foreach (NetWriter.CWriteBase stPack in m_WaitSend)
                    {
                        byte[] buffer = m_NetWriter.Process(stPack);
                        if (buffer != null) m_Client.Send(buffer);
                    }
                    m_WaitSend.Clear();*/
                    yield break;
                case EnNetState.Lost:
                    //Globals.It.UIMgr.StopWaiting();
                    //Globals.It.UIMgr.ShowMsgBox("网络不给力，稍后再试吧", "朕知道了");
                    yield break;
            }
        }
    }
}

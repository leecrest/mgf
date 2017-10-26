using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;


public class NetMgr : MgrBase
{
    public static NetMgr It;
    void Awake() { It = this; }

    //���ݰ���س���
    public static readonly int CLIENT_READ_BUFF_MAX = 10240;
    public static readonly int CLIENT_WRITE_BUFF_MAX = 10240;
    public static readonly int PACKET_HEAD = 2;
    public static readonly int PACKET_LEN_MIN = 3;

    public enum EnNetState
    {
        Lost = 0,
        Connecting,
        Connected
    }

    public struct WriteData
    {
        public ushort pos;
        public ushort size;
    }

    private EnNetState m_enNetState;
    private Socket m_Socket;
    private IPEndPoint m_Address;
    private int m_AddrPort;
    private Thread m_Thread;
    private bool m_bStopRun;
    private bool m_ConnectSync; // true��ʾͬ����false��ʾ�첽
    private bool m_NewThread; //�Ƿ��������߳�����дЭ��
    private Protocol m_Protocol;
    private byte[] m_ReadBuffer;
    private int m_ReadSize;
    private byte[] m_WriteBuffer;
    private int m_WriteSize;
    private Queue<Protocol.PtoBase> m_WriteQueue;
    private Queue<Protocol.PtoBase> m_ReadQueue;
    private Queue<SocketException> m_ErrorQueue;
    private object m_Lock = new object();

    public override void Init()
    {
        m_enNetState = EnNetState.Lost;
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_bStopRun = false;
        m_Protocol = new Protocol();

        m_ReadBuffer = new byte[CLIENT_READ_BUFF_MAX];
        m_ReadSize = 0;
        m_WriteBuffer = new byte[CLIENT_WRITE_BUFF_MAX];
        m_WriteSize = 0;

        m_WriteQueue = new Queue<Protocol.PtoBase>();
        m_ReadQueue = new Queue<Protocol.PtoBase>();
        m_ErrorQueue = new Queue<SocketException>();
    }

    public void StartNet(string ip, int port, bool bNewThread)
    {
        m_Address = new IPEndPoint(IPAddress.Parse(ip), port);
        m_NewThread = bNewThread;
        if (m_NewThread)
        {
            m_Thread = new Thread(new ThreadStart(LoopThread))
            {
                IsBackground = true
            };
        }
        StartCoroutine("CoConnect");
    }

    public override void UnInit()
    {
        Close();
    }

    private void LoopThread()
    {
        while (!m_bStopRun && IsConnected())
        {
            if (m_Socket.Poll(-1, SelectMode.SelectRead))
            {
                OnSocketRead();
            }
            if (m_Socket.Poll(-1, SelectMode.SelectWrite))
            {
                OnSocketWrite();
            }
            if (m_Socket.Poll(-1, SelectMode.SelectError))
            {
                OnSocketError(null);
            }
        }
    }

    private void LoopNoThread()
    {
        if (m_Socket.Poll(50, SelectMode.SelectRead))
        {
            OnSocketRead();
        }
        if (m_Socket.Poll(50, SelectMode.SelectWrite))
        {
            OnSocketWrite();
        }
        if (m_Socket.Poll(50, SelectMode.SelectError))
        {
            OnSocketError(null);
        }
    }

    private void OnSocketRead()
    {
        if (m_Socket.Available <= 0)
        {
            return;
        }

        try
        {
            int size = m_Socket.Available;
            int left = m_ReadBuffer.Length - m_ReadSize;
            if (left < size)
            {
                // ���������ˣ�˵����������ٶ�̫��
                return;
            }
            m_Socket.Receive(m_ReadBuffer, m_ReadSize, size, SocketFlags.None);
            m_ReadSize += size;
            // �ְ�
            ushort len = 0;
            ushort pos = 0;
            while (size >= PACKET_LEN_MIN)
            {
                // �ȶ�ȡ2�ֽڵİ��峤��
                len = System.BitConverter.ToUInt16(m_ReadBuffer, pos);
                if (len > m_ReadSize - PACKET_HEAD)
                {
                    // ���岻�������ȴ��������ȡ
                    break;
                }
                pos += 2;
                Protocol.PtoBase pack = m_Protocol.ReadPacket(ref m_ReadBuffer, pos, len);
                pos += len;
                lock (m_Lock)
                {
                    m_ReadQueue.Enqueue(pack);
                }
            }
            if (pos > 0)
            {
                Buffer.BlockCopy(m_ReadBuffer, pos, m_ReadBuffer, 0, m_ReadSize - pos);
                m_ReadSize -= pos;
            }
        }
        catch (ObjectDisposedException)
        {
            Close();
        }
        catch (SocketException ex)
        {
            OnError(ex);
            Close();
        }
    }

    private void OnSocketWrite()
    {
        if (m_NewThread) Monitor.Enter(m_WriteQueue);
        while (m_WriteQueue.Count > 0 && IsConnected())
        {
            WriteData data = m_WriteQueue.Dequeue();
            if (data.pos >= 0 && data.pos + data.size <= m_WriteSize)
            {
                m_Socket.Send(m_WriteBuffer, data.pos, data.size, SocketFlags.Broadcast);
            }
        }
        if (m_NewThread) Monitor.Exit(m_WriteQueue);
    }

    private void OnSocketError(SocketException ex)
    {
        m_ErrorQueue.Enqueue(ex);
    }

    public void ConnectSync()
    {
        m_ConnectSync = true;
        try
        {
            m_Socket.Connect(m_Address);
        }
        catch (SocketException ex)
        {
            OnError(ex);
            return;
        }
        if (IsConnected())
        {
            if (m_NewThread) m_Thread.Start();
            OnConnected();
        }
        else
        {
            OnDisconnected();
        }
    }

    public void ConnectAsync()
    {
        m_ConnectSync = false;
        m_Socket.BeginConnect(m_Address, new AsyncCallback(ConnectCallback), m_Socket);
    }

    private void ConnectCallback(IAsyncResult async)
    {
        try
        {
            m_Socket.EndConnect(async);
            if (IsConnected())
            {
                if (m_NewThread) m_Thread.Start();
                OnConnected();
            }
        }
        catch (SocketException ex)
        {
            OnError(ex);
        }
    }

    public void Write(byte[] buff)
    {
        if (buff == null) return;
        if (m_NewThread) Monitor.Enter(m_WriteQueue);
        m_WriteQueue.Enqueue(buff);
        if (m_NewThread) Monitor.Exit(m_WriteQueue);
    }

    public void MainLoop()
    {
        if (!m_NewThread)
        {
            if (!IsConnected()) return;
            LoopNoThread();
        }
        if (m_ReadQueue.Count <= 0) return;

        Protocol.PtoBase pack = null;
        for (int i = 0; i < 1; i++)
        {
            if (m_NewThread)
            {
                Monitor.Enter(m_ReadQueue);
                if (m_ReadQueue.Count > 0)
                {
                    pack = m_ReadQueue.Dequeue();
                }
                Monitor.Exit(m_ReadQueue);
            }
            else
            {
                if (m_ReadQueue.Count > 0)
                {
                    pack = m_ReadQueue.Dequeue();
                }
            }
            if (pack == null) break;
            m_Protocol.HandlePacket(ref pack);
        }
    }

    public void HandleError()
    {
        if (m_ErrorQueue.Count == 0)
        {
            return;
        }
        SocketException ex = null;
        if (m_NewThread) Monitor.Enter(m_ErrorQueue);
        ex = m_ErrorQueue.Dequeue();
        if (m_NewThread) Monitor.Exit(m_ErrorQueue);
        OnError(ex);
    }

    public void Close()
    {
        m_bStopRun = true;
        if (IsConnected())
        {
            m_Socket.Close();
            OnDisconnected();
        }
    }

    public void ReConnect()
    {
        Close();
        if (m_ConnectSync)
        {
            ConnectSync();
        }
        else
        {
            ConnectAsync();
        }
    }

    public bool IsConnected()
    {
        return m_Socket != null && m_Socket.Connected;
    }

    void OnError(SocketException ex)
	{
        m_enNetState = EnNetState.Lost;
        Debug.LogWarning("NetMgr:OnError");
        //Globals.It.UIMgr.StopWaiting();
        //Globals.It.UIMgr.ShowMsgBox("���ӷ�����ʧ�ܣ����Ժ�����");
        //Globals.It.LogicMgr.OnServerError();
	}

    void OnDisconnected()
	{
        m_enNetState = EnNetState.Lost;
		Debug.LogWarning("NetMgr:OnDisconnected");
        //Globals.It.UIMgr.ShowMsgBox("���ӷ�����ʧ�ܣ����Ժ�����", "Ϊʲô");
        //Globals.It.LogicMgr.OnServerDisconnected();
	}

    void OnConnected()
	{
        m_enNetState = EnNetState.Connected;
        Debug.Log("NetMgr:OnConnected.." + IsConnected());
        //Globals.It.UIMgr.StopWaiting();
        //Globals.It.LogicMgr.OnServerConnected();
	}
	
	void FixedUpdate()
	{
        if (IsConnected()) 
		{
            MainLoop();
		}
        HandleError();
	}

	private void SendPto(ref Protocol.PtoBase data) 
	{
        switch (m_enNetState)
        {
            case EnNetState.Connected:
                byte[] buffer = m_NetWriter.Process(stPack);
                if (buffer != null) m_Client.Send(buffer);
                break;
            case EnNetState.Connecting:
                m_WaitSend.Add(stPack);
                break;
            case EnNetState.Lost:
                m_WaitSend.Add(stPack);
                StartCoroutine("CoConnect");
                break;
        }
	}

    IEnumerator CoConnect()
    {
        //Globals.It.UIMgr.StartWaiting();
        m_enNetState = EnNetState.Connecting;
        ConnectAsync();
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            switch (m_enNetState)
            {
                case EnNetState.Connected:
                    //Globals.It.UIMgr.StopWaiting();
                    foreach (NetWriter.CWriteBase stPack in m_WaitSend)
                    {
                        byte[] buffer = m_NetWriter.Process(stPack);
                        if (buffer != null) m_Client.Send(buffer);
                    }
                    m_WaitSend.Clear();
                    yield break;
                case EnNetState.Lost:
                    //Globals.It.UIMgr.StopWaiting();
                    //Globals.It.UIMgr.ShowMsgBox("���粻�������Ժ����԰�", "��֪����");
                    yield break;
            }
        }
    }
}

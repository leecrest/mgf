using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class XTcpClient
{
    public delegate void NetHandler();
    public delegate void NetError(SocketException ex);

    public NetHandler OnConnectedHandler;
    public NetHandler OnDisconnectedHandler;
    public NetError OnErrorHandler;

    private Socket m_Socket;
    private IPEndPoint m_Address;
    private bool m_ConnectSync; // true表示同步，false表示异步
    private Network m_Net;
    public Network Net { get { return m_Net; } }
    private BinaryReader m_Reader;
    private byte[] m_ReadBuffer;
    private int m_ReadSize;
    private BinaryWriter m_Writer;
    private byte[] m_WriteBuffer;
    private Queue<Network.PtoBase> m_ReadQueue;

    public XTcpClient(string ip, int port)
    {
        m_Address = new IPEndPoint(IPAddress.Parse(ip), port);
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Net = new Network();
        m_ReadBuffer = new byte[Network.CLIENT_READ_BUFF_MAX];
        m_ReadSize = 0;
        m_Reader = new BinaryReader(new MemoryStream(m_ReadBuffer));
        m_WriteBuffer = new byte[Network.CLIENT_WRITE_BUFF_MAX];
        m_Writer = new BinaryWriter(new MemoryStream(m_WriteBuffer));
        m_ReadQueue = new Queue<Network.PtoBase>();
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
                OnConnected();
            } else
            {
                OnDisconnected();
            }
        }
        catch (SocketException ex)
        {
            OnError(ex);
        }
    }

    void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            int size = m_Socket.EndReceive(ar);
            m_ReadSize += size;
            // 分包
            ushort len = 0;
            ushort pos = 0;
            while (size >= Network.PACKET_LEN_MIN)
            {
                // 先读取2字节的包体长度
                len = System.BitConverter.ToUInt16(m_ReadBuffer, pos);
                if (len > m_ReadSize - Network.PACKET_HEAD)
                {
                    // 包体不完整，等待完整后读取
                    break;
                }
                pos += 2;
                m_Reader.Read(m_ReadBuffer, pos, len);
                Network.PtoBase pack = m_Net.ReadPacket(ref m_Reader);
                pos += len;
                m_ReadQueue.Enqueue(pack);
            }
            if (pos > 0)
            {
                Buffer.BlockCopy(m_ReadBuffer, pos, m_ReadBuffer, 0, m_ReadSize - pos);
                m_ReadSize -= pos;
            }
            m_Socket.BeginReceive(m_ReadBuffer, m_ReadSize, Network.CLIENT_READ_BUFF_MAX - m_ReadSize,
                SocketFlags.None, ReceiveCallBack, m_Socket);
        }
        catch (SocketException e)
        {
            OnError(e);
            m_Socket.Close();
            OnDisconnected();
        }
    }

    public bool Write(ref Network.PtoBase data)
    {
        if (!IsConnected()) return false;
        m_Writer.Seek(0, SeekOrigin.Begin);
        bool ret = m_Net.WritePacket(ref data, ref m_Writer);
        if (!ret) return false;
        m_Socket.BeginSend(m_WriteBuffer, 0, (int)(m_Writer.BaseStream.Position), 
            SocketFlags.None, SendCallBack, m_Socket);
        //m_Socket.Send(m_WriteBuffer, 0, (int)m_Writer.BaseStream.Position, SocketFlags.None);
        return true;
    }

    void SendCallBack(IAsyncResult ar)
    {
        try
        {
            m_Socket.EndSend(ar);
        }
        catch (SocketException e)
        {
            OnError(e);
        }
    }

    public void MainLoop()
    {
        if (m_ReadQueue.Count <= 0) return;
        Network.PtoBase pack = null;
        for (int i = 0; i < Network.READ_PACKET_PER_FRAME; i++)
        {
            if (m_ReadQueue.Count <= 0) break;
            pack = m_ReadQueue.Dequeue();
            if (pack == null) break;
            m_Net.HandlePacket(ref pack);
        }
    }

    public void Close()
    {
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
        if (OnErrorHandler != null)
        {
            OnErrorHandler(ex);
        }
    }

    void OnDisconnected()
    {
        if (OnDisconnectedHandler != null)
        {
            OnDisconnectedHandler();
        }
    }

    // 连接成功，开始读协议
    void OnConnected()
    {
        m_Socket.BeginReceive(m_ReadBuffer, 0, Network.CLIENT_READ_BUFF_MAX,
            SocketFlags.None, ReceiveCallBack, m_Socket);
        if (OnConnectedHandler != null)
        {
            OnConnectedHandler();
        }
    }
}
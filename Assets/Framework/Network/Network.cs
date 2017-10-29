using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Network
{
    //数据包相关常量
    public static readonly int CLIENT_READ_BUFF_MAX = 10240;
    public static readonly int CLIENT_WRITE_BUFF_MAX = 10240;
    public static readonly int PACKET_HEAD = 2;
    public static readonly int PACKET_LEN_MIN = 3;
    // 每帧处理多少个数据包
    public static readonly int READ_PACKET_PER_FRAME = 100;

    public class PtoBase
    {
        public string _name;
    }

    // 委托
    public delegate void PtoHandle(ref PtoBase data);
    public delegate bool PtoWriter(ref PtoBase data, ref BinaryWriter bw);
    public delegate bool PtoReader(ref BinaryReader br, out PtoBase data);

    private Protocol m_Protocol;
    private Dictionary<string, Network.PtoHandle> m_Handlers;

    public Network()
    {
        m_Protocol = new Protocol();
        m_Handlers = new Dictionary<string, Network.PtoHandle>();
    }

    public void AddHandle(string name, Network.PtoHandle handle)
    {
        if (!m_Protocol.m_Name2ID.ContainsKey(name)) return;
        if (m_Handlers.ContainsKey(name))
        {
            m_Handlers[name] += handle;
        }
        else
        {
            m_Handlers[name] = handle;
        }
    }

    public void DelHandle(string name, Network.PtoHandle handle)
    {
        if (!m_Handlers.ContainsKey(name)) return;
        if (handle == null)
        {
            m_Handlers.Remove(name);
        }
        else
        {
            m_Handlers[name] -= handle;
        }
    }

    public PtoBase ReadPacket(ref BinaryReader br)
    {
        ushort id = br.ReadUInt16();
        if (!m_Protocol.m_Readers.ContainsKey(id)) return null;
        return m_Protocol.m_Readers[id](ref br);
    }

    public bool WritePacket(ref PtoBase data, ref BinaryWriter bw)
    {
        if (!m_Protocol.m_Name2ID.ContainsKey(data._name)) return false;
        ushort ptoID = m_Protocol.m_Name2ID[data._name];
        if (!m_Protocol.m_Writers.ContainsKey(ptoID)) return false;
        bw.Write(ptoID);
        return m_Protocol.m_Writers[ptoID](ref data, ref bw);
    }

    public bool HandlePacket(ref PtoBase data)
    {
        if (!m_Handlers.ContainsKey(data._name)) return false;
        m_Handlers[data._name](ref data);
        return true;
    }



    public static bool Read_bool(ref BinaryReader br, out bool value)
    {
        value = br.ReadBoolean();
        return true;
    }
    public static bool Write_bool(ref BinaryWriter bw, bool value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_uint8(ref BinaryReader br, out byte value)
    {
        value = br.ReadByte();
        return true;
    }
    public static bool Write_uint8(ref BinaryWriter bw, byte value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_int8(ref BinaryReader br, out sbyte value)
    {
        value = br.ReadSByte();
        return true;
    }
    public static bool Write_int8(ref BinaryWriter bw, sbyte value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_uint16(ref BinaryReader br, out ushort value)
    {
        value = br.ReadUInt16();
        return true;
    }
    public static bool Write_uint16(ref BinaryWriter bw, ushort value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_int16(ref BinaryReader br, out short value)
    {
        value = br.ReadInt16();
        return true;
    }
    public static bool Write_int16(ref BinaryWriter bw, short value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_uint32(ref BinaryReader br, out uint value)
    {
        value = br.ReadUInt32();
        return true;
    }
    public static bool Write_uint32(ref BinaryWriter bw, uint value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_int32(ref BinaryReader br, out int value)
    {
        value = br.ReadInt32();
        return true;
    }
    public static bool Write_int32(ref BinaryWriter bw, int value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_uint64(ref BinaryReader br, out ulong value)
    {
        value = br.ReadUInt64();
        return true;
    }
    public static bool Write_uint64(ref BinaryWriter bw, ulong value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_int64(ref BinaryReader br, out long value)
    {
        value = br.ReadInt64();
        return true;
    }
    public static bool Write_int64(ref BinaryWriter bw, long value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_float(ref BinaryReader br, out float value)
    {
        value = br.ReadSingle();
        return true;
    }
    public static bool Write_float(ref BinaryWriter bw, float value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_double(ref BinaryReader br, out double value)
    {
        value = br.ReadDouble();
        return true;
    }
    public static bool Write_double(ref BinaryWriter bw, double value)
    {
        bw.Write(value);
        return true;
    }

    public static bool ReadArraySize(ref BinaryReader br, out ushort size)
    {
        size = 0;
        return true;
    }

    public static bool WriteArraySize(ref BinaryWriter bw, uint size)
    {
        return true;
    }

    public static bool Read_string(ref BinaryReader br, out string value)
    {
        value = br.ReadString();
        return true;
    }

    public static bool Write_string(ref BinaryWriter bw, string value)
    {
        bw.Write(value);
        return true;
    }

    public static bool Read_number(ref BinaryReader br, out int value)
    {
        value = 0;
        return true;
    }

    public static bool Write_number(ref BinaryWriter bw, int value)
    {
        return true;
    }
}
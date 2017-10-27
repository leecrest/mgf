
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Protocol {
	public static readonly int CLIENT_READ_BUFF_MAX = 102400;
	public static readonly int CLIENT_WRITE_BUFF_MAX = 10240;

	public class PtoBase
	{
		public string _name;
	}
	public delegate bool PtoWriter(ref PtoBase data, ref BinaryWriter bw);
	private Dictionary<string, ushort> m_Name2ID;
	private Dictionary<ushort, PtoWriter> m_Writers;
	public delegate PtoBase PtoReader(ref byte[] buff, ushort pos, ushort len);
	private Dictionary<ushort, PtoReader> m_Readers;
    public delegate void PtoHandle(ref PtoBase data);
    private Dictionary<string, PtoHandle> m_Handlers;

	public Protocol() {
		m_Readers = new Dictionary<ushort, PtoReader>();
		m_Readers[1] = Reader_c2s_login_version;
		m_Readers[2] = Reader_s2c_login_error;

		m_Name2ID = new Dictionary<string, ushort>();
		m_Writers = new Dictionary<ushort, PtoWriter>();
		m_Name2ID["c2s_login_version"] = 1;
		m_Writers[1] = Writer_c2s_login_version;
		m_Name2ID["s2c_login_error"] = 2;
		m_Writers[2] = Writer_s2c_login_error;

		m_Handlers = new Dictionary<string, PtoHandle>();
	}

	public void AddHandle(string name, PtoHandle handle)
    {
        if (!m_Name2ID.ContainsKey(name)) return;
        if (m_Handlers.ContainsKey(name))
        {
            m_Handlers[name] += handle;
        }
        else
        {
            m_Handlers[name] = handle;
        }
    }

    public void DelHandle(string name, PtoHandle handle)
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

    public PtoBase ReadPacket(ref byte[] buff, ushort pos, ushort len)
    {
        if (pos + 2 > len) return null;
        ushort id = System.BitConverter.ToUInt16(buff, pos);
        pos += 2;
        if (!m_Readers.ContainsKey(id)) return null;
        return m_Readers[id](ref buff, pos, len);
    }

	public bool HandlePacket(ref PtoBase data)
    {
        if (!m_Handlers.ContainsKey(data._name)) return false;
        m_Handlers[data._name](ref data);
        return true;
    }

	public byte[] WritePacket(PtoBase data, ref BinaryWriter bw)
	{
		if (!m_Name2ID.ContainsKey(data._name)) return null;
        ushort ptoID = m_Name2ID[data._name];
		if (!m_Writers.ContainsKey(ptoID)) return null;
		byte[] buff = m_Writers[ptoID](ref data, ref bw);
		if (buff == null) return null;
		return Packet.Pack(ptoID, buff);
	}

	public static ushort ReadArraySize(ref BinaryReader br) {
		return 0;
	}

	public static bool WriteArraySize(ref BinaryWriter bw, uint size) {
		return true;
	}

	


	[System.Serializable]
	public class T_c2s_login_version : PtoBase {
		public byte m_checkSum;
		public string m_username;
		public string m_password;

		public bool Read(ref BinaryReader br) {
			m_checkSum = br.ReadByte();
			m_username = br.ReadString();
			m_password = br.ReadString();
			return true;
		}
		public bool Write(ref BinaryWriter bw) {
			bw.Write(m_checkSum);
			bw.Write(m_username);
			bw.Write(m_password);
			return true;
		}
	}
	public delegate void Delegate_c2s_login_version(ref T_c2s_login_version t);
	public Delegate_c2s_login_version c2s_login_version;
	public PtoBase Reader_c2s_login_version(ref byte[] buff, ushort pos, ushort len) {
		T_c2s_login_version t = new T_c2s_login_version();
		t._name = "c2s_login_version";
		BinaryReader br = new BinaryReader(new MemoryStream(buff, pos, len-pos+1));
		t.Read(ref br);
		return t;
	}
	public bool Writer_c2s_login_version(ref PtoBase data, ref BinaryWriter bw) {
		T_c2s_login_version t = (T_c2s_login_version)data;
		t.Write(ref bw);
		return true;
	}


	[System.Serializable]
	public class T_s2c_login_error : PtoBase {
		public byte m_msg;
		public List<string> m_args = new List<string>();

		public bool Read(ref BinaryReader br) {
			m_msg = br.ReadByte();
			ushort size = ReadArraySize(ref br);
			for (ushort i = 0; i < size; i++) {
				m_args.Add(br.ReadString());
			}
			return true;
		}
		public bool Write(ref BinaryWriter bw) {
			bw.Write(m_msg);
			WriteArraySize(ref bw, (uint)m_args.Count);
			foreach (var item in m_args) {
				bw.Write(item);
			}
			return true;
		}
	}
	public delegate void Delegate_s2c_login_error(ref T_s2c_login_error t);
	public Delegate_s2c_login_error s2c_login_error;
	public PtoBase Reader_s2c_login_error(ref byte[] buff, ushort pos, ushort len) {
		T_s2c_login_error t = new T_s2c_login_error();
		t._name = "s2c_login_error";
		BinaryReader br = new BinaryReader(new MemoryStream(buff, pos, len-pos+1));
		t.Read(ref br);
		return t;
	}
	public bool Writer_s2c_login_error(ref PtoBase data, ref BinaryWriter bw) {
		T_s2c_login_error t = (T_s2c_login_error)data;
		t.Write(ref bw);
		return true;
	}

}

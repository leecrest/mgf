// create by ptomaker
using System.Collections.Generic;
using System.IO;

public class Protocol {
	public Dictionary<string, ushort> m_Name2ID;
    public Dictionary<ushort, Network.PtoWriter> m_Writers;
    public Dictionary<ushort, Network.PtoReader> m_Readers;

	public Protocol() {
		m_Readers = new Dictionary<ushort, Network.PtoReader>();
		m_Readers[1] = Reader_s2c_login_error;
		m_Readers[2] = Reader_c2s_login_version;

		m_Name2ID = new Dictionary<string, ushort>();
		m_Writers = new Dictionary<ushort, Network.PtoWriter>();
		m_Name2ID["s2c_login_error"] = 1;
		m_Writers[1] = Writer_s2c_login_error;
		m_Name2ID["c2s_login_version"] = 2;
		m_Writers[2] = Writer_c2s_login_version;
	}
	


	[System.Serializable]
	public class T_s2c_login_error : Network.PtoBase {
		public int m_msg;
		public List<string> m_args = new List<string>();

		public bool Read(ref BinaryReader br) {
			bool ret = false;
			ret = Network.Read_number(ref br, out m_msg);
			if (!ret) { return false; }
			ushort size = 0;
			ret = Network.ReadArraySize(ref br, out size);
			if (!ret) return false;
			for (ushort i = 0; i < size; i++) {
				string value;
				ret = Network.Read_string(ref br, out value);
				if (!ret) { return false; }
				m_args.Add(value);
			}
			return true;
		}
		public bool Write(ref BinaryWriter bw) {
			bool ret = false;
			ret = Network.Write_number(ref bw, m_msg);
			if (!ret) { return false; }
			ret = Network.WriteArraySize(ref bw, (uint)m_args.Count);
			if (!ret) { return false; }
			foreach (var value in m_args) {
				ret = Network.Write_string(ref bw, value);
				if (!ret) { return false; }
			}
			return true;
		}
	}
	public delegate void Delegate_s2c_login_error(ref T_s2c_login_error t);
	public Delegate_s2c_login_error s2c_login_error;
	public bool Reader_s2c_login_error(ref BinaryReader br, out Network.PtoBase data) {
		T_s2c_login_error t = new T_s2c_login_error();
		t._name = "s2c_login_error";
		data = t;
		return t.Read(ref br);
	}
	public bool Writer_s2c_login_error(ref Network.PtoBase data, ref BinaryWriter bw) {
		T_s2c_login_error t = (T_s2c_login_error)data;
		return t.Write(ref bw);
	}


	[System.Serializable]
	public class T_c2s_login_version : Network.PtoBase {
		public int m_checkSum;
		public string m_username;
		public string m_password;

		public bool Read(ref BinaryReader br) {
			bool ret = false;
			ret = Network.Read_number(ref br, out m_checkSum);
			if (!ret) { return false; }
			ret = Network.Read_string(ref br, out m_username);
			if (!ret) { return false; }
			ret = Network.Read_string(ref br, out m_password);
			if (!ret) { return false; }
			return true;
		}
		public bool Write(ref BinaryWriter bw) {
			bool ret = false;
			ret = Network.Write_number(ref bw, m_checkSum);
			if (!ret) { return false; }
			ret = Network.Write_string(ref bw, m_username);
			if (!ret) { return false; }
			ret = Network.Write_string(ref bw, m_password);
			if (!ret) { return false; }
			return true;
		}
	}
	public delegate void Delegate_c2s_login_version(ref T_c2s_login_version t);
	public Delegate_c2s_login_version c2s_login_version;
	public bool Reader_c2s_login_version(ref BinaryReader br, out Network.PtoBase data) {
		T_c2s_login_version t = new T_c2s_login_version();
		t._name = "c2s_login_version";
		data = t;
		return t.Read(ref br);
	}
	public bool Writer_c2s_login_version(ref Network.PtoBase data, ref BinaryWriter bw) {
		T_c2s_login_version t = (T_c2s_login_version)data;
		return t.Write(ref bw);
	}

}

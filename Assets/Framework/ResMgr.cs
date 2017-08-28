using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//��Դ����
//�ɼ���AssetBundle��Ҳ���Լ��ر��ص�Prefabs��
//˵�����������Դ�����AssetBundle��ʽ���ļ�������Ҫ��Թ̶�


//����״̬
public enum EnLoadState
{
    None = 0,
    Load,
    Success,
    Fail,
}

//��Դ����
public struct STResConfig
{
    public string sName;    //��Դ���ƣ�Ҳ���ļ�����
    public bool bLoadStart; //�Ƿ�����ʱ����
    public bool bStayMem;   //�Ƿ�פ�ڴ�
}





public class ResMgr : MgrBase 
{
    public static ResMgr It;
    void Awake() { It = this; }

    #region ��Դ�ļ�
    private Dictionary<string, Object> m_Objects;
    private EnLoadState m_enLoadState;

    public EnLoadState LoadState { get { return m_enLoadState; } }
    public delegate void LoadCallback(string sName, Object obj);

    // ��Դ��
    private Dictionary<string, Stack<GameObject>> m_Pool;
    #endregion


    public override void Init()
    {
        base.Init();
        m_Objects = new Dictionary<string, Object>();
        m_enLoadState = EnLoadState.None;
        m_Pool = new Dictionary<string, Stack<GameObject>>();
        //PreLoad(null);
    }

    public override void UnInit()
    {
        base.UnInit();
        foreach (var stack in m_Pool.Values)
        {
            foreach (var obj in stack)
            {
                DestroyObject(obj);
            }
            stack.Clear();
        }
        m_Pool.Clear();

        foreach (var obj in m_Objects.Values)
        {
            Resources.UnloadAsset(obj);
        }
        m_Objects.Clear();
    }


    // Ԥ���ػ�����Դ
    public void PreLoad(LoadCallback callback)
    {
        Object asset = LoadResource("preload", false);
        if (asset == null) return;
        TextAsset textAsset = (TextAsset)asset;
        LitJson.JsonReader jsonR = new LitJson.JsonReader(textAsset.text);
        LitJson.JsonData jsonD = LitJson.JsonMapper.ToObject(jsonR);
        if (!jsonD.IsArray || jsonD.Count == 0) return;
        // ���������Դ��ȫ��������Ϻ�ִ�лص�����
        for (int i = 0; i < jsonD.Count; i++)
        {
            LoadResource((string)jsonD[i]["name"], true);
        }
        if (callback != null)
        {
            callback(null, null);
        }
    }

    public Object GetResource(string name, bool load = true, System.Type type = null)
    {
        if (m_Objects.ContainsKey(name))
        {
            return m_Objects[name];
        }
        if (!load) return null;
        return LoadResource(name, load, type);
    }

    public Object LoadResource(string name, bool save = true, System.Type type = null)
    {
        Object asset;
        if (type == null)
        {
            asset = Resources.Load(name);
        }
        else
        {
            asset = Resources.Load(name, type);
        }
        if (save && asset != null)
        {
            m_Objects[name] = asset;
        }
        return asset;
    }

    public bool LoadResourceAsync(string name, LoadCallback cbFunc, bool save = true)
    {
        Object obj = GetResource(name, false);
        if (obj != null)
        {
            if (cbFunc != null)
            {
                cbFunc(name, obj);
            }
            return true;
        }

        ResourceRequest req = Resources.LoadAsync(name);
        if (save && req.asset != null)
        {
            m_Objects[name] = req.asset;
        }
        if (cbFunc != null)
        {
            cbFunc(name, req.asset);
        }
        return true;
    }

    public void UnLoadResource(string name)
    {
        if (!m_Objects.ContainsKey(name)) return;
        Resources.UnloadAsset(m_Objects[name]);
        m_Objects.Remove(name);
    }


    #region ��Դ��
    public GameObject CreatePrefab(string name)
    {
        if (m_Pool.ContainsKey(name))
        {
            var stack = m_Pool[name];
            if (stack.Count > 0)
            {
                return stack.Pop();
            }
        }
        GameObject obj = GetResource(name, true) as GameObject;
        return Instantiate(obj);
    }

    public void ReleasePrefab(string name, GameObject obj)
    {
        obj.SetActive(false);
        if (m_Pool.ContainsKey(name))
        {
            var stack = m_Pool[name];
            stack.Push(obj);
        }
        else
        {
            Stack<GameObject> stack = new Stack<GameObject>();
            stack.Push(obj);
            m_Pool.Add(name, stack);
        }
    }

    #endregion
}

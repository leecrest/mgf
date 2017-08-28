using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class UIMgr : MgrBase {
    public static UIMgr It;
    void Awake() { It = this; }

    public UIBase[] m_UIList;
    private Dictionary<string, UIBase> m_UI;
    private UIBase m_CurUI;

    public override void Init()
    {
        m_UI = new Dictionary<string, UIBase>();
        for (int i = 0; i < m_UIList.Length; i++)
        {
            AddUI(m_UIList[i]);
        }
        m_CurUI = null;
    }

    public void AddUI(UIBase ui)
    {
        m_UI[ui.name] = ui;
        ui.OnInit();
        ui.gameObject.SetActive(false);
    }

    public bool OpenUI(string name, bool hideOld = true, System.Action callback = null)
    {
        if (!m_UI.ContainsKey(name)) return false;
        if (m_CurUI != null && hideOld && m_CurUI.name != name)
        {
            m_CurUI.OnHide();
        }
        m_CurUI = m_UI[name];
        m_CurUI.OnShow();
        if (callback != null)
        {
            callback();
        }
        return true;
    }

    public UIBase GetUI(string name)
    {
        if (!m_UI.ContainsKey(name)) return null;
        return m_UI[name];
    }

    public void CloseUI(string name)
    {
        if (!m_UI.ContainsKey(name)) return;
        m_UI[name].OnHide();
    }

    public void GotoUI(string name, System.Action callback = null)
    {
        UIBase dst = m_UI[name];
        Transform tf = Camera.main.transform;
        Vector3 target = dst.transform.position;
        Quaternion q = dst.transform.localRotation;
        target.z = -10f;
        if (m_CurUI == null)
        {
            tf.position = target;
            tf.localRotation = q;
            m_CurUI = dst;
            dst.OnShow();
            if (callback != null)
            {
                callback();
            }
            return;
        }
        Tweener tw1 = tf.DOMove(target, 1f);
        tw1.SetUpdate(true);
        tw1.SetEase(Ease.Linear);

        Tweener tw2 = tf.DORotateQuaternion(q, 1f);
        tw2.SetUpdate(true);
        tw2.SetEase(Ease.Linear);

        Sequence seq = DOTween.Sequence();
        seq.Append(tw1);
        seq.Join(tw2);

        seq.OnComplete(delegate {
            if (m_CurUI != null)
            {
                m_CurUI.OnHide();
            }
            m_CurUI = dst;
            if (callback != null)
            {
                callback();
            }
        });
        dst.OnShow();
    }

    public void UpdateSkin()
    {
        foreach (var ui in m_UI.Values)
        {
            ui.UpdateSkin();
        }
    }
}

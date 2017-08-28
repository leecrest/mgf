using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Advertisements;

// 全局对象，单例模式
// 所有通用的功能集成在这里进行管理


// ui飞行的方向
public enum EnUIFly
{
    Left = 0,
    Right,
    Top,
    Bottom,
}

public class Globals : MonoBehaviour
{
    public static Globals It;
    void Awake() { It = this; }

    // 启动执行
    void Start()
    {
        CreateNode<ResMgr>();
        CreateNode<SoundMgr>();
        CreateNode<IapMgr>();
        CreateNode<GameCenterMgr>();
        CreateNode<UIMgr>();
        CreateNode<GameMgr>();

        ResMgr.It.Init();
        SoundMgr.It.Init();
        IapMgr.It.Init();
        GameCenterMgr.It.Init();
        UIMgr.It.Init();
        GameMgr.It.Init();
    }

    void OnDestroy()
    {
        GameMgr.It.UnInit();
        UIMgr.It.UnInit();
        GameCenterMgr.It.UnInit();
        IapMgr.It.UnInit();
        SoundMgr.It.UnInit();
        ResMgr.It.UnInit();
    }

    T CreateNode<T>() where T : Component
    {
        T link = this.GetComponentInChildren<T>();
        if (link == null)
        {
            GameObject linkObject = new GameObject(typeof(T).Name);
            link = linkObject.AddComponent<T>();
            linkObject.transform.parent = transform;
        }
        return link;
    }

    public static void SetParent(Transform parent, Transform child)
    {
        Vector3 scale = child.localScale;
        Vector3 position = child.localPosition;
        child.SetParent(parent);
        child.localScale = scale;
        child.localPosition = position;
    }

    #region 界面动画
    // 界面飞入动画
    public static void UIFlyIn(Transform tf, EnUIFly fly, System.Action action = null, float time = 0.5f)
    {
        //SoundMgr.It.SoundPlay("ui_fly_in");
        RectTransform rt = tf.GetComponent<RectTransform>();
        if (tf.DOPause() > 0)
        {
            tf.DOPlay();
            if (action != null) action();
            return;
        }
        tf.gameObject.SetActive(true);
        Vector3 target = rt.localPosition;
        Tweener tw;
        switch (fly)
        {
            case EnUIFly.Left:
                // 从左侧飞入
                rt.localPosition = new Vector3(-Screen.width - rt.rect.width, target.y);
                tw = tf.DOLocalMoveX(target.x, time);
                break;
            case EnUIFly.Right:
                // 从右侧飞入
                rt.localPosition = new Vector3(Screen.width + rt.rect.width, target.y);
                tw = tf.DOLocalMoveX(target.x, time);
                break;
            case EnUIFly.Top:
                // 从顶部飞入
                rt.localPosition = new Vector3(target.x, Screen.height + rt.rect.height);
                tw = tf.DOLocalMoveY(target.y, time);
                break;
            case EnUIFly.Bottom:
            default:
                // 从底部飞入
                rt.localPosition = new Vector3(target.x, -Screen.height - rt.rect.height);
                tw = tf.DOLocalMoveY(target.y, time);
                break;
        }
        tw.SetUpdate(true);
        tw.SetEase(Ease.Linear);
        tw.OnComplete(delegate ()
        {
            rt.localPosition = target;
            if (action != null) action();
        });
    }

    // 界面飞出动画
    public static void UIFlyOut(Transform tf, EnUIFly fly, System.Action action = null, float time = 0.5f)
    {
        //SoundMgr.It.SoundPlay("ui_fly_out");
        RectTransform rt = tf.GetComponent<RectTransform>();
        if (tf.DOPause() > 0)
        {
            tf.DOPlay();
            if (action != null) action();
            return;
        }
        Vector3 target = rt.localPosition;
        Tweener tw;
        switch (fly)
        {
            case EnUIFly.Left:
                // 从左侧飞出
                tw = tf.DOLocalMoveX(-Screen.width - rt.rect.width, time);
                break;
            case EnUIFly.Right:
                // 从右侧飞出
                tw = tf.DOLocalMoveX(Screen.width + rt.rect.width, time);
                break;
            case EnUIFly.Top:
                // 从顶部飞出
                tw = tf.DOLocalMoveY(Screen.height + rt.rect.height, time);
                break;
            case EnUIFly.Bottom:
            default:
                // 从底部飞出
                tw = tf.DOLocalMoveY(-Screen.height - rt.rect.height, time);
                break;
        }
        tw.SetUpdate(true);
        tw.SetEase(Ease.Linear);
        tw.OnComplete(delegate ()
        {
            rt.localPosition = target;
            tf.gameObject.SetActive(false);
            if (action != null) action();
        });
    }

    // 界面跳入
    public static void UIPopIn(Transform tf, System.Action action = null, float time = 0.3f)
    {
        //SoundMgr.It.SoundPlay("ui_pop_in");
        if (tf.DOPause() > 0)
        {
            tf.DOPlay();
            if (action != null) action();
            return;
        }
        Vector3 target = tf.localScale;
        tf.localScale = new Vector3(0, 0);
        tf.gameObject.SetActive(true);
        Tweener tw = tf.DOScale(1f, time);
        tw.SetUpdate(true);
        tw.SetEase(Ease.Linear);
        tw.OnComplete(delegate ()
        {
            tf.localScale = target;
            if (action != null) action();
        });
    }

    // 界面跳出
    public static void UIPopOut(Transform tf, System.Action action = null, float time = 0.3f)
    {
        //SoundMgr.It.SoundPlay("ui_pop_out");
        if (tf.DOPause() > 0)
        {
            tf.DOPlay();
            if (action != null) action();
            return;
        }
        Vector3 target = tf.localScale;
        Tweener tw = tf.DOScale(0f, time);
        tw.SetUpdate(true);
        tw.SetEase(Ease.Linear);
        tw.OnComplete(delegate ()
        {
            tf.localScale = target;
            tf.gameObject.SetActive(false);
            if (action != null) action();
        });
    }

    // 界面按钮点击音效
    public static void ButtonEffect(Transform tf, bool sound = true, System.Action callback = null)
    {
        if (sound)
        {
            SoundMgr.It.SoundPlay("ui_click");
        }
        if (tf.DOPause() > 0)
        {
            tf.DOPlay();
            return;
        }
        Tweener tw = tf.DOScale(new Vector3(0.8f, 0.8f), 0.2f);
        tw.SetUpdate(true);
        tw.SetEase(Ease.Linear);
        tw.OnComplete(delegate ()
        {
            tf.localScale = new Vector3(1, 1);
            if (callback != null) callback();
        });
    }
    #endregion

    #region 时间相关，用于保存时间的时候仅记录特定的时间差秒数，避免数据超过int范围等问题
    private static readonly System.DateTime m_TimeBase = new System.DateTime(2016, 7, 1, 0, 0, 0);

    // 获取指定时间和基准时间的时间差秒数
    public static int GetTimeOffset(ref System.DateTime time)
    {
        return (int)((time - m_TimeBase).TotalSeconds);
    }

    // 根据时间差秒数，获取时间对象
    public static System.DateTime GetTimeFromOffset(int offset)
    {
        return m_TimeBase.AddSeconds(offset);
    }
    #endregion

    #region 广告
    /*private System.Action<bool> m_AdsCallback;

    void InitAds()
    {
        if (!Advertisement.isSupported || Advertisement.isInitialized) return;
        Advertisement.Initialize(Advertisement.gameId);
    }

    public void ShowNormalAd(System.Action<bool> callback = null)
    {
        ShowAd("video", callback);
    }

    public void ShowRewardAd(System.Action<bool> callback = null)
    {
        ShowAd("rewardedVideo", callback);
    }

    private void ShowAd(string name, System.Action<bool> callback = null)
    {
        if (!Advertisement.isSupported || !Advertisement.IsReady() || Advertisement.isShowing)
        {
            if (callback != null)
            {
                callback(false);
            }
            if (!Advertisement.IsReady())
            {
                Advertisement.Initialize(Advertisement.gameId);
            }
            return;
        }
        m_AdsCallback = callback;
        ShowOptions opt = new ShowOptions();
        opt.resultCallback = HandleShowResult;
        Advertisement.Show(name, opt);
    }

    private void HandleShowResult(ShowResult ret)
    {
        if (m_AdsCallback == null)
        {
            return;
        }
        m_AdsCallback(ret == ShowResult.Finished);
        m_AdsCallback = null;
    }*/
    #endregion

    #region 事件系统
    public delegate void EventHandler(string eventName, object eventData);
    private Dictionary<string, EventHandler> m_EventMap = new Dictionary<string, EventHandler>();

    public bool AddEvent(string name, EventHandler handler)
    {
        if (handler == null) return false;
        if (m_EventMap.ContainsKey(name))
        {
            m_EventMap[name] += handler;
        }
        else
        {
            m_EventMap[name] = handler;
        }
        return true;
    }

    public bool DelEvent(string name, EventHandler handler)
    {
        if (!m_EventMap.ContainsKey(name)) return true;
        if (handler == null)
        {
            m_EventMap.Remove(name);
        }
        else
        {
            m_EventMap[name] -= handler;
        }
        return true;
    }

    public void DispatchEvent(string name, object data = null)
    {
        if (!m_EventMap.ContainsKey(name)) return;
        m_EventMap[name](name, data);
    }

    #endregion

    #region 本地文件
    public static void WriteLocalFile(string path, string name, string msg, bool append)
    {
        StreamWriter sw;
        FileInfo f = new FileInfo(path + "//" + name);
        if (!f.Exists)
        {
            sw = f.CreateText();
        }
        else
        {
            if (append)
            {
                sw = f.AppendText();
            }
            else
            {
                sw = f.CreateText();
            }
        }
        sw.WriteLine(msg);
        sw.Close();
        sw.Dispose();
    }

    #endregion
}

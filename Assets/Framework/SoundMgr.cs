using UnityEngine;
using System.Collections.Generic;

//音效管理
public class SoundMgr : MgrBase
{
    public static SoundMgr It;
    void Awake() { It = this; }

    private bool m_Music;
    public bool Sound { get; set; }
    public bool Music { get { return m_Music; } set { m_Music = value; UpdateMusic(); } }
    private Dictionary<string, AudioSource> m_Audios;
    private AudioSource m_Bgm;

    public override void Init ()
    {
        m_Music = PlayerPrefs.GetInt("Music", 1) == 1;
        Sound = PlayerPrefs.GetInt("Sound", 1) == 1;
        m_Audios = new Dictionary<string, AudioSource>();
        m_Bgm = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            AudioSource a = transform.GetChild(i).GetComponent<AudioSource>();
            m_Audios.Add(a.name, a);
            if (a.name == "bgm")
            {
                m_Bgm = a;
            }
        }
        UpdateMusic();
    }

    public override void UnInit()
    {
        PlayerPrefs.SetInt("Sound", Sound ? 1 : 0);
        PlayerPrefs.SetInt("Music", m_Music ? 1 : 0);
        m_Audios.Clear();
    }

    public void OnButtonClick()
    {
        SoundPlay("button_click");
    }

    public void SoundPlay(string name, bool exclude = false)
    {
        if (!Sound) return;
        if (!m_Audios.ContainsKey(name)) return;
        if (exclude)
        {
            foreach (string key in m_Audios.Keys)
            {
                if (key != name)
                {
                    m_Audios[key].Stop();
                }
                else
                {
                    m_Audios[key].Play();
                }
            }
        }
        else
        {
            m_Audios[name].Play();
        }
    }

    public void SoundPause()
    {
        foreach (string key in m_Audios.Keys)
        {
            m_Audios[key].Pause();
        }
    }

    public void SoundResume()
    {
        if (!Sound) return;
        foreach (string key in m_Audios.Keys)
        {
            m_Audios[key].UnPause();
        }
    }

    void UpdateMusic()
    {
        if (m_Bgm == null) return;
        if (m_Music)
        {
            
            m_Bgm.Play();
        }
        else
        {
            m_Bgm.Stop();
        }
    }


}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    // 等待加载的场景名称
    public string m_LoadName = "game";
    public Slider m_Progress;

    private AsyncOperation m_Async;

    void Start () {
        m_Progress.value = 0;
        StartCoroutine(LoadScene());
	}
	
	void Update () {
        if (m_Async == null) return;
        if (m_Async.progress < 0.9f)
        {
            m_Progress.value = m_Async.progress;
        }
        else
        {
            m_Progress.value = 1f;
            m_Async.allowSceneActivation = true;
        }
	}

    IEnumerator LoadScene()
    {
        m_Async = SceneManager.LoadSceneAsync(m_LoadName);
        m_Async.allowSceneActivation = false;
        yield return m_Async;
    }


}

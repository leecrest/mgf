using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBase : MonoBehaviour
{
    public virtual void OnInit()
    {

    }

    public virtual void OnUnInit()
    {

    }

    public virtual void OnShow()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnHide()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void UpdateSkin()
    {

    }
}

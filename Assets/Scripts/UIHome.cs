using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHome : UIBase, IPointerClickHandler {

    public override void OnInit()
    {
        base.OnInit();
    }

    public override void OnUnInit()
    {
        base.OnUnInit();
    }

    public override void OnShow()
    {
        base.OnShow();
    }

    public override void OnHide()
    {
        base.OnHide();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void UpdateSkin()
    {
        base.UpdateSkin();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIMgr.It.OpenUI("UIGame");
    }
}

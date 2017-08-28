using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ScrollPage : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    ScrollRect m_ScrollRect;
    List<float> m_PageList = new List<float>();
    int m_CurPageIdx = 0;
    int m_MaxPage = 0;

    //滑动速度
    public float m_SmootingSpeed = 10;

    //滑动的起始坐标
    float m_TargetPosH = 0;
    float m_TargetPosV = 0;

    //是否拖拽结束
    bool m_IsDrag = false;

    public System.Action<int, int> OnPageChanged;
    public System.Action<int, int> OnPageClicked;

    float m_StartTime = 0f;
    float m_Delay = 0.2f;

    void Start()
    {
        m_ScrollRect = transform.GetComponent<ScrollRect>();
        if (m_ScrollRect.horizontal)
        {
            m_ScrollRect.horizontalNormalizedPosition = 0;
        }
        if (m_ScrollRect.vertical)
        {
            m_ScrollRect.verticalNormalizedPosition = 0;
        }
        m_StartTime = Time.time;
        m_MaxPage = (byte)m_ScrollRect.content.childCount;
        m_PageList.Clear();
        for (int i = 0; i < m_MaxPage; i++)
        {
            float page = 0;
            if (m_MaxPage != 1)
            {
                page = i / ((float)(m_MaxPage - 1));
            }
            m_PageList.Add(page);
        }
    }
    
    void Update()
    {
        if (Time.time < m_StartTime + m_Delay) return;
        if (m_IsDrag) return;
        if (m_ScrollRect.horizontal)
        {
            m_ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                m_ScrollRect.horizontalNormalizedPosition,
                m_TargetPosH,
                Time.deltaTime * m_SmootingSpeed);
        }
        if (m_ScrollRect.vertical)
        {
            m_ScrollRect.verticalNormalizedPosition = Mathf.Lerp(
                m_ScrollRect.verticalNormalizedPosition,
                m_TargetPosV,
                Time.deltaTime * m_SmootingSpeed);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_IsDrag = true;
    }

    public void OnEndDrag(PointerEventData data)
    {
        m_IsDrag = false;
        if (m_ScrollRect.horizontal)
        {
            OnDragHorizontal(data.delta.x);
        }
        if (m_ScrollRect.vertical)
        {
            OnDragVertical(data.delta.y);
        }
    }

    void OnDragHorizontal(float dx)
    {
        if (Mathf.Approximately(dx, 0f)) return;

        int index = 0;
        if (dx <= -1f)
        {
            index = Math.Min(m_MaxPage - 1, m_CurPageIdx + 1);
        }
        else if (dx >= 1f)
        {
            index = Math.Max(0, m_CurPageIdx - 1);
        }

        if (index != m_CurPageIdx)
        {
            m_CurPageIdx = index;
            if (OnPageChanged != null)
            {
                OnPageChanged(m_MaxPage, m_CurPageIdx);
            }
        }
        m_TargetPosH = m_PageList[index];
    }

    void OnDragVertical(float dy)
    {
        if (Mathf.Approximately(dy, 0f)) return;

        int index = 0;
        if (dy <= -1f)
        {
            index = Math.Min(m_MaxPage - 1, m_CurPageIdx+1);
        }
        else if (dy >= 1f)
        {
            index = Math.Max(0, m_CurPageIdx - 1);
        }
        Debug.Log(">>>>" + dy + "," + index + "," + m_CurPageIdx);
        if (index != m_CurPageIdx)
        {
            m_CurPageIdx = index;
            if (OnPageChanged != null)
            {
                OnPageChanged(m_MaxPage, m_CurPageIdx);
            }
        }
        m_TargetPosV = m_PageList[index];
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (data.dragging) return;
        if (OnPageClicked != null)
        {
            OnPageClicked(m_MaxPage, m_CurPageIdx);
        }
    }
}

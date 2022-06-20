using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserInput : SingletonBehaviour<UserInput>
{
    public static System.Action _OnDown;
    public static System.Action _OnUp;
    public static System.Action _OnClick;
    public static System.Action<Vector3, Vector3> _OnDrag;

    public bool InputEnabled { get; private set; }
    public bool Dragged { get; private set; }

    private bool m_down;
    private bool m_wasDown;
    private Vector2 m_downPos;
    private Vector2 m_prevPos;
    private Vector3 m_preScreenPos;
    private Vector3 m_screenPos;

    private float m_touchRatioX;
    private float m_touchRatioY;

    private Vector2 m_touchPos { get { return new Vector2(m_touchRatioX, m_touchRatioY); } }

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
    }

    private void Start()
    {
        SetInputEnabled(true);
    }

    public static void SetInputEnabled(bool enabled)
    {
        if (null == Instance)
        {
            return;
        }
        Instance.InputEnabled = enabled;
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
        {
            m_touchRatioX = Input.mousePosition.x / Screen.width;
            m_touchRatioY = Input.mousePosition.y / Screen.height;

            m_screenPos = Input.mousePosition;
        }
        else if (Input.touches.Length == 1)
        {
            m_touchRatioX = Input.touches[0].position.x / Screen.width;
            m_touchRatioY = Input.touches[0].position.y / Screen.height;

            m_screenPos = Input.touches[0].position;
        }

        if (!InputEnabled)
        {
            return;
        }

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            m_down = Input.GetMouseButton(0);
        else
            m_down = Input.GetMouseButton(0) && !IsPointerOverUIObject();

        if (m_down && !m_wasDown)
        {
            m_downPos = m_touchPos;
            m_prevPos = m_downPos;
            m_preScreenPos = m_screenPos;
            m_wasDown = true;

            _OnDown?.Invoke();
        }

        if (m_down)
        {
            Vector2 diff = m_downPos - m_touchPos;

            if ((Dragged || (m_downPos - m_touchPos).magnitude > 0.005f) && _OnDrag != null)
            {
                Dragged = true;

                _OnDrag(m_preScreenPos, m_screenPos);
            }

            m_prevPos = m_touchPos;
            m_preScreenPos = m_screenPos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _OnUp?.Invoke();

            if ((m_downPos - m_touchPos).magnitude < 0.05f && !Dragged)
                _OnClick?.Invoke();

            Dragged = false;
            m_down = false;
            m_wasDown = false;
        }
    }

    public static bool IsPointerOverUIObject()
    {
        try
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
        }
        catch
        {
            return false;
        }
    }
}


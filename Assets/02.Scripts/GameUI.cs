using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] Text m_goldCountText;
    [SerializeField] Text m_nickNameText;
    [SerializeField] Text m_timerText;

    [SerializeField] Button m_upBtn;
    [SerializeField] Button m_downBtn;

    [HideInInspector] public int m_curGold;
    [HideInInspector] public string m_nickName;

    public int m_amount = 10;

    void Start()
    {
        Initialized();
    }

    void Initialized()
    {
        if (m_upBtn != null && m_downBtn != null)
        {
            m_upBtn.onClick.RemoveAllListeners();
            m_downBtn.onClick.RemoveAllListeners();
            m_upBtn.onClick.AddListener(UpButtonEvent);
            m_downBtn.onClick.AddListener(DownButtonEvent);
        }
    }

    public void SetGold(int gold)
    {
        m_curGold = gold;
        m_goldCountText.text = m_curGold.ToString();
    }

    public void SetNickName(string nickName)
    {
        m_nickName = nickName;
        m_nickNameText.text = m_nickName.ToString();
    }

    void UpButtonEvent()
    {
        DatabaseManager.Instance.ChangeGold(+m_amount);
    }

    void DownButtonEvent()
    {
        DatabaseManager.Instance.ChangeGold(-m_amount);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [SerializeField] Text m_goldCountText;
    [SerializeField] Text m_nickNameText;
    [SerializeField] Text m_timerText;

    [SerializeField] Button m_upBtn;
    [SerializeField] Button m_downBtn;

    [SerializeField] GameObject gunImage;
    [SerializeField] GameObject speedImage;

    [HideInInspector] public int m_curGold;
    [HideInInspector] public string m_nickName;

    [Header("플레이어 UI 리스트 (동적 생성)")]
    [SerializeField] Transform playerListParent;   //UI 패널 안에 PlayerUI 프리팹이 들어갈 위치
    [SerializeField] PlayerUI playerUIPrefab;

    [Header("플레이어 패널 (Tab으로 토글)")]
    public GameObject playerUIPanel;

    Dictionary<ulong, PlayerUI> playerUIMap = new Dictionary<ulong, PlayerUI>();

    public int m_amount = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //if (DatabaseManager.Instance != null)
        //{
        //    // 골드/닉네임 불러오기
        //    DatabaseManager.Instance.LoadGoldFromDatabase();
        //    DatabaseManager.Instance.LoadNickFromDatabase();

        //    // 모든 플레이어 데이터 불러오기
        //    DatabaseManager.Instance.LoadAllPlayersData();
        //}

        if (playerUIPanel != null) playerUIPanel.SetActive(false);

        //if (m_upBtn != null) m_upBtn.onClick.AddListener(() => DatabaseManager.Instance.ChangeGold(+m_amount));
        //if (m_downBtn != null) m_downBtn.onClick.AddListener(() => DatabaseManager.Instance.ChangeGold(-m_amount));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && playerUIPanel != null)
        {
            playerUIPanel.SetActive(!playerUIPanel.activeSelf);
        }
    }

    public void SetGold(int gold)
    {
        m_curGold = gold;
        if (m_goldCountText != null) m_goldCountText.text = m_curGold.ToString();
    }

    public void SetNickName(string nickName)
    {
        m_nickName = nickName;
        if (m_nickNameText != null) m_nickNameText.text = m_nickName;
    }

    public void RegisterPlayer(PlayerData playerData)   //전체 플레이어 UI 관리
    {
        if (playerUIMap.ContainsKey(playerData.OwnerClientId)) return;

        PlayerUI ui = Instantiate(playerUIPrefab, playerListParent);
        playerUIMap[playerData.OwnerClientId] = ui;
        UpdatePlayer(playerData);
    }

    public void UpdatePlayer(PlayerData playerData)
    {
        if (playerUIMap.TryGetValue(playerData.OwnerClientId, out var ui))
        {
            ui.SetPlayerData(playerData.Nickname.Value.ToString(), playerData.Gold.Value);
        }
        
        if (playerData.IsOwner)     //오너라면 내 UI도 업데이트
        {
            SetGold(playerData.Gold.Value);
            SetNickName(playerData.Nickname.Value.ToString());
        }
    }

    public void UnregisterPlayer(PlayerData playerData)
    {
        if (playerUIMap.TryGetValue(playerData.OwnerClientId, out var ui))
        {
            Destroy(ui.gameObject);
            playerUIMap.Remove(playerData.OwnerClientId);
        }
    }

    private PlayerData FindLocalPlayerData()    //본인 PlayerData 찾기
    {
        foreach (var pd in FindObjectsByType<PlayerData>(FindObjectsSortMode.None))
        {
            if (pd.IsOwner) return pd;
        }
        return null;
    }

    public void ShowItem(string itemType)
    {
        switch (itemType)
        {
            case "Gun":
                if (gunImage != null) gunImage.SetActive(true);
                break;
            case "Speed":
                if (speedImage != null) speedImage.SetActive(true);
                break;
        }
    }

    public void HideItem(string itemType)
    {
        switch (itemType)
        {
            case "Gun":
                if (gunImage != null) gunImage.SetActive(false);
                break;
            case "Speed":
                if (speedImage != null) speedImage.SetActive(false);
                break;
        }
    }

//    void UpButtonEvent()
//    {
//        DatabaseManager.Instance.ChangeGold(+m_amount);
//    }

//    void DownButtonEvent()
//    {
//        DatabaseManager.Instance.ChangeGold(-m_amount);
//    }
}

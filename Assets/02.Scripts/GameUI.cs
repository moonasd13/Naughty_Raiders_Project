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

    [SerializeField] Button upButton;
    [SerializeField] Button downButton;

    [SerializeField] GameObject gunImage;
    [SerializeField] GameObject speedImage;
    [SerializeField] GameObject coinImage;

    [HideInInspector] public int m_curGold;
    [HideInInspector] public string m_nickName;

    [Header("플레이어 UI 리스트")]
    [SerializeField] Transform playerListParent;
    [SerializeField] PlayerUI playerUIPrefab;

    [SerializeField] private Transform uiParent;
    [SerializeField] private PlayerUI2 playerUIPrefab2;

    [Header("플레이어 패널 (Tab으로 토글)")]
    public GameObject playerUIPanel;

    Dictionary<ulong, PlayerUI> playerUIMap = new Dictionary<ulong, PlayerUI>();

    private PlayerData myPlayer;

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
        if (playerUIPanel != null) playerUIPanel.SetActive(false);

        upButton.onClick.AddListener(OnClickUp);
        downButton.onClick.AddListener(OnClickDown);
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Tab) && playerUIPanel != null)
    //    {
    //        playerUIPanel.SetActive(!playerUIPanel.activeSelf);
    //    }

    //    if (myPlayer == null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
    //    {
    //        myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerData>();
    //    }
    //}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && playerUIPanel != null)
        {
            playerUIPanel.SetActive(!playerUIPanel.activeSelf);
        }

        // 안전하게 LocalPlayer 가져오기
        if (myPlayer == null)
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.LocalClient != null &&
                NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerData>();
                if (myPlayer == null)
                {
                    Debug.LogWarning("GameUI.Update: Local PlayerObject에 PlayerData가 없습니다.");
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            myPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerData>();
        }
    }

    public void SetGold(int gold)
    {
        m_curGold = gold;
        if (m_goldCountText != null) m_goldCountText.text = " - " + m_curGold.ToString();
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
            m_goldCountText.text = " - " + playerData.Gold.Value.ToString();
            m_nickNameText.text = playerData.Nickname.Value.ToString();
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
            case "Coin":
                if (coinImage != null) coinImage.SetActive(true);
                break;
        }
    }

    public PlayerUI2 CreatePlayerUI(PlayerData player)
    {
        PlayerUI2 ui = Instantiate(playerUIPrefab2, uiParent);
        ui.SetOwner(player);
        return ui;
    }

    void OnClickUp()
    {
        if (myPlayer != null)
        {
            myPlayer.ChangeGoldServerRpc(+10); // 골드 10 증가
        }
    }

    void OnClickDown()
    {
        if (myPlayer != null)
        {
            myPlayer.ChangeGoldServerRpc(-10); // 골드 10 감소
        }
    }
}

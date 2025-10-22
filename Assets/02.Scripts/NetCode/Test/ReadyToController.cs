using DefineEnum;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class ReadyToController : NetworkBehaviour
{
    public static Dictionary<ulong, bool> playerReady = new Dictionary<ulong, bool>();
    private GameObject readyPanel;
    private Text readyTxt;
    private Text warningTxt;
    private bool localReady = false;

    private void Awake()
    {
        //UI를 미리 찾아둠
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            readyPanel = canvas.transform.Find("ReadyPanel")?.gameObject;
            readyTxt = canvas.transform.Find("ReadyPanel/ReadyText")?.GetComponent<Text>();
            warningTxt = canvas.transform.Find("ReadyPanel/WarningText")?.GetComponent<Text>();

            if (warningTxt != null) warningTxt.gameObject.SetActive(false);
        }
    }

    private void Start()
    {            
        ulong hostId = NetworkManager.Singleton.LocalClientId;

        if (IsServer)
        {
            //호스트는 기본적으로 ready 상태
            playerReady[hostId] = true; 
            Debug.Log($"Host({hostId}) Ready = true");
        }


        //Local Owner만 자신의 ReadyPanel을 켜도록
        if (IsOwner && readyPanel != null) readyPanel.SetActive(true);

        if (IsOwner)
        {
            if (IsServer)
            {
                localReady = true;
                if (readyTxt != null) readyTxt.text = "F6키를 눌러서 게임을 시작하세요!";
            }
            else
            {
                if (readyTxt != null) readyTxt.text = "F5키를 눌러서 준비하세요!";
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return; 

        //클라이언트용: F5로 준비/해제
        if (!IsServer && Input.GetKeyDown(KeyCode.F5))
        {
            localReady = !localReady;
            OnLocalReadyChanged(localReady);
            ToggleReadyServerRpc(localReady);
        }

        //호스트용: F6으로 게임 시작 시도
        if (IsServer && Input.GetKeyDown(KeyCode.F6))
        {
            TryStartGame();
        }
    }

    private void OnLocalReadyChanged(bool isReady)
    {
        if (readyTxt == null) return;
        if (isReady) readyTxt.text = "준비 완료!";
        else readyTxt.text = "F5키를 눌러서 준비하세요!";
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(bool readyState, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        ToggleReady(clientId, readyState);

        UpdateReadyClientRpc(clientId, readyState); //UI 업데이트
    }

    private void ToggleReady(ulong clientId, bool readyState)
    {
        playerReady[clientId] = readyState;
        Debug.Log($"Client {clientId} Ready = {readyState}");
    }

    [ClientRpc]
    private void UpdateReadyClientRpc(ulong targetId, bool readyState)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        //자기 자신만 갱신
        if (targetId != localId) return;

        var canvas = GameObject.Find("Canvas");
        var readyTxt = canvas.transform.Find("ReadyPanel/ReadyText")?.GetComponent<Text>();

        if (readyTxt == null) return;
        if (readyState) readyTxt.text = "준비 완료!";
        else readyTxt.text = "F5키를 눌러서 준비하세요!";
    }

    private void TryStartGame()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReady.ContainsKey(id) || !playerReady[id])
            {
                Debug.Log($"아직 준비 안 된 플레이어 있음! (ClientId={id})");
                ShowWarningClientRpc("아직 준비되지 않은 플레이어가 있습니다!");
                return;
            }
        }
        Debug.Log("모든 플레이어 준비 완료! 게임 시작!");
        HideReadyPanelClientRpc();
        GameManger.Instance.GameStateChange(GameState.firstTime);
    }

    [ClientRpc]
    private void ShowWarningClientRpc(string msg)
    {
        if (!IsServer) return;

        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var warning = canvas.transform.Find("ReadyPanel/WarningText")?.GetComponent<Text>();
        if (warning != null)
        {
            warning.text = msg;
            warning.gameObject.SetActive(true);
            StartCoroutine(HideWarningAfterDelay(warning, 3f));
        }
    }

    private System.Collections.IEnumerator HideWarningAfterDelay(Text warningTxt, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (warningTxt != null) warningTxt.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void HideReadyPanelClientRpc()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        var panel = canvas.transform.Find("ReadyPanel")?.gameObject;
        if (panel != null) panel.SetActive(false);
    }
}
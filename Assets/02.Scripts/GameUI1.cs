using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI1 : MonoBehaviour
{
    [Header("멀티플레이어 UI")]
    public List<PlayerUI1> playersUI; //4명 분 UI

    //특정 index에 userId 연결
    public void SetPlayerUI(int index, string userId)
    {
        if (index < 0 || index >= playersUI.Count) return;
        playersUI[index].userId = userId;
    }

    //플레이어 데이터 갱신
    public void UpdatePlayerData(string userId, string nickName, int gold)
    {
        var playerUI = playersUI.Find(p => p.userId == userId);
        if (playerUI != null)
        {
            if (!string.IsNullOrEmpty(nickName))
                playerUI.SetNickName(nickName);

            playerUI.SetGold(gold);
        }
    }
}

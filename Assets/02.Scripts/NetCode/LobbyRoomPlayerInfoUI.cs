using TMPro;
using UnityEngine;

public class LobbyRoomPlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNickName;

    public void SetPlayer(PlayerNameUI player)
    {
        _playerNickName.text = player.GetPlayerName();
    }
}

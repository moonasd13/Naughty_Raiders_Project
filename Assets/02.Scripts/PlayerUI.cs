using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Text nickNameText;
    [SerializeField] Text goldText;

    public void SetPlayerData(string nick, int gold)
    {
        nickNameText.text = nick;
        goldText.text = gold.ToString();
    }
}


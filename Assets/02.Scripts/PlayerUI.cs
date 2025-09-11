using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Text nickNameText;
    [SerializeField] Text goldText;

    public void SetPlayerData(string nick, int gold)
    {
        if (nickNameText != null) nickNameText.text = nick;
        if (goldText != null) goldText.text = gold.ToString();
    }
}


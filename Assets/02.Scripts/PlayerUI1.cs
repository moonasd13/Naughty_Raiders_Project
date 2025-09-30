using UnityEngine;
using UnityEngine.UI;

public class PlayerUI1 : MonoBehaviour
{
    //닉네임과 골드를 표시할 TextMeshPro 텍스트
    public Text nickNameText;
    public Text goldText;

    //해당 UI가 연결된 플레이어의 userId
    [HideInInspector] public string userId;

    //닉네임 업데이트
    public void SetNickName(string name)
    {
        if (nickNameText != null) nickNameText.text = name;
    }

    //골드 업데이트
    public void SetGold(int amount)
    {
        if (goldText != null) goldText.text = amount.ToString();
    }
}


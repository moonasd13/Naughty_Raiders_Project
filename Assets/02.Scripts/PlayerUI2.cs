using Define_Enums;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI2 : MonoBehaviour
{
    [SerializeField] private GameObject gunImage;
    [SerializeField] private GameObject speedImage;
    [SerializeField] private GameObject coinImage;

    private PlayerData owner;

    public void SetOwner(PlayerData player)
    {
        owner = player;
        HideAll();
    }

    public void UpdateItem(Define_Enums.ItemKind itemKind)
    {
        HideAll();

        switch (itemKind)
        {
            case Define_Enums.ItemKind.Gun:
                gunImage.SetActive(true);
                break;
            case Define_Enums.ItemKind.Speed:
                speedImage.SetActive(true);
                break;
        }
    }

    public void HideAll()
    {
        gunImage.SetActive(false);
        speedImage.SetActive(false);
    }

    public void UpdateItemUI(int itemType)
    {
        gunImage.gameObject.SetActive(itemType == 1);
        speedImage.gameObject.SetActive(itemType == 2);
    }

    public void ShowCoin()
    {
        coinImage.SetActive(true);
    }

    public void HideCoin()
    {
        coinImage.SetActive(false);
    }
}

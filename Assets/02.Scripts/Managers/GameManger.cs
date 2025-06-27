using UnityEngine;
using UnityEngine.AI;

public class GameManger : MonoBehaviour
{
    [SerializeField]
    [Header("�÷��̾� ��������")]
    public Transform _Room01_StartPos;
    public Transform _Room02_StartPos;
    public Transform _Room03_StartPos;
    public Transform _Room04_StartPos;

    void Start()
    {
        InitPlayer();
    }

void Update()
    {
        
    }


    /// <summary>
    /// �÷��̾� ����
    /// </summary>
    private void InitPlayer()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Player/PlayerEx");
        if (prefab != null)
        {
            Instantiate(prefab, _Room01_StartPos.position, _Room01_StartPos.rotation);
        }
        else
        {
            Debug.LogError("�������� ã�� �� �����ϴ�.");
        }

    }

    public void GameStart()
    {

    }
}

using UnityEngine;
using UnityEngine.AI;

public class GameManger : MonoBehaviour
{
    [SerializeField]
    [Header("플레이어 시작지점")]
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
    /// 플레이어 생성
    /// </summary>
    private void InitPlayer()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Player/Player01");
        if (prefab != null)
        {
            Instantiate(prefab, _Room01_StartPos.position, _Room01_StartPos.rotation);
        }
        else
        {
            Debug.LogError("프리팹을 찾을 수 없습니다.");
        }

    }

    public void GameStart()
    {

    }
}

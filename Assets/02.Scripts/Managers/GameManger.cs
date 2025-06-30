using UnityEngine;
using UnityEngine.AI;

public class GameManger : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField]
    public GateController gateController;
    public ItemSpawner itemSpawner;
    [SerializeField]
    [Header("플레이어 시작지점")]
    public Transform[] Room_StartPoses;
    public GameObject[] Room_Chests;

    private int Room01_Score = 0;
    private int Room02_Score = 0;
    private int Room03_Score = 0;
    private int Room04_Score = 0;
    private bool countingEnd = false;

    void Start()
    {
        Room01_Score = 0;
        Room02_Score = 0;
        Room03_Score = 0;
        Room04_Score = 0;
        InitPlayer();
        itemSpawner.SpawnerOn();
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
            Instantiate(prefab, Room_StartPoses[0].position, Room_StartPoses[0].rotation);
        }
        else
        {
            Debug.LogError("프리팹을 찾을 수 없습니다.");
        }

    }

    /// <summary>
    /// 점수 계산
    /// </summary>
    private void ScoreCounting()
    {
        int[] scores = new int[4];

        for (int i = 0; i < Room_Chests.Length; i++)
        {
            foreach (Transform child in Room_Chests[i].transform)
            {
                RoomItemBox box = child.GetComponent<RoomItemBox>();
                if (box != null)
                {
                    scores[i] += box.boxCount;
                }
            }
        }

        Room01_Score = scores[0];
        Room02_Score = scores[1];
        Room03_Score = scores[2];
        Room04_Score = scores[3];

        countingEnd = true;

        if (countingEnd == true)
        {
            Debug.LogFormat($"{Room01_Score}, {Room02_Score}, {Room03_Score}, {Room04_Score}");
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 120, 40), "Click Me"))
        {
            ScoreCounting();
        }
    }

}

using UnityEngine;
using UnityEngine.AI;

public class GameManger : MonoBehaviour
{
    [Header("������Ʈ")]
    [SerializeField]
    public GateController gateController;
    public ItemSpawner itemSpawner;
    [SerializeField]
    [Header("�÷��̾� ��������")]
    public Transform[] Room_StartPoses;
    public GameObject[] Room_Chests;

    private int Room01_Score = 0;
    private int Room02_Score = 0;
    private int Room03_Score = 0;
    private int Room04_Score = 0;
    private bool countingEnd = false;

    void Start()
    {
        GameSatart();
    }

void Update()
    {
        
    }

    /// <summary>
    /// �÷��̾� ����
    /// </summary>
    private void InitPlayer()
    {
        GameObject prefab01 = Resources.Load<GameObject>("Prefabs/Player/Player01");
        GameObject prefab02 = Resources.Load<GameObject>("Prefabs/Player/Player02");
        GameObject prefab03 = Resources.Load<GameObject>("Prefabs/Player/Player03");
        GameObject prefab04 = Resources.Load<GameObject>("Prefabs/Player/Player04");

        if (prefab01 != null)
        {
            Instantiate(prefab01, Room_StartPoses[0].position, Room_StartPoses[0].rotation);
            //Instantiate(prefab02, Room_StartPoses[1].position, Room_StartPoses[1].rotation);
            //Instantiate(prefab03, Room_StartPoses[2].position, Room_StartPoses[2].rotation);
            //Instantiate(prefab04, Room_StartPoses[3].position, Room_StartPoses[3].rotation);
        }
        else
        {
            Debug.LogError("�������� ã�� �� �����ϴ�.");
        }

    }

    /// <summary>
    /// ���� ���
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

    /// <summary>
    /// ���� ���۽� ����
    /// </summary>
    private void GameSatart()
    {
        Room01_Score = 0;
        Room02_Score = 0;
        Room03_Score = 0;
        Room04_Score = 0;
        InitPlayer();
        itemSpawner.SpawnerOn();
    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 120, 40), "Click Me"))
        {
            ScoreCounting();
        }
    }

}

using UnityEngine;
using System.Collections;
using DefineEnum;

public class GateController : MonoBehaviour
{
    [SerializeField]
    [Header("초기구역")]
    public GameObject GateWalls;
    public float targetHeight = 5.0f;
    public float moveSpeed = 2.0f;

    public void OpenGate()
    {
        foreach (Transform child in GateWalls.transform)
        {
            StartCoroutine(MoveUp(child));
        }

    }

    private IEnumerator MoveUp(Transform gatePart)
    {
        Vector3 startPos = gatePart.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y + targetHeight, startPos.z);

        while (Vector3.Distance(gatePart.position, endPos) > 0.01f)
        {
            gatePart.position = Vector3.MoveTowards(gatePart.position, endPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        gatePart.position = endPos;
        
        // 렌더러 비활성화
        Renderer renderer = gatePart.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;
        // 콜라이더 비활성화
        Collider col = gatePart.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
        GameManger.Instance.GameStateChange(GameState.secondTime);
    }
}

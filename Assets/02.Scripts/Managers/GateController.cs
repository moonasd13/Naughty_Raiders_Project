using DefineEnum;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GateController : NetworkBehaviour
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

        if (IsServer)
        {
            DestroyGateServerRpc();
            GameManger.Instance.GameStateChange(GameState.secondTime);
        }
        GameManger.Instance.GameStateChange(GameState.secondTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyGateServerRpc()
    {
        foreach (Transform child in GateWalls.transform)
        {
            var netObj = child.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(child.gameObject);
            }
        }
    }
}

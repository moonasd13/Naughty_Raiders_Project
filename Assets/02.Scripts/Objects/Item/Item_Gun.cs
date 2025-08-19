using Unity.Netcode;
using UnityEngine;

public class Item_Gun : ItemObject
{
    [SerializeField]
    public GameObject bullet;
    public Transform bulletPos;

    [ServerRpc]
    public override void UseServerRpc(Vector3 direction)
    {
        Vector3 pos = bulletPos.transform.position;
        Quaternion fireRotation = Quaternion.LookRotation(direction);

        GameObject itemObj = Instantiate(bullet, pos, fireRotation);
        var netObj = itemObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        Bullet codebullet = netObj.GetComponent<Bullet>();
        codebullet.SetDirection(direction);
    }
}

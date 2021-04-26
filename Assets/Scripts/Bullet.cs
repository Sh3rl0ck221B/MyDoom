using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            return;
        }

        if (collision.transform.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerMovement>().hit();
            photonView.RPC("kill", RpcTarget.All);
        }
    }

    [PunRPC]
    void kill()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            return;
        }
        
        photonView.RPC(nameof(kill), RpcTarget.All);

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

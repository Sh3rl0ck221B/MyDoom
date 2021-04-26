using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public void Respawn()
    {
        Invoke(nameof(Spawn),5);
    }

    private void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(22, 5, 60), Quaternion.identity, 0);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class InitializingNetworkManager : NetworkManager
{
    public GameObject networkMessageManager;

    public void Start()
    {
        gameObject.SetActive(true);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, new Vector3(), new Quaternion());
        // do player init stuff
        Debug.Log("PlayerControllerId: " + playerControllerId);
        player.GetComponent<StateMachineInputManager>().networkMessageManager = networkMessageManager.GetComponent<NetworkMessageManager>();
        Camera.main.GetComponent<CameraController>().Player1 = player;
        Camera.main.GetComponent<CameraController>().Player2 = player;
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}

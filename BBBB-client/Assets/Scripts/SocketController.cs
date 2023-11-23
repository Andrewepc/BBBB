using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

public class SocketController : MonoBehaviour
{
    WebSocket socket;

    public List<PlayerData> playerData;
    public UnityEvent clientConnected;
    
    // Start is called before the first frame update
    void Start()
    {

        socket = new WebSocket("ws://localhost:8080");
        socket.Connect();
        clientConnected.Invoke();

        //WebSocket onMessage function
        socket.OnMessage += (sender, e) =>
        {

            //If received data is type text...
            if (e.IsText)
            {
                //Debug.Log("IsText");
                //Debug.Log(e.Data);
                PlayerData tempPlayerData = JsonUtility.FromJson<PlayerData>(e.Data);
                tempPlayerData.position = new Vector3(tempPlayerData.position.x * -1, tempPlayerData.position.y, tempPlayerData.position.z * -1);
                
                playerData[1].Copy(tempPlayerData);
                //Debug.Log(playerData[1].position.x);
                return;

            }

        };

        //If server connection closes (not client originated)
        socket.OnClose += (sender, e) =>
        {
            Debug.Log(e.Code);
            Debug.Log(e.Reason);
            Debug.Log("Connection Closed!");
        };
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (socket == null || playerData == null)
        {
            return;
        }


        //If player is correctly configured, begin sending player data to server
        if (playerData[0] != null && playerData[0].id != "")
        {

            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            //Debug.Log(timestamp);
            playerData[0].timestamp = timestamp;

            string playerDataJSON = JsonUtility.ToJson(playerData[0]);
            socket.Send(playerDataJSON);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            string messageJSON = "{\"message\": \"Some Message From Client\"}";
            socket.Send(messageJSON);
        }
    }

    private void OnDestroy()
    {
        //Close socket when exiting application
        socket.Close();
    }

}
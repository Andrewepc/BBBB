using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

public class SocketController : MonoBehaviour
{
    WebSocket socket;
    private List<Message> messages = new List<Message>();
    private string localId;
    public Dictionary<string, PlayerData> playerData;
    public UnityEvent<string> clientConnected;
    public UnityEvent<string> opponentConnected;

    class Message
    {
        public string type;
        public string payload;
    }

    private void CheckMessages()
    {
        if (messages.Count > 0)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].type == "setupId")
                {

                    localId = messages[i].payload;
                    clientConnected.Invoke(localId);
                    continue;
                }
                if (messages[i].type == "gameData")
                {
                    continue;
                    Dictionary<string, PlayerData> gameData = JsonUtility.FromJson<Dictionary<string, PlayerData>>(messages[i].payload);
                    //tempPlayerData.position = new Vector3(tempPlayerData.position.x * -1, tempPlayerData.position.y, tempPlayerData.position.z * -1);
                    foreach (KeyValuePair<string, PlayerData> entry in gameData)
                    {
                        if (playerData[entry.Key] != null)
                        {
                            playerData[entry.Key].Copy(entry.Value);
                            continue;
                        }
                        opponentConnected.Invoke(entry.Key);
                        playerData[entry.Key].Copy(entry.Value);
                    }
                    continue;
                }
            }
            messages.Clear();
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        socket = new WebSocket("ws://localhost:8080");
        socket.Connect();
        
        
        //WebSocket onMessage function
        socket.OnMessage += (sender, e) =>
        {
            
            //If received data is type text...
            if (e.IsText)
            {
                messages.Add(JsonUtility.FromJson<Message>(e.Data));

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

        CheckMessages();

        if (socket == null || playerData == null || !playerData.ContainsKey(localId))
        {
            return;
        }


        //If player is correctly configured, begin sending player data to server
        if (playerData[localId].id != "")
        {

            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            //Debug.Log(timestamp);
            playerData[localId].timestamp = timestamp;

            string playerDataJSON = JsonUtility.ToJson(playerData[localId]);
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
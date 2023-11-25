using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NativeWebSocket;
using UnityEngine.Networking;

public class SocketController : MonoBehaviour
{
    WebSocket socket;
    private List<PlayerData> messages = new List<PlayerData>();
    private string localId = "";
    public Dictionary<string, PlayerData> playerData;
    public UnityEvent<string> clientConnected;
    public UnityEvent<string> opponentConnected;
    public UnityEvent<string> opponentDisconnected;
    public UnityEvent death;
    private Config configs = new Config();
    private bool connected;
    class Config
    {
        public string ws_uri = "";
    }

    private void CheckMessages()
    {
        if (messages.Count > 0)
        {
            for (var i = 0; i < messages.Count; i++)
            {
                
                if (messages[i].timestamp == 0 && localId == "")
                {
                    
                    localId = messages[i].id;
                    clientConnected.Invoke(localId);
                    continue;
                }
                if (messages[i].timestamp > 0d)
                {
                    if (playerData.ContainsKey(messages[i].id))
                    {
                        playerData[messages[i].id].Copy(messages[i]);
                        Debug.Log(playerData[messages[i].id].movingSpeed);
                        continue;
                    }
                    opponentConnected.Invoke(messages[i].id);
                    playerData[messages[i].id].Copy(messages[i]);
                    continue;
                    //Debug.Log(messages[i].payload);
                    //continue;
                    //Dictionary<string, PlayerData> gameData = JsonUtility.FromJson<Dictionary<string, PlayerData>>(messages[i].payload);
                    ////tempPlayerData.position = new Vector3(tempPlayerData.position.x * -1, tempPlayerData.position.y, tempPlayerData.position.z * -1);
                    //foreach (KeyValuePair<string, PlayerData> entry in gameData)
                    //{
                    //    if (playerData[entry.Key] != null)
                    //    {
                    //        playerData[entry.Key].Copy(entry.Value);
                    //        continue;
                    //    }
                    //    opponentConnected.Invoke(entry.Key);
                    //    playerData[entry.Key].Copy(entry.Value);
                    //}
                    //continue;
                }
                if (messages[i].timestamp == -1) 
                {
                    Debug.Log("HI");
                    opponentDisconnected.Invoke(messages[i].id);
                };
            }
            messages = new List<PlayerData>();
        }
    }
    // Start is called before the first frame update

    IEnumerator GetPath()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "config.json");

        string result;
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            
            UnityWebRequest www = new UnityWebRequest(filePath);
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();
            Debug.Log("TEST: " + filePath);
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.result);
                result = www.error;
            }
            else
            {
                Debug.Log("TEST: " + www.downloadHandler);
                result = www.downloadHandler.text;
                Debug.Log("TEST: " + filePath);
            }
                
            Debug.Log("TEST: " + filePath);
        }
        else
        {
            result = System.IO.File.ReadAllText(filePath);
        }
        Debug.Log("Loaded file: " + result);
        configs = JsonUtility.FromJson<Config>(result);
    }
     void Start()
    {

        StartCoroutine(GetPath());
        
    }
    async void Update()
    {
        if (configs.ws_uri == "") return;
        if (!connected)
        {
            connected = true;
            Debug.Log("DONE?");
            socket = new WebSocket(configs.ws_uri);


            //WebSocket onMessage function
            socket.OnMessage += (e) =>
            {

                //If received data is type text...
                string msg = System.Text.Encoding.UTF8.GetString(e);

                messages.Add(JsonUtility.FromJson<PlayerData>(msg));
                //Debug.Log(messages[0].id);
            };

            //If server connection closes (not client originated)
            socket.OnClose += (e) =>
            {
                Debug.Log(e);
                Debug.Log("Connection Closed!");
                death.Invoke();
            };
            InvokeRepeating("SendWebSocketMessage", 0.0f, 0.016f);

            // waiting for messages
            await socket.Connect();
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!connected) return;
#if !UNITY_WEBGL || UNITY_EDITOR
            socket.DispatchMessageQueue();
#endif
        CheckMessages();

        if (localId == null) return;

        if (socket == null || playerData == null || !playerData.ContainsKey(localId))
        {
            return;
        }


        //If player is correctly configured, begin sending player data to server
        if (playerData[localId].id != "")
        {

            //System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            //double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            ////Debug.Log(timestamp);
            //playerData[localId].timestamp = timestamp;

            string playerDataJSON = JsonUtility.ToJson(playerData[localId]);
            
            socket.SendText(playerDataJSON);
        }

    }

    async void SendWebSocketMessage()
    {
        if (socket.State == WebSocketState.Open)
        {
            if (localId == null) return;

            if (socket == null || playerData == null || !playerData.ContainsKey(localId))
            {
                return;
            }


            //If player is correctly configured, begin sending player data to server
            if (playerData[localId].id != "")
            {

                //System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
                //double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
                ////Debug.Log(timestamp);
                //playerData[localId].timestamp = timestamp;

                string playerDataJSON = JsonUtility.ToJson(playerData[localId]);
                await socket.SendText(playerDataJSON);
            }
        }
    }

    private async void OnApplicationQuit()
    {
        await socket.Close();
    }

}
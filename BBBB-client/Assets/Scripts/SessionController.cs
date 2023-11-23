using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SessionController : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;
    [SerializeField]
    private Canvas Canvas;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private SocketController socketController;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    // GameObject opponentPlayer;
    private Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();
    // Start is called before the first frame update

    public void onClientConnected(string localId)
    {
        //Debug.Log(localId);
        players[localId] = Instantiate(prefab, new Vector3(500, 5, 500), Quaternion.identity);
        players[localId].GetComponent<PlayerController>().Setup(Canvas, Camera, players[localId].AddComponent<InputController>());
        playersData[localId] = players[localId].GetComponent<PlayerController>().playerData;
        playersData[localId].id = localId;
        Camera.GetComponent<FollowTarget>().Target = players[localId].transform;
        
        
    }
    public void onOpponentConnected(string oppId)
    {
        players[oppId] = Instantiate(prefab, new Vector3(502, 5, 500), Quaternion.identity);
        players[oppId].GetComponent<PlayerController>().Setup(Canvas, Camera);
        playersData[oppId] = players[oppId].GetComponent<PlayerController>().playerData;
        playersData[oppId].id = oppId;

    }
    public void onOpponentDisconnected(string oppId)
    {
        Destroy(players[oppId]);
        players.Remove(oppId);
        playersData.Remove(oppId);

    }
    public void onDeath()
    {
        SceneManager.LoadScene("Dead");
    }
        void Start()
    {
        
        socketController.playerData = playersData;

    }

    // Update is called once per frame
    void Update()
    {
        //playersData[1].position = new Vector3(0, 5, 0);
        //if (localPlayer != null)
        //{
        //    socketController.playerData.xPos = localPlayer.transform.position.x;
        //    socketController.playerData.yPos = localPlayer.transform.position.y;
        //    socketController.playerData.zPos = localPlayer.transform.position.z;
        //}
        
    }
}

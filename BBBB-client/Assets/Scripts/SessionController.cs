using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    //private GameObject localPlayer;
    // GameObject opponentPlayer;
    private Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();
    // Start is called before the first frame update

    public void onClientConnected(string localId)
    {
        Debug.Log(localId);
        GameObject localPlayer = Instantiate(prefab, new Vector3(500, 5, 500), Quaternion.identity);
        localPlayer.GetComponent<PlayerController>().Setup(Canvas, Camera, localPlayer.AddComponent<InputController>());
        playersData[localId] = localPlayer.GetComponent<PlayerController>().playerData;
        playersData[localId].id = localId;
        Camera.GetComponent<FollowTarget>().Target = localPlayer.transform;
        
        
    }
    public void onOpponentConnected(string oppId)
    {
        GameObject opponentPlayer = Instantiate(prefab, new Vector3(502, 5, 500), Quaternion.identity);
        opponentPlayer.GetComponent<PlayerController>().Setup(Canvas, Camera);
        playersData[oppId] = opponentPlayer.GetComponent<PlayerController>().playerData;
        playersData[oppId].id = oppId;

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

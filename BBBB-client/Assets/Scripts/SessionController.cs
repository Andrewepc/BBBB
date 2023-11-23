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
    private GameObject localPlayer;
    private GameObject opponentPlayer;
    private List<PlayerData> playersData = new List<PlayerData>();
    // Start is called before the first frame update

    public void onClientConnected()
    {
        
        localPlayer = Instantiate(prefab, new Vector3(500, 5, 500), Quaternion.identity);
        localPlayer.GetComponent<PlayerController>().Setup(Canvas, Camera, localPlayer.AddComponent<InputController>());
        playersData.Add(localPlayer.GetComponent<PlayerController>().playerData);
        Camera.GetComponent<FollowTarget>().Target = localPlayer.transform;
        
        opponentPlayer = Instantiate(prefab, new Vector3(502, 5, 500), Quaternion.identity);
        opponentPlayer.GetComponent<PlayerController>().Setup(Canvas, Camera);
        playersData.Add(opponentPlayer.GetComponent<PlayerController>().playerData);
        
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

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
    // Start is called before the first frame update
    void Start()
    {
        GameObject agent = Instantiate(prefab, new Vector3(500,2,500), Quaternion.identity);
        agent.GetComponent<PlayerController>().Setup(Canvas, Camera);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

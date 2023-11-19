using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;

    public float movementSpeed = 6;
    public float gravity = 10f;
    private Vector3 movingSpeed = new Vector3();
    private float fallingSpeed;
    // Start is called before the first frame update

    private void PhysicsUpdate()
    {
        
        if (characterController.isGrounded) fallingSpeed = 0;
        else fallingSpeed -= gravity * Time.fixedDeltaTime;

        characterController.Move(Time.fixedDeltaTime * (movingSpeed + transform.up * fallingSpeed));
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PhysicsUpdate();
    }
}

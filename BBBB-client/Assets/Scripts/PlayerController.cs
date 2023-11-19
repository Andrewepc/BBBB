using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public Animator animator;
    public float movementSpeed = 6;
    public float acceleration = 10;
    public float jumpHeight = 10;
    public float gravity = 10f;
    public float sprintMult = 2;
    public Camera playerCam;
    public Vector3 cameraOffset = new Vector3(0, 10, -10);

    private float fallingSpeed;

    private byte lastInputStates = 0b00000000;
    private byte actionStates = 0b00000000;
    private Dictionary<KeyCode, byte> inputsToByte = new Dictionary<KeyCode, byte>
    {
        { KeyCode.Space, 0b00100000 },
        { KeyCode.LeftShift, 0b00010000 },
        { KeyCode.W, 0b00001000 },
        { KeyCode.A, 0b00000100 },
        { KeyCode.S, 0b00000010 },
        { KeyCode.D, 0b00000001 }
    };
    private Dictionary<byte, System.Func<byte, byte, bool>> inputsToActions = new Dictionary<byte, System.Func<byte, byte, bool>>
    {
        
        { 5, (i, a) => { return ((i & 0b00100000) != 0) && ((a & 0b11000000) == 0); } },
        { 4, (i, a) => { return ((i & 0b00010000) != 0) && ((a & 0b11000000) == 0); } },
        { 3, (i, a) => { i &= 0b00001010; return ((i ^ 0b00001000) == 0); } },
        { 2, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000100) == 0); } },
        { 1, (i, a) => { i &= 0b00001010; return ((i ^ 0b00000010) == 0); } },
        { 0, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000001) == 0); } }
    };
    // Start is called before the first frame update
    Vector3 movingSpeed = new Vector3();

    private int movingHash;
    private int animSpeedHash;
    private int velocityHash;
    private int jumpingHash;
    private int actionHash;
    private int triggerNumHash;
    
    private void AnimatorHashSetup()
    {
        movingHash = Animator.StringToHash("Moving");
        animSpeedHash = Animator.StringToHash("Animation Speed");
        velocityHash = Animator.StringToHash("Velocity");
        jumpingHash = Animator.StringToHash("Jumping");
        actionHash = Animator.StringToHash("Action");
        triggerNumHash = Animator.StringToHash("Trigger Number");
        animator.SetFloat(animSpeedHash, 1);
    }

    private void CheckInputs()
    {
        byte inputStates = 0b00000000;
        foreach (KeyValuePair<KeyCode, byte> entry in inputsToByte)
        {
            if (Input.GetKey(entry.Key)) inputStates += entry.Value;
        }
        
        lastInputStates = inputStates;
    }
    private void UpdateStates()
    {
        
        byte intendedActionStates = 0;
        for(int i = 7; i >= 0; i--)
        {
            intendedActionStates = (byte)(intendedActionStates << 1);
            if (inputsToActions.ContainsKey((byte)i) && inputsToActions[(byte)i](lastInputStates, actionStates)) intendedActionStates++;  
        }
        actionStates = intendedActionStates;
    }
    private void UpdatePhysics()
    {
        Vector3 facingDirection = new Vector3();
        Vector3 facingSpeed = new Vector3();
        //Debug.Log(System.Convert.ToString(actionStates, 2).PadLeft(8, '0'));
        
        facingDirection.x = (actionStates & 0b00000001) != 0 ? 1 : 0 + (actionStates & 0b00000100) != 0 ? -1 : 0;
        facingDirection.z = (actionStates & 0b00001000) != 0 ? 1 : 0 + (actionStates & 0b00000010) != 0 ? -1 : 0;
        facingDirection.Normalize();
        facingSpeed = facingDirection * movementSpeed;
        if ((actionStates & 0b00010000) != 0) 
        {
            facingSpeed *= sprintMult;
            animator.SetFloat(animSpeedHash, sprintMult);
        }
        else animator.SetFloat(animSpeedHash, 1);

        if (transform.position.y > 1.5)
        {
            fallingSpeed -= gravity * Time.fixedDeltaTime;
            actionStates |= 0b01000000;
            if (fallingSpeed < 0 && animator.GetInteger(jumpingHash) != 2) { 
                animator.SetInteger(jumpingHash, 2);
                animator.SetInteger(triggerNumHash, 1);
                animator.SetTrigger("Trigger");

            }
        }
        else 
        {
            if (movingSpeed.x < facingSpeed.x) movingSpeed.x += MathF.Min(Time.fixedDeltaTime * acceleration, facingSpeed.x - movingSpeed.x);
            else if (movingSpeed.x > facingSpeed.x) movingSpeed.x -= MathF.Min(Time.fixedDeltaTime * acceleration, movingSpeed.x - facingSpeed.x);
            if (movingSpeed.z < facingSpeed.z) movingSpeed.z += MathF.Min(Time.fixedDeltaTime * acceleration, facingSpeed.z - movingSpeed.z);
            else if (movingSpeed.z > facingSpeed.z) movingSpeed.z -= MathF.Min(Time.fixedDeltaTime * acceleration, movingSpeed.z - facingSpeed.z);
            if (movingSpeed.magnitude != 0) transform.rotation = Quaternion.LookRotation(movingSpeed);

            if (fallingSpeed < 0)
            {
                animator.SetInteger(jumpingHash, 0);
                animator.SetInteger(triggerNumHash, 1);
                animator.SetTrigger("Trigger");
            }
                

            fallingSpeed = 0;
            actionStates &= 0b10111111;
        }
        
        if ((actionStates & 0b00100000) != 0)
        {
            fallingSpeed = jumpHeight;
            if (Math.Abs(movingSpeed.x) < Math.Abs((facingDirection.x * movementSpeed))) movingSpeed.x = facingDirection.x * movementSpeed;
            if (Math.Abs(movingSpeed.z) < Math.Abs((facingDirection.z * movementSpeed))) movingSpeed.z = facingDirection.z * movementSpeed;
            
            actionStates |= 0b01000000;
            animator.SetInteger(jumpingHash, 1);
            animator.SetInteger(triggerNumHash, 1);
            animator.SetTrigger("Trigger");
        }
        
        
        characterController.Move(Time.fixedDeltaTime * (movingSpeed + transform.up * fallingSpeed));
        playerCam.transform.position = transform.position + cameraOffset;
    }

    private void UpdateAnimations()
    {
        animator.SetBool(movingHash, (actionStates & 0b00001111) != 0 && (actionStates & 0b11000000) == 0 && (actionStates & 0b00001010) != 10 && (actionStates & 0b00000101) != 5);
        animator.SetFloat(velocityHash, movingSpeed.magnitude);

        animator.SetInteger(actionHash, (actionStates & 0b00100000) != 0 ? 1 : 0);
    }
    void Start()
    {
        AnimatorHashSetup();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckInputs();
        UpdateStates();
        UpdatePhysics();
        UpdateAnimations();
    }
}

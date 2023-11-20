using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ProgressBar healthBar;
    [SerializeField]
    private Camera playerCam;
    public Vector3 cameraOffset = new Vector3(0, 10, -10);
    public float movementSpeed = 6;
    public float acceleration = 50;
    public float jumpHeight = 10;
    public float gravity = 30f;
    public float sprintMult = 2;
    public float mouseDepth = 14;
    private float fallingSpeed;
    public int health;

    private Vector3 mousePoint;
    private byte lastInputStates = 0b00000000;
    private byte actionStates = 0b00000000;
    private byte busyStates = 0;
    private Dictionary<KeyCode, byte> inputsToByte = new Dictionary<KeyCode, byte>
    {
        { KeyCode.Mouse0, 0b10000000 },
        { KeyCode.Space, 0b00100000 },
        { KeyCode.LeftShift, 0b00010000 },
        { KeyCode.W, 0b00001000 },
        { KeyCode.A, 0b00000100 },
        { KeyCode.S, 0b00000010 },
        { KeyCode.D, 0b00000001 }
    };
    private Dictionary<byte, System.Func<byte, byte, bool>> inputsToActions = new Dictionary<byte, System.Func<byte, byte, bool>>
    {
        { 7, (i, a) => { return ((i & 0b10000000) != 0) && ((a & 0b11000000) == 0); } },
        { 5, (i, a) => { return ((i & 0b00100000) != 0) && ((a & 0b11000000) == 0); } },
        { 4, (i, a) => { return ((i & 0b00010000) != 0) && ((a & 0b11000000) == 0); } },
        { 3, (i, a) => { i &= 0b00001010; return ((i ^ 0b00001000) == 0); } },
        { 2, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000100) == 0); } },
        { 1, (i, a) => { i &= 0b00001010; return ((i ^ 0b00000010) == 0); } },
        { 0, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000001) == 0); } }
    };
    private float busyTimeElapsed;
    private Dictionary<byte, float> busyTimes = new Dictionary<byte, float>
    {
        { 1, 1 }
    };
   

    // Start is called before the first frame update
    Vector3 movingSpeed = new Vector3();

    private int movingHash;
    private int animSpeedHash;
    private int velocityHash;
    private int jumpingHash;
    private int actionHash;
    private int triggerNumHash;
    
    public void Setup(Canvas cav, Camera cam)
    {
        playerCam = cam;
        healthBar.transform.SetParent(cav.transform);
        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = cam;
        }
    }

    private void AnimatorHashSetup()
    {
        movingHash = Animator.StringToHash("Moving");
        animSpeedHash = Animator.StringToHash("Animation Speed");
        velocityHash = Animator.StringToHash("Velocity");
        jumpingHash = Animator.StringToHash("Jumping");
        actionHash = Animator.StringToHash("Action");
        triggerNumHash = Animator.StringToHash("Trigger Number");
        animator.SetFloat(animSpeedHash, movementSpeed / 3);
    }

    private void CheckInputs()
    {
        Vector3 p = Input.mousePosition;
        p.z = mouseDepth;
        mousePoint = Camera.main.ScreenToWorldPoint(p);
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
    private void UpdateBusy()
    {
        if (busyStates == 0) return;
        busyTimeElapsed += Time.fixedDeltaTime;
        Debug.Log(busyTimeElapsed);
        if (busyTimeElapsed > busyTimes[busyStates]) busyStates = 0;
    }
    private void UpdatePhysics()
    {
        Vector3 facingDirection = new Vector3();
        Vector3 facingSpeed = new Vector3();
        //Debug.Log(System.Convert.ToString(actionStates, 2).PadLeft(8, '0'));
        if ((actionStates & 0b10000000) != 0 && busyStates == 0)
        {
            busyStates |= 0b00000001;
            busyTimeElapsed = 0;
            animator.SetInteger(triggerNumHash, 2);
            animator.SetTrigger("Trigger");
        }

        if (busyStates == 0)
        {
            facingDirection.x = (actionStates & 0b00000001) != 0 ? 1 : 0 + (actionStates & 0b00000100) != 0 ? -1 : 0;
            facingDirection.z = (actionStates & 0b00001000) != 0 ? 1 : 0 + (actionStates & 0b00000010) != 0 ? -1 : 0;
            animator.SetFloat(animSpeedHash, 1);
            Vector3 aimDirection = mousePoint - transform.position;
            aimDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(aimDirection);
        }
        
        facingDirection.Normalize();
        facingSpeed = facingDirection * movementSpeed;
        if ((actionStates & 0b00010000) != 0)
        {
            facingSpeed *= sprintMult;
        }

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

            if (fallingSpeed < 0)
            {
                animator.SetInteger(jumpingHash, 0);
                animator.SetInteger(triggerNumHash, 1);
                animator.SetTrigger("Trigger");
            }
                

            fallingSpeed = 0;
            actionStates &= 0b10111111;
        }

        if ((actionStates & 0b00010000) != 0 && busyStates == 0)
        {
            if (movingSpeed.magnitude != 0) transform.rotation = Quaternion.LookRotation(movingSpeed);
        }

        if ((actionStates & 0b00100000) != 0 && busyStates == 0)
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
        animator.SetFloat(animSpeedHash, movementSpeed / 4);
        animator.SetBool(movingHash, (actionStates & 0b00001111) != 0 && (actionStates & 0b11000000) == 0 && (actionStates & 0b00001010) != 10 && (actionStates & 0b00000101) != 5);
        animator.SetFloat(velocityHash, movingSpeed.magnitude / movementSpeed);

        animator.SetInteger(actionHash, (actionStates & 0b10100000) != 0 ? 1 : 0);
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
        UpdateBusy();
        UpdatePhysics();
        UpdateAnimations();
    }
}

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
    private InputController inputController;

    public float movementSpeed = 6;
    public float acceleration = 50;
    public float jumpHeight = 10;
    public float gravity = 30f;
    public float sprintMult = 2;

    
    public int health;
    private float maxHealth;

    public PlayerData playerData = new PlayerData();
    /*
    private Vector3 mousePoint;
    private byte lastInputStates = 0b00000000;
    private byte actionStates = 0b00000000;
    private byte busyStates = 0;
    */
    
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
    

    private int movingHash;
    private int animSpeedHash;
    private int velocityHash;
    private int jumpingHash;
    private int actionHash;
    private int triggerNumHash;
    
    public void plus()
    {
        playerData.position = new Vector3(500, 5, 500);
    }
    public PlayerData Setup(Canvas cav, Camera cam)
    {
        maxHealth = health;
        healthBar.transform.SetParent(cav.transform);
        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = cam;
        }
        return playerData;
    }
    public PlayerData Setup(Canvas cav, Camera cam, InputController input)
    {
        maxHealth = health;
        healthBar.transform.SetParent(cav.transform);
        if (healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.Camera = cam;
        }
        inputController = input;
        return playerData;
    }
    public void OnTakeDamage(int Damage)
    {

        if (health < 0)
        {
            health = 0;
            Debug.Log("Ded lul");
            return;
            //OnDied();
            //Agent.enabled = false;
        }
        health -= Damage;
        healthBar.SetProgress((float)health / maxHealth, 3);

        
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

    
    private void UpdateStates()
    {
        
        if (inputController != null)
        {
            playerData.mousePoint = inputController.mousePoint;
            playerData.lastInputStates = inputController.lastInputStates;
            
        }
        byte intendedActionStates = 0;
        for(int i = 7; i >= 0; i--)
        {
            intendedActionStates = (byte)(intendedActionStates << 1);
            if (inputsToActions.ContainsKey((byte)i) && inputsToActions[(byte)i](playerData.lastInputStates, playerData.actionStates)) intendedActionStates++;  
        }
        playerData.actionStates = intendedActionStates;
    }
    private void UpdateBusy()
    {
        if (playerData.busyStates == 0) return;
        busyTimeElapsed += Time.fixedDeltaTime;

        if (busyTimeElapsed > busyTimes[playerData.busyStates]) playerData.busyStates = 0;
    }
    private void UpdatePhysics()
    {
        Vector3 facingDirection = new Vector3();
        Vector3 facingSpeed = new Vector3();
        //Debug.Log(System.Convert.ToString(actionStates, 2).PadLeft(8, '0'));
        if ((playerData.actionStates & 0b10000000) != 0 && playerData.busyStates == 0)
        {
            playerData.busyStates |= 0b00000001;
            busyTimeElapsed = 0;
            OnTakeDamage(20);
            animator.SetInteger(triggerNumHash, 2);
            animator.SetTrigger("Trigger");
        }

        if (playerData.busyStates == 0)
        {
            facingDirection.x = (playerData.actionStates & 0b00000001) != 0 ? 1 : 0 + (playerData.actionStates & 0b00000100) != 0 ? -1 : 0;
            facingDirection.z = (playerData.actionStates & 0b00001000) != 0 ? 1 : 0 + (playerData.actionStates & 0b00000010) != 0 ? -1 : 0;
            animator.SetFloat(animSpeedHash, 1);
            Vector3 aimDirection = playerData.mousePoint - transform.position;
            aimDirection.y = 0;
            if (aimDirection.magnitude != 0) transform.rotation = Quaternion.LookRotation(aimDirection);
        }
        
        facingDirection.Normalize();
        facingSpeed = facingDirection * movementSpeed;
        if ((playerData.actionStates & 0b00010000) != 0)
        {
            facingSpeed *= sprintMult;
        }

        if (transform.position.y > 0)
        {
            playerData.fallingSpeed -= gravity * Time.fixedDeltaTime;
            playerData.actionStates |= 0b01000000;
            if (playerData.fallingSpeed < 0 && animator.GetInteger(jumpingHash) != 2) { 
                animator.SetInteger(jumpingHash, 2);
                animator.SetInteger(triggerNumHash, 1);
                animator.SetTrigger("Trigger");

            }
        }
        else 
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            if (playerData.movingSpeed.x < facingSpeed.x) playerData.movingSpeed.x += MathF.Min(Time.fixedDeltaTime * acceleration, facingSpeed.x - playerData.movingSpeed.x);
            else if (playerData.movingSpeed.x > facingSpeed.x) playerData.movingSpeed.x -= MathF.Min(Time.fixedDeltaTime * acceleration, playerData.movingSpeed.x - facingSpeed.x);
            if (playerData.movingSpeed.z < facingSpeed.z) playerData.movingSpeed.z += MathF.Min(Time.fixedDeltaTime * acceleration, facingSpeed.z - playerData.movingSpeed.z);
            else if (playerData.movingSpeed.z > facingSpeed.z) playerData.movingSpeed.z -= MathF.Min(Time.fixedDeltaTime * acceleration, playerData.movingSpeed.z - facingSpeed.z);

            if (playerData.fallingSpeed < 0)
            {
                animator.SetInteger(jumpingHash, 0);
                animator.SetInteger(triggerNumHash, 1);
                animator.SetTrigger("Trigger");
            }


            playerData.fallingSpeed = 0;
            playerData.actionStates &= 0b10111111;
        }

        if ((playerData.actionStates & 0b00010000) != 0 && playerData.busyStates == 0)
        {
            if (playerData.movingSpeed.magnitude != 0) transform.rotation = Quaternion.LookRotation(playerData.movingSpeed);
        }

        if ((playerData.actionStates & 0b00100000) != 0 && playerData.busyStates == 0)
        {
            playerData.fallingSpeed = jumpHeight;
            if (Math.Abs(playerData.movingSpeed.x) < Math.Abs((facingDirection.x * movementSpeed))) playerData.movingSpeed.x = facingDirection.x * movementSpeed;
            if (Math.Abs(playerData.movingSpeed.z) < Math.Abs((facingDirection.z * movementSpeed))) playerData.movingSpeed.z = facingDirection.z * movementSpeed;

            playerData.actionStates |= 0b01000000;
            animator.SetInteger(jumpingHash, 1);
            animator.SetInteger(triggerNumHash, 1);
            animator.SetTrigger("Trigger");
        }

        transform.position += (Time.fixedDeltaTime * (playerData.movingSpeed + transform.up * playerData.fallingSpeed));
        //characterController.Move(Time.fixedDeltaTime * (movingSpeed + transform.up * fallingSpeed));Debug.Log(transform.position.y);
    }

    private void UpdateAnimations()
    {
        animator.SetFloat(animSpeedHash, movementSpeed / 4);
        animator.SetBool(movingHash, (playerData.actionStates & 0b00001111) != 0 && (playerData.actionStates & 0b11000000) == 0 && (playerData.actionStates & 0b00001010) != 10 && (playerData.actionStates & 0b00000101) != 5);
        animator.SetFloat(velocityHash, playerData.movingSpeed.magnitude / movementSpeed);

        animator.SetInteger(actionHash, (playerData.actionStates & 0b10100000) != 0 ? 1 : 0);
    }
    void Start()
    {
        AnimatorHashSetup();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = playerData.position;
        
        UpdateStates();
        UpdateBusy();
        
        UpdatePhysics();
        
        UpdateAnimations();
        playerData.position = transform.position;
        
    }
    private void OnDestroy()
    {
        Debug.Log("LUL");
        Destroy(healthBar.gameObject);
    }
}

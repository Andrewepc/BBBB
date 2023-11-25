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
    private ScoreCounter scroeCounter;
    [SerializeField]
    private InputController inputController;

    public float movementSpeed = 6;
    public float acceleration = 50;
    public float jumpHeight = 20;
    public float gravity = 50f;
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
    private Dictionary<byte, float> busyTimes = new Dictionary<byte, float>
    {
        { 1, 0.66f }
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
        playerData.health = health;
        playerData.position = transform.position;
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
        playerData.health = health;
        playerData.position = transform.position;
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
            playerData.aimDirection = inputController.mousePoint - transform.position;
            playerData.aimDirection.y = 0;
            playerData.aimDirection.Normalize();
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
        playerData.busyTimeElapsed += Time.fixedDeltaTime;
        if (playerData.busyTimeElapsed > busyTimes[playerData.busyStates]) playerData.busyStates = 0;
    }
    private void UpdatePhysics()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
        double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
        
        float latency = (float)(timestamp - playerData.timestamp);
        latency = Time.fixedDeltaTime;
        //Debug.Log(Time.fixedDeltaTime + "  |  " + latency);
        Vector3 facingDirection = new Vector3();
        Vector3 facingSpeed = new Vector3();
        float pTime1 = Time.fixedDeltaTime;
        if ((playerData.actionStates & 0b10000000) != 0 && playerData.busyStates == 0)
        {
            playerData.busyStates |= 0b00000001;
            playerData.busyTimeElapsed = 0;
            //OnTakeDamage(20);
            animator.SetInteger(triggerNumHash, 2);
            animator.SetTrigger("Trigger");
        }

        if (playerData.busyStates == 0)
        {
            facingDirection.x = (playerData.actionStates & 0b00000001) != 0 ? 1 : 0 + (playerData.actionStates & 0b00000100) != 0 ? -1 : 0;
            facingDirection.z = (playerData.actionStates & 0b00001000) != 0 ? 1 : 0 + (playerData.actionStates & 0b00000010) != 0 ? -1 : 0;
            animator.SetFloat(animSpeedHash, 1);
            
            if (playerData.aimDirection.magnitude != 0) transform.rotation = Quaternion.LookRotation(playerData.aimDirection);
        }
        
        facingDirection.Normalize();
        facingSpeed = facingDirection * movementSpeed;
        if ((playerData.actionStates & 0b00010000) != 0)
        {
            facingSpeed *= sprintMult;
        }

        if (transform.position.y > 0)
        {
            playerData.fallingSpeed = playerData.fallingSpeed - gravity * pTime1;
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
            if (playerData.movingSpeed.x < facingSpeed.x) 
            {
                playerData.movingSpeed.x = playerData.movingSpeed.x + MathF.Min(pTime1 * acceleration, facingSpeed.x - playerData.movingSpeed.x);
                
            }
            else if (playerData.movingSpeed.x > facingSpeed.x)
            {
                playerData.movingSpeed.x = playerData.movingSpeed.x - MathF.Min(pTime1 * acceleration, playerData.movingSpeed.x - facingSpeed.x);
                
            }
            if (playerData.movingSpeed.z < facingSpeed.z)
            {
                playerData.movingSpeed.z = playerData.movingSpeed.z + MathF.Min(pTime1 * acceleration, facingSpeed.z - playerData.movingSpeed.z);
                
            }
            else if (playerData.movingSpeed.z > facingSpeed.z)
            {
                playerData.movingSpeed.z = playerData.movingSpeed.z - MathF.Min(pTime1 * acceleration, playerData.movingSpeed.z - facingSpeed.z);
                
            }

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
            if (Math.Abs(playerData.movingSpeed.x) < Math.Abs((facingDirection.x * movementSpeed))) { playerData.movingSpeed.x = facingDirection.x * movementSpeed; playerData.movingSpeed.x = facingDirection.x * movementSpeed; }
            if (Math.Abs(playerData.movingSpeed.z) < Math.Abs((facingDirection.z * movementSpeed))) { playerData.movingSpeed.z = facingDirection.z * movementSpeed; playerData.movingSpeed.z = facingDirection.z * movementSpeed; }

            playerData.actionStates |= 0b01000000;
            animator.SetInteger(jumpingHash, 1);
            animator.SetInteger(triggerNumHash, 1);
            animator.SetTrigger("Trigger");
        }


        transform.position += (pTime1 * (playerData.movingSpeed + transform.up * playerData.fallingSpeed));
        playerData.position = transform.position;
        playerData.timestamp = timestamp;
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
        //Debug.Log("player " + playerData.movingSpeed.ToString());
        transform.position = playerData.position;
        if (playerData.health != health) OnTakeDamage(health - playerData.health);
        scroeCounter.SetScore(playerData.score);
        UpdateStates();
        UpdateBusy();
        
        UpdatePhysics();
        
        UpdateAnimations();
        playerData.health = health;
        
    }
    private void OnDestroy()
    {
        Debug.Log("LUL");
        Destroy(healthBar.gameObject);
    }
}

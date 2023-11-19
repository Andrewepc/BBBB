using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;

    public float movementSpeed = 6;
    public float jumpHeight = 10;
    public float gravity = 10f;
    
    private float fallingSpeed;

    private byte lastInputStates = 0b00000000;
    private byte actionStates = 0b00000000;
    private Dictionary<KeyCode, byte> inputsToByte = new Dictionary<KeyCode, byte>
    {
        { KeyCode.Space, 0b00100000 },
        { KeyCode.W, 0b00001000 },
        { KeyCode.A, 0b00000100 },
        { KeyCode.S, 0b00000010 },
        { KeyCode.D, 0b00000001 }
    };
    private Dictionary<byte, System.Func<byte, byte, bool>> inputsToActions = new Dictionary<byte, System.Func<byte, byte, bool>>
    {
        
        { 5, (i, a) => { return ((i & 0b00100000) != 0) && ((a & 0b11000000) == 0); } },
        { 4, (i, a) => { return ((i & 0b00001111) != 0); } },
        { 3, (i, a) => { i &= 0b00001010; return ((i ^ 0b00001000) == 0); } },
        { 2, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000100) == 0); } },
        { 1, (i, a) => { i &= 0b00001010; return ((i ^ 0b00000010) == 0); } },
        { 0, (i, a) => { i &= 0b00000101; return ((i ^ 0b00000001) == 0); } }
    };
    // Start is called before the first frame update

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
        Vector3 movingSpeed = new Vector3();
        Debug.Log(System.Convert.ToString(actionStates, 2).PadLeft(8, '0'));
        if ((actionStates & 0b00010000) != 0)
        {
            
            movingSpeed.x = (actionStates & 0b00000001) != 0 ? 1 : 0 + (actionStates & 0b00000100) != 0 ? -1 : 0;
            movingSpeed.z = (actionStates & 0b00001000) != 0 ? 1 : 0 + (actionStates & 0b00000010) != 0 ? -1 : 0;
        }
        if (movingSpeed.magnitude != 0) transform.rotation = Quaternion.LookRotation(movingSpeed);
        movingSpeed.Normalize();
        movingSpeed *= movementSpeed;

        if (transform.position.y > 1.5)
        {
            fallingSpeed -= gravity * Time.fixedDeltaTime;
            actionStates |= 0b01000000;
        }
        else 
        {
            fallingSpeed = 0;
            actionStates &= 0b10111111;
            
            
        }
        
        if ((actionStates & 0b00100000) != 0)
        {
            fallingSpeed = jumpHeight;
            actionStates |= 0b01000000;
        }
        
        
        characterController.Move(Time.fixedDeltaTime * (movingSpeed + transform.up * fallingSpeed));
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckInputs();
        UpdateStates();
        UpdatePhysics();
    }
}

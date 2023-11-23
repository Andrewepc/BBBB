using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Vector3 mousePoint;
    public byte lastInputStates = 0b00000000;
    public float mouseDepth = 14;
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

    private void CheckInputs()
    {
        if (!Application.isFocused) return;
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckInputs();
    }
}

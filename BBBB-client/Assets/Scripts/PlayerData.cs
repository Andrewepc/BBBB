using System;
using UnityEngine;


public class PlayerData 
{
    public string id;
    public Vector3 position;
    public Vector3 movingSpeed;
    public float fallingSpeed;
    public Vector3 mousePoint;
    public byte lastInputStates;
    public byte actionStates;
    public byte busyStates;
    public int health;
    public double timestamp;

    public void Copy(PlayerData newPlayerData)
    {
        id = newPlayerData.id;
        position = newPlayerData.position;
        movingSpeed = newPlayerData.movingSpeed;
        fallingSpeed = newPlayerData.fallingSpeed;
        mousePoint = newPlayerData.mousePoint;
        lastInputStates = newPlayerData.lastInputStates;
        actionStates = newPlayerData.actionStates;
        busyStates = newPlayerData.busyStates;
        health = newPlayerData.health;
        timestamp = newPlayerData.timestamp;

    }
}
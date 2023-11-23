using System;
using UnityEngine;


public class PlayerData 
{
    public string id;
    public Vector3 position;
    public Vector3 movingSpeed;
    public float fallingSpeed;
    public Vector3 aimDirection;
    public byte lastInputStates;
    public byte actionStates;
    public byte busyStates;
    public float busyTimeElapsed;
    public int health;
    public double timestamp;

    public void Copy(PlayerData newPlayerData)
    {
        id = newPlayerData.id;
        position = newPlayerData.position;
        movingSpeed = newPlayerData.movingSpeed;
        fallingSpeed = newPlayerData.fallingSpeed;
        aimDirection = newPlayerData.aimDirection;
        lastInputStates = newPlayerData.lastInputStates;
        actionStates = newPlayerData.actionStates;
        busyStates = newPlayerData.busyStates;
        busyTimeElapsed = newPlayerData.busyTimeElapsed;
        health = newPlayerData.health;
        timestamp = newPlayerData.timestamp;

    }
}
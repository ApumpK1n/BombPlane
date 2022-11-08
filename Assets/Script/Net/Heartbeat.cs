using UnityEngine;
using System;
using System.Collections;
//using PlayFab;
//using PlayFab.ClientModels;

public class Heartbeat : SingletonBehaviour<Heartbeat>
{

    public Action<Vector2Int> OnReceived;

    private float interval = 1f;

    private float timer = 1f;
    // Update is called once per frame
    public bool CanSendCmd = false;

    public Vector2Int SyncPos;
    void Update()
    {

    }

    public void PlayerClickCheckboard(Vector2Int pos)
    {
        CanSendCmd = true;
        SyncPos = pos;
    }

    public void OnReceivedShotPos(Vector2Int pos)
    {
        OnReceived?.Invoke(pos);
    }
}
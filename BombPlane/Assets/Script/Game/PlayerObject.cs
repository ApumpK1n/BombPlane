using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : NetworkBehaviour
{

    [SyncVar(hook = nameof(OnShotPosChanged))]
    public Vector2Int shotPosSynced;

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Heartbeat.Instance.CanSendCmd)
            {
                Heartbeat.Instance.CanSendCmd = false;
                CmdClickPos();
            }
        }
    }

    //服务器执行
    [Command]
    void CmdClickPos()
    {
        shotPosSynced = Heartbeat.Instance.SyncPos;
    }

    void OnShotPosChanged(Vector2Int _Old, Vector2Int _New)
    {
        if (isLocalPlayer)
        {
            return;
        }
        Debug.Log("OnShotPosChanged:" + _New);

        Heartbeat.Instance.OnReceivedShotPos(_New);
    }
}

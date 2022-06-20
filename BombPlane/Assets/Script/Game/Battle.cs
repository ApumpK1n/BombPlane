using UnityEngine;
using System.Collections;
//using PlayFab.ClientModels;

public class Battle : MonoBehaviour
{

    public EnemyGrid enemyGrid;
    public AllyGrid allyGrid;


    private void Start()
    {
        Heartbeat.Instance.OnReceived += OnReceived;
    }

    private void OnDisable()
    {
        Heartbeat.Instance.OnReceived -= OnReceived;
    }

    private void OnReceived(Vector2Int pos)
    {
        //enemyGrid.OnReceiveBattle();
        allyGrid.OnRecevie(pos);
    }

    private void OnRound()
    {

    }

    private void OnEnd()
    {

    }


}

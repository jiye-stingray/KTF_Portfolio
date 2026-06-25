using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataBase : Singleton<GameDataBase>
{
    public PlayerInfo playerInfo = new PlayerInfo();
    
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public void Init()
    {
        MyLogger.Log("GameDataBase Init");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}

using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;

public class TreasureBoxManager : MonoBehaviour
{
    private int Interval => ClientLocalDB.GetFieldConfigInt("TreasureBoxSpawnCoolTime");
    public float _time;
    private bool _isPlay = false;
    bool EnableSpawnTreasureBox => _treasureBoxData != null;

    private UserInfoData UserInfo => Managers.Instance.UserInfo();
    private Squad Squad => Managers.Instance.GetObjectUnitManager().playerSquad;
    private ClientLocalDB_Simple ClientLocalDB => Managers.Instance.GetSimpleDBManager();
    private TreasureBoxData _treasureBoxData;

    public void StartTimer()
    {
        _isPlay = true;
        _time = Interval;
        _treasureBoxData = UserInfo.GetTreasureBoxItemData();
    }

    public void Stop()
    {
        _isPlay = false;
    }
    
    void Update()
    {
        if (!EnableSpawnTreasureBox)
            return;
        
        if (!_isPlay)
            return;

        if (Managers.Instance.GetTutorialManager().isTutorialActive) return;
        
        _time -= Time.deltaTime;
        if (_time <= 0)
            SpawnTreasureBox();
    }
    
    private void SpawnTreasureBox()
    {
        if (!EnableSpawnTreasureBox)
            return;
        
        if (Squad.IsTownZone.Value)
            return;

        Stop();
        Managers.Instance.GetMapManager().CreateTreasureBox(_treasureBoxData.boxIndex).Forget();
    }
}

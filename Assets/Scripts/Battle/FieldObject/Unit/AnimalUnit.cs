using UnityEngine;

public class AnimalUnit : EnemyUnit
{
    private float idleTime;
    private const int maxIdleTime = 10;

    public override void Init()
    {
        ResetTime();
        base.Init();
    }

    public override void UpdateIdle()
    {
    }

    private void Update()
    {
        //if (!isAIDecideCor)
        //    return;

        if (_state.GetCurState() == State_Die.Instance)
            return;

        if (idleTime <= 0)
        {
            NextPosition();
            return;
        }

        idleTime -= Time.deltaTime;
    }

    public override void UpdateMove()
    {
        //LerpCellPosistion();
    }

    private void NextPosition()
    {
        int nextX = Random.Range(-1, 2);
        int nextY = Random.Range(-1, 2);

        //Vector3Int cellPosition = new Vector3Int(_cellPos.x + nextX, _cellPos.y + nextY);
        //if (MapManager.MoveTo(this, cellPosition))
        //{
        //    LerpCellPosCompleted = false;
        //    _state.ChangeState(State_Move.Instance);
        //}
        ResetTime();
    }

    private void ResetTime()
    {
        int time = Random.Range(3, maxIdleTime);
        idleTime = time;
    }
}
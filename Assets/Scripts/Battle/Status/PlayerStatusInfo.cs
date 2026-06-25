using System;
using static Define;

[Serializable]
public class PlayerStatusInfo : StatusInfo
{
    private UserInfoData _userInfoData;
    private CharacterClassItemData _characterClassItemData;
    private int Id => _characterClassItemData.id;
    public Status _synergyStatus = new Status();

    public void Init(CharacterClassItemData itemData, UserInfoData userInfoData)
    {
        _characterClassItemData = itemData;
        _userInfoData = userInfoData;

        _unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, Id);
        _statusData = ClientLocalDB_Simple.GetData<StatusData>(DBKey.PlayerStatus, Id);
        _growStatusData = ClientLocalDB_Simple.GetData<GrowStatusData>(DBKey.PlayerGrowStatus, Id);
    }

    public void SetGrade(int grade)
    {
        _grade = grade;
    }

    public void OnCalculateStatus()
    {
        Status calculateStatus = new Status();
        _growthStatus.Reset();
        
        _level = _characterClassItemData.Level;
        _gradeGrowStatusData = ClientLocalDB_Simple.GetData<GrowStatusData>(DBKey.GradeGrowthStatus, _characterClassItemData._grade);
        _levelStatus = CalculateStatus.GetPlayerLevelStatus(_level, _statusData, _growStatusData, _gradeGrowStatusData);
        SetLevelStatus();

        if (_userInfoData != null)
        {
            calculateStatus += _userInfoData._trainingStatus;
            calculateStatus += _userInfoData._constellationStatus;

            switch (_unitData.Faction)
            {
                case EFactionType.Celestial:
                    calculateStatus += _userInfoData._CelestialEquipmentStatus;
                    break;
                case EFactionType.Crusher:
                    calculateStatus += _userInfoData._CrusherEquipmentStatus;
                    break;
                case EFactionType.Guardian:
                    calculateStatus += _userInfoData._GuardianEquipmentStatus;
                    break;
                case EFactionType.Human:
                    calculateStatus += _userInfoData._HumanEquipmentStatus;
                    break;
                default:
                    break;
            }

            if (_characterClassItemData.equipRelicId != 0)
                calculateStatus += _userInfoData._relicStatus[_characterClassItemData.equipRelicId];
        }

        _growthStatus += calculateStatus;
        CalculatePercentStatus(_growthStatus);
        var beforeBattlePower = _battlePower;
        _battlePower = CalculateStatus.CalculateBattlePoint(_growthStatus, _characterClassItemData._grade, _statusData.ClassType);
        if(beforeBattlePower != _battlePower && _userInfoData != null)
            _userInfoData.AllBattlePower.Value = _userInfoData.ReturnAllBattlePower();
        ResetPlayStatus();
        ApplyPlayHealth();
    }

    public Status GetCharacterStatus()
    {
        return _growthStatus;
    }

    // PlayerStatusInfo 플레이 체력은 MAX로 초기화 되지 않는다
    public override void ResetPlayStatus()
    {
        _playStatus.Set(_growthStatus + _sumStatus + _synergyStatus);
        CalculatePercentStatus(_playStatus);
    }

    public void SetSynergyStatus(Status status)
    {
        _synergyStatus = status;
        ResetPlayStatus();
        ApplyPlayHealth();
    }
}
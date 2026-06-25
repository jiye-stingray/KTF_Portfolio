using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CharacterClassItemData : ItemData
{
    public int id;
    private UserInfoData _userInfoData;
    public bool isOpen;         // 획득 했는지 안 했는지 
    
    public int _currentCount; // 조각 갯수
    public PlayerStatusInfo _statusInfo = new PlayerStatusInfo();

    public EGradeType _grade;   // 현재 grade
    public Dictionary<ESkillSlotType, List<BaseSkill>> Skills = new Dictionary<ESkillSlotType, List<BaseSkill>>();
    public SkillBase _passiveSkill;
    public PassiveSkillDetailData _passiveSkillDetailData;
    
    public UnitData _unitData;
    public int defaultSkillLevel => 1;
    public int activeSkillLevel;
    public int equipRelicId = 0;
    
    
    private int _level = 1;
    public int Level
    {
        get
        {
            // if (_userInfoData != null && !_userInfoData.IsSimulation)
            // {
            //     if (_userInfoData.ReturnResonanceCharacter(id))
            //         return _userInfoData._resonanceLevel;
            // }
            return _level;
        }
        set => _level = value;
    }

    // 유저 세팅
    public void InitStatus(UserInfoData user)
    {
        _userInfoData = user;
        _statusInfo.Init(this, _userInfoData);
        _unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);
        
        SkillBase defaultSkillBase = ClientLocalDB_Simple.GetData<SkillBase>(DBKey.PcDefaultSkillBase, _unitData.DefaultSkills);
        List<SkillDetailData> defaultSkillDetailData = new List<SkillDetailData>();
        foreach (var detailId in defaultSkillBase.SkillDetail)
        {
            defaultSkillDetailData.Add(ClientLocalDB_Simple.GetData<SkillDetailData>(DBKey.PcDefaultSkillDetail, detailId));
        }
        List<BaseSkill> defaultSkills = new List<BaseSkill>();
        BaseSkill defaultSkill = new BaseSkill(0, defaultSkillBase, defaultSkillDetailData, 1);
        defaultSkills.Add(defaultSkill);
        
        //ActiveSkill
        List<BaseSkill> activeSkills = new List<BaseSkill>();
        List<BaseSkill> specialSkills = new List<BaseSkill>();
        for (int i = 0; i < _unitData.ActiveSkills.Length; i++)
        {
            int baseId = _unitData.ActiveSkills[i];
            if (baseId == 0)
                continue;
            
            SkillBase activeSkillBase = ClientLocalDB_Simple.GetData<SkillBase>(DBKey.PcActiveSkillBase, baseId);
            List<SkillDetailData> activeSkillDetailData = new List<SkillDetailData>();
            List<SkillDetailData> specialSkillDetailData = new List<SkillDetailData>();
            
            foreach (var detailId in activeSkillBase.SkillDetail)
            {
                SkillDetailData detailData = ClientLocalDB_Simple.GetData<SkillDetailData>(DBKey.PcActiveSkillDetail, detailId);
                if(activeSkillBase.SkillSlotType == ESkillSlotType.ActiveSkill)
                    activeSkillDetailData.Add(detailData);
                else
                    specialSkillDetailData.Add(detailData);
            }
            
            if(activeSkillBase.SkillSlotType == ESkillSlotType.ActiveSkill)
                activeSkills.Add(new BaseSkill(activeSkills.Count, activeSkillBase, activeSkillDetailData, 1));
            else
                specialSkills.Add(new BaseSkill(specialSkills.Count, activeSkillBase, specialSkillDetailData, 1));
        }
        
        if (_unitData.PassiveSkills.Length > 0)
        {
            if (_unitData.PassiveSkills[0] != 0)
            {
                _passiveSkill = ClientLocalDB_Simple.GetData<SkillBase>(DBKey.PcPassiveSkillBase, _unitData.PassiveSkills[0]);
                _passiveSkillDetailData = ClientLocalDB_Simple.GetData<PassiveSkillDetailData>(DBKey.PcPassiveSkillDetail, _passiveSkill.SkillDetail[0]);
            }
        }
        
        Skills.TryAdd(ESkillSlotType.DefaultSkill, defaultSkills);
        Skills.TryAdd(ESkillSlotType.ActiveSkill, activeSkills);
        Skills.TryAdd(ESkillSlotType.SpecialSkill, specialSkills);
    }

    // 능력치 갱신
    public void RefreshStatus()
    {
        foreach (var skill in Skills[ESkillSlotType.DefaultSkill])
        {
            skill._level = defaultSkillLevel;
        }
        
        foreach (var skill in Skills[ESkillSlotType.ActiveSkill])
        {
            skill._level = activeSkillLevel;
        }

        _statusInfo.OnCalculateStatus();
    }

    public bool IsMaxGrade()
    { 
        return _grade >= Define.ReturnMaxGrade(ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id).StartGrade);
    }
}
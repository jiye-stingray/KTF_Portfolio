using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

[Serializable]
public struct SkillDescriptionArg
{
    public ESkillType SkillType;
    public EStatus Status;
    public double Power;

    public SkillDescriptionArg(ESkillType skillType, EStatus status, double power)
    {
        SkillType = skillType;
        Status = status;
        Power = power;
    }
}

public class CharacterSkillTab : UITabBase
{
    [Header("Skill Button")] [SerializeField]
    RectTransform skillIconArea;

    [SerializeField] GameObject _select_ActiveSkill;
    [SerializeField] SkillIconButton _skillIconBtn_ActiveSkill;
    [SerializeField] GameObject _select_PassiveSkill;
    [SerializeField] SkillIconButton _skillIconBtn_PassiveSkill;
    [SerializeField] SkillIconButton _selectedSkillIconBtn;
    [SerializeField] private SkillLevelUpCostButton _skillLevelUpButton;
    [SerializeField] private GameObject _skillInfoButton;

    [Header("Skill Description")] 
    [SerializeField] TMP_Text _nameTxt;

    [SerializeField] private GameObject _coolTimeRoot;
    [SerializeField] TMP_Text _coolTimeTxt;
    [SerializeField] TMP_Text _descriptionTxt;

    [SerializeField] private GameObject _effectRateRoot;
    [SerializeField] private TMP_Text _effectRateTxt;

    public CharacterClassItemData _data;
    private BaseSkill BaseSkill => _data.Skills[_currentSkillType][0];
    private SkillBase CurrentSkillBase => BaseSkill._skillData;
    public ESkillSlotType _currentSkillType = ESkillSlotType.ActiveSkill;
    
    int _activeSkillLevel = 0;
    
    private Dictionary<int, List<SkillDescriptionArg>> _argsDic = new Dictionary<int, List<SkillDescriptionArg>>();
    private UISubSkillInfo _subSkillInfo;
    private bool _isDummyData;
    /// <summary>
    /// SkillIcon Button 에서 클릭하면 Settiing
    /// </summary>
    /// <param name="skillSlotType"></param>
    public void SettingCurrentSkillData(int skillSlotType)
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        _currentSkillType = (ESkillSlotType)skillSlotType;
        Refresh();
    }

    public override void Refresh()
    {
        DrawSkillIconButton();
        if (_currentSkillType == ESkillSlotType.ActiveSkill)
            DrawActiveSkillDescription();
        else
            DrawPassiveSkillDescription();
        
        SetSkillButton();
    }

    private void SetSkillButton()
    {
        if (_isDummyData)
        {
            _skillLevelUpButton.gameObject.SetActive(false);
            return;
        }
        
        _skillLevelUpButton.gameObject.SetActive(_currentSkillType == ESkillSlotType.ActiveSkill);
        _skillInfoButton.SetActive(_currentSkillType == ESkillSlotType.ActiveSkill);
        
        if (_currentSkillType == ESkillSlotType.PassiveSkill)
            return;

        _skillLevelUpButton.gameObject.SetActive(_data.isOpen);
        if (!_data.isOpen) return;
        CharacterLevel skillLevelUpCostData =
            ClientLocalDB_Simple.GetData<CharacterLevel>(DBKey.SkillLevelUpCost, _activeSkillLevel);
        _skillLevelUpButton.Init(skillLevelUpCostData.CurrencyID, skillLevelUpCostData.CurrencyValue);
        _skillLevelUpButton.Refresh();

        
    }

    private void DrawSkillIconButton()
    {
        SkillBase activeSkill = _data.Skills[ESkillSlotType.ActiveSkill][0]._skillData;
        SkillBase passiveSkill = _data._passiveSkill;
        _skillIconBtn_ActiveSkill.Init(ESkillSlotType.ActiveSkill, activeSkill, _activeSkillLevel);
        _skillIconBtn_PassiveSkill.gameObject.SetActive(passiveSkill != null);
        if (passiveSkill != null)
            _skillIconBtn_PassiveSkill.Init(ESkillSlotType.PassiveSkill, passiveSkill, 1);

        _select_ActiveSkill.SetActive(_currentSkillType == ESkillSlotType.ActiveSkill);
        _select_PassiveSkill.SetActive(_currentSkillType == ESkillSlotType.PassiveSkill);

        _selectedSkillIconBtn.Init(_currentSkillType,
            _currentSkillType == ESkillSlotType.ActiveSkill ? activeSkill : passiveSkill,
            _currentSkillType == ESkillSlotType.ActiveSkill ? _activeSkillLevel : 1);
    }


    private void DrawPassiveSkillDescription()
    {
        _effectRateRoot.SetActive(false);
        SkillBase passiveSkill = _data._passiveSkill;
        PassiveSkillDetailData detailData = _data._passiveSkillDetailData;
        int coolTime = passiveSkill.CoolTime;
        _coolTimeRoot.SetActive(coolTime > 0);
        _nameTxt.text = passiveSkill.Name;
        _coolTimeTxt.text = $"쿨타임 : {coolTime / 100}초";

        EStatus status = passiveSkill.SkillBaseStatus;
        string statusText = $"<color=#EA8202>{Status.ReturnStatusString(status)}</color>";
        string effectPowerText = $"<color=#EA8202>{CalculateStatus.ToPercent(detailData.EffectPower)}</color>";

        switch (detailData.PassiveSkillType)
        {
            case EPassiveSkillType.DefaultDamage:
            case EPassiveSkillType.Damage:
            case EPassiveSkillType.Buff:
            case EPassiveSkillType.Heal:
                _descriptionTxt.text = string.Format(passiveSkill.Description, statusText, effectPowerText);
                break;
            case EPassiveSkillType.CoolDown:
                _descriptionTxt.text = string.Format(passiveSkill.Description, detailData.TriggerValue);
                break;
            case EPassiveSkillType.Reflection:
                _descriptionTxt.text = string.Format(passiveSkill.Description, effectPowerText);
                break;
        }
    }


    /// <summary>
    /// 스킬 설명
    /// </summary>
    private void DrawActiveSkillDescription()
    {
        _coolTimeRoot.SetActive(true);
        _nameTxt.text = CurrentSkillBase.Name;
        _coolTimeTxt.text = $"쿨타임 : {CurrentSkillBase.CoolTime / 100f:0.##}초";
        _descriptionTxt.text = GetSkillDescriptionText(_argsDic[_activeSkillLevel]);

        int effectRate = GetEffectRate();
        _effectRateRoot.SetActive(effectRate != 0);
        _effectRateTxt.text = $"확률 : {effectRate / 100}%";
    }

    private string GetSkillDescriptionText(List<SkillDescriptionArg> args)
    {
        string description = CurrentSkillBase.Description;
        
        if (args.Count == 0)
            return description;

        List<object> formatArgs = new List<object>(args.Count * 2);

        foreach (SkillDescriptionArg arg in args)
        {
            formatArgs.Add(GetColoredStatusText(arg.Status));
            formatArgs.Add(GetColoredValueText(arg.Power));
        }

        try
        {
            return string.Format(description, formatArgs.ToArray());
        }
        catch (FormatException e)
        {
            Debug.LogError(
                $"[SkillDescription] Format Error\n" +
                $"SkillName: {CurrentSkillBase.Name}\n" +
                $"Description: {description}\n" +
                $"ArgsCount: {formatArgs.Count}\n" +
                $"{e}"
            );

            return description;
        }
    }

    private Dictionary<int, List<SkillDescriptionArg>> GetSkillDescriptionArgs()
    {
        Dictionary<int, List<SkillDescriptionArg>> result = new Dictionary<int, List<SkillDescriptionArg>>();
        SkillDetailData detailData = BaseSkill._skillDetailData[0];
        int maxSkillLevel = ReturnMaxSkillLevel();
        for (int i = 1; i <= maxSkillLevel; i++)
        {
            List<SkillDescriptionArg> args = new List<SkillDescriptionArg>();
            AppendMainSkillDescriptionArg(detailData, args, i);
            AppendDurationEffectDescriptionArgs(detailData, args, i);
            if (args.Count > 0)
                result.TryAdd(i, args);
        }
        return result;
    }

    private void AppendMainSkillDescriptionArg(SkillDetailData detailData, List<SkillDescriptionArg> result, int level)
    {
        switch (detailData.SkillType)
        {
            case ESkillType.Damage:
            case ESkillType.Heal:
                result.Add(new SkillDescriptionArg(detailData.SkillType, CurrentSkillBase.SkillBaseStatus, detailData.GetTotalEffectPower(level)));
                break;
            
            case ESkillType.GroundEffect:
            {
                GroundEffectData groundEffectData =
                    ClientLocalDB_Simple.GetData<GroundEffectData>(DBKey.GroundEffectData, detailData.SkillTypeDetail);

                if (groundEffectData == null)
                    return;

                result.Add(new SkillDescriptionArg(groundEffectData.EffectType, groundEffectData.BaseStatus, groundEffectData.GetTotalEffectPower(level)));
                break;
            }

            case ESkillType.Projectile:
            {
                ProjectileData projectileData =
                    ClientLocalDB_Simple.GetData<ProjectileData>(DBKey.Projectile, detailData.SkillTypeDetail);

                if (projectileData == null)
                    return;

                if (projectileData.EffectType == ESkillType.Damage)
                    result.Add(new SkillDescriptionArg(projectileData.EffectType, CurrentSkillBase.SkillBaseStatus, detailData.GetTotalEffectPower(level)));

                break;
            }
        }
    }

    private void AppendDurationEffectDescriptionArgs(SkillDetailData detailData, List<SkillDescriptionArg> result, int level)
    {
        if (detailData.DurationEffect == null)
            return;

        foreach (int durationId in detailData.DurationEffect)
        {
            if (durationId == 0)
                continue;

            DurationEffectData durationEffectData =
                ClientLocalDB_Simple.GetData<DurationEffectData>(DBKey.DurationEffectData, durationId);

            if (durationEffectData == null)
                continue;

            EStatus targetStatus = durationEffectData.BaseStatus == EStatus.NULL ? durationEffectData.TargetStatus : durationEffectData.BaseStatus;
            result.Add(new SkillDescriptionArg(durationEffectData.EffectType, targetStatus, durationEffectData.GetTotalEffectPower(level)));
        }
    }

    private string GetColoredStatusText(EStatus status)
    {
        return $"<color=#EA8202>{Status.ReturnStatusString(status)}</color>";
    }

    private string GetColoredValueText(double value)
    {
        return $"<color=#6A9B68>{CalculateStatus.ToPercent(value)}</color>";
    }

    public void OpenSkillInfoPopup()
    {
        if(_subSkillInfo == null)
            _subSkillInfo = UIManager.ShowUISubBase<UISubSkillInfo>(UIManager.UICharacterDetail, "UISubSkillInfo");
        _subSkillInfo.SetSkillInfo(_activeSkillLevel, _argsDic);
    }

    public void SkillLevelUp()
    {
        BestHttp_GameManager.OnPostActiveSkillLevelUp(_data._unitData.ID);
    }
    
    public void SetData(CharacterClassItemData itemData, bool isDummyData)
    {
        _data = itemData;
        _activeSkillLevel = _data.activeSkillLevel;
        _isDummyData = isDummyData;
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillIconArea);
        _currentSkillType = ESkillSlotType.ActiveSkill;
        _argsDic = GetSkillDescriptionArgs();
        SettingCurrentSkillData((int)_currentSkillType);
    }

    private int GetEffectRate()
    {
        int effectRate = 0;
        SkillDetailData detailData = BaseSkill._skillDetailData[0];
        {
            for (int i = 0; i < detailData.DurationEffect.Length; i++)
            {
                int durationId = detailData.DurationEffect[i];
                if (durationId == 0)
                    continue;
                
                DurationEffectData data = ClientLocalDB_Simple.GetData<DurationEffectData>(DBKey.DurationEffectData, durationId);
                if (data.EffectType != ESkillType.StatusEffect)
                    continue;
                
                effectRate = data.EffectRate;
            }
        }
        
        return effectRate;
    }
}

using Assets.SimpleSignIn.Google.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static Define;

public class RedDotManager
{
    #region CharacterList

    public static bool EnableCharacterLevelUp(int id)
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();
        if (!userInfo._dicCharacterItemData[id].isOpen) return false;
        int currentLevel = userInfo.GetCharacterItemData(id).Level;
        int maxLevel = ClientLocalDB_Simple.GetDB<CharacterLevel>(DBKey.CharacterLevel).Last().Value.Level;
        if (currentLevel >= maxLevel) return false;
        CharacterLevel slotLevel = ClientLocalDB_Simple.GetData<CharacterLevel>(DBKey.CharacterLevel, currentLevel);
        for (int i = 0; i < slotLevel.CurrencyID.Length; i++)
        {
            ECurrency currencyType = (ECurrency)slotLevel.CurrencyID[i];
            int value = slotLevel.CurrencyValue[i];
            int userCurrency = userInfo.GetCurrencyValue(currencyType);
            
            if(userCurrency < value)
                return false;
        }

        return true;
    }

    public static bool EnableCharacterAwakening(CharacterClassItemData itemData)
    {
        if(itemData.IsMaxGrade()) return false;
        int pieceCost = Utils.ReturnAwakenPieceCost(itemData.id, (int)itemData._grade);
        return itemData._currentCount >= pieceCost;
    }
    
    public static bool CharacterListRedDot()
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();
        foreach (var itemData in userInfo._dicCharacterItemData.Values)
        {
            if(!itemData._unitData.Live)
                continue;
            
            if(EnableCharacterLevelUp(itemData.id))
                return true;

            if (EnableCharacterAwakening(itemData))
                return true;
        }
        
        return false;
    }

    #endregion

    #region Gacha

    public static bool GachaRedDot()
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();
        for (int i = 0; i <= (int)EGachaType.Celestial; i++)
        {
            GachaGroup gachaGroup = ClientLocalDB_Simple.GetData<GachaGroup>(DBKey.GachaGroup, i);
            ECurrency currencyType = gachaGroup.GachaCurrencyType;
            int value = 1;
            int userCurrency = userInfo.GetCurrencyValue(currencyType);

            if (userCurrency >= value)
                return true;
        }
        
        return false;
    }

    #endregion

    #region Training

    public static bool BasicTrainingRedDot(TrainingItemData data)
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();
        
        ECurrency currency = data._trainingBasicData.LevelUpCostCurrency;

        if (userInfo.GetCurrencyValue(currency) < data._trainingBasicData.LevelUpCostValue)
            return false;
        else if(data._trainingBasicData.AccountLevelLimit > userInfo.userLevel.Value)
            return false;

        return data._trainingBasicData.ID == userInfo.UnlockBasicIdx + 1;
    }

    public static bool HardTrainingRedDot(TrainingItemData data)
    {

        if(data._trainingHardData == null) return false;

        UserInfoData userInfo = Managers.Instance.UserInfo();

        ECurrency currency = data._trainingHardData.LevelUpCostCurrency;

        if (userInfo.GetCurrencyValue(currency) < data._trainingHardData.LevelUpCostValue)
            return false;
        else if (userInfo.UnlockBasicIdx < data._trainingHardData.BasicTrainingLimit) 
            return false;

            return data._trainingHardData.ID == userInfo.UnlockHardIdx + 1;
    }

    public static bool TrainingRedDot()
    {
        UserInfoData userInfo = Managers.Instance.UserInfo();

        foreach (var item in userInfo._trainingItemList)
        {
            if (BasicTrainingRedDot(item)) return true;
            if (HardTrainingRedDot(item)) return true;
        }

        return false;
    }

    #endregion

    #region Inventory
    public static bool InventoryAllDecompositionBtnRedDot()
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        var decompositionItmes = UserInfoData._dicEquipmentItemData
            .Where(item => !item.Value.isSet &&
            !item.Value.isLock &&
            item.Value.data.Grade != EGradeType.Mythic &&
            (UserInfoData._dicEquipment[item.Value.data.Faction][(int)item.Value.data.Type] != null &&
            item.Value.data.Grade <= UserInfoData._dicEquipment[item.Value.data.Faction][(int)item.Value.data.Type].data.Grade))
            .ToDictionary(item => item.Key, item => item.Value);

        return decompositionItmes.Count > 0;
    }
    #endregion

    #region Quest
    public static bool QuestPointRedDot(QuestPointItemData data)
    {
        return  data.isClear && !data.isFinish;
    }

    public static bool RoutineQuestRedDot(RoutineQuestItemData data)
    {
        // 전부 클리어 했을 때는 reddot 비활성화
        if (Managers.Instance.UserInfo().RoutineQuestFinish(data._resetType)) return false;
        return data.isClear && !data.isFinish;
    }

    public static bool AllDailyRoutineQuestRedDot()
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        // 전부 클리어 했을 때는 reddot 비활성화
        if (UserInfoData.RoutineQuestFinish(EResetType.Daily)) return false;

        foreach (var item in UserInfoData._dicDailyQuestPointData.Values)
        {
            if(QuestPointRedDot(item)) return true;
        }

        foreach (var item in UserInfoData._dicDailyRoutineQuestData.Values)
        {
            if (RoutineQuestRedDot(item)) return true;
        }

        return false;
    }

    public static bool AllWeeklyRoutineQuestRedDot()
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        // 전부 클리어 했을 때는 reddot 비활성화
        if (UserInfoData.RoutineQuestFinish(EResetType.Weekly)) return false;

        foreach (var item in UserInfoData._dicWeeklyQuestPointData.Values)
        {
            if (QuestPointRedDot(item)) return true;
        }

        foreach (var item in UserInfoData._dicWeeklyRoutineQuestData.Values)
        {
            if (RoutineQuestRedDot(item)) return true;
        }

        return false;
    }
    #endregion

    #region Equipment

    public static bool AllAlterLevelRedDot()
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        // Currency 계산
        ECurrency slotCostID1 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "Essence").Value);
        ECurrency slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Cellestial").Value);      // 초기 셋팅

        foreach (var alter in UserInfoData._dicAltarLevel)
        {
            EFactionType type = alter.Key;
            int alterLevel = alter.Value;
            switch (type)
            {
                case EFactionType.Celestial:
                    slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Cellestial").Value);
                    break;
                case EFactionType.Crusher:
                    slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Crusher").Value);
                    break;
                case EFactionType.Guardian:
                    slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Guardian").Value);
                    break;
                case EFactionType.Human:
                    slotCostID2 = (ECurrency)(ClientLocalDB_Simple.GetData<EquipmentSetting>(DBKey.EquipmentSetting, "FactionCostID_Human").Value);
                    break;
                default:
                    break;
            }
            ECurrency[] currencies = new ECurrency[]
            {
                slotCostID1,
                slotCostID2
            };
            EquipmentEnchant equipmentEnchant = ClientLocalDB_Simple.GetData<EquipmentEnchant>(DBKey.EquipmentEnchant, UserInfoData._dicAltarLevel[type]);
            int[] values = new int[]
            {
                equipmentEnchant.EssenceCostValue,
                equipmentEnchant.FactionCostValue
            };

            if ((UserInfoData.GetCurrencyValue(slotCostID1) >= values[0]) &&
                (UserInfoData.GetCurrencyValue(slotCostID2) >= values[1]))
            {
                return true;
            }

        }
        return false;
    }

    public static bool AllSettingEquipmentRedDot()
    {
        foreach (EFactionType type in Enum.GetValues(typeof(EFactionType)))
        {
            if (type == EFactionType.All || type == EFactionType.None) continue;
            if(AutoSettingEquipmentRedDot(type))
                return true;
        }
        return false;
    }

    public static bool AutoSettingEquipmentRedDot(EFactionType faction)
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        if(faction == EFactionType.All)         // 장비 분해 팝업에서 호출할 때 사용
            return AllSettingEquipmentRedDot();

        for (int i = 0; i < UserInfoData._dicEquipment[faction].Length; i++)
        {
            var item = UserInfoData._dicEquipment[faction][i];
            EquipmentItemData data = null;
            if (item == null)
            {
                data = UserInfoData._dicEquipmentItemData
                        .Where(e =>
                                e.Value.data.Faction == faction &&
                                e.Value.data.Type == (EEquipmentType)i)
                                .OrderByDescending(e => e.Value.data.Grade)      // 내림차순 정렬하면 첫 값이 가장 가까움
                        .FirstOrDefault().Value;
            }
            else
            {

                data = UserInfoData._dicEquipmentItemData
                            .Where(e => e.Value.data.Grade > item.data.Grade &&
                                    e.Value.data.Faction == faction &&
                                    e.Value.data.Type == item.data.Type)
                                    .OrderByDescending(e => e.Value.data.Grade)      // 내림차순 정렬하면 첫 값이 가장 가까움
                            .FirstOrDefault().Value;

            }

            if (data != null)
                return true;
        }

        return false;   
    }

    public static bool AllDecompositionRedDot(EFactionType faction)
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();

        var decompositionItmes = UserInfoData._dicEquipmentItemData
            .Where(item => !item.Value.isSet &&
            !item.Value.isLock &&
            item.Value.data.Faction == faction &&
            item.Value.data.Grade != EGradeType.Mythic &&
            (UserInfoData._dicEquipment[faction][(int)item.Value.data.Type] != null &&
            item.Value.data.Grade <= UserInfoData._dicEquipment[faction][(int)item.Value.data.Type].data.Grade))
            .ToDictionary(item => item.Key, item => item.Value);

        return decompositionItmes.Count > 0;
    }

    public static bool EquipEquipmentAbleRedDot(EFactionType factionType, EEquipmentType equipmentType)
    {
        UserInfoData UserInfoData = Managers.Instance.UserInfo();
        var decompositionItmes = UserInfoData._dicEquipmentItemData
            .Where(item => 
                    (item.Value.data.Faction == factionType && item.Value.data.Type == equipmentType) &&
                     (UserInfoData._dicEquipment[factionType][(int)equipmentType] == null || 
                    item.Value.data.Grade > UserInfoData._dicEquipment[factionType][(int)equipmentType].data.Grade))
            .ToDictionary(item => item.Key,item =>  item.Value);

        return decompositionItmes.Count > 0;
    }       


    #endregion

    #region Dungeon

    public static bool AllDungeonRedDot()
    {
        foreach (EDungeonType item in Enum.GetValues(typeof(EDungeonType)))
        {
            if(DungeonRedDot(item))
                return true;
        }
        return false;
    }

    public static bool DungeonRedDot(EDungeonType dungeonType)
    {
        UserInfoData userinfoData = Managers.Instance.UserInfo();
        switch (dungeonType)
        {
            case EDungeonType.Gold:
                return userinfoData.GetCurrencyValue(ECurrency.AdmissionTicket_GoldDungeon) > 0;
            case EDungeonType.Equipment:
                return userinfoData.GetCurrencyValue(ECurrency.AdmissionTicket_EquipmentDungeon) > 0;
            default:
                break;
        }

        return false;
    }

    #endregion

    #region Constellation

    public static bool AllConstellationRedDot()
    {
        UserInfoData userinfoData = Managers.Instance.UserInfo();
        foreach (EConstellationBoardType item in Enum.GetValues(typeof(EConstellationBoardType)))
        {
            if (TypeConstellationRedDot(item)) return true;
        }
        return false;
    }

    public static bool TypeConstellationRedDot(EConstellationBoardType type)
    {
        UserInfoData userinfoData = Managers.Instance.UserInfo();

        // 대형 노트 오픈되어 있지 않을때 Lock
        ConstellationItemData itemData = userinfoData.GetConstellationItemData(
                ClientLocalDB_Simple.GetData<ConstellationBoard>(DBKey.ConstellationBoard, (int)type).OpenCondition);
        if (itemData != null && !itemData._isOpen)
            return false;

        var boradConstellationItemData = userinfoData._dicConstellationItemData.Where(c => (c.Value.data.BoardID) == (int)type)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        
        foreach (var item in boradConstellationItemData.Values)
        {
            if(ConstellationRedDot(item)) return true;
        }
 
        return false;
    }

    private static bool ConstellationRedDot(ConstellationItemData data)
    {
        UserInfoData userinfoData = Managers.Instance.UserInfo(); 

        ConstellationPrice price = ClientLocalDB_Simple.GetData<ConstellationPrice>(DBKey.ConstellationPrice, data.data.StarSize);
        if (userinfoData.GetCurrencyValue(ECurrency.StarPiece) < price.CostValue)
            return false;

        if (data.data.PreviousNode == 0 && !data._isOpen)
            return true;

        if (!data._isOpen && userinfoData.GetConstellationItemData(data.data.PreviousNode)._isOpen)
            return true;

        return false;
    }

    #endregion

    #region Guild
    public static bool AllGuildRedDot()
    {
        if (GuildAttendanceRedDot() == true)
            return true;

        return false;

    }
    public static bool GuildAttendanceRedDot()
    {
        UserInfoData userinfoData = Managers.Instance.UserInfo();

        string serverTimeStr = ServerTime.Instance.CurrentTime().ToString("yyyy-MM-dd");
        string joinTime = Convert.ToDateTime(userinfoData.guildUserInfo.joinDate).ToString("yyyy-MM-dd");

        bool IsTodayJoin = Utils.GetDateTimeCompare(joinTime, serverTimeStr) == 0;
        bool EnableAttendance = Utils.GetDateTimeCompare(userinfoData.guildUserInfo.attendanceDate, serverTimeStr) < 0;

        return !IsTodayJoin && EnableAttendance;
    }
    #endregion

    #region Mail
    public static bool AllMailRedDot()
    {
        if (NomalMailRedDot() == true)
            return true;

        if (PayMailRedDot() == true)
            return true;

        return false;
    }
    public static bool NomalMailRedDot()
    {
        if (Managers.Instance.UserInfo().nomalMailItemList.Count > 0)
            return true;
        return false;
    }
    public static bool PayMailRedDot()
    {
        if (Managers.Instance.UserInfo().payMailItemList.Count > 0)
            return true;
        return false;
    }
    #endregion

    #region Pass
    public static bool AllPassRedDot()
    {
        foreach (var item in Managers.Instance.UserInfo()._dicPassItem.Values)
        {
            if (PassBannerRedDot(item)) return true;
        }
        return false;
    }
    public static bool PassRewardRedDot(Pass pass, bool isPreimum)
    {
        UserInfoData userinfo = Managers.Instance.UserInfo();
        PassItemData passItemData = userinfo.GetPassItemData(pass.PassType);
        bool getPre = true;
        int getPassLevel = 0;
        if (isPreimum)
        {
            getPre = passItemData.isPremium;
            getPassLevel = passItemData.premiumGetLevel;
        }
        else
            getPassLevel = passItemData.freeGetLevel;

        return (getPre) && pass.PassLevel <= passItemData.passLevel && pass.PassLevel > getPassLevel;
    }

    public static bool PassBannerRedDot(PassItemData passItemData)
    {
        if (passItemData.data.OpenLevel > Managers.Instance.UserInfo().userLevel.Value) return false;
        return passItemData.passLevel > passItemData.freeGetLevel  || (passItemData.isPremium && passItemData.passLevel > passItemData.premiumGetLevel);
    }


    #endregion

    #region Field

    public static bool FieldQuestRewardRedDot(FieldQuestReward fieldQuestRewardItemData)
    {
        UserInfoData  userinfo = Managers.Instance.UserInfo();
        return userinfo._dicFieldItemData[fieldQuestRewardItemData.FieldID].progress >= fieldQuestRewardItemData.ClearCount
            && !userinfo._dicFieldItemData[fieldQuestRewardItemData.FieldID].isGet[fieldQuestRewardItemData.Index];
    }

    #endregion

    #region Event

    public static bool AllOpenEventQuestRedDot()
    {
        
        for (int i = 1; i <= 7; i++)
        {
            if (Managers.Instance.UserInfo().OpenEventCurrentDay < i) break;
            if (OpenEventQuestRedDot(i))
                return true;
        }

        return false;
    }

    public static bool OpenEventQuestRedDot(int day)
    {
        var userinfo = Managers.Instance.UserInfo();
        if (!userinfo._dicOpenEventQuestItemData.TryGetValue(day, out var dayDict))
            return false;

        return dayDict.Values.Any(item => item.isClear && !item.isFinish);
    }

    #endregion

}

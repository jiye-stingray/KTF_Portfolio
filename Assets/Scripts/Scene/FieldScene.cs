using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Define;

public class FieldScene : FieldBaseScene
{
    [SerializeField]
    private int characterId = 1;
    private FieldInfo _fieldInfo;
    private FieldDetail _fieldDetail;
    
    private int TownZoneIndex => _fieldInfo.TownIndex;
    
    IEnumerator Start()
    {
        var loadingUI = GameObject.FindObjectOfType<LoadingUI>();
        Init();
        yield return null;
        _fieldInfo = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, UserInfo._fieldId);
        _fieldDetail = ClientLocalDB_Simple.GetData<FieldDetail>(DBKey.FieldDetail, $"{_fieldInfo.ID}_{UserInfo.CurrentDifficultyLevel}");
        yield return LoadFieldMap(_fieldInfo, _fieldDetail).ToCoroutine();
        yield return null;
        UIManager.MainInfoUI.Init();
        
        yield return null;
        BattleData battleData = BattleData.Get();
        Squad.Init(Managers.Instance.UserInfo());
        Squad.SetResurrectionState(true);
        Squad.SetDistanceValue(500, 1800, 1800);
        if (battleData == null)
        {
            int zoneIndex = GetStartFieldID();
            Squad.SquadMove(zoneIndex, Managers.Instance.GetMapManager().GetStartPosition(zoneIndex)).Forget();
        }
        else
            Squad.SquadMove(UserInfo.zoneId, UserInfo.squadPosition).Forget();
        
        Squad.IsTownZone.Value = Squad._zoneIndex == TownZoneIndex;
        FollowCamera.LookAtNow();
        yield return null;
        Managers.Instance.GetMapManager()._townIndex = TownZoneIndex;
        Managers.Instance.GetMapManager().CreateTownPortal(Squad.transform.position).Forget();
        
        yield return null;
        yield return SpawnCharacter(EContent.Field, EFactionType.None).ToCoroutine();
        SpawnPortal();
        Squad.SetGameOverAction(OpenGameOverPopup);
        yield return null;
        yield return CreateDamageText().ToCoroutine();
        yield return null;
        
        Managers.Instance.Sound.PlayBGMAsync(_fieldInfo.BGM).Forget();
        if (loadingUI != null)
            yield return loadingUI.FadeOutAndHideAsync().ToCoroutine();
        
        yield return null;
        Squad.BattleStart();
        MapManager.CheckMapData(Squad._zoneIndex);
        SyncCurrencyManager.StartTimer();
        TreasureBoxManager.StartTimer(); // 보물상자
        
        Managers.Instance.GetEconomySystem().SetPause(true);

        // setting ( 기본 셋팅이 모두 끝난 후 호출)
        UserInfo.InitSetting();

        Managers.Instance.GetTutorialManager().Init();

#if IAP
        Managers.Instance.InitializeIAP_FromDB();
#endif

        //공지 사항 (타이틀 씬에서 넘어온 경우에만 띄운다.)
        if (UserInfo._showNoticePopup)
        {
            UserInfo._showNoticePopup = false;
            yield return StartCoroutine(NoticePopupCoroutine());
        }

        // 최초 1회 공지 팝업
        if (UserInfo._isFirstLoginToday)
        {
            UserInfo._isFirstLoginToday = false;
            yield return UIManager.AttendancePopupCoroutine().ToCoroutine();
        }
        else
        {
            if (battleData != null)
            {
                if (battleData._contentType == EContent.Fog)
                {
                    BuildingInfo fogDungeonBuildingInfo = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo).Values.ToList().Find(building => 
                        building.BuildingConditionType == EBuildingConditionType.DungeonClear &&
                        building.CurrencyList[0] == battleData._index);
                    BuildingInfo fogBuildingInfo = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo).Values.ToList()
                        .Find(building => building.BuildOpenConditionType == EBuildingOpenConditionType.BuildingOpen &&
                                          building.BuildOpenConditionValue == fogDungeonBuildingInfo.ID);
                    BuildingData dungeonData = UserInfo.GetInstallationBuilding(fogDungeonBuildingInfo.ID);
                    dungeonData._isOpening = false;
                    if (dungeonData._isBuild.Value)
                        yield return StartCoroutine(MapManager.DungeonOpenCoroutine(fogBuildingInfo.ID));
                }
                else if (battleData._contentType == EContent.FieldDungeon)
                {
                    BuildingInfo fieldDungeonBuildingInfo = ClientLocalDB_Simple.GetDB<BuildingInfo>(DBKey.BuildingInfo).Values.ToList().Find(building => 
                        building.BuildingConditionType == EBuildingConditionType.DungeonClear &&
                        building.CurrencyList[0] == battleData._index);
                    
                    BuildingData dungeonData = UserInfo.GetInstallationBuilding(fieldDungeonBuildingInfo.ID);
                    dungeonData._isOpening = false;
                    if (dungeonData._isBuild.Value)
                        yield return StartCoroutine(MapManager.FieldDungeonOpenCoroutine(fieldDungeonBuildingInfo.ID));
                }
                else
                    UIManager.MainInfoUI.GoNavigation(battleData._contentType, battleData._factionType.ToString());
            }
        }
        
        // 최초 Dialogure체크
        if(UserInfo.dialogKey.Value == 0)
            Squad.StartDialogue(1);


        // tutorial 최초 체크
        yield return new WaitForSeconds(0.5f);      // side menu setting 시간 기다림
        
        // 리뷰 조건 체크 필요 (임시. 추후 모든 조건 체크 & 튜토리얼 id로 변경 ) 
        Managers.Instance.GetTutorialManager().CheckQuestTutorial();
    }

    private void SpawnPortal()
    { 
        if(!UserInfo._enablePortal.Value)
            return;

        if (!UserInfo._portalData.active)
            return;
        
        int fieldId = UserInfo._fieldId;
        
        if (fieldId != UserInfo._portalData.fieldInfoId)
            return;
        
        Vector2 position = new Vector2(UserInfo._portalData.x, UserInfo._portalData.y);
        Managers.Instance.GetMapManager().SpawnTownPortal(UserInfo._portalData.zoneId, position);
    }

    private void OpenGameOverPopup()
    {
        Squad.BattleStop();
        Managers.Instance.GetUIManager().AllCloseStackUI();
        UIResurrectionPopup resurrectionPopup = UIManager.UIResurrectionPopup;
        resurrectionPopup.InitData();
        resurrectionPopup.OpenToStack();
    }
    
    private void GetUserData()
    {
        CommandManager.Instance.AddCommand("GetUserData.php", "", ResponseUserData);
    }
    
    private void ResponseUserData(int result, string data)
    {
        if (result == 1)
        {
            var userData =  JsonConvert.DeserializeObject<DummyUserData>(data);
            MyLogger.Log(userData.userName);
        }
    }


    IEnumerator NoticePopupCoroutine()
    {
        if (GameNoticePopup.ShouldShowToday())
        {
            var noticePopup = UIManager.ShowPopup<GameNoticePopup>("GameNoticePopup");
            yield return new WaitUntil(() => noticePopup == null);
        }
        yield return null;
    }

    private int GetStartFieldID()
    {
        if (UserInfo._fieldId == 1)
            return UserInfo.dialogKey.Value >= 2 ? TownZoneIndex : 27;
        
        return TownZoneIndex;
    }
}

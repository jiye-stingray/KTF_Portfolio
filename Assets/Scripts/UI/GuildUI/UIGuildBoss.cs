using Cysharp.Threading.Tasks;
using I2.Loc;
using PolyAndCode.UI;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class UIGuildBoss : UISubBase, IRecyclableScrollRectDataSource
{
    [SerializeField] private UIGuildBossGradeWidget gradeWidget;

    [SerializeField] private TMP_Text txtBossName;
    [SerializeField] private SpineAnimation spineAnimation;
    [SerializeField] private TMP_Text txtTimeToInit;
    [SerializeField] private UIGaugeWidget bossHpGauge;

    [SerializeField] private SpineAnimation bossSpineAnimation;
    [SerializeField] private RecyclableScrollRect scrollView;
    [SerializeField] private UIButtonBase btn_Play;
    [SerializeField] private TMP_Text txtTryCount;
    [SerializeField] private UITimer _timer;
    List<RankingItemData> _dataList = new List<RankingItemData>();

    UIGuildRanking guildRanking;

    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    UIManager UImanager => Managers.Instance.GetUIManager();
    UserInfoData userInfo => Managers.Instance.UserInfo();

    /*
     * *
     */
    GuildDungeon _guildDungeonData;
    private ScheduleDto _guildBossScheduleDto;
    private EBossScheduleType _bossScheduleType;

    #region ScrollView
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as UIGuildBossRankingScrollItem;
        item.SetData(_dataList[index], index);
    }
    #endregion

    public void Init(ScheduleDto guildBossScheduleDto)
    {
        _guildBossScheduleDto = guildBossScheduleDto;
        _bossScheduleType =  CheckBossScheduleType(_guildBossScheduleDto);
        
        DateTime endTime = DateTime.Parse(_guildBossScheduleDto.endTime);
        DateTime now = ServerTime.Instance.CurrentTime();
        TimeSpan durationTimeSpan = endTime - now;

        TimeData timeData = new TimeData();
        timeData.SetByDuration(durationTimeSpan.TotalSeconds);
        _timer.RegisterOnFinished(delegate { RefreshUI(); });
        _timer.Set(timeData, "{0} 남음");

        RefreshUI();
        
        _dataList.Clear();
        bestHttp_GameManager.OnGetGuildBossInfo((guildBossInfo) =>
        {
            userInfo.currentGuildBossDto = guildBossInfo;
            gradeWidget.SetData(guildBossInfo.step);
            // 마지막 step 인 경우에는 hpgauge 비활성화
            bossHpGauge.gameObject.SetActive(BOSS_MAX_GRADE > guildBossInfo.step);
            if (BOSS_MAX_GRADE > guildBossInfo.step)
            {
                bossHpGauge.Refresh((float)guildBossInfo.hp / guildBossInfo.maxHp, $"{guildBossInfo.hp} / {guildBossInfo.maxHp}");
            }

            _guildDungeonData = ClientLocalDB_Simple.GetData<GuildDungeon>(DBKey.GuildDungeon, guildBossInfo.step);

            bestHttp_GameManager.OnGetGuildBossRanking((rankingList) =>
            {
                _dataList = rankingList;
                scrollView.Initialize(this);
            });
            Refresh();
        });
    }

    public override void Refresh()
    {
        base.Refresh();
        gradeWidget.Refresh();

        // Spine
        UnitData bossUnit = ClientLocalDB_Simple.GetData<UnitData>(DBKey.MonsterCharacter,
        ClientLocalDB_Simple.GetData<SpawnObjectGroupData>(DBKey.SpawnObjectGroup,
        ClientLocalDB_Simple.GetData<SpawnPointInfoData>(DBKey.SpawnPointInfo, _guildDungeonData.SpawnPointInfo).SpawnObjectGroup).ObjectList.First());

        SetCharacterSpine(bossUnit).Forget();

        // Boss Name
        txtBossName.text = bossUnit.Name;
        scrollView.ReloadData();
    }

    private async UniTask SetCharacterSpine(UnitData unitData)
    {
        spineAnimation.gameObject.SetActive(false);
        
        string spineName = unitData.Resource;
        SkeletonDataAsset skeletonData = await Managers.Instance.GetResObjectManager().LoadAsync<SkeletonDataAsset>($"{spineName}/{spineName}_SkeletonData.asset");

        if (skeletonData == null)
            return;
        
        spineAnimation.gameObject.SetActive(true);
        spineAnimation.SetSpine(skeletonData);
        spineAnimation.SetAnimation(Define.ObjectAnimationName.IDLE,true);
    }

    private void RefreshUI()
    {
        btn_Play.SetGray(_bossScheduleType > EBossScheduleType.OnGoing || !userInfo.guildUserInfo.canTry);
        txtTryCount.text = userInfo.guildUserInfo.canTry == false ? $"0/1" : $"1/1";
    }
    
    public void OnClickGuildBossPlay()
    {
        UImanager.UIDeckSetting.InitContentType(EContent.GuildBoss, userInfo.currentGuildBossDto.step);
        UImanager.UIDeckSetting.OpenToStack();
    }

    public void OnClickGuildBossGray()
    {
        if(_bossScheduleType == EBossScheduleType.End)
            UImanager.ShowCommonToastMessage("길드 보스 입장이 불가능한 시간입니다.");
        else if(!userInfo.guildUserInfo.canTry)
            UImanager.ShowCommonToastMessage("입장 횟수가 부족합니다.");
    }

    public void OnClickGuildRanking()
    {
        if (guildRanking == null)
            guildRanking = UIManager.ShowUISubBase<UIGuildRanking>(UIManager.UIGuildHome, "UIGuildRanking");
        guildRanking.OpenToStack();
    }

    UISubRankingDungeonRewardInfo subUi;
    public void ClickRewardInfoBtn()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        if (subUi != null)
        {
            subUi.ClickCloseBtn();
            subUi = null;
        }
        subUi = UIManager.ShowUISubBase<UISubRankingDungeonRewardInfo>(UImanager.UIGuildHome, "UISubRankingDungeonRewardInfo");
        subUi.SetDungeonType(DungeonRewardType.Guild);
        subUi.OpenToStack();
    }
}

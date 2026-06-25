using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static Define;

public class GuildInfoListPage : UIBase, IRecyclableScrollRectDataSource
{
    [SerializeField] TMP_InputField _guildNameInput;
    [SerializeField] private RecyclableScrollRect _scrollview;
    bool _inputChanged = false;

    private int searchPage; //현재 페이지
    private int searchMaxPage; //최대 페이지
    private string defaultRichText = ""; //검색 된 길드 이름

    List<GuildInfoItemData> _dataList = new List<GuildInfoItemData>();
    List<GuildInfoDto> guildList;
    GuildInfoListScrollviewItem selectedGuild = null;
    BestHttp_GameManager bestHttp_GameManager => Managers.Instance.GetServerManager();
    UserInfoData userInfo => Managers.Instance.UserInfo();
    UIGuildInfoJoin guildInfoPopup;
    UIGuildCreate guildCreate;

    void Start()
    {
        _guildNameInput.onSubmit.AddListener(OnSerchSubmit);
    }
    /*
     * *
     */
    #region Recycle Scrollview
    public int GetItemCount()
    {
        return _dataList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        var item = cell as GuildInfoListScrollviewItem;
        item.SetData(_dataList[index], index, ScrollItemClick);
    }
    #endregion

    public override void Open()
    {
        base.Open();
        RecommendGuildList();
        RequestGuildList();
        Refresh();        
        _inputChanged = false;

        defaultRichText = string.Empty;
        _guildNameInput.text = defaultRichText;

    }
    public override void Close()
    {
        base.Close();
        selectedGuild = null;
    }
    public override void Refresh()
    {
        base.Refresh();
        _scrollview.ReloadData();
    }

    //// 추천길드 리스트
    public void RecommendGuildList()
    {
        bestHttp_GameManager.OnGetRecommendGuildList((a) =>
        {
            _dataList.Clear();

            if (Managers.Instance.GetLoadingUI() != null)
                Managers.Instance.GetLoadingUI().Hide();
            guildList = a;
            for (int i = 0; i < guildList.Count; i++)
            {
                GuildInfoItemData data = new GuildInfoItemData();
                data.id = guildList[i].id;
                data.name = guildList[i].name;
                data.mark = guildList[i].guildPattern;
                data.color = guildList[i].guildColor;
                data.rank = guildList[i].ranking;
                data.level = guildList[i].level;
                data.minLevel = guildList[i].minLevel;
                data.memberCount = guildList[i].joinNum;
                data.maxMemberCount = guildList[i].limNum;
                data.point = guildList[i].point;
                data.approvalType = guildList[i].approvalType;
                _dataList.Add(data);
            }

            _scrollview.Initialize(this);
            Refresh();
            _guildNameInput.text = defaultRichText;
            //_scrollView.SetData(_dataList, _cellviewPrefab, OnClickGuildList);
        });


    }

    public void OnSearchTextChanged()
    {
        if (_guildNameInput.text.Equals(defaultRichText)) 
            return;
        _inputChanged = true;
    }

    void OnSerchSubmit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        GuildSearch();

    }
    ////길드 검색
    public void GuildSearch()
    {
        if (false == _inputChanged)
            return;
        GuildSearchRequestDto searchRequestDto = new GuildSearchRequestDto();
        searchRequestDto.name = _guildNameInput.text;
        searchRequestDto.page = 0;

        bestHttp_GameManager.OnPostGuildSearch(searchRequestDto, (a) =>
        {
            guildList = a.content;
            _dataList.Clear();
            for (int i = 0; i < guildList.Count; i++)
            {
                GuildInfoItemData data = new GuildInfoItemData();
                data.id = guildList[i].id;
                data.name = guildList[i].name;
                data.mark = guildList[i].guildPattern;
                data.color = guildList[i].guildColor;
                data.rank = guildList[i].ranking;
                data.level = guildList[i].level;
                data.minLevel = guildList[i].minLevel;
                data.memberCount = guildList[i].joinNum;
                data.maxMemberCount = guildList[i].limNum;
                data.approvalType = guildList[i].approvalType;
                data.point = guildList[i].point;

                _dataList.Add(data);
            }
            _scrollview.Initialize(this);
            Refresh();

            _inputChanged = false;

            //_scrollView.SetData(_dataList, _cellviewPrefab, OnClickGuildList);

            //Close();
        });
    }

    void ScrollItemClick(GuildInfoListScrollviewItem item)
    {
        selectedGuild = item;
        GuildInfoDto guildInfo = guildList[selectedGuild._index];

        if(guildInfoPopup == null)
            guildInfoPopup = Managers.Instance.GetUIManager().ShowUISubBase<UIGuildInfoJoin>(this, "UIGuildInfoJoin");
        guildInfoPopup.OpenToStack();
        guildInfoPopup.SetGuildInfoDto(guildInfo);
        Refresh();
    }

    public void OnClickCreateGuild()
    {
        if(guildCreate == null)
            guildCreate = Managers.Instance.GetUIManager().ShowUISubBase<UIGuildCreate>(this, "UIGuildCreate");
        guildCreate.OpenToStack();
    }

    //// 가입신청한 길드 리스트
    void RequestGuildList()
    {
        bestHttp_GameManager.OnGetRequestGuildList((a) =>
        {
            userInfo.guildRequestList = a;
        });
    }

}

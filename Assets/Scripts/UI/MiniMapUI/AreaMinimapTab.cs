using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MinimapInfo
{
    public Vector2Int _cellZoneCnt = new Vector2Int();
    public Vector2 _offset = new Vector2();
    public Vector2 _minimapSize = new Vector2();        // 미니맵 이미지에서 실제 맵 크기 (여백 제외) 
}

public class AreaMinimapTab : UITabBase
{
    int _mapID;

    private Vector2 _cellSize = new Vector2(4, 2);      // 각 셀의 크기
    private Vector2Int _cellPerZone = new Vector2Int(40, 40);     // zone 당 셀의 개수
    private Vector2Int _zoneGridSize;    // zone 배치 개수

    [Header("Map Settings")]
    [SerializeField] RectTransform _content;
    [Tooltip("비어 있으면 _content 부모에서 자동 탐색")]
    [SerializeField] ScrollRect _scrollRect;
    private Vector2 _mapSize;   // 월드 맵 크기
    private Vector2 _mapMinPosition = Vector2.zero; // 월드 맵의 최소 좌표
    [Tooltip("미니맵 UI의 RectTransform")]
    [SerializeField] Transform _miniMapPosition;
    [SerializeField] Image _minimapImg;
    [SerializeField] RectTransform _minimapImgRect;
    private Vector2 _miniMapSize;  // 미니맵 UI 크기
    private Vector2 _minimapRatio;  // 월드 좌표 -> 미니맵 좌표 비율

    private Transform _playerWorld => Managers.Instance.GetObjectUnitManager().playerSquad.transform;  // 플레이어의 월드 Transfrom
    [SerializeField] RectTransform _playerMinimapIcon;      // 미니맵 UI의 RectTransform 

    [Tooltip("중점 offset")]
    [SerializeField] private RectTransform _offsetPointRect;
    private MinimapInfo[] _minimapInfos = new MinimapInfo[] {
        new MinimapInfo     // 초원 (MainField)
        {
            _cellZoneCnt = new Vector2Int(10,10),
           _offset = new Vector2(-805f, -292f),
           _minimapSize = new Vector2(2001f,1002f)     
        },
        new MinimapInfo     // 설산 (MainField)
        {
            _cellZoneCnt = new Vector2Int(5,5),
            _offset = new Vector2(-600f, -320),
            _minimapSize = new Vector2(1350f,680f)
        }
    };

    [Header("Portal")]
    [SerializeField] private MinimapActionButton _townPortalBtn;
    [SerializeField] private MinimapActionButton _spawnPortalBtn;
    [Header("WarpPoint")]
    [SerializeField] List<MinimapActionWarpPointButton> _warpPointBtnList = new List<MinimapActionWarpPointButton>();
    [HideInInspector] public List<string> _warpPointBuildingKeylist;
    [SerializeField] private RectTransform _warpBtnArea;
    [Header("GuideQuest")]
    [SerializeField] private RectTransform _guideQuestPortal;

    [Header("Title")]
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] TMP_Text _tabNameTxt;

    [Header("Progress")]
    [SerializeField] TMP_Text _mapProgressCountTxt;
    [SerializeField] Slider _mapProgressSlider;
    [SerializeField] GameObject _mapProgressPopup;
    [SerializeField] Transform _mapProgressRewardListTrans;
    List<MapProgressRewardListItem> _mapProgressRewardListItems = new List<MapProgressRewardListItem>();

    MapManager mapManager => Managers.Instance.GetMapManager();


    public override void Open()
    {
        base.Open();
        HideProgressPassPopup();
        Refresh();
        StartCoroutine(FocusScrollOnPlayerNextFrame());
    }

    private IEnumerator FocusScrollOnPlayerNextFrame()
    {
        yield return null; // 한 프레임 대기 → Canvas rebuild 완료 후 실행
        FocusScrollOnPlayer();
    }

    /// <summary>
    /// ScrollView를 플레이어 미니맵 아이콘 위치로 포커스(중앙 정렬)
    /// </summary>
    private void FocusScrollOnPlayer()
    {
        ScrollRect scroll = _scrollRect != null ? _scrollRect : _content.GetComponentInParent<ScrollRect>();
        if (scroll == null || scroll.viewport == null || _playerMinimapIcon == null)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scroll.viewport;
        Rect contentRect = _content.rect;
        Rect viewportRect = viewport.rect;
        Vector2 contentSize = contentRect.size;
        Vector2 viewportSize = viewportRect.size;

        // 스크롤할 수 있는 영역이 없으면 스킵
        if (contentSize.x <= viewportSize.x && contentSize.y <= viewportSize.y)
            return;

        // 플레이어 아이콘 중심을 content 로컬 좌표(피벗 기준)로 변환
        Vector3[] iconCorners = new Vector3[4];
        _playerMinimapIcon.GetWorldCorners(iconCorners);
        Vector3 iconCenter = (iconCorners[0] + iconCorners[2]) * 0.5f;
        Vector2 iconPosInContent = _content.InverseTransformPoint(iconCenter);

        // content 피벗 기준 좌표 → 왼쪽 아래(0,0) 기준 위치로 변환
        Vector2 contentPivot = _content.pivot;
        float posX = iconPosInContent.x + contentSize.x * contentPivot.x;
        float posY = iconPosInContent.y + contentSize.y * contentPivot.y;

        float normX = scroll.horizontal ? 0.5f : scroll.horizontalNormalizedPosition;
        float normY = scroll.vertical ? 0.5f : scroll.verticalNormalizedPosition;

        if (scroll.horizontal && contentSize.x > viewportSize.x)
        {
            float scrollRangeX = contentSize.x - viewportSize.x;
            float targetOffsetX = posX - viewportSize.x * 0.5f;
            targetOffsetX = Mathf.Clamp(targetOffsetX, 0f, scrollRangeX);
            normX = scrollRangeX > 0.0001f ? (targetOffsetX / scrollRangeX) : 0.5f;
        }

        if (scroll.vertical && contentSize.y > viewportSize.y)
        {
            float scrollRangeY = contentSize.y - viewportSize.y;
            float targetOffsetY = posY - viewportSize.y * 0.5f;
            targetOffsetY = Mathf.Clamp(targetOffsetY, 0f, scrollRangeY);

            // ScrollRect의 verticalNormalizedPosition은 1(위) → 0(아래)이므로 반전
            normY = scrollRangeY > 0.0001f ? 1f - (targetOffsetY / scrollRangeY) : 0.5f;
        }

        scroll.normalizedPosition = new Vector2(normX, normY);
    }

    /// <summary>
    /// 월드 맵 변경 시 Zone Setting
    /// </summary>
    /// <param name="cellPerZone">zone 당 셀의 갯수</param>
    /// <param name="zoneGridSize">zone 배치 갯수</param>
    public void SetZone(int mapId)
    {
        _mapID = mapId;
        MinimapInfo info = _minimapInfos[mapId - 1];
        _zoneGridSize = info._cellZoneCnt;
        _minimapImg.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/Minimap/Minimap{mapId}");
        _offsetPointRect.anchoredPosition = info._offset;        // 일단은 초원 맵기반 기본으로

        InitMapSize();
    }

    private void InitMapSize()
    {

        _mapSize = CalculateIsometricMapSize();
        //_miniMapSize = _minimapImgRect.sizeDelta;

        // 맵 사이즈 2배로 셋팅 
        _miniMapSize = _minimapInfos[_mapID -1 ]._minimapSize * 2;
        _minimapRatio = new Vector2(
                _miniMapSize.x / _mapSize.x,
                _miniMapSize.y / _mapSize.y
        );

        _minimapImgRect.anchoredPosition = Vector2.zero;
        var sz = _minimapImgRect.rect.size;                // MapImage의 현재 UI 크기

        // stretch 앵커에서도 안전하게 축별 크기 강제 설정
        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sz.x);
        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sz.y);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }



    /// <summary>
    /// 아이소매트릭으로 world map size 계산
    /// </summary>
    /// <returns></returns>
    private Vector2 CalculateIsometricMapSize()
    {
        // 전체 타일 개수 계산
        int gridSizeX = _cellPerZone.x * _zoneGridSize.x;   // x 방향 총 타일 개수
        int gridSizeY = _cellPerZone.y * _zoneGridSize.y;   // y 방향 총 타일 개수

        // 전체 맵 크기 계산
        float worldWidth = (gridSizeX + gridSizeY - 1) * (_cellSize.x / 2f);    // 가로 길이
        float worldHeight = (gridSizeX + gridSizeY - 1) * (_cellSize.y / 2f);   // 세로 길이

        return new Vector2(worldWidth, worldHeight);
    }

    public override void Refresh()
    {
        string nameSt = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, UserInfoData._fieldId).Name;
        _nameTxt.text = nameSt;
        _tabNameTxt.text = nameSt;

        // Minimap
        DrawPlayerPosition();
        DrawPortal();
        DrawWarpPoint();
        DrawGuideQuest();

        RefreshMapProgress();
    }
    #region Minimap
    private void DrawPlayerPosition()
    {
        if (_playerWorld == null) return;
        // 미니맵 아이콘 위치 업데이트
        _playerMinimapIcon.anchoredPosition = ReturnWorldToMinimapAnchoredPosition(_playerWorld.position);
    }

    private void DrawPortal()
    {
        // town portal 
        _townPortalBtn.gameObject.SetActive(mapManager._townPortal.gameObject.activeSelf);
        if (_townPortalBtn.gameObject.activeSelf)
        {
            _townPortalBtn.Init(mapManager._townPortal, mapManager._townPortal.transform.position, true);
            _townPortalBtn.GetComponent<RectTransform>().anchoredPosition = ReturnWorldToMinimapAnchoredPosition(mapManager._townPortal.gameObject.transform.position);
        }

        // spawn portal
        _spawnPortalBtn.gameObject.SetActive(mapManager._spawnPortal.gameObject.activeSelf);
        if (_spawnPortalBtn.gameObject.activeSelf)
        {
            _spawnPortalBtn.Init(mapManager._spawnPortal, mapManager._spawnPortal.transform.position, false);
            _spawnPortalBtn.GetComponent<RectTransform>().anchoredPosition = ReturnWorldToMinimapAnchoredPosition(mapManager._spawnPortal.gameObject.transform.position);
        }

    }

    private void DrawWarpPoint()
    {
        for (int i = 0; i < _warpPointBtnList.Count; i++)
        {
            Destroy(_warpPointBtnList[i].gameObject);
        }
        _warpPointBtnList.Clear();

        for (int i = 0; i < _warpPointBuildingKeylist.Count; i++)
        {
            BaseBuilding building = Managers.Instance.GetObjectUnitManager().hashBuilding.FirstOrDefault(b =>
            b.BuildingData != null &&
            b.BuildingData._data.ID == int.Parse(_warpPointBuildingKeylist[i]));
            if (building == null) continue;

            MinimapActionWarpPointButton btn = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Button/MinimapActionWarpPointButton", _warpBtnArea).GetComponent<MinimapActionWarpPointButton>();
            btn.Init(int.Parse(_warpPointBuildingKeylist[i]), building.GetZoneIndex(), building.gameObject.transform.position);
            btn.GetComponent<RectTransform>().anchoredPosition = ReturnWorldToMinimapAnchoredPosition(building.transform.position);

            _warpPointBtnList.Add(btn);

        }
    }

    private void DrawGuideQuest()
    {
        GuideQuest currentQuest = UserInfoData.GetCurrentGuideQuest;
        _guideQuestPortal.gameObject.SetActive(currentQuest != null && currentQuest.ArrivalTarget != 0
            && (currentQuest.ConditionType == Define.EQuestConditionType.BuildingTarget || currentQuest.ConditionType == Define.EQuestConditionType.MonsterKillTarget ));
        if(_guideQuestPortal.gameObject.activeSelf)
        {
            Vector3 worldPos = Vector3.zero;
            switch (currentQuest.ConditionType)
            {
                case Define.EQuestConditionType.BuildingTarget:
                    worldPos = Managers.Instance.GetMapManager().FindBuilding(currentQuest.ArrivalTarget).transform.position;
                    break;
                case Define.EQuestConditionType.MonsterKillTarget: 
                    worldPos = Managers.Instance.GetMapManager().FindSpawnPoint(currentQuest.ArrivalTarget).transform.position; 
                    break;
                default:
                    break;
            }
            _guideQuestPortal.anchoredPosition = ReturnWorldToMinimapAnchoredPosition(worldPos);
        }
    }
    private Vector2 ReturnWorldToMinimapAnchoredPosition(Vector3 worldPosition)
    {
        // 월드 좌표 -> 미니맵 좌표 변환
        float minimapX = ((worldPosition.x - _mapMinPosition.x) * _minimapRatio.x) + _offsetPointRect.anchoredPosition.x;
        float minimapY = ((worldPosition.y - _mapMinPosition.y) * _minimapRatio.y) + _offsetPointRect.anchoredPosition.y;

        // 미니맵 아이콘 위치 업데이트
        return new Vector2(minimapX, minimapY);
    }
    #endregion

    public void ShowProgressPassPopup()
    {
        _mapProgressPopup.SetActive(true);
    }
    public void HideProgressPassPopup()
    {
        _mapProgressPopup.SetActive(false);
    }

    public void RefreshMapProgress()
    {
        DrawProgressReward();
    }

private void DrawProgressReward()
    {
        FieldItemData fieldItemData = UserInfoData._dicFieldItemData[_mapID];

        // 해당 fieldID 의 FieldQuestReward row 목록을 Index(ClearCount) 순으로 정렬
        List<FieldQuestReward> rewards = ClientLocalDB_Simple.GetDB<FieldQuestReward>(DBKey.FieldQuestReward).Values
            .Where(r => r.FieldID == _mapID)
            .OrderBy(r => r.ClearCount)
            .ToList();

        _mapProgressCountTxt.text = $"{fieldItemData.progress} / {fieldItemData.MaxProgress}";
        _mapProgressSlider.value = fieldItemData.MaxProgress > 0
            ? (float)fieldItemData.progress / fieldItemData.MaxProgress
            : 0f;

        foreach (var item in _mapProgressRewardListItems)
        {
            DestroyImmediate(item.gameObject);
        }
        _mapProgressRewardListItems.Clear();

        for (int i = 0; i < rewards.Count; i++)
        {
            MapProgressRewardListItem mapProgressListItem
                = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/EtcUI/MapProgressRewardListItem", _mapProgressRewardListTrans)
                .GetComponent<MapProgressRewardListItem>();

            int clearCount = rewards[i].ClearCount;
            mapProgressListItem.SetData(clearCount, rewards[i]);

            _mapProgressRewardListItems.Add(mapProgressListItem);
        }
    }




}

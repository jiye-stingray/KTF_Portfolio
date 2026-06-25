using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Define;

public class AvatarThumbnailPanel : MonoBehaviour
{
    [Header("Target")]
    public PlayerProfileUI playerProfile;          // 메인 썸네일을 바꿀 대상

    [Header("UI Refs")]
    public ScrollRect scrollRect;                  // ScrollView
    public RectTransform content;                  // ScrollView/Viewport/Content
    public AvatarThumbnailItem itemPrefab;         // 썸네일 프리팹( Image+Button )

    [Header("Frame")]
    [SerializeField] ScrollRect _frameScrollRect;
    [SerializeField] RectTransform _frameContent;
    [SerializeField] AvatarThumbnailFrameItem _frameItemPrefab;

    private readonly List<AvatarThumbnailItem> _items = new();
    private AvatarThumbnailItem _current;

    private readonly List<AvatarThumbnailFrameItem> _frameItems = new List<AvatarThumbnailFrameItem>();
    private AvatarThumbnailFrameItem _frameCurrent;

    void Reset()
    {
        if (!scrollRect) scrollRect = GetComponentInChildren<ScrollRect>(true);
        if (!content && scrollRect) content = scrollRect.content;
    }

    public void PopulateFromKeys(Define.EAtlasType atlasType, int[] keys, string selectedKeyName = null)
    {
        if (!content || !itemPrefab || keys == null) return;
        Clear();

        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            if (key < 0) continue;

            var s = Managers.Instance.GetAtlasManager().GetSprite(atlasType, $"Thum_SD_Cr_{key.ToString("000")}");
            if (s == null)
            {
                MyLogger.LogWarning($"[AvatarPanel] 스프라이트 로드 실패: {atlasType} / {key}");
                continue;
            }

            var go = Instantiate(itemPrefab, content);
            // 이름 기준으로 선택 판단(레퍼런스 비교보다 안전)
            bool isSelected = !string.IsNullOrEmpty(selectedKeyName) && s.name == selectedKeyName;
            
            go.Init(key, this, isSelected);

            if (isSelected) _current = go;
            _items.Add(go);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
    
    public void OnItemClicked(AvatarThumbnailItem item)
    {
        if (item == null) return;

        // 선택 표시 갱신
        if (_current) _current.SetSelected(false);
        _current = item;
        _current.SetSelected(true);
    }

    private void Clear()
    {
        foreach (var it in _items)
            if (it) Destroy(it.gameObject);
        _items.Clear();
        _current = null;
    }

    public void ChangeBtnClick()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");

        Managers.Instance.GetServerManager().OnPostChangeThumbnail(_current.id, _frameCurrent.id , (heroId, frameId) =>
        {
            Managers.Instance.UserInfo()._thumbnailID = heroId;
            Managers.Instance.UserInfo()._frameID = frameId;

            // Chat Thumbnail
            #if CHAT
            Managers.Instance.Chat.SetThumbnail(heroId);
            #endif

            // 메인 적용
            playerProfile?.ApplyAvatar(
                Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.CharacterIconAtlas, $"Thum_SD_Cr_{heroId.ToString("000")}")
                ,Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{frameId.ToString("000")}")
                );
            playerProfile.OnClickThumbnailClose();
        });
    }

    // ── Frame ────────────────────────────────────────────────────────────────

    public void PopulateFrameFromKeys(int[] keys, string selectedKeyName = null)
    {
        if (!_frameContent || !_frameItemPrefab || keys == null) return;
        ClearFrame();

        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            if (key < 0) continue;

            var s = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.FrameAtlas, $"FrameImg_{key.ToString("000")}");
            if (s == null)
            {
                MyLogger.LogWarning($"[AvatarPanel] 프레임 스프라이트 로드 실패: FrameAtlas / {key}");
                continue;
            }

            var go = Instantiate(_frameItemPrefab, _frameContent);
            bool isSelected = !string.IsNullOrEmpty(selectedKeyName) && s.name == selectedKeyName;

            go.Init(key, this, isSelected);

            if (isSelected) _frameCurrent = go;
            _frameItems.Add(go);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_frameContent);
    }

    public void OnFrameItemClicked(AvatarThumbnailFrameItem item)
    {
        if (item == null) return;

        if (_frameCurrent) _frameCurrent.SetSelected(false);
        _frameCurrent = item;
        _frameCurrent.SetSelected(true);
    }

    private void ClearFrame()
    {
        foreach (var it in _frameItems)
            if (it) Destroy(it.gameObject);
        _frameItems.Clear();
        _frameCurrent = null;
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class UIFishDex : MonoBehaviour, IRecyclableScrollRectDataSource
{
    private const string SaveKey = "FISH_DEX";

    [Header("Data")]
    [SerializeField] private TextAsset fishJson;

    [Header("UI")]
    [SerializeField] private RecyclableScrollRect scrollView;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Sprite lockedFish;

    [Header("Atlas")]
    [SerializeField] private string fishAtlasPath = "Atlas/FishAtlas";

    [Header("Option")]
    [SerializeField] private string lockedName = "???";

    [Serializable]
    private class SaveData
    {
        public List<string> caught = new List<string>();
    }

    [Serializable]
    private class FishDatabase
    {
        public List<Fish> Fishes;
    }

    private readonly List<Fish> _fishList = new List<Fish>();
    private readonly List<Fish> _sortedFishList = new List<Fish>();
    private readonly HashSet<string> _caughtFish = new HashSet<string>();

    [SerializeField] private TextMeshProUGUI fishDexCountText;
    
    private SpriteAtlas _atlas;
    private bool _initialized;

    // -----------------------
    // RecyclableScrollRect
    // -----------------------
    public int GetItemCount()
    {
        return _sortedFishList.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        FishDexScrollItem item = cell as FishDexScrollItem;
        if (item == null) return;

        Fish fish = _sortedFishList[index];
        bool caught = _caughtFish.Contains(fish.fishCode);

        Sprite iconSprite = lockedFish;

        // 잡은 물고기면 Atlas sprite 사용
        if (caught && _atlas != null)
        {
            Sprite atlasSprite = _atlas.GetSprite(fish.fishCode);
            if (atlasSprite != null)
                iconSprite = atlasSprite;
        }

        item.SetData(index, fish, caught, iconSprite, lockedName);
    }

    // -----------------------
    // Unity
    // -----------------------
    private void Awake()
    {
        InitializeIfNeeded();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeBtn != null)
            closeBtn.onClick.RemoveListener(Hide);
    }

    private void InitializeIfNeeded()
    {
        if (_initialized) return;
        _initialized = true;

        if (closeBtn != null)
            closeBtn.onClick.AddListener(Hide);

        LoadFishList();
        LoadAtlas();
    }

    // -----------------------
    // Public
    // -----------------------
    public void Show()
    {
        MyLogger.Log("[UIFishDex] Show 호출");

        InitializeIfNeeded();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        LoadSave();
        BuildSortedFishList();

        UpdateDexCountUI();
        
        Refresh();
        
        Canvas.ForceUpdateCanvases();
        
        MoveToTop();
        
        MyLogger.Log("[UIFishDex] Open Complete");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    private void UpdateDexCountUI()
    {
        if (fishDexCountText == null)
            return;

        int caughtCount = _caughtFish.Count;
        int totalCount = _fishList.Count;

        fishDexCountText.text = $"[ 물고기 도감 ] {caughtCount} / {totalCount}";
    }
    
    private void MoveToTop()
    {
        if (scrollView == null)
            return;

        Transform contentTr = scrollView.transform.Find("Viewport/Content");
        if (contentTr == null)
        {
            MyLogger.LogWarning("[UIFishDex] Viewport/Content를 찾지 못함");
            return;
        }

        RectTransform content = contentTr as RectTransform;
        if (content == null)
            return;

        Canvas.ForceUpdateCanvases();

        Vector2 pos = content.anchoredPosition;
        pos.y = 0f;
        content.anchoredPosition = pos;
    }

    public static void RegisterFish(string fishCode)
    {
        if (string.IsNullOrEmpty(fishCode))
            return;

        SaveData data = LoadData();
        if (data == null)
            data = new SaveData();

        if (data.caught == null)
            data.caught = new List<string>();

        if (data.caught.Contains(fishCode))
            return;

        data.caught.Add(fishCode);
        Save(data);

        MyLogger.Log($"[UIFishDex] 도감 등록: {fishCode}");
    }

    public static bool IsRegistered(string fishCode)
    {
        if (string.IsNullOrEmpty(fishCode))
            return false;

        SaveData data = LoadData();
        if (data?.caught == null)
            return false;

        return data.caught.Contains(fishCode);
    }

    // -----------------------
    // Load
    // -----------------------
    private void LoadFishList()
    {
        _fishList.Clear();

        if (fishJson == null || string.IsNullOrWhiteSpace(fishJson.text))
        {
            Debug.LogError("[UIFishDex] fishJson 연결 안됨");
            return;
        }

        try
        {
            // 1) JSON 배열 포맷
            List<Fish> list = JsonConvert.DeserializeObject<List<Fish>>(fishJson.text);
            if (list != null && list.Count > 0)
            {
                _fishList.AddRange(list);
                MyLogger.Log($"[UIFishDex] 물고기 목록 로드(array): {_fishList.Count}");
                return;
            }

            // 2) 래퍼 포맷
            FishDatabase db = JsonConvert.DeserializeObject<FishDatabase>(fishJson.text);
            if (db?.Fishes != null && db.Fishes.Count > 0)
            {
                _fishList.AddRange(db.Fishes);
                MyLogger.Log($"[UIFishDex] 물고기 목록 로드(wrapper): {_fishList.Count}");
                return;
            }

            MyLogger.LogWarning("[UIFishDex] Fish.json 파싱은 되었지만 목록이 비어있음");
        }
        catch (Exception e)
        {
            Debug.LogError($"[UIFishDex] Fish.json 파싱 실패: {e.Message}");
        }
    }

    private void LoadAtlas()
    {
        _atlas = Resources.Load<SpriteAtlas>(fishAtlasPath);

        if (_atlas == null)
            MyLogger.LogWarning($"[UIFishDex] SpriteAtlas 로드 실패: Resources/{fishAtlasPath}");
    }

    private void LoadSave()
    {
        _caughtFish.Clear();

        SaveData data = LoadData();
        if (data?.caught == null)
        {
            MyLogger.Log("[UIFishDex] 저장된 도감 수: 0");
            return;
        }

        for (int i = 0; i < data.caught.Count; i++)
        {
            if (!string.IsNullOrEmpty(data.caught[i]))
                _caughtFish.Add(data.caught[i]);
        }

        MyLogger.Log($"[UIFishDex] 저장된 도감 수: {_caughtFish.Count}");
    }

    private void BuildSortedFishList()
    {
        _sortedFishList.Clear();
        _sortedFishList.AddRange(_fishList);

        _sortedFishList.Sort((a, b) =>
        {
            bool aCaught = _caughtFish.Contains(a.fishCode);
            bool bCaught = _caughtFish.Contains(b.fishCode);

            // 잡은 물고기 우선
            if (aCaught != bCaught)
                return aCaught ? -1 : 1;

            // 같은 그룹에서는 index 오름차순
            return a.index.CompareTo(b.index);
        });
    }

    private static SaveData LoadData()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return new SaveData();

        try
        {
            return JsonConvert.DeserializeObject<SaveData>(json) ?? new SaveData();
        }
        catch
        {
            return new SaveData();
        }
    }

    private static void Save(SaveData data)
    {
        string json = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    // -----------------------
    // Refresh
    // -----------------------
    private void Refresh()
    {
        if (scrollView == null)
        {
            Debug.LogError("[UIFishDex] scrollView 연결 안됨");
            return;
        }

        scrollView.Initialize(this);
        scrollView.ReloadData();

        MyLogger.Log($"[UIFishDex] 도감 UI 리로드 완료: {_sortedFishList.Count}");
    }

    [ContextMenu("Clear FishDex Save")]
    private void ClearFishDexSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        _caughtFish.Clear();
        BuildSortedFishList();

        if (gameObject.activeInHierarchy)
            Refresh();

        MyLogger.Log("[UIFishDex] 도감 저장 삭제");
    }
}
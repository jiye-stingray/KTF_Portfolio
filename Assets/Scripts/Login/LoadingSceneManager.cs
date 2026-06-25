using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public static class Loading
{
    public static readonly string Field = "FieldScene";
    public static readonly string Dungeon = "DungeonScene";
    public static readonly string Title = "TitleScene";
    public static readonly string Patch = "PatchScene";

    public static void Load(string nextSceneName)
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            LoadingSceneManager.Load(nextSceneName);
    }
}

public class LoadingSceneManager : MonoBehaviour
{
    private static string _nextSceneName;
    private const string _loadingSceneName = "LoadingScene";

    // [최적화] 비동기 작업 취소를 위한 소스
    private CancellationTokenSource _cts;

    private void Awake()
    {
        // 씬 시작 시 CTS 초기화
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        // [최적화] 객체가 파괴될 때 모든 비동기 루프(Tips 등) 즉시 중단 및 메모리 해제
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    async UniTask Start()
    {
        // 이전 씬 사운드 정리
        Managers.Instance.Sound.StopBGM();
        Managers.Instance.Sound.ReleaseSFXCache();
        // 이전 환경 정리
        Managers.Instance.GetUIManager().AllCloseStackUI();
        await UniTask.Yield();
        Managers.Instance.GetMapManager().UnLoadMap();
        await UniTask.Yield();
        // 씬 로드 전 메모리 정리
        Resources.UnloadUnusedAssets();
        await UniTask.Yield();
        GC.Collect();
        await UniTask.Yield();
        // 비동기 로딩 시작 (CTS 토큰 전달)
        await SyncLoadSceneAsync(_cts.Token);
    }

    public static void Load(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;
        _nextSceneName = nextSceneName;
        SceneManager.LoadScene(_loadingSceneName);
    }

    private async UniTask SyncLoadSceneAsync(CancellationToken token)
    {
        // 로딩 UI 생성
        GameObject loadingPrefab = Resources.Load<GameObject>("Prefabs/UI/LoadingUI");
        if (loadingPrefab == null) return;

        GameObject loading = Instantiate(loadingPrefab);
        LoadingUI loadingUI = loading.GetComponent<LoadingUI>();
        loadingUI.SetTip(_nextSceneName);
        loadingUI.name = "LoadingUI";
        loadingUI.Show();

        DontDestroyOnLoad(loading);

        await LoadMap();
        // 연출을 위한 최소 대기
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true, cancellationToken: token);
        await ToNextAsync(loadingUI, token);
    }

    private async UniTask ToNextAsync(LoadingUI loadingUI, CancellationToken token)
    {
        var sync = SceneManager.LoadSceneAsync(_nextSceneName, LoadSceneMode.Single);
        sync.allowSceneActivation = false;

        float displayProgress = 0f;

        // 0% ~ 90% 보간
        while (sync.progress < 0.5f)
        {
            float target = Mathf.Clamp01(sync.progress / 0.9f);
            displayProgress = Mathf.MoveTowards(displayProgress, target, Time.unscaledDeltaTime * 1.5f);
            loadingUI.UpdateProgress(displayProgress);
            await UniTask.Yield(token); 
        }

        // 90% ~ 100% 보간
        while (displayProgress < 1f)
        {
            displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.unscaledDeltaTime * 0.5f);
            loadingUI.UpdateProgress(displayProgress);
            await UniTask.Yield(token); 
        }

        // 씬 활성화
        sync.allowSceneActivation = true;
        
        // 실제 씬 전환 완료 대기
        await UniTask.WaitUntil(() => sync.isDone, cancellationToken: token);

        // 후처리 메모리 정리
        await UniTask.Yield(token);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private async UniTask LoadMap()
    {
        string assetKey = "";
        if (_nextSceneName.Equals(Loading.Field))
        {
            UserInfoData userInfoData = Managers.Instance.UserInfo();
            FieldInfo fieldInfo = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, userInfoData._fieldId);

            assetKey = fieldInfo.Resource;
        }
        else if (_nextSceneName.Equals(Loading.Dungeon))
        {
            BattleData battleData = BattleData.Get();
            EContent contentType = battleData._contentType;
            switch (contentType)
            {
                case EContent.Gold:
                case EContent.Equipment:
                case EContent.Constellation:
                    Dungeon dungeonBase = ClientLocalDB_Simple.GetData<Dungeon>(DBKey.Dungeon,$"{contentType}_{battleData._index}");
                    assetKey = $"{contentType}DungeonMap_{dungeonBase.ZoneFlow}";
                    break;
                case EContent.GuildBoss:
                    GuildDungeon guildDungeon = ClientLocalDB_Simple.GetData<GuildDungeon>(DBKey.GuildDungeon, battleData._index);
                    assetKey = $"{contentType}DungeonMap_{guildDungeon.ZoneFlow}";
                    break;
                case EContent.Ranking:
                    assetKey = $"{contentType}DungeonMap_1";
                    break;
                case EContent.Tower:
                    TowerDungeon towerDungeonBase = ClientLocalDB_Simple.GetData<TowerDungeon>(DBKey.TowerDungeon, $"{battleData._factionType}_{battleData._index}");
                    assetKey = $"{contentType}DungeonMap_{towerDungeonBase.ZoneFlow}";
                    break;
                case EContent.Fog:
                    FogDungeon fogDungeonBase = ClientLocalDB_Simple.GetData<FogDungeon>(DBKey.FogDungeon, battleData._index);
                    assetKey = $"{contentType}DungeonMap_{fogDungeonBase.ZoneFlow}";
                    break;
            }
            BattleData.Set(battleData);
        }
        
        if (string.IsNullOrEmpty(assetKey))
            return;
        
        await AddressableLoader.LoadCachedAssetAsync<GameObject>(assetKey);
    }
}
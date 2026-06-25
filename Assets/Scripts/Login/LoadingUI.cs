using Cysharp.Threading.Tasks;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadingUI : MonoBehaviour
{
    // public Slider progressBar;
    public TextMeshProUGUI loadingText;
    [SerializeField] private GameObject loadingEff;
    
    [SerializeField] private TMP_Text rndTipText;   // 인스펙터에서 할당
    [SerializeField] private CanvasGroup _canvasGroup;
    
    [Header("Spine (UI) Settings")]
    [SerializeField] private SpineAnimation[] characterSlots;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(0.95f, 1.05f);
    
    private CancellationTokenSource _cts;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetTip(string nextSceneName)
    {
        if(_cts == null)
            _cts = new CancellationTokenSource();
        
        TipCategory category = ResolveTipCategory(nextSceneName);
        SpawnRandomCharactersUI().Forget();
        RotateTipsDuringLoadingAsync(category, 1.5f, _cts.Token).Forget();
    }
    
    public async UniTask FadeOutAndHideAsync()
    {
        // 씬 전환 로딩이므로 Unscaled Time 사용
        float duration = 0.4f;
        float t = 0f;
        
        // 캔버스 그룹의 알파 값을 1.0에서 0.0으로 Lerp하며 비동기 대기
        while (t < duration)
        {
            if (!this || !_canvasGroup) // 유효성 검사 추가
            {
                MyLogger.LogWarning("[LoadingUI] FadeOut 도중 객체가 파괴되어 중단되었습니다.");
                return;
            }
            
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
            await UniTask.Yield(PlayerLoopTiming.Update); // 다음 프레임까지 Unscaled Time으로 대기
        }

        Destroy(gameObject);
    }
    public void SetTipText(string tip)
    {
        if (rndTipText != null)
            rndTipText.text = string.IsNullOrEmpty(tip) ? "" : $"TIP: {tip}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (!this || !gameObject) return;
        gameObject.SetActive(false);
    }
    

    public void UpdateProgress(float progress)
    {
        if (loadingText != null)
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
    }
    
    public void OnComplete()
    {
        loadingText.text = "Complete!";
        if (loadingEff != null)
            loadingEff.SetActive(false);
    }
    
    private async UniTask RotateTipsDuringLoadingAsync(
        TipCategory category,
        float intervalSec,
        CancellationToken token)
    {
        var wait = TimeSpan.FromSeconds(intervalSec);

        while (!token.IsCancellationRequested)
        {
            string tip = TipService.Instance.GetRandomTip(category);
            SetTipText(tip);

            await UniTask.Delay(wait, cancellationToken: token);
        }
    }
    
    private TipCategory ResolveTipCategory(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName)) return TipCategory.Common;
        if (nextSceneName.Contains("Battle", StringComparison.OrdinalIgnoreCase)) return TipCategory.Battle;
        if (nextSceneName.Contains("Field", StringComparison.OrdinalIgnoreCase)) return TipCategory.Field;
        if (nextSceneName.Contains("Shop", StringComparison.OrdinalIgnoreCase)) return TipCategory.Shop;
        return TipCategory.Common;
    }

    private async UniTask SpawnRandomCharactersUI()
    {
        if (characterSlots == null || characterSlots.Length == 0) return;
        
        int wantCount = 4;
        string[] keys;
        UserInfoData userInfoData = Managers.Instance.UserInfo();
        if (userInfoData == null)
            keys = ClientLocalDB_Simple.GetDB<UnitData>(DBKey.PlayerCharacter).Keys.ToArray();
        else
        {
             keys = userInfoData._dicCharacterItemData
            .Where(kv => kv.Value.isOpen)
            .Select(kv => kv.Key.ToString())
            .ToArray();
        }

        var slotIdx = Shuffle(characterSlots.Length);
        var assetIdx = Shuffle(keys.Length);

        for (int i = 0; i < wantCount; i++)
        {
            var slot = characterSlots[slotIdx[i]];
            if (slot == null) continue;

            UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, keys[i]);
            var data = await AddressableLoader.LoadCachedAssetAsync<SkeletonDataAsset>($"{unitData.Resource}/{unitData.Resource}_SkeletonData.asset");
            if (data == null) continue;
            slot.SetSpine(data);
            slot.SetFlip(true);
            float s = Random.Range(randomScaleRange.x, randomScaleRange.y);
            slot.transform.localScale = new Vector3(s, s, 1f);

            slot.SetTimeScale(1);
            slot.SetAnimation(Define.CharacterAnimationName.MOVE, true);

            slot.gameObject.SetActive(true);
        }
        
        for (int i = wantCount; i < characterSlots.Length; i++)
        {
            var slot = characterSlots[slotIdx[i]];
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
                slot.SpineClear();
            }
        }
    }
    
    private void CleanupSpineUI()
    {
        if (characterSlots == null) return;
        foreach (var slot in characterSlots)
        {
            slot.SpineClear();
            slot.gameObject.SetActive(false);
        }
    }
    
    public static int[] Shuffle(int count)
    {
        var arr = Enumerable.Range(0, count).ToArray();

        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        return arr;
    }
    
    private void OnDestroy()
    {
        CleanupSpineUI();
        _cts?.Cancel();
        _cts?.Dispose();
    }
}

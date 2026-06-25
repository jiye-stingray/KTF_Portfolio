// /Scripts/Tips/TipService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum TipCategory { Common, Battle, Field, Shop }
public enum TipPlatform { Android, iOS, PC }

public sealed class TipService
{
    // 세션 중복 방지용 (소진되면 풀 리셋)
    private readonly HashSet<int> _seenThisSession = new HashSet<int>();
    private TipDatabase _db;
    private TipPlatform _currentPlatform;

    // 싱글톤 식으로 간단하게
    private static TipService _instance;
    public static TipService Instance => _instance ??= new TipService();

    private TipService() {
        _currentPlatform =
#if UNITY_ANDROID
            TipPlatform.Android;
#elif UNITY_IOS
            TipPlatform.iOS;
#else
            TipPlatform.PC;
#endif
        LoadDatabase();
    }

    private void LoadDatabase()
    {
        try {
            // 다국어 대응: tips_ko.json / tips_en.json 등
            string langCode = Application.systemLanguage == SystemLanguage.Korean ? "ko" : "en";
            string file = Path.Combine(Application.streamingAssetsPath, $"tips_{langCode}.json");

            string json;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android는 스트리밍에셋 접근이 WWW/UnityWebRequest가 안전
            // 단순화를 위해 동기 사용이 가능한 환경이면 다음처럼 처리, 아니면 코루틴으로 UWR 권장
            var www = new UnityEngine.Networking.UnityWebRequest(file) {
                downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer()
            };
            var op = www.SendWebRequest();
            while(!op.isDone) {}
            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                throw new Exception(www.error);
            json = www.downloadHandler.text;
#else
            json = File.ReadAllText(file);
#endif
            _db = JsonUtility.FromJson<TipDatabase>(json) ?? new TipDatabase();
        }
        catch (Exception e) {
            MyLogger.LogWarning($"[TipService] Load failed: {e.Message}");
            _db = new TipDatabase();
        }
    }

    public string GetRandomTip(TipCategory context = TipCategory.Common)
    {
        if (_db == null || _db.tips == null || _db.tips.Count == 0)
            return string.Empty;

        // 1) 카테고리/플랫폼 필터
        var cat = context.ToString().ToLower(); // "Common" -> "common"
        IEnumerable<TipEntry> pool = _db.tips.Where(t =>
        {
            bool okCat = (t.categories == null || t.categories.Length == 0) 
                         || t.categories.Any(c => string.Equals(c, "common", StringComparison.OrdinalIgnoreCase)
                                                || string.Equals(c, cat, StringComparison.OrdinalIgnoreCase));
            bool okPlat = (t.platforms == null || t.platforms.Length == 0)
                          || t.platforms.Any(p => string.Equals(p, _currentPlatform.ToString(), StringComparison.OrdinalIgnoreCase));
            return okCat && okPlat;
        });

        // 2) 세션 중복 제거
        var available = pool.Where(t => !_seenThisSession.Contains(t.id)).ToList();

        if (available.Count == 0) {
            // 모두 소진 → 리셋
            _seenThisSession.Clear();
            available = pool.ToList();
            if (available.Count == 0)
                return string.Empty;
        }

        // 3) 가중치 랜덤
        int totalWeight = available.Sum(t => Math.Max(1, t.weight));
        int pick = UnityEngine.Random.Range(1, totalWeight + 1);
        int cum = 0;
        TipEntry chosen = available[0];
        foreach (var t in available) {
            cum += Math.Max(1, t.weight);
            if (pick <= cum) { chosen = t; break; }
        }

        _seenThisSession.Add(chosen.id);
        return chosen.text;
    }
}

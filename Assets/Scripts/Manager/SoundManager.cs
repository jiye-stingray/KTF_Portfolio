using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Reflection;
using Object = UnityEngine.Object;

public class SoundManager
{
    private bool isInitialized = false;
    private bool isPlaylistReady = false;

    private AsyncOperationHandle<AudioClip> _bgmHandle;
    private bool _hasBgmHandle;
    private string _dynamicPlaylistName;

    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _sfxHandles = new();

    private float _bgmVolume = 0.3f; // 기본값 1로 변경
    private float _sfxVolume = 1f;
    private bool _isBgmMuted = false;
    private bool _isSfxMuted = false;
    private string _currentBGM = string.Empty;
    
    // 볼륨 상세 조절용 키 (슬라이더 등)
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";
    private const string SFX_VOLUME_KEY = "SFX_VOLUME";
    
    // 음소거 여부 키
    private const string BGM_MUTE_KEY = "BGM_MUTE";
    private const string SFX_MUTE_KEY = "SFX_MUTE";
    
    public void Init()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        // 1. MasterAudio 로드
        GameObject masterRes = Resources.Load<GameObject>("MasterAudio/MasterAudio");
        if (masterRes != null)
        {
            Object.Instantiate(masterRes, Managers.Instance.transform);
            MyLogger.Log("MasterAudio 로드 완료");
        }
        else
        {
            Debug.LogError("MasterAudio 로드 실패");
        }
        
        // 2. Playlist Controller 로드
        if (Object.FindObjectOfType<PlaylistController>() == null)
        {
            var controller = Resources.Load<GameObject>("MasterAudio/PlaylistController");
            if (controller != null)
            {
                Object.Instantiate(controller, Managers.Instance.transform);
                isPlaylistReady = true;
                MyLogger.Log("PlaylistController 로드 완료");
            }
            else
            {
                Debug.LogError("PlaylistController 로드 실패");
            }
        }
        else
        {
            isPlaylistReady = true;
        }
        
        // 3. DSGC Load
        var dsgc = Resources.Load<GameObject>("MasterAudio/DSGC");
        if (dsgc != null)
        {
            Object.Instantiate(dsgc, Managers.Instance.transform);
            MyLogger.Log("DSGC 로드 완료");
        }
        
        // 초기화 직후 저장된 설정을 불러와 적용
        LoadUserSoundSettings();
    }
    
    public void PlaySFX(string groupName, string variationName = null)
    {
        // [수정 1] 값이 없으면 1(켜짐)로 처리
        int sfxSetting = PlayerPrefs.GetInt(ToggleSettingType.SFX.ToString(), 1);

        if (sfxSetting == 0)
        {
            return; // 효과음 꺼짐 설정이면 재생 안 함
        }
        
        if (!MasterAudio.SoundGroupExists(groupName))
        {
            MyLogger.LogWarning($"[SoundManager] 그룹 '{groupName}' 없음");
            return;
        }

        if (!string.IsNullOrEmpty(variationName))
        {
            MasterAudio.PlaySound(groupName, variationName: variationName);
        }
        else
        {
            MasterAudio.PlaySound(groupName);
        }
    }

    public void PlayBGM(string playlistName)
    {
        // BGM도 켜져있는지 확인 (값이 없으면 1)
        int bgmSetting = PlayerPrefs.GetInt(ToggleSettingType.BGM.ToString(), 1);
        if (bgmSetting == 0)
        {
            StopBGM(); // BGM 꺼짐 설정이면 멈춤
            return;
        }

        if (!isPlaylistReady)
        {
            MyLogger.LogWarning("PlaylistController 준비되지 않음");
            return;
        }

        var controller = MasterAudio.OnlyPlaylistController;
        if (controller == null)
        {
            Debug.LogError("PlaylistController가 씬에 없음");
            return;
        }

        // 이미 재생 중인 곡이면 리턴
        if (controller.CurrentPlaylist != null && controller.CurrentPlaylist.playlistName == playlistName)
        {
            MyLogger.Log($"[SoundManager] 이미 '{playlistName}' 재생 중");
            return;
        }
        
        _currentBGM = playlistName;
        
        MasterAudio.StartPlaylist(playlistName);
        
        // 볼륨 재적용 (가끔 초기화되는 이슈 방지)
        SetBGMVolume(_bgmVolume);

        MyLogger.Log($"BGM 재생 시작: {playlistName}");
    }
    
    public void StopCharacterVoice(string groupName)
    {
        if (!MasterAudio.SoundGroupExists(groupName))
        {
            // MyLogger.LogWarning($"[SoundManager] 중지하려는 그룹 '{groupName}' 없음");
            return;
        }
        MasterAudio.StopAllOfSound(groupName);
    }

    public void StopBGM()
    {
        var controller = MasterAudio.OnlyPlaylistController;
        controller?.StopPlaylist();
        _currentBGM = string.Empty;

        if (!string.IsNullOrEmpty(_dynamicPlaylistName))
        {
            MasterAudio.DeletePlaylist(_dynamicPlaylistName);
            _dynamicPlaylistName = string.Empty;
        }

        if (_hasBgmHandle)
        {
            Addressables.Release(_bgmHandle);
            _hasBgmHandle = false;
        }
    }

    /// <summary>
    /// Addressable 주소로 BGM을 동적으로 로드해 재생한다.
    /// 에디터에서 미리 만든 Playlist가 있으면 그대로 사용하고, 없으면 런타임에 생성한다.
    /// </summary>
    public async UniTask PlayBGMAsync(string bgmKey, bool loop = true)
    {
        int bgmSetting = PlayerPrefs.GetInt(ToggleSettingType.BGM.ToString(), 1);
        if (bgmSetting == 0)
        {
            StopBGM();
            return;
        }

        if (!isPlaylistReady)
        {
            MyLogger.LogWarning("[SoundManager] PlaylistController 준비되지 않음");
            return;
        }

        var controller = MasterAudio.OnlyPlaylistController;
        if (controller == null)
        {
            Debug.LogError("[SoundManager] PlaylistController가 씬에 없음");
            return;
        }

        if (_currentBGM == bgmKey)
            return;

        if (MasterAudio.GrabPlaylist(bgmKey, false) == null)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(bgmKey);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[SoundManager] BGM 로드 실패: {bgmKey}");
                Addressables.Release(handle);
                return;
            }

            _bgmHandle = handle;
            _hasBgmHandle = true;

            var playlist = new MasterAudio.Playlist { playlistName = bgmKey };
            MasterAudio.CreatePlaylist(playlist, false);
            MasterAudio.AddSongToPlaylist(bgmKey, handle.Result, loopSong: loop);
            _dynamicPlaylistName = bgmKey;
        }

        _currentBGM = bgmKey;
        MasterAudio.StartPlaylist(bgmKey);
        SetBGMVolume(_bgmVolume);
        MyLogger.Log($"[SoundManager] BGM 재생: {bgmKey}");
    }

    public bool IsBGMPlaying(string playlistName)
    {
        var controller = MasterAudio.OnlyPlaylistController;
        return controller != null &&
               controller.CurrentPlaylist != null &&
               controller.CurrentPlaylist.playlistName == playlistName;
    }
    
    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);

        var controller = MasterAudio.OnlyPlaylistController;
        if (controller != null)
        {
            controller.PlaylistVolume = _bgmVolume;
        }
    }

    // SFX 볼륨 설정 (모든 그룹 일괄 적용)
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        MasterAudio.MasterVolumeLevel = _sfxVolume;
    }
    
    public void ToggleBGMMute(bool mute)
    {
        _isBgmMuted = mute;
        MasterAudio.PlaylistsMuted = _isBgmMuted;
        
        // 뮤트 해제 시 현재 BGM 다시 재생 시도 (선택 사항)
        // if (!mute && !string.IsNullOrEmpty(_currentBGM)) PlayBGM(_currentBGM);
    }

    public void ToggleSFXMute(bool mute)
    {
        _isSfxMuted = mute;
        MasterAudio.MixerMuted = mute;
    }
    
    private static Action<string, float> _setGroupVolume;
    private static Action<string, float, float> _fadeGroupVolume;

    private static bool _maMethodsCached;
    
    private static void CacheMasterAudioMethods()
    {
        if (_maMethodsCached) return;
        _maMethodsCached = true;

        _setGroupVolume = CreateAction<string, float>(
            typeof(MasterAudio),
            new[] { "SetGroupVolume", "SetSoundGroupVolume" }
        );

        _fadeGroupVolume = CreateAction<string, float, float>(
            typeof(MasterAudio),
            new[] { "FadeSoundGroupToVolume", "FadeGroupToVolume" }
        );
    }

    private static Action<T1, T2> CreateAction<T1, T2>(Type type, string[] names)
    {
        foreach (var n in names)
        {
            var mi = type.GetMethod(n, BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(T1), typeof(T2) }, null);
            if (mi != null)
                return (Action<T1, T2>)Delegate.CreateDelegate(typeof(Action<T1, T2>), mi);
        }
        return null;
    }

    private static Action<T1, T2, T3> CreateAction<T1, T2, T3>(Type type, string[] names)
    {
        foreach (var n in names)
        {
            var mi = type.GetMethod(n, BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(T1), typeof(T2), typeof(T3) }, null);
            if (mi != null)
                return (Action<T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3>), mi);
        }
        return null;
    }

    private static void SetGroupVolumeSafe(string groupName, float vol01)
    {
        CacheMasterAudioMethods();
        _setGroupVolume?.Invoke(groupName, vol01);
    }

    private static void FadeGroupVolumeSafe(string groupName, float vol01, float fadeTime)
    {
        CacheMasterAudioMethods();
        _fadeGroupVolume?.Invoke(groupName, vol01, fadeTime);
    }

    //
    
    /// <summary>
    /// 환경음/비 같은 "루프 SFX 그룹" 재생.
    /// - SFX 토글이 OFF면 재생하지 않음.
    /// - MasterAudio 쪽 SoundGroup(variation)에 Loop가 설정되어 있어야 "계속" 재생됨.
    /// </summary>
    public void PlayLoopingSFXGroup(string groupName, float targetVolume01 = 1f, float fadeTime = 0f)
    {
        // 효과음 토글 (값 없으면 1)
        int sfxSetting = PlayerPrefs.GetInt(ToggleSettingType.SFX.ToString(), 1);
        if (sfxSetting == 0) return;

        if (!MasterAudio.SoundGroupExists(groupName))
        {
            MyLogger.LogWarning($"[SoundManager] 그룹 '{groupName}' 없음");
            return;
        }

        targetVolume01 = Mathf.Clamp01(targetVolume01);

        // 페이드 인을 할 거면 먼저 0으로 내려두고 재생 후 페이드
        if (fadeTime > 0f)
            SetGroupVolumeSafe(groupName, 0f);
        else
            SetGroupVolumeSafe(groupName, targetVolume01);

        MasterAudio.PlaySound(groupName);

        if (fadeTime > 0f)
            FadeGroupVolumeSafe(groupName, targetVolume01, fadeTime);
    }

    /// <summary>
    /// 루프 SFX 그룹 정지.
    /// - fadeTime > 0이면 0으로 페이드 후 Stop.
    /// </summary>
    public void StopLoopingSFXGroup(string groupName, float fadeTime = 0f)
    {
        if (!MasterAudio.SoundGroupExists(groupName))
            return;

        if (fadeTime > 0f)
        {
            FadeGroupVolumeSafe(groupName, 0f, fadeTime);
            // 엄밀히 하려면 fadeTime 이후에 Stop 해야 하지만,
            // 대부분 즉시 Stop해도 큰 문제 없고,
            // 필요 시 코루틴/딜레이 Stop 버전으로 확장 가능.
        }

        MasterAudio.StopAllOfSound(groupName);
    }

    /// <summary>
    /// 낮/밤 앰비언스 전환(기존 그룹 stop + 새 그룹 play)
    /// </summary>
    public void SwitchLoopingAmbience(string fromGroup, string toGroup, float toVolume01, float fadeTime)
    {
        if (string.IsNullOrEmpty(toGroup))
            return;

        // 같은 그룹이면 아무것도 하지 않음
        if (!string.IsNullOrEmpty(fromGroup) && fromGroup == toGroup)
            return;

        if (!string.IsNullOrEmpty(fromGroup))
            StopLoopingSFXGroup(fromGroup, fadeTime);

        PlayLoopingSFXGroup(toGroup, toVolume01, fadeTime);
    }
    
    #region SFX Addressable

    public async UniTask PlaySFXAddressableAsync(string addressKey)
    {
        int sfxSetting = PlayerPrefs.GetInt(ToggleSettingType.SFX.ToString(), 1);
        if (sfxSetting == 0) return;

        if (!_sfxHandles.ContainsKey(addressKey))
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(addressKey);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[SoundManager] SFX 로드 실패: {addressKey}");
                Addressables.Release(handle);
                return;
            }

            _sfxHandles[addressKey] = handle;

            if (!MasterAudio.SoundGroupExists(addressKey))
                CreateSFXGroup(addressKey, handle.Result);
        }

        MasterAudio.PlaySound(addressKey);
    }

    public void ReleaseSFXCache()
    {
        foreach (var pair in _sfxHandles)
        {
            if (MasterAudio.SoundGroupExists(pair.Key))
                MasterAudio.DeleteSoundGroup(pair.Key);
            Addressables.Release(pair.Value);
        }
        _sfxHandles.Clear();
    }

    private static void CreateSFXGroup(string groupName, AudioClip clip)
    {
        var go = new GameObject(groupName);
        var dsg = go.AddComponent<DynamicSoundGroup>();
        dsg.groupMasterVolume = 0.5f;

        var variationGo = new GameObject(clip.name);
        variationGo.transform.SetParent(go.transform);
        variationGo.AddComponent<AudioSource>().clip = clip;
        var dgv = variationGo.AddComponent<DynamicGroupVariation>();
        dgv.audLocation = MasterAudio.AudioLocation.Clip;
        dsg.groupVariations.Add(dgv);

        MasterAudio.CreateSoundGroup(dsg, null, errorOnExisting: false);
        Object.Destroy(go);
    }

    #endregion

    #region Save/Load
    public void LoadUserSoundSettings()
    {
        // 1. 볼륨 및 뮤트 설정 로드 (기본값 설정)
        _bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.3f);
        _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        _isBgmMuted = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1; // 0(False)이 기본값
        _isSfxMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;

        // 2. MasterAudio에 적용
        SetBGMVolume(_bgmVolume);
        SetSFXVolume(_sfxVolume);
        ToggleBGMMute(_isBgmMuted);
        ToggleSFXMute(_isSfxMuted);
        
        // 3.토글 설정(BGM 켜기/끄기)도 확인하여 BGM 멈춤 처리
        // 슬라이더 볼륨은 1이어도, 토글 스위치가 꺼져있으면 소리가 안나야 함
        int bgmToggle = PlayerPrefs.GetInt(ToggleSettingType.BGM.ToString(), 1);
        if (bgmToggle == 0)
        {
            StopBGM();
        }
    }

    public void SaveUserSoundSettings()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _bgmVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
        PlayerPrefs.SetInt(BGM_MUTE_KEY, _isBgmMuted ? 1 : 0);
        PlayerPrefs.SetInt(SFX_MUTE_KEY, _isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    #endregion
}
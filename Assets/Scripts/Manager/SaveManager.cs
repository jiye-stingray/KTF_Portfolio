using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveQuestData
{
    public int idx;
    public int progressValue;
}

[Serializable]
public class SaveCharacterData
{
    public Dictionary<int, SaveCharacterDto> dicCharacterItemData = new Dictionary<int, SaveCharacterDto>();
}

public class SaveCharacterDto
{
    public int id;
    public int level;
    public bool isOpen;
}

[Serializable]
public class SaveBuildingData
{
    public Dictionary<int,bool> dicInstallBuildingData = new Dictionary<int,bool>();
}


public class SaveManager
{
    private Dictionary<string, int> saveValueDic = new Dictionary<string, int>();

    public static string GetPath() //플랫폼 별 파일이 저장되는 위치 불러오기
    {
        string userPath = $"Resources/SaveDB/{Managers.Instance.UserInfo().userId}/";
        string path = null;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(Application.persistentDataPath, userPath);
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                path = Application.persistentDataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", userPath);
            case RuntimePlatform.WindowsEditor:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, "Assets", userPath);
            default:
                path = Application.dataPath;
                path = path.Substring(0, path.LastIndexOf('/'));
                return Path.Combine(path, userPath);
        }
    }

    public static void SaveData(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }
    
    public static string LodeData(string key)
    {
        if(!PlayerPrefs.HasKey(key))
            return "";
        
        return PlayerPrefs.GetString(key);
    }

    public static void RemoveData(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }
}

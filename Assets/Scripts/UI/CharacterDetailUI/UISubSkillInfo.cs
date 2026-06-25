using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class UISubSkillInfo : UISubBase
{
    [Header("Reward")]
    [SerializeField] ScrollRectDynamicPopulator _scrollView;
    List<ItemData> _items = new List<ItemData>();

    public void SetSkillInfo(int level, Dictionary<int, List<SkillDescriptionArg>> argDic)
    {
        OpenToStack();
        _items.Clear();
        
        int scrollIndex = 0;
        int[] keys = argDic.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            int key = keys[i];
            List<SkillDescriptionArg> arg = argDic[key];
            SkillInfoItemData skillInfoItemData = new SkillInfoItemData();
            skillInfoItemData._skillLevel = key;
            skillInfoItemData._activeLevel = level;
            skillInfoItemData._args = arg;
            
            if(key == level)
                scrollIndex = i;
            
            _items.Add(skillInfoItemData);
        }
        
        _scrollView.Init((cell, data, index) =>
        {
            cell.SetData(data, index);
        });

        _scrollView.Populate(_items);
        _scrollView.ScrollToIndexNextFrame(scrollIndex);
    }
}
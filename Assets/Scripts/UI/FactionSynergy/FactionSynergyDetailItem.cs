using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class FactionSynergyDetailItem : MonoBehaviour
{
    [SerializeField] EFactionType _factionType;
    [SerializeField] Image _factionImage;
    [SerializeField] TMP_Text _factionName;
    [SerializeField] TMP_Text _deckCountText;
    
    [SerializeField] GameObject[] _factionTextObjects;
    [SerializeField] GameObject[] _factionEnable;
    [SerializeField] TMP_Text[] _factionCountText;
    [SerializeField] TMP_Text[] _factionStatusText;

    public EFactionType GetFactionType() => _factionType;
    
    StringBuilder _stringBuilder = new StringBuilder();
    private List<FactionSynergy> _factionSynergies;
    private void Start()
    {
        _factionImage.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.IconAtlas, $"UI_Icon_Type_Race_0{(int)_factionType}");
        _factionName.text = ReturnFactionString(_factionType);
        
        _factionSynergies ??= ClientLocalDB_Simple.GetDB<FactionSynergy>(DBKey.FactionSynergy).Values.ToList()
            .FindAll(row => row.Faction == _factionType);
        for (int i = 0; i < _factionTextObjects.Length; i++)
        {
            GameObject factionText = _factionTextObjects[i];
            factionText.SetActive(i < _factionSynergies.Count);

            if (i >= _factionSynergies.Count)
                continue;

            FactionSynergy factionSynergy = _factionSynergies[i];
            _stringBuilder.Clear();
            _factionCountText[i].text = $"{factionSynergy.RequiredCount}명";
            if (factionSynergy.AttackPercent > 0)
                _stringBuilder.Append(
                    $"{Status.ReturnStatusString(EStatus.AttackPercent)} +{factionSynergy.AttackPercent / 100}%");

            if (factionSynergy.AttackPercent > 0 && factionSynergy.DefPercent > 0)
                _stringBuilder.Append("/");

            if (factionSynergy.DefPercent > 0)
                _stringBuilder.Append(
                    $"{Status.ReturnStatusString(EStatus.DefPercent)} +{factionSynergy.DefPercent / 100}%");
            
            if ((factionSynergy.AttackPercent > 0 || factionSynergy.DefPercent > 0) && factionSynergy.MaxHealthPointPercent > 0)
                _stringBuilder.Append("/");
            
            if (factionSynergy.MaxHealthPointPercent > 0)
                _stringBuilder.Append(
                    $"{Status.ReturnStatusString(EStatus.MaxHealthPointPercent)} +{factionSynergy.MaxHealthPointPercent / 100}%");
            
            _factionStatusText[i].text = _stringBuilder.ToString();
        }
    }

    public void SetData(int deckCount)
    {
        _deckCountText.text = $"현재 편성 인원: {deckCount}명";

        int enableIndex = -1;
        _factionSynergies ??= ClientLocalDB_Simple.GetDB<FactionSynergy>(DBKey.FactionSynergy).Values.ToList()
            .FindAll(row => row.Faction == _factionType);
        int statusCount = _factionSynergies.Count;
        for (int i = 0; i < statusCount; i++)
        {
            FactionSynergy factionSynergy = _factionSynergies[i];
            if(factionSynergy.RequiredCount <= deckCount)
                enableIndex = i;
        }

        for (int i = 0; i < statusCount; i++)
        {
            bool isEnable = enableIndex == i;
            _factionEnable[i].SetActive(isEnable);
            _factionCountText[i].color = isEnable ? Utils.HexToColor("#261E17") : Utils.HexToColor("#D2AC8C");
            _factionStatusText[i].color = isEnable ? Utils.HexToColor("#261E17") : Utils.HexToColor("#D2AC8C");
        }
    }
}
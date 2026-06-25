using PolyAndCode.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class TrainingScrollviewItem : ICell
{
    [SerializeField] GameObject _basicRedDot;
    [SerializeField] GameObject _hardRedDot;

    [SerializeField] Image _bgImg;
    [SerializeField] Image _lineImg;
    [SerializeField] Image _bgGrayImg;
    [SerializeField] Image _lineGrayImg;

    [Header("Basic")]
    [SerializeField] Image _basicBarImg;
    [SerializeField] Image _basicBtnImg;
    [SerializeField] Image _basicIconImg;
    [SerializeField] Image _basicGrayImg;
    [SerializeField] Image _basicGrayIcon;
    [SerializeField] Image _basicGrayAbleImg;
    [SerializeField] TMP_Text _basicValueTxt;
    [SerializeField] Animator _basicAnim;

    [Header("Level")]
    [SerializeField] GameObject _levelObj;
    [SerializeField] TMP_Text _levelTxt;

    [Header("Hard")]
    [SerializeField] Image _hardBarImg;
    [SerializeField] Image _hardBtnImg;
    [SerializeField] Image _hardIconImg;
    [SerializeField] Image _hardGrayImg;
    [SerializeField] Image _hardGrayIcon;
    [SerializeField] Image _hardGrayAbleImg;
    [SerializeField] Animator _hardAnim;

    public TrainingItemData _data;
    
    public override void SetData(ItemData data, int index)
    {
        _data = data as TrainingItemData;
        _index = index;
        RefreshCellView();
    }

    public void RefreshCellView()
    {
        if (_data == null) Debug.LogError("data null!!");

        // Basic
        DrawBasicItem(_data._trainingBasicData);

        // Level
        if (_data.islevelShow)
        {
            _levelObj.SetActive(true);
            _levelTxt.text = _data.level.ToString();
        }
        else
        {
            _levelObj.SetActive(false);
        }

        //Hard
        DrawHardItem(_data._trainingHardData);
    }

    private void DrawBasicItem(BasicTraining basicData)
    {
        // bar
        _basicBarImg.gameObject.SetActive((basicData.ID < UserInfo._trainingItemList.Count));       // 가장 위에 Bar 비활성화
        if (_basicBarImg.gameObject.activeSelf)
        {
            if (basicData.ID <= UserInfo.UnlockBasicIdx)      // 해금 완료 상태
                _basicBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"Line_Yellow");
            else
                _basicBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, "Line_Gray");
        }

        // gray
        _basicGrayImg.gameObject.SetActive(basicData.ID > UserInfo.UnlockBasicIdx);

        // Icon
        _basicGrayIcon.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"{basicData.Icon}_Gray");
        _basicIconImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"{basicData.Icon}");

        // text
        _basicValueTxt.text = $"+{basicData.StatusValue}";

        // able
        _basicGrayAbleImg.gameObject.SetActive(false);
        if (_basicGrayImg.gameObject.activeSelf)
        {
            // level 제한
/*            if (_data.level <= UserInfoData._resonanceLevel.Value)
            {
                _basicGrayAbleImg.gameObject.SetActive(basicData.ID - 1 == UserInfoData.UnlockBasicIdx);
                _basicGrayImg.gameObject.SetActive(!_basicGrayAbleImg.gameObject.activeSelf);
            }*/

        }

        string valueTxtHaxCode = _basicGrayImg.gameObject.activeSelf ? "947B52" : "F7F7F7";
        _basicValueTxt.color = Utils.HexToColor(valueTxtHaxCode);

        // -----------------------------------------------------

        // bg
        _bgGrayImg.gameObject.SetActive(basicData.ID > UserInfo.UnlockBasicIdx);

        // -------------------------------------------------------

        // line
        // 활성화 된 최상단에서만 활성화

        _lineImg.gameObject.SetActive(!_bgGrayImg.gameObject.activeSelf && basicData.ID >= UserInfo.UnlockBasicIdx);

        // LevelLine
        if (_data.islevelShow && basicData.ID > UserInfo.UnlockBasicIdx)
            _lineGrayImg.gameObject.SetActive(true);
        else
            _lineGrayImg.gameObject.SetActive(false);


        // RedDot
        _basicRedDot.SetActive(RedDotManager.BasicTrainingRedDot(_data));

        // Anim
        if (UIManager.TrainingUI.ShowAnim(ETrainingType.Basic, basicData.ID))
        {
            _basicAnim.SetTrigger("Unlock");
            UIManager.TrainingUI._isAnim = false;
        }

    }

    private void DrawHardItem(HardTraining hardData)
    {
        _hardBtnImg.gameObject.SetActive(hardData != null);

        int firstLevel = ClientLocalDB_Simple.GetDB<HardTraining>(DBKey.HardTraining).FirstOrDefault().Value.BasicTrainingLimit;

        if (hardData == null)
        {
            //Bar 
            _hardBarImg.gameObject.SetActive(_data._trainingBasicData.ID > firstLevel);      // 맨 아래 Bar는 비활성화

            CheckLevelHardData();
            _hardRedDot.SetActive(false);

            return;
        }

        _hardBarImg.gameObject.SetActive(true);
        //Bar
        if (hardData.ID <= UserInfo.UnlockHardIdx)      // 해금 완료 상태 
        {
            _hardBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"Line_Purple");
        }
        else    // 미해금
        {
            _hardBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, "Line_Gray");
        }

        //Icon
        _hardIconImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"{hardData.Icon}");
        _hardGrayImg.gameObject.SetActive(hardData.ID > UserInfo.UnlockHardIdx);
        if (_hardGrayImg.gameObject.activeSelf) _hardGrayIcon.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"{hardData.Icon}_Gray"); 


        // able
        _hardGrayAbleImg.gameObject.SetActive(false);
        if (_hardGrayImg.gameObject.activeSelf)
        {
            // level 제한
/*                if (_data.level <= UserInfoData._resonanceLevel.Value)
            {
                _hardGrayAbleImg.gameObject.SetActive(hardData.ID - 1 == UserInfoData.UnlockHardIdx);
                _hardGrayImg.gameObject.SetActive(!_hardGrayAbleImg.gameObject.activeSelf);
            }*/

        }

        // RedDot
        _hardRedDot.SetActive(RedDotManager.HardTrainingRedDot(_data));

        // Anim
        if (UIManager.TrainingUI.ShowAnim(ETrainingType.Hard, hardData.ID))
        {
            _hardAnim.SetTrigger("Unlock");
            UIManager.TrainingUI._isAnim = false;
        }


    }

    private void CheckLevelHardData()
    {
        for (int i = _data._trainingBasicData.ID; i > 0; i--)
        {
            HardTraining tempData = ClientLocalDB_Simple.GetDB<HardTraining>(DBKey.HardTraining).Where(item => item.Value.BasicTrainingLimit == i - 1).FirstOrDefault().Value;
            if (tempData != null)      
            {
                if (tempData.ID <= UserInfo.UnlockHardIdx)
                {
                    _hardBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"Line_Purple");
                }
                else // 미해금
                {
                    _hardBarImg.sprite = AtlasManager.GetSprite(EAtlasType.TrainingAtlas, $"Line_Gray");
                }

                return;
            }

        }
    }

    public void BasicBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        Managers.Instance.GetUIManager().TrainingUI.SetCurrentUITrainingUnlock(_data, ETrainingType.Basic, _basicIconImg.GetComponent<RectTransform>());
    }

    public void HardBtnClick()
    {
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        Managers.Instance.GetUIManager().TrainingUI.SetCurrentUITrainingUnlock(_data, ETrainingType.Hard, _hardBtnImg.GetComponent<RectTransform>());
    }

    /// <summary>
    /// 튜토리얼을 위한 함수
    /// </summary>
    /// <returns></returns>
    public RectTransform ReturnBasicIconImgRect()
    {
        return _basicIconImg.GetComponent<RectTransform>();
    }
}

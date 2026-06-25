using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UITrainingUnlock : UISubBase
{
    [SerializeField] RectTransform bgRectTransform;

    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] TrainingUnlockCostButton _costBtn;
    [SerializeField] GameObject _clearGo;
    [SerializeField] GameObject _lockGo;

    ETrainingType _type;
    TrainingItemData _data;

    int _bgBasicOffsetY = 125;
    int _bgHardOffsetY = 142;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void InitData(TrainingItemData data, ETrainingType type, RectTransform transform)
    {
        _type = type;
        _data = data;

        bgRectTransform.position = transform.position;
        float offsety = _type == ETrainingType.Basic ? _bgBasicOffsetY : _bgHardOffsetY;
        bgRectTransform.anchoredPosition = new Vector3(bgRectTransform.anchoredPosition.x,
            bgRectTransform.anchoredPosition.y + offsety, 
            0);

    }

    public override void Refresh()
    {
        _lockGo.gameObject.SetActive(false);
        switch (_type)
        {
            case ETrainingType.Basic:

                _nameText.text = $"<color=#D6000C>{Status.ReturnStatusString(_data._trainingBasicData.StatusType)}</color> 증가";
                _valueTxt.text = $"모든 영웅들의 {Status.ReturnStatusString(_data._trainingBasicData.StatusType)}이 \n" +
                                    $"<color=#198400>+{_data._trainingBasicData.StatusValue}</color> 만큼 증가합니다";

                // 계정 레벨로 버튼 비활성화
                if (_data._trainingBasicData.AccountLevelLimit > UserInfoData.userLevel.Value)
                {
                    _costBtn.gameObject.SetActive(false);
                    _clearGo.SetActive(false);
                    _lockGo.SetActive(true);
                    return;
                }

                _costBtn.gameObject.SetActive(_data._trainingBasicData.ID == UserInfoData.UnlockBasicIdx + 1);
                _clearGo.SetActive(_data._trainingBasicData.ID <= UserInfoData.UnlockBasicIdx);

                ECurrency currencyB = _data._trainingBasicData.LevelUpCostCurrency;
                if (_costBtn.gameObject.activeSelf)
                    _costBtn.Init(new Define.ECurrency[] { currencyB },new int[] { _data._trainingBasicData.LevelUpCostValue });

                break;


            case ETrainingType.Hard:

                switch (_data._trainingHardData.RewardType)
                {
                    case Define.EHardTrainingType.PortalEnable:
                        _nameText.text = "포탈 기능 활성화";
                        _valueTxt.text = "포탈 기능 활성화";
                        break;
                    case Define.EHardTrainingType.Status:
                        _nameText.text = $"<color=#D6000C>{Status.ReturnStatusString(_data._trainingHardData.StatusType)}</color> 증가";
                        _valueTxt.text = $"모든 영웅들의 {Status.ReturnStatusString(_data._trainingHardData.StatusType)}이 " + 
                                            $"<color=#198400>+{_data._trainingHardData.StatusValue / 100f}%</color> 만큼 증가합니다";
                        break;
                    default:
                        break;
                }

                // 기본 훈련 레벨에 따른 제한
                if (UserInfoData.UnlockBasicIdx < _data._trainingHardData.BasicTrainingLimit)
                {
                    _costBtn.gameObject.SetActive(false);
                    _clearGo.SetActive(false);
                    _lockGo.SetActive(true);
                    return;
                }

                _costBtn.gameObject.SetActive(_data._trainingHardData.ID == UserInfoData.UnlockHardIdx + 1);
                _clearGo.SetActive(_data._trainingHardData.ID <= UserInfoData.UnlockHardIdx);

                ECurrency currencyH = _data._trainingHardData.LevelUpCostCurrency;
                if (_costBtn.gameObject.activeSelf)
                    _costBtn.Init(new Define.ECurrency[] { Define.ECurrency.Special_Ingot }, new int[] { _data._trainingHardData.LevelUpCostValue });

                break;


            default:
                break;
        }



    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public void UnlockSuccessAction()
    {
        switch (_type)
        {
            case ETrainingType.Basic:

                // 서버 통신 
                BestHttp_GameManager.OnBasicTrainingResponse(_data._trainingBasicData.ID);

                break;
            case ETrainingType.Hard:

                // 서버 통신 
                BestHttp_GameManager.OnHardTrainingResponse(_data._trainingHardData.ID);
                break;
            default:
                break;
        }
        

        Close();
    }

    public void ClickLockBtn()
    {
        switch (_type)
        {
            case ETrainingType.Basic:
                UIManager.ShowCommonToastMessage("계정 레벨이 낮습니다");
                break;
            case ETrainingType.Hard:
                UIManager.ShowCommonToastMessage("일반 훈련 레벨이 낮습니다");
                break;
            default:
                break;
        }
    }
}

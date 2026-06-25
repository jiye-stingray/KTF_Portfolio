using PolyAndCode.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassBannerScrollviewItem : ICell
{
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] UITimer _uiTimer;
    [SerializeField] Image _bgImg;

    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _timericon;

    [SerializeField] GameObject _reddot;

    PassItemData _passItemData;

    public override void SetData(ItemData data, int index)
    {
        _passItemData = data as PassItemData;
        _index = index;

        _uiTimer.gameObject.SetActive(!_passItemData.data.BannerClose);
        _timericon.gameObject.SetActive(!_passItemData.data.BannerClose);
        if (!_passItemData.data.BannerClose)
        {
            //TimeData 
            DateTime endTime = _passItemData.endTime;
            DateTime now = ServerTime.Instance.CurrentTime();
            TimeSpan durationTimeSpan = endTime - now;
            _timericon.SetActive(durationTimeSpan.TotalDays <= 7); // 7일 이하일 때


            TimeData timeData = new TimeData();
            timeData.SetByDuration(durationTimeSpan.TotalSeconds);
            _uiTimer.Set(timeData, "{0} 남음");
        }

        Refresh();
    }

    private void Refresh()
    {

        _bgImg.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/PassBanner/pass_full_banner_{_passItemData.passType.ToString()}");
        _nameTxt.text = ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup, (int)_passItemData.passType).Name;
        _gray.gameObject.SetActive(_passItemData.data.OpenLevel > UserInfo.userLevel.Value);

        _reddot.SetActive(RedDotManager.PassBannerRedDot(_passItemData));
    }

    public void Click()
    {
        if (_gray.activeSelf)
        {
            UIManager.ShowCommonToastMessage("아직 해금되지 않은 패스 입니다.");
            return;
        }

        Managers.Instance.GetServerManager().OnGetPassInfo(() =>
        {
            if (OnClick != null)
                OnClick((int)_passItemData.passType);
        });

    }
}

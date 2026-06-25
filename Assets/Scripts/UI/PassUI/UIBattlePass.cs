using PolyAndCode.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#if IAP
using UnityEngine.Purchasing;
#endif
using UnityEngine.UI;
using static Define;

public class UIBattlePass : UIBase, IRecyclableScrollRectDataSource
{
    int _passType = 1;
    int _xp;

    [Header("Info")]
    [SerializeField] Image _passTopBanner;
    [SerializeField] TMP_Text _nameTxt;
    [SerializeField] UITimer _timer;
    [SerializeField] Slider _xpSlider;
    [SerializeField] TMP_Text _xpSlidertxt;
    [SerializeField] TMP_Text _currentLevelTxt;
    [SerializeField] TMP_Text _desciption;


    [Header("Scrollview")]
    [SerializeField] BattlePassScrollviewItem _maxLevelScrollviewItem;          // 최 하단에 고정된 scrollview Item
    [SerializeField] RecyclableScrollRect _scrollRect;

    [Header("Button")]
    [SerializeField] GameObject _buttonGray;
    [SerializeField] TMP_Text _primiumPassBtnTxt;

    PassItemData _passItemData;

    public void SetPassTypeOpenToStack(int passType)
    {
        _passType = passType;
        _passItemData = UserInfoData.GetPassItemData(_passType);

        _timer.gameObject.SetActive(!_passItemData.data.BannerClose);

        if (!_passItemData.data.BannerClose)
        {
            //TimeData 
            DateTime endTime = _passItemData.endTime;
            DateTime now = ServerTime.Instance.CurrentTime();
            TimeSpan durationTimeSpan = endTime - now;


            TimeData timeData = new TimeData();
            timeData.SetByDuration(durationTimeSpan.TotalSeconds);
            _timer.SetTextFormat("{0} 남음");
            _timer.RegisterOnFinished(delegate { ClickCloseBtn(); });
            _timer.Set(timeData);
        }
        OpenToStack();
    }

    public override void Open()
    {
        base.Open();
#if IAP
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.RegisterShopItems(new[] { _passItemData.data });
        }
        SubscribePass(true);
#endif
        Refresh();
    }

    #region Recycle ScrollView
    public int GetItemCount()
    {
        return _passItemData.data.PassMaxLevel;
    }

    public void SetCell(ICell cell, int index)
    {
        int level = index + 1;
        var item = cell as BattlePassScrollviewItem;
        item.SetData(index, level, _passType);
    }

    #endregion

    public override void Refresh()
    {
        _passItemData = UserInfoData.GetPassItemData(_passType);
        _xp = UserInfoData.GetPassItemData(_passType).passXp;

        PassGroup passGroup = ClientLocalDB_Simple.GetData<PassGroup>(DBKey.PassGroup, (int)_passType);
        _nameTxt.text = passGroup.Name;
        _desciption.text = passGroup.Desc;
        _passTopBanner.sprite = Managers.Instance.GetResObjectManager().Load<Sprite>($"Texture/PassBanner/pass_full_banner_{_passItemData.passType.ToString()}");


        // 예외처리 --------------------------
        if (_passItemData.CurrentPassData.LevelUpValue == 0)
        {
            _xpSlider.value = _xpSlider.maxValue;
            _xpSlidertxt.text = "MAX";
            _currentLevelTxt.text = _passItemData.passLevel.ToString();
        }
        else
        {
            if(_passItemData.data.LevelUpType == EpassLevelUpType.UserLevel)
            {
                int level = UserInfoData.userLevel.Value;
                UserLevelData userLevel = ClientLocalDB_Simple.GetData<UserLevelData>(DBKey.UserLevel, level);
                int maxExp = userLevel.Exp;
                int exp = UserInfoData.userExp.Value;
                float gauge = exp / (float)maxExp;

                _xpSlider.value = gauge;
                _xpSlidertxt.text = $"{exp}/{maxExp}";

                _currentLevelTxt.text = level.ToString();

            }
            else
            {

                //slider
                float xpSliderTargetValue = (((float)_xp) / _passItemData.CurrentPassData.LevelUpValue);
                _xpSlider.value = xpSliderTargetValue;
                _xpSlidertxt.text = $"{_xp}/{_passItemData.CurrentPassData.LevelUpValue}";
                _currentLevelTxt.text = _passItemData.passLevel.ToString();
            }

        }

        // scrollview
        _scrollRect.Initialize(this);
        _maxLevelScrollviewItem.SetData(_passItemData.data.PassMaxLevel, _passType);

        
        _scrollRect.ScrollToIndex(_passItemData.passLevel - 1);

        _buttonGray.SetActive(_passItemData.isPremium);

#if IAP
        if(!_buttonGray.activeSelf )
        {
            Product p = Managers.Instance.IAP.GetProduct(_passItemData.ProductID);
            _primiumPassBtnTxt.text = $"{p.metadata.localizedPriceString}";
        }
#endif
    }




    public void PrimiumPassBtnClick()
    {
#if USE_SERVER

#if IAP
        if (ShopManager.Instance == null)
        {
            Managers.Instance.GetUIManager()
                .ShowUIToast<UIToastBase>("PassManager 없습니다.", "ToastMessage");
            return;
        }

        if (_buttonGray.activeSelf) return;
        ShopManager.Instance.TryPurchase(_passItemData.data);

#endif
#else

        _passItemData.isPremium = true;
        Refresh();
#endif
    }

    public override void Close()
    {
#if IAP
        SubscribePass(false);
#endif
        base.Close();
    }

#if IAP
    private void SubscribePass(bool sub)
    {
        var shop = ShopManager.Instance;
        if (shop == null) return;

        // 중복 구독 방지(안전)
        shop.OnPurchaseCompleted -= OnPassPurchaseCompleted;
        shop.OnPurchaseFailed -= OnPassPurchaseFailed;
        shop.OnPurchaseStarted -= OnPassPurchaseStarted;
        shop.OnPurchaseAction -= OnPurchaseAction;

        if (!sub) return;

        shop.OnPurchaseCompleted += OnPassPurchaseCompleted;
        shop.OnPurchaseFailed += OnPassPurchaseFailed;
        shop.OnPurchaseStarted += OnPassPurchaseStarted;
        shop.OnPurchaseAction += OnPurchaseAction;
        
        // 구독 직후, 보류돼 있던 미완료 거래 복구 시도
        Managers.Instance?.IAP?.RepublishPendingOrders(
            pid => ShopManager.Instance.IsProductOfKind(pid, ShopManager.EShopKind.Pass));

    }

    private void OnPassPurchaseStarted(ProductShopData item)
    {
        // 선택: 로딩/버튼 잠금 등 하고 싶으면 여기서
        // Managers.Instance.GetUIManager().ShowUIToast<UIToastBase>("구매 처리중...", "ToastMessage");
    }

    private void OnPassPurchaseCompleted(ProductShopData item)
    {
        Managers.Instance.GetUIManager()
            .ShowUIToast<UIToastBase>("구매 완료!", "ToastMessage");

        Refresh();

        // reddot
        UIManager.MainInfoUI.Refresh();

    }

    private void OnPassPurchaseFailed(ProductShopData item, ShopFailReason reason, string detail)
    {
        // detail까지 노출하면 개발 중 디버깅에 좋고, 출시 시엔 reason만 보여줘도 됨
        Managers.Instance.GetUIManager()
            .ShowUIToast<UIToastBase>($"구매 실패: {reason}", "ToastMessage");
        Debug.LogError(detail);
    }

    public void OnPurchaseAction(string goodsId, string payLoad, string shop,
    Action<string> onSuccess)
    {
        Managers.Instance.GetServerManager().OnPostPassPurchase(
        goodsId,
        payLoad,
        shop,
        (response) => { 
            onSuccess.Invoke(response);
            _passItemData = UserInfoData.GetPassItemData(_passType);
            Refresh();
        });
    }
#endif


#if USE_SERVER
#else

    /// <summary>
    /// 클라이언트 용
    /// </summary>
    /// <param name="value"></param>
    public void AddXp(int value)
    {
        // max level 체크
        if (_passItemData.passLevel >= _passItemData.data.PassMaxLevel)
            return;

        int tempXp = _xp + value;

        // 서버 통신 ? & 연출

        // level up 처리
        if(tempXp >= _passItemData.CurrentPassData.LevelUpValue)      // 레벨업 가능한 경험치
        {

            tempXp -= _passItemData.CurrentPassData.LevelUpValue;
            _passItemData.passLevel += 1;
        }

        _xp = tempXp;
        _passItemData.passXp = _xp;
        Refresh();
    }

    /// <summary>
    /// 다음 레벨 구매
    /// not use Server
    /// </summary>
    public void BuyNextLevel()
    {
        // 서버통신? 

        _passItemData.passLevel += 1;
        Refresh();
    }

    #region Test
    [ContextMenu("패스 구매")]
    public void BuyPrimium()
    {
        PrimiumPassBtnClick();
    }

    [ContextMenu("경험치 추가")]
    public void AddTempXP()
    {
        AddXp(120);
    }
    #endregion
#endif


}

using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UICraft : UIBase
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TMP_Text _valueTxt;
    [SerializeField] protected TMP_Text _descTxt;

    [SerializeField] private Transform _content;
    [SerializeField] protected Transform _craftCurrencyAreaContent;

    [SerializeField] private GameObject _selectImg;
    [SerializeField] private GameObject _gray;

    List<UICraftScrollviewItem> _craftScrollviewList = new List<UICraftScrollviewItem>();
    List<CraftCurrencyItem> _craftCurrencyItemList = new List<CraftCurrencyItem>();

    CraftData _currentCraftData = null;
    UserInfoData UserInfo => Managers.Instance.UserInfo();


    public void SetCraftData(CraftData data)
    {
        _currentCraftData = data;
        Refresh();
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        DrawScrollview();

        _selectImg.SetActive(_currentCraftData == null);
        _gray.SetActive(_currentCraftData == null);
        _descTxt.text = "-";
        if (_currentCraftData == null) return;
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);

        int maxCount = GetMaxCraftCount();
        if (maxCount <= 0)
        {
            _slider.minValue = 0;
            _slider.maxValue = 0;
            _slider.value = 0;
            _slider.interactable = false;
            _gray?.SetActive(true);
        }
        else
        {
            _slider.minValue = 1;
            _slider.maxValue = maxCount;
            _slider.value = 1;
            _slider.interactable = true;
            _gray?.SetActive(false);
        }
        _valueTxt.text = _slider.value.ToString();

        _slider.wholeNumbers = true;
        _slider.onValueChanged.AddListener(OnSliderValueChanged);

        CurrencyData rewardCurrency = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _currentCraftData.RewardID);
        _descTxt.text = rewardCurrency.UIName;

        DrawCraftCurrency();
    }

    private void OnSliderValueChanged(float value)
    {
        int count = Mathf.Max(1, (int)value);
        for (int i = 0; i < _craftCurrencyItemList.Count; i++)
        {
            _craftCurrencyItemList[i].SetCurrency(
                (ECurrency)_currentCraftData.CostID[i],
                _currentCraftData.CostCount[i] * count);
        }

        _valueTxt.text = count.ToString();
    }

    private void DrawScrollview()
    {
        List<CraftData> craftdbs = ClientLocalDB_Simple.GetDB<CraftData>(DBKey.CraftData).Values.ToList();

        _craftScrollviewList.ForEach(data => Destroy(data.gameObject));
        _craftScrollviewList.Clear();

        for (int i = 0; i < craftdbs.Count; i++)
        {
            UICraftScrollviewItem item =
                Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/ScrollItem/UICraftScrollviewItem",_content).GetComponent<UICraftScrollviewItem>();
            item.SetData(craftdbs[i], _currentCraftData != null && _currentCraftData.ID == craftdbs[i].ID);
            _craftScrollviewList.Add(item);
        }

    }


    private void DrawCraftCurrency()
    {

        _craftCurrencyItemList.ForEach(item => Destroy(item.gameObject));
        _craftCurrencyItemList.Clear();

        if (_currentCraftData == null || _currentCraftData.CostID == null)
            return;

        for (int i = 0; i < _currentCraftData.CostID.Length; i++)
        {
            CraftCurrencyItem item =
                Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/EtcUI/CraftCurrencyItem", _craftCurrencyAreaContent).GetComponent<CraftCurrencyItem>();
            item.SetCurrency((ECurrency)_currentCraftData.CostID[i], _currentCraftData.CostCount[i]);
            _craftCurrencyItemList.Add(item);
        }
    }

    private int GetMaxCraftCount()
    {
        // 조건 1: RewardID currency의 LimitCount 기준으로 최대 제작 가능 수량
        CurrencyData rewardCurrencyData = ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _currentCraftData.RewardID);
        int currentRewardCount = UserInfo.GetCurrencyValue((ECurrency)_currentCraftData.RewardID);
        int availableSpace = rewardCurrencyData.LimitCount - currentRewardCount;

        // 이미 LimitCount에 도달했거나 초과한 경우
        if (availableSpace <= 0)
            return 0;

        int maxFromLimit = availableSpace / _currentCraftData.RewardCount;

        // 조건 2: 소지 재화 기준으로 최대 제작 가능 수량 (n개 제작 시 비용 = CostCount * n)
        int maxFromCost = int.MaxValue;
        for (int i = 0; i < _currentCraftData.CostID.Length; i++)
        {
            // CostCount가 0이면 해당 재화는 소모 없음 (제한 없음)
            if (_currentCraftData.CostCount[i] <= 0)
                continue;

            int owned = UserInfo.GetCurrencyValue((ECurrency)_currentCraftData.CostID[i]);

            // 재화가 하나도 없으면 제작 불가
            if (owned <= 0)
                return 0;

            maxFromCost = Mathf.Min(maxFromCost, owned / _currentCraftData.CostCount[i]);
        }

        int result = Mathf.Min(maxFromLimit, maxFromCost);

        // 최종적으로 1개도 제작 불가한 경우
        return result <= 0 ? 0 : result;
    }

    public override void Close()
    {
        _currentCraftData = null;
        base.Close();
    }

    public void Click()
    {
        if (_gray.activeSelf) return;

        Managers.Instance.GetServerManager().OnPostCrafting(_currentCraftData.ID, (int)_slider.value);
    }
}

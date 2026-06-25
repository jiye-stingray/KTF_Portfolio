using Cysharp.Threading.Tasks;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class OpenRandomItemButton : MonoBehaviour
{
    [SerializeField] GameObject _gray;
    [SerializeField] GameObject _adImg;
    [SerializeField] TMP_Text _valueTxt;
    [SerializeField] Image _icon;
    [SerializeField] GameObject _free;
    [SerializeField] GameObject _freeGray;
    

    [SerializeField] UISubRandomOpenItem _subUI;


    int _keyID;
    int _keyCnt;
    int _adCnt;

    int _value;

    public void Init(int keyID, int useKeyCnt, int adCnt, int value)
    {
        _keyID = keyID;
        _adCnt = adCnt;
        _keyCnt = useKeyCnt;
        _value = value;
        Refresh();
    }

    private void Refresh()
    {
        _gray.SetActive(false);
        _adImg.SetActive(false);
        _freeGray.SetActive(false);

        //  무료 오픈 
        _free.SetActive(_keyID == 0);
        if (_free.activeSelf)
        {
            _freeGray.SetActive(_value <= 0);
            return;
        }

        if(_value <= 0)
        {
            _gray.SetActive(true);
            return;
        }


        if(_keyCnt > 0)
        {
            _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _keyID).Icon);
            _valueTxt.text = $"{_keyCnt}/{Managers.Instance.UserInfo().GetCurrencyValue((ECurrency)_keyID)}";
        }
        else if(_adCnt > 0)     
        {
            _adImg.SetActive(true);
            _valueTxt.text = $"{_adCnt}/{ClientLocalDB_Simple.GetData<FieldConfig>(DBKey.FieldConfig, "RandomBoxCount").Value}";
            _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, $"Advert_Icon");
        }
        else
        {
            _icon.sprite = Managers.Instance.GetAtlasManager().GetSprite(EAtlasType.ItemIconAtlas, ClientLocalDB_Simple.GetData<CurrencyData>(DBKey.Currency, _keyID).Icon);
            _valueTxt.text = $"{_keyCnt}/{Managers.Instance.UserInfo().GetCurrencyValue((ECurrency)_keyID)}";
            _gray.gameObject.SetActive(true);
        }    

    }

    public void Click()
    {
        if(_freeGray.activeSelf)
        {
            if (_value <= 0)
                Managers.Instance.GetUIManager().ShowCommonToastMessage("1개 이상을 선택하세요.");
            return;
        }

        if (_gray.activeSelf)
        {
            if (_value <= 0)
                Managers.Instance.GetUIManager().ShowCommonToastMessage("1개 이상을 선택하세요.");
            else 
                Managers.Instance.GetUIManager().ShowCommonToastMessage("열쇠가 부족합니다.");
            return;
        }
        if (_free.activeSelf)
        {
            // 바로 오픈 
            _subUI.BtnConnect(false);
            return;
        }

        if(_adImg.gameObject.activeSelf)
        {
            // 광고 
            _subUI.BtnConnect(true);

        }
        else
        {
            // 서버 연결
            _subUI.BtnConnect(false);

        }
    }
}

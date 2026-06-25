using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITabGroupButton : MonoBehaviour
{
    UITabGroup _group;
    public int _index;
    Button _btn;
    GameObject _gray;
    protected GameObject _lock;

    public void Init(UITabGroup group, int index)
    {
        _group = group;
        _index = index;
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);
        _gray = transform.Find("gray")?.gameObject;
        _lock = transform.Find("Lock")?.gameObject;
    }

    public virtual void Set(bool isnotGray)
    {
        _gray?.gameObject.SetActive(!isnotGray);
    }

    public virtual void OnClick()
    {
        // Sound
        Managers.Instance.Sound.PlaySFX("Effect", "BTN_Touch");
        
        _group.Set(_index);
    }
}

using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EmblemScrollviewItem : ICell
{
    [SerializeField] private Image gradeBg;
    [SerializeField] private GameObject goSelect;
    [SerializeField] private GameObject goLock;
    [SerializeField] private GameObject goUnLock;
    [SerializeField] private EmblemIcon emblemIcon;
    
    public EmblemItemData _data;
    public bool isSelect = false;
    AtlasManager _atlasManager => Managers.Instance.GetAtlasManager();
    UnityAction<EmblemScrollviewItem> _clickAction = null;

    public void SetData(ItemData data, int index, UnityAction<EmblemScrollviewItem> clickAction)
    {
        base.SetData(data, index);
        _data = data as EmblemItemData;
        _index = index;
        _clickAction = clickAction;
        emblemIcon.SetData(_data);
        Refresh();
    }

    private void Refresh()
    {
        if (_data == null)
        {
            Debug.LogError("data null!!");
            return;
        }
        
        goSelect.SetActive(isSelect);
        goLock.SetActive(_data.isLock);
        goUnLock.SetActive(_data.isLock == false);
        emblemIcon.Refresh();
    }
    
    public void Click()
    {
        _clickAction?.Invoke(this);
    }

    
}

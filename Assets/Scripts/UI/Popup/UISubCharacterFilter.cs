using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UISubCharacterFilter : UISubBase
{
    UICharacterInventory _tap;

    [SerializeField] FactionFilterButton[] _factionBtns;

    public EFactionType factionType;
    public EGradeType gradeType;

    public void SetTap(UICharacterInventory tap)
    {
        _tap = tap;
        factionType = tap._filterFactionType;

        Referesh();
    }

    void Referesh()
    {
        foreach (FactionFilterButton btn in _factionBtns)
        {
            btn.Refresh();
        }
    }

    public void SetFilterType(EFactionType factionType)
    {
        this.factionType = factionType;
        _tap._filterFactionType = this.factionType;
        _tap.DrawWaitingCharacterItem();
        Referesh();

    }


    public override void ClickCloseBtn()
    {
        _tap.uISubCharacterFilter = null;
        _tap.DrawWaitingCharacterItem();

        base.ClickCloseBtn();
    }
}

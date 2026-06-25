using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

namespace Only1Games.UI
{
    public class UIDropDownMenuItem : MonoBehaviour
    {
        UIDropDownMenu popupMenu = null;
        public int index = 0;

        public TMP_Text txtInfo = null;
        public GameObject goSelect = null;
        public I2.Loc.Localize localize;

        /*
         * *
         */

        public void InitInstance(UIDropDownMenu _popupMenu, int _index)
        {
            popupMenu = _popupMenu;
            index = _index;
        }

        public void SetSelect(bool flag)
        {
            if(goSelect != null) goSelect.SetActive(flag);
        }

        public void OnClick()
        {
            popupMenu.Set(this);

        }
    }
}
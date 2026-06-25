using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Only1Games.UI
{
    public class UIDropDownMenu : MonoBehaviour
    {
        public UIDropDownMenuItem[] items = null;
        public UIDropDownMenuItem currentMenuItem = null;
        public GameObject goPopup = null;
        public TMP_Text txtInfo = null;

        public int currentIndex = -1;
        public int CurrentIndex { get { return currentIndex; } }

        public UnityEvent changeEvent = null;

        bool isHold = false;
        
        /*
         * *
         */

        void Awake()
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].InitInstance(this, i);
            }
        }

        void OnEnable()
        {
            isHold = false;

            Localize();
        }

        void Localize()
        {
            for (int i = 0; i < items.Length; i++)
            {

                if (currentIndex == items[i].index)
                {
                    txtInfo.text = items[i].txtInfo.text;
                    if (items[i].localize != null)
                        txtInfo.text = I2.Loc.LocalizationManager.GetTranslation(items[i].localize.mTerm);
                    break;
                }
            }
        }
        public void Refresh() { }

        public void Set(UIDropDownMenuItem item, bool closePopup = true)
        {
            currentIndex = -1;
            if (closePopup == true)
                goPopup.SetActive(false);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Equals(item) == false)
                {
                    items[i].SetSelect(false);
                    continue;
                }

                if (currentIndex == items[i].index)
                    continue;

                items[i].SetSelect(true);

                txtInfo.text = item.txtInfo.text;
                currentIndex = items[i].index;

                changeEvent.Invoke();
            }
        }

        public void Set(int _index, bool closePopup = true, bool needEvent = true)
        {
            currentIndex = -1;
            if (closePopup == true)
                goPopup.SetActive(false);
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].index != _index)
                {
                    items[i].SetSelect(false);
                    continue;
                }

                if (currentIndex == items[i].index)
                    continue;

                items[i].SetSelect(true);

                currentIndex = items[i].index;
                txtInfo.text = items[i].txtInfo.text;
                if (needEvent == true)
                    changeEvent.Invoke();
            }
            Localize();
        }
        public void SetItemText(int index,string txt)
        {
            items[index].txtInfo.text = txt;
        }

        public void SetHold(bool flag)
        {
            isHold = flag;
        }
        public void OnClickOpen()
        {
            if (isHold == true) return;
            goPopup.SetActive(!goPopup.activeInHierarchy);
            Set(currentIndex, false, false);
        }
        public void OnClickClose()
        {
            goPopup.SetActive(false);
            Set(currentIndex, false, false);
        }
    }
}
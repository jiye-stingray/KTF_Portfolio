// using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ConstellationNodeBar : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] GameObject _gray;

    [SerializeField] int sizeH = 20;
    [SerializeField] RectTransform rt;
    [SerializeField] Transform positionA;
    [SerializeField] Transform positionB;


    public void Refresh()
    {
        {
            if (positionA != null)
            {
                Vector2 v1 = new Vector2(positionA.localPosition.x, positionA.localPosition.y);
                Vector2 v2 = new Vector2(positionB.localPosition.x, positionB.localPosition.y);
                UILineUtil.Apply(rt, v1, v2, sizeH);
            }

            _gray.SetActive(!Managers.Instance.UserInfo().GetConstellationItemData(id)._isOpen);
        }
    }
}

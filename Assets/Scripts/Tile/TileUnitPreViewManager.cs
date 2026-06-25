// using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
public class TileUnitPreViewManager : MonoBehaviour
{
    List<TileUnit> unitList = new List<TileUnit>();

    /*
     * *
     */

    List<Transform> gameObjects = new List<Transform>();
    // [Button("ClearTileUnit",ButtonSizes.Large)]
    public void ClearTileUnit()
    {
        unitList = GetComponentsInChildren<TileUnit>().ToList();
        foreach (var unit in unitList)
        {
            var childCount = unit.transform.childCount;
            unit.isFixedPreview = false;
            for (int i = childCount - 1; i >= 0; i--) // 역순 삭제
            {
                Transform child = unit.transform.GetChild(i);

                // TileUnit 컴포넌트가 없다면 삭제 대상
                if (child.GetComponent<TileUnit>() == null)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
#endif

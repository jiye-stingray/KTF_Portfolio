// 3D Obbject Trigger
// using UnityEngine;
// using UnityEngine.EventSystems;
//
// public class ObjectTooltipTrigger : MonoBehaviour
// {
//     [Header("Tooltip Content")]
//     [TextArea]
//     public string text;
//     public Sprite tooltipIcon;
//
//     [Header("Position Settings")]
//     public bool useTouchPosition = true; // 모바일 기본
//     public Vector3 offset;
//
//     public float tooltipDuration = 0f;
//
//     private bool isTooltipVisible = false;
//
//     private void Update()
//     {
//         // PC
//         #region PC
//
// #if UNITY_STANDALONE || UNITY_WEBGL
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//         RaycastHit hit;
//         if (Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>())
//         {
//             if (!isTooltipVisible)
//             {
//                 ShowTooltip(Input.mousePosition);
//             }
//         }
//         else
//         {
//             if (isTooltipVisible)
//             {
//                 HideTooltip();
//             }
//         }
// #endif
//
//         #endregion
//
//         // Mobile
//         #region Mobile
//
// #if UNITY_IOS || UNITY_ANDROID
//         if (Input.touchCount > 0 && !isTooltipVisible)
//         {
//             Touch touch = Input.GetTouch(0);
//             Ray ray = Camera.main.ScreenPointToRay(touch.position);
//             if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
//             {
//                 if (touch.phase == TouchPhase.Began)
//                 {
//                     ShowTooltip(touch.position);
//                 }
//             }
//         }
//
//         if (Input.touchCount == 0 && isTooltipVisible)
//         {
//             HideTooltip();
//         }
// #endif
//
//         #endregion
//         
//
//     }
//
//     private void ShowTooltip(Vector3 screenPos)
//     {
//         TooltipView.Instance.ShowTooltip(text, screenPos + offset, tooltipIcon, tooltipDuration);
//         isTooltipVisible = true;
//     }
//
//     private void HideTooltip()
//     {
//         TooltipView.Instance.HideTooltip();
//         isTooltipVisible = false;
//     }
// }
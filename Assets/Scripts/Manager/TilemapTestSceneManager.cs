using UnityEngine;

public class TilemapTestSceneManager : MonoBehaviour
{
   [Header("타일 맵")]
   [SerializeField] GameObject _tileMapPrefab;
   
   [SerializeField] TestSquad _squad;
   [SerializeField] Transform _tileMapParent;
   [SerializeField] UIJoystick _joystick;
   [SerializeField] Camera _uiCamera;
   [SerializeField] FollowCamera _followCamera;
   
   private JoystickController _joystickController = new JoystickController();
   private int _zoneIndex;
   private void Start()
   {
       if (_tileMapPrefab == null)
       {
           Debug.LogError("Tilemap Prefab is null");
           return;
       }
       
       MapStageMeta mapStageMeta = CreateMap().GetComponent<MapStageMeta>();
       _squad.Init(_joystickController);
       _squad.transform.position = mapStageMeta.MapStageInfos[0].StartPosition;
       
       _followCamera.SetTarget(_squad.transform);
       _joystickController.joystickType = Define.EJoystickType.Fixed;
       _joystick.SetJoystick(_joystickController, _uiCamera);
       _joystick.Init();
   }
   
   private GameObject CreateMap()
   {
       GameObject map = Instantiate(_tileMapPrefab, _tileMapParent);
       map.transform.localPosition = Vector3.zero;

       return map;
   }
}

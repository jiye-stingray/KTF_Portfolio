using System.Linq;
using static Define;

public class FieldDungeonBuilding : InstallationBuilding
{
    public override void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null) DestroyImmediate(_SpeechBox.gameObject);

        if (!BuildingData.isOpen)
        {
            UIFieldDungeonSpeechBox fieldDungeonSpeechBox = Managers.Instance.GetUIManager().ShowUIBase<UIFieldDungeonSpeechBox>("UIFieldDungeonSpeechBox", UIManager.SpeechCanvas);
            fieldDungeonSpeechBox.SetData(BuildingData._data);         
            _SpeechBox = fieldDungeonSpeechBox;
            _SpeechBox.InitData(idx, this);
            _SpeechBox.Close();
        }
    }
    
    public override void CreateBuildingObject()
    {
        if (this == null || gameObject == null)
            return;

        if (_spineAnimation == null)
            return;

        if (BuildingData._isOpening)
        {
            _buildingState = EBuildingState.Idle;
            _spineAnimation.SetAnimation("action", false);
            _spineAnimation.AnimationStop();
        }
        else if (BuildingData.isOpen)
            gameObject.SetActive(false);
        else
        {
            _buildingState = EBuildingState.Idle;
            _spineAnimation.SetAnimation(ObjectAnimationName.IDLE, true);
            ChangeSpeechBox(_idx);
        }
    }
}
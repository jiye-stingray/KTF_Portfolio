using System.Linq;
using static Define;

public class FogDungeonBuilding : InstallationBuilding
{
    public override void ChangeSpeechBox(int idx)
    {
        if (_SpeechBox != null) DestroyImmediate(_SpeechBox.gameObject);

        if (!BuildingData.isOpen)
        {
            _SpeechBox = Managers.Instance.GetUIManager()
                .ShowUIBase<UISpeechBox>("UIFogDungeonSpeechBox", UIManager.SpeechCanvas);
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
        
        _buildingState = EBuildingState.Idle;
        _spineAnimation.SetAnimation(ObjectAnimationName.IDLE, true);
        
        if (BuildingData.isOpen)
            gameObject.SetActive(false);
        else
            ChangeSpeechBox(_idx);
    }
}
using System.Linq;
using UniRx;

public class FieldItemData : ItemData
{
    public int ID;
    public bool isOpen;         // 필드가 해금 되었는지
    public int progress;        // 달성도
    public int difficultyLevel = 1;     // 최대 난이도 (어디까지 해금 되었는지 1 ~  10) 
    public int currentDifficultyLevel = 1;      // 맵 이동시 초기화 (mainfield 제외)
    public int MaxProgress;
    public bool[] isGet;        // 달성도 보상 받았는지 갯수 배열
    public bool[] isFirstClearRewardGet = new bool[10];        // 최초 보상 받았는지 난이도 별로

    public bool isQuestClearable; // 가이드 퀘스트 보상을 받을수 있는 상태
    public bool IsFirstFieldID => ID == GetFirstFieldId();

    public ReactiveProperty<int> guideQuestId = new ReactiveProperty<int>(); // 현재 가이드 퀘스트 id
    public bool isQuestFinish = false;
    public int questProgressValue;
    public int dungeonQuestClearCount;

    public bool IsGuideQuestFinish()
    {
        if (IsFirstFieldID)
            return isQuestFinish;
        else
            return GetCurrentQuest() == null;
    }
    
    public GuideQuest GetCurrentQuest()
    {
        if (IsFirstFieldID)
            return ClientLocalDB_Simple.GetData<GuideQuest>(DBKey.GuideQuest, guideQuestId.Value);
        else
            return ClientLocalDB_Simple.GetData<GuideQuest>(DBKey.DungeonQuest, guideQuestId.Value);
        
    }

    public void RefreshCurrentQuestId()
    {
        if (!IsFirstFieldID)
        {
            FieldInfo fieldInfo = ClientLocalDB_Simple.GetData<FieldInfo>(DBKey.FieldInfo, ID);
            guideQuestId.Value = fieldInfo.StartQuestValue + dungeonQuestClearCount;
        }
    }

    public static int GetFirstFieldId()
    {
        return int.Parse(ClientLocalDB_Simple.GetDB<FieldInfo>(DBKey.FieldInfo).First().Key);
    }
}

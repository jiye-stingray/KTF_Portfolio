using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class RewardTooltip : MonoBehaviour
{
    List<RewardItem> itemList = new List<RewardItem>();

    public void InitQuestReward(int rewardId)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < itemList.Count; i++)
        {
            Destroy(itemList[i].gameObject);
        }
        itemList.Clear();

        QuestReward questReward = ClientLocalDB_Simple.GetData<QuestReward>(DBKey.QuestReward, rewardId);

        for (int i = 0; i < questReward.RewardType.Length; i++)
        {
            RewardItem item = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/UI/Common/RewardItem", gameObject.transform).GetComponent<RewardItem>();

            item.Init(questReward.RewardType[i], questReward.RewardID[i], questReward.RewardValue[i]);

            itemList.Add(item);
        }
    }
}

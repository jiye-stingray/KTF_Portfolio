using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    [System.Serializable]
    public class PlayerInfoData
    {
        public Define.ESOCIAL_TYPE socialType = Define.ESOCIAL_TYPE.Guest;
        
    }
    public PlayerInfoData data;
    

    // public UserInfo userInfo = null; // 플레이어의 유저 정보 

}

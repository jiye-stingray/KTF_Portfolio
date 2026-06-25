using Cysharp.Threading.Tasks;
using MarchingBytes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class ObjectUnitManager
{
    Transform gatherResTransform = null;
    public Transform GatherResTransform
    {
        get
        {
            if (gatherResTransform == null)
            {
                gatherResTransform = GameObject.Find("GatherResTransform").transform;
            }
            return gatherResTransform;
        }
    }
    Transform monstersTrans = null;
    public Transform MonsterTrans
    {
        get
        {
            if (monstersTrans == null)
            {
                monstersTrans = GameObject.Find("MonsterTransform").transform;
            }
            return monstersTrans;
        }
    }
    
    Transform playersTrans = null;
    public Transform PlayersTrans
    {
        get
        {
            if (playersTrans == null)
            {
                playersTrans = GameObject.Find("PlayerTransform").transform;
            }
            return playersTrans;
        }
    }

    Transform buildingTrans = null;
    public Transform BuildingTrans
    {
        get
        {
            if (buildingTrans == null)
            {
                buildingTrans = GameObject.Find("BuildingTransform").transform;
            }
            return buildingTrans;
        }
    }

    Transform mapTrans = null;
    public Transform MapTrans
    {
        get
        {
            if (mapTrans == null)
            {
                mapTrans = GameObject.Find("MapTransform").transform;
            }
            return mapTrans;
        }
    }

    public HashSet<FieldObject> hashFieldObject = new HashSet<FieldObject>();
    public HashSet<BaseBuilding> hashBuilding = new HashSet<BaseBuilding>();
    public HashSet<SpawnPointInfoUnit> hashCircleUnit = new HashSet<SpawnPointInfoUnit>();
    public HashSet<SpellIndicator> hashSpellIndicator = new HashSet<SpellIndicator>();
    
    public Squad playerSquad = null;
    
    public T SpawnUnit<T>(Vector3 pos, string unitType) where T : BaseUnit
    {
        System.Type type = typeof(T);
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseUnit");

        if (go == null)
            return null;

        go.name = $"{OBJECT_KEY_UNIT}_{unitType}";
        go.transform.position = pos;

        // Player Unit
        if (type == typeof(PlayerUnit))
        {
            PlayerUnit tPlayer = go.GetOrAddComponent<PlayerUnit>();
            tPlayer.gameObject.layer = LayerMask.NameToLayer("Player");
            hashFieldObject.Add(tPlayer);
            return tPlayer as T;
        }

        // Enemy Unit
        if (type == typeof(EnemyUnit) ||
            type == typeof(BossUnit) ||
            type == typeof(GuildBossUnit) ||
            type == typeof(RankingBossUnit))
        {
            EnemyUnit tMonster = null;
            if (type == typeof(EnemyUnit))
                tMonster = go.GetOrAddComponent<EnemyUnit>();
            else if (type == typeof(BossUnit))
                tMonster = go.GetOrAddComponent<BossUnit>();
            else if (type == typeof(GuildBossUnit))
                tMonster = go.GetOrAddComponent<GuildBossUnit>();
            else if (type == typeof(RankingBossUnit))
                tMonster = go.GetOrAddComponent<RankingBossUnit>();

            if (tMonster == null)
            {
                MyLogger.Log($"SpawnUnit: tMonster is null for type {type}");
                Managers.Instance.GetResObjectManager().Destroy(go);
                return null;
            }

            go.AddComponent<SpineVisibilityManager>();
            tMonster.gameObject.layer = LayerMask.NameToLayer("Enemy");
            hashFieldObject.Add(tMonster);
            return tMonster as T;
        }

        // Gather Unit
        if (type == typeof(GrTree))
        {
            GrTree tGather = go.GetOrAddComponent<GrTree>();
            go.AddComponent<SpineVisibilityManager>();
            tGather.gameObject.layer = LayerMask.NameToLayer("Gather");
            hashFieldObject.Add(tGather);
            return tGather as T;
        }

        // 지원하지 않는 타입 — GO 누수 방지
        MyLogger.Log($"SpawnUnit: Unsupported type {type}");
        Managers.Instance.GetResObjectManager().Destroy(go);
        return null;
    }

    public async UniTask SpawnCharacter(UserInfoData userInfoData, Squad squad, int id, ETeamType teamType, bool enableResurrection)
    {
        MapManager mapManager = Managers.Instance.GetMapManager();
        UnitData unitData = ClientLocalDB_Simple.GetData<UnitData>(DBKey.PlayerCharacter, id);
        PlayerUnit playerUnit = Managers.Instance.GetObjectUnitManager().SpawnUnit<PlayerUnit>(new Vector3(0, 0, 0), $"Mercenar");
        playerUnit.transform.parent = PlayersTrans;
        playerUnit.transform.position = playerSquad.GetUnitPosition();
        playerUnit.SetPosition(playerUnit.transform.position);
        await playerUnit.SetUnitId(userInfoData, unitData.ID);
        playerUnit._squad = squad;
        playerUnit._teamType = teamType;
        playerUnit.Init();
        playerUnit._enableResurrection = enableResurrection;
        playerUnit.SetZoneIndex(squad._zoneIndex);
        if (mapManager.IsTownZone(squad._zoneIndex))
            playerUnit.StartAiMode();
        playerUnit.SetSynergyStatus(squad.GetActiveSynergyStatus(playerUnit._unitDataTable.Faction));
        squad.AddCharacter(playerUnit);
    }

    public Squad SpawnSquad()
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate("Prefabs/Squad/Squad");
        go.name = OBJECT_KEY_SQUAD;

        Squad squad = go.GetComponent<Squad>();
        return squad;
    }

    public async UniTask<T> SpawnProjectile<T>(string resource, Vector3 pos) where T : Component
    {
        GameObject obj = await EasyObjectPool.instance.GetObjectFromPool(EPoolType.Projectile, resource, pos, true);
        T tProjectile = obj.GetOrAddComponent<T>();
        return tProjectile;
    }

    public async UniTask<SentenceEffect> SpawnSentenceEffect(EPoolType type, string name)
    {
        GameObject obj = await EasyObjectPool.instance.GetObjectFromPool(type, name, Vector3.zero, true);
        SentenceEffect effect = obj.GetOrAddComponent<SentenceEffect>();
        return effect;
    }

    public async UniTask<T> SpawnGroundEffect<T>(string resource, Vector3 pos) where T : Component
    {
        GameObject obj = await EasyObjectPool.instance.GetObjectFromPool(EPoolType.GroundEffect, resource, pos, true);
        T groundEffect = obj.GetOrAddComponent<T>();
        return groundEffect;
    }
    
    public async UniTask<BaseAttackEffectObject> SpawnAttackEffect(string resource, Vector3 pos)
    {
        GameObject obj = await EasyObjectPool.instance.GetObjectFromPool(EPoolType.CallEffect, resource, pos, true);
        BaseAttackEffectObject attackEffect = obj.GetOrAddComponent<BaseAttackEffectObject>();
        return attackEffect;
    }

    public T SpawnSpellIndicatorObject<T>(Vector3 pos, string name) where T : SpellIndicator
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/Indicator/{name}");
        go.name = name;
        go.transform.position = pos;

        T spellIndicator = go.GetOrAddComponent<T>();
        spellIndicator.Init();
        hashSpellIndicator.Add(spellIndicator);

        return spellIndicator;
    }

    public async UniTask<BaseBuilding> SpawnBuildingUnit(int id, Vector3 pos, string name)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        InstallationBuilding tBuilding = go.AddComponent<InstallationBuilding>();
        hashBuilding.Add(tBuilding);
        
        await tBuilding.Init(id);
        return tBuilding; 
    }

    public async UniTask<StorageBuilding> SpawnStorageBuildingUnit(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        StorageBuilding tBuilding = go.AddComponent<StorageBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }

    public T SpawnCircleUnit<T>(Vector3 pos, string name) where T : SpawnPointInfoUnit
    {
        System.Type type = typeof(T);
        
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/CircleObject/{name}");
        go.name = name;
        go.transform.position = pos;

        SpawnPointInfoUnit circleUnit = null;
        if(type == typeof(SpawnPointInfoUnit))
            circleUnit = go.AddComponent<SpawnPointInfoUnit>();
        else if(type == typeof(SpawnPointGrowthUnit))
            circleUnit = go.AddComponent<SpawnPointGrowthUnit>();
        
        hashCircleUnit.Add(circleUnit);
        return circleUnit as T;
    }

    public async UniTask<PortalBuilding> SpawnPortal(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        PortalBuilding tBuilding = go.AddComponent<PortalBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }

    public async UniTask<TreasureBoxBuilding> SpawnTreasureBox(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        TreasureBoxBuilding tBuilding = go.AddComponent<TreasureBoxBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }
    
    public async UniTask<RewardBoxBuilding> SpawnRewardBox(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        RewardBoxBuilding tBuilding = go.AddComponent<RewardBoxBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }

    public async UniTask<FogPointBuilding> SpawnFogPoint(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        FogPointBuilding tBuilding = go.AddComponent<FogPointBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }
    
    public async UniTask<FogDungeonBuilding> SpawnFogDungeonBuilding(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        FogDungeonBuilding tBuilding = go.AddComponent<FogDungeonBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }
    
    public async UniTask<FieldDungeonBuilding> SpawnFieldDungeonBuilding(int id, string name, Vector3 pos)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        FieldDungeonBuilding tBuilding = go.AddComponent<FieldDungeonBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }

    public async UniTask<FieldQuestBuilding> SpawnFieldQuestObject(int id, string name, Vector3 pos, bool flip = false)
    {
        GameObject go = Managers.Instance.GetResObjectManager().Instantiate($"Prefabs/SpawnObject/FieldObject/BaseBuilding");
        go.name = $"{OBJECT_KEY_BUILDING}_{name}";
        go.transform.position = pos;

        FieldQuestBuilding tBuilding = go.AddComponent<FieldQuestBuilding>();
        hashBuilding.Add(tBuilding);

        await tBuilding.Init(id);
        return tBuilding;
    }
    
    public async UniTask SpawnFieldDropItem(int id, Vector2 pos)
    {
        GameObject dropObject = await EasyObjectPool.instance.GetObjectFromPool(EPoolType.FieldDrop, "FieldDropItem", pos);
        FieldDropItem fieldDropItem = dropObject.GetComponent<FieldDropItem>();

        fieldDropItem.Init(id);
    }

    public async UniTask ShowDamageText(Vector2 pos, double damage, string damageTextName)
    {
        if (!Managers.Instance.UserInfo()._isDamageOn) return;
        
        // pool manager
        GameObject textObject = await EasyObjectPool.instance.GetObjectFromPool(EPoolType.DamageText, damageTextName, pos);
        UIDamageText damageText = textObject.GetComponent<UIDamageText>();
        damageText.Init(damage);
    }
    
    public void DestroySquad()
    {
        if (playerSquad == null)
            return;
        
        playerSquad.Remove();
        Managers.Instance.GetResObjectManager().Destroy(playerSquad.gameObject);
        playerSquad = null;
    }
    
    public void DestroyUnit(FieldObject fieldObject)
    {
        hashFieldObject.Remove(fieldObject);
        Managers.Instance.GetResObjectManager().Destroy(fieldObject.gameObject);
    }
    
    public void DestroyBuilding(BaseBuilding building)
    {
        hashBuilding.Remove(building);
        Managers.Instance.GetResObjectManager().Destroy(building.gameObject);
    }

    public void DestroySpellIndicator(SpellIndicator spellIndicator)
    {
        hashSpellIndicator.Remove(spellIndicator);
        Managers.Instance.GetResObjectManager().Destroy(spellIndicator.gameObject);
    }

    public void DestroyAll()
    {
        EasyObjectPool.instance.RemoveAllPoolInfo();
        for (int i = hashFieldObject.Count - 1; i >= 0; i--)
        {
            FieldObject fieldObject = hashFieldObject.ElementAt(i);
            
            if (fieldObject == null)
                continue;
            
            hashFieldObject.Remove(fieldObject);
            Managers.Instance.GetResObjectManager().Destroy(fieldObject.gameObject);
        }
        hashFieldObject.Clear();

        for (int i = hashBuilding.Count - 1; i >= 0; i--)
        {
            BaseBuilding building = hashBuilding.ElementAt(i);
            
            if (building == null)
                continue;
            
            hashBuilding.Remove(building);
            Managers.Instance.GetResObjectManager().Destroy(building.gameObject);
        }
        hashBuilding.Clear();

        for (int i = hashCircleUnit.Count - 1; i >= 0; i--)
        {
            SpawnPointInfoUnit spawnPointInfoUnit = hashCircleUnit.ElementAt(i);
            
            if (spawnPointInfoUnit == null)
                continue;
            
            hashCircleUnit.Remove(spawnPointInfoUnit);
            Managers.Instance.GetResObjectManager().Destroy(spawnPointInfoUnit.gameObject);
        }
        hashCircleUnit.Clear();

        for (int i = hashSpellIndicator.Count - 1; i >= 0; i--)
        {
            SpellIndicator spellIndicator = hashSpellIndicator.ElementAt(i);
            
            if (spellIndicator == null)
                continue;
            
            hashSpellIndicator.Remove(spellIndicator);
            Managers.Instance.GetResObjectManager().Destroy(spellIndicator.gameObject);
        }
        hashSpellIndicator.Clear();
    }
}
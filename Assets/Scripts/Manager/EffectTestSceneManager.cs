using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using Event = Spine.Event;

public enum ETestEffectPositionType
{
    Left,
    Right,
    FirePoint
}

public enum ETestCharacterAnimationType
{
    Player,
    Enemy
}

public enum EMonsterSkillType
{
    skill1,
    skill2,
    skill3
}

public class EffectTestSceneManager : MonoBehaviour
{
   [Header("캐릭터 스파인")]
   [SerializeField] ETestCharacterAnimationType _characterAnimationType = ETestCharacterAnimationType.Player;
   [SerializeField] SkeletonDataAsset _leftSkeletonAsset;
   [SerializeField] SkeletonDataAsset _rightSkeletonAsset;
   
   [Header("기본 공격")]
   [SerializeField] GameObject _startDefaultEffectPrefab;
   [SerializeField] GameObject _callDefaultEffectPrefab;
   
   [Header("스킬")]
   [SerializeField] private EMonsterSkillType _skillAnimationName = EMonsterSkillType.skill1;
   [SerializeField] GameObject _startActiveEffectPrefab;
   [SerializeField] GameObject _callActiveEffectPrefab;
   [SerializeField] GameObject _hitEffectPrefab;
   
   [Header("Projectile")]
   [SerializeField] private ETestEffectPositionType _callEffectPosition = ETestEffectPositionType.Left;
   [SerializeField] private bool _isProjectile;
   [SerializeField] private float _projectileTime = 2f;
   [Range(0, 4)]
   [SerializeField] private int _parabolaType;
   
   [SerializeField] SpineAnimation leftSpine;
   [SerializeField] SpineAnimation rightSpine;
   [SerializeField] Color _spineColor;
   [SerializeField] Color _hitColor;
   [SerializeField] Color _damageColor;
   public bool _enableDamageFont;
   private List<GameObject> _effectList = new List<GameObject>();
   
   public Transform _effectRoot;
   private Vector2 _leftPosition => (Vector2)leftSpine.transform.position + new Vector2(0, 0.5f);
   private Vector2 _rightPosition => (Vector2)rightSpine.transform.position + new Vector2(0, 0.5f);
   private Vector2 _firePosition;
   private ESkillSlotType _skillType = ESkillSlotType.DefaultSkill;
   private HurtFlashEffect _flashEffect;
   private void Start()
   {
       leftSpine.SetSpine(_leftSkeletonAsset);
       rightSpine.SetSpine(_rightSkeletonAsset);
       _flashEffect = Utils.GetOrAddComponent<HurtFlashEffect>(rightSpine.gameObject);
       string animationName = _characterAnimationType == ETestCharacterAnimationType.Player
           ? CharacterAnimationName.IDLE
           : ObjectAnimationName.IDLE;
       leftSpine.SetAnimation(animationName, true);
       
       SetFirePoint();
       leftSpine.SkeletonAnimation.state.Event += HandleSpineEvent;
   }

   private void SetFirePoint()
   {
       Bone bone = leftSpine.FindBone("FirePoint");
       
       if (bone == null)
           _firePosition = _leftPosition;
       else
           _firePosition= bone.GetWorldPosition(leftSpine.transform);
   }

   private void HandleSpineEvent(TrackEntry trackEntry, Event e)
   {
       if (e.Data.Name == "callEffect")
           CreateCallEffect();
   }
   
   private void CreateStartEffect()
   {
       GameObject effectPrefab = _skillType == ESkillSlotType.DefaultSkill ? _startDefaultEffectPrefab : _startActiveEffectPrefab;
       if (effectPrefab == null)
       {
           MyLogger.Log("이펙트가 연결되어있지 않습니다.");
           return;
       }

       CreateEffect(effectPrefab, leftSpine.transform.position);
   }
   
   private void CreateCallEffect()
   {
       GameObject effectPrefab = _skillType == ESkillSlotType.DefaultSkill ? _callDefaultEffectPrefab : _callActiveEffectPrefab;
       if (effectPrefab == null)
       {
           MyLogger.Log("이펙트가 연결되어있지 않습니다.");
           return;
       }
       
       Vector2 position = Vector2.zero;
       switch (_callEffectPosition)
       {
           case ETestEffectPositionType.Left:
               position = leftSpine.transform.position;
               break;
           case ETestEffectPositionType.Right:
               position = rightSpine.transform.position;
               break;
           case ETestEffectPositionType.FirePoint:
               position = _firePosition;
               break;
       }
       
       if(!_isProjectile)
           CreateEffect(effectPrefab, position);
       else
           CreateProjectile(effectPrefab, position);
   }

   private void CreateProjectile(GameObject effectPrefab, Vector2 position)
   {
       GameObject effect = CreateEffect(effectPrefab, position);
       if(_parabolaType == 0)
            effect.transform.DOMove(new Vector3(10, effect.transform.position.y, 0), _projectileTime);
       else
       {
           EffectTestStraightArrow straightArrow = effect.GetOrAddComponent<EffectTestStraightArrow>();
           straightArrow.Init(_rightPosition, _parabolaType, _projectileTime);
       }
   }
   
   private void CreateHitEffect()
   {
       if(_enableDamageFont)
            CreateDamageText();
       _flashEffect.flashColor = _hitColor;
       _flashEffect.Flash();
       if (_hitEffectPrefab == null)
       {
           MyLogger.Log("이펙트가 연결되어있지 않습니다.");
           return;
       }
       
       CreateEffect(_hitEffectPrefab, _rightPosition);
   }

   private void CreateDamageText()
   {
       GameObject damageObject = Resources.Load<GameObject>("Prefabs/UI/Common/DamageText/TestDamageText");
       UIDamageText damageText = Instantiate(damageObject, _effectRoot).GetComponent<UIDamageText>();
       
       damageText.transform.position = _rightPosition;
       damageText.Init(1250, ColorUtility.ToHtmlStringRGBA(_damageColor));
       _effectList.Add(damageText.gameObject);
   }

   private GameObject CreateEffect(GameObject effectPrefab, Vector2 position)
   {
       GameObject effect = Instantiate(effectPrefab, _effectRoot);
       effect.transform.position = position;
       
       _effectList.Add(effect);
       return effect;
   }
   
   private GameObject CreateProjectileEffect(GameObject effectPrefab, Vector2 position)
   {
       GameObject effect = Instantiate(effectPrefab, _effectRoot);
       effect.transform.position = position;
       
       
       _effectList.Add(effect);
       return effect;
   }


   public void AttackButtonClick()
   {
       _skillType = ESkillSlotType.DefaultSkill;
       string animationName = _characterAnimationType == ETestCharacterAnimationType.Player
           ? CharacterAnimationName.ATTACK
           : ObjectAnimationName.ATTACK;
       leftSpine.SetAnimation(animationName, false);
       CreateStartEffect();
   }

    public void SkillButtonClick()
    {
        _skillType = ESkillSlotType.ActiveSkill;

        string monsterSkillName = _skillAnimationName.ToString();

        // 플레이어인지 몬스터인지 확인 후 이름 결정.
        string animationName = _characterAnimationType == ETestCharacterAnimationType.Player
            ? CharacterAnimationName.ACTIVE_SKILL
            : monsterSkillName;
        leftSpine.SetAnimation(animationName, false);
        CreateStartEffect();
    }

    public void HitButtonClick()
   {
       CreateHitEffect();
   }

   public void FrozenClicked()
   {
       MaterialPropertyBlock mpb = new MaterialPropertyBlock();
       MeshRenderer meshRenderer = leftSpine.GetComponent<MeshRenderer>();
       meshRenderer.GetPropertyBlock(mpb);

       int fillPhase = Shader.PropertyToID("_FillPhase");
       int fillColor = Shader.PropertyToID("_FillColor");
       
       mpb.SetColor(fillColor, _spineColor);
       mpb.SetFloat(fillPhase, 1f);
       meshRenderer.SetPropertyBlock(mpb);
   }
   
   public void ResetClicked()
   {
       MaterialPropertyBlock mpb = new MaterialPropertyBlock();
       MeshRenderer meshRenderer = leftSpine.GetComponent<MeshRenderer>();
       meshRenderer.GetPropertyBlock(mpb);

       int fillPhase = Shader.PropertyToID("_FillPhase");
       
       mpb.SetFloat(fillPhase, 0f);
       meshRenderer.SetPropertyBlock(mpb);
   }

   public void RemoveEffectClick()
   {
       for (int i = _effectList.Count - 1; i >= 0; i--)
       {
           GameObject effect = _effectList[i];
           DestroyImmediate(effect);
       }
       
       _effectList.Clear();
   }
}

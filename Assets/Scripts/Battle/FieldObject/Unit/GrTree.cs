using UnityEngine;
using Animation = Spine.Animation;

public class GrTree : BaseGatherResUnit
{
    [SerializeField] Sprite sprite; // 추후 tyep 으로 sprite 설정하기

    public override void Init()
    {
        base.Init();
        Animation anim = _spineAnimation.FindAnimation("Spawn");
        if (anim != null)
        {
            _spineAnimation.SetAnimation("Spawn", false);
            _spineAnimation.AddAnimation(Define.ObjectAnimationName.IDLE, true);
        }
    }

    public override void OnDamage(double damage, string damageTextName, BaseUnit attacker)
    {
        // add code sound SFX
        if (GatherType == Define.EGatherType.Tree)
            Managers.Instance.Sound.PlaySFX("Effect", "SE_wood_chop");
        else if (GatherType == Define.EGatherType.Mine)
            Managers.Instance.Sound.PlaySFX("Effect", "SE_stone");

        Managers.Instance.GetObjectUnitManager().ShowDamageText(CenterPosition, damage, damageTextName);

        double hp = _playStatus.Hp - damage;
        if (hp > _playStatus.MaxHp)
            hp = _playStatus.MaxHp;
        else if (hp < 0)
            hp = 0;

        _playStatus.Hp = hp;

        SetAnimation(Define.EUnitState.Hit);

        if (_playStatus.Hp <= 0)
            OnDie(attacker);
    }
}
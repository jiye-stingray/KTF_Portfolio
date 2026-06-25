using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class FieldObject : MonoBehaviour
{
    [SerializeField] protected string _name;

    public CircleCollider2D _collider;
    public Rigidbody2D _rigidbody;
    protected float ColliderRadius => _collider.radius;
    public Vector2 CenterPosition => (Vector2)transform.position + new Vector2(0, _collider.offset.y);
    public SpineAnimation _spineAnimation;
    protected SortingGroup tSort = null;
    private bool leftDir = true;
    
    protected SyncCurrencyManager SyncCurrencyManager => Managers.Instance.GetSyncCurrencyManager();

    public bool LeftDir
    {
        get => leftDir;
        set
        {
            leftDir = value;
            _spineAnimation.Skeleton.ScaleX = value ? 1 : -1;
        }
    }

    public Vector2 _unitPos;

    public virtual void Awake()
    {
        _collider = this.gameObject.GetOrAddComponent<CircleCollider2D>();
        _collider.isTrigger = false;
        _rigidbody = this.gameObject.GetOrAddComponent<Rigidbody2D>();
        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        _rigidbody.sleepMode = RigidbodySleepMode2D.StartAwake;
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        _spineAnimation = this.gameObject.GetOrAddComponent<SpineAnimation>();

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을같이 계산함.
        tSort = Utils.GetOrAddComponent<SortingGroup>(gameObject);
        tSort.sortingOrder = Define.SortingLayers.UNIT;
    }
}
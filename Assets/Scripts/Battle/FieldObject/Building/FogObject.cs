using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class FogObject : MonoBehaviour
{
    public int _FogId;
    public Tilemap _tilemap;
    public List<SpawnPointInfoUnit> _triggers = new List<SpawnPointInfoUnit>();
    private SpriteRenderer[] _decoObject;
    private Collider2D _collider;

    private void Awake()
    {
        _decoObject = GetComponentsInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("SpawnPoint"))
        {
            SpawnPointInfoUnit spawnPointInfo = collision.gameObject.GetComponent<SpawnPointInfoUnit>();
            spawnPointInfo.EnterFog();
            _triggers.Add(spawnPointInfo);
        }
    }

    public IEnumerator FinishCoroutine()
    {
        DOTween.ToAlpha(() => _tilemap.color, x => _tilemap.color = x, 0, 1.0f);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < _decoObject.Length; i++)
        {
            SpriteRenderer sprite = _decoObject[i];
            Vector3 ranPosition = FindRandomPosition(sprite.transform.localPosition);
            DOTween.To(() => sprite.transform.localPosition, x => sprite.transform.localPosition = x, ranPosition,
                1.0f);
            DOTween.ToAlpha(() => sprite.color, x => sprite.color = x, 0, 1.0f);
        }

        yield return new WaitForSeconds(1.0f);
        ActiveFog(false);
    }

    public void ActiveFog(bool active)
    {
        if (active)
        {
            _collider.enabled = true;
            gameObject.SetActive(true);
        }
        else
        {
            _collider.enabled = false;
            _triggers.ForEach(spawnPointInfo =>
            {
                spawnPointInfo._isFogIn = false;
                spawnPointInfo.BattleStart();
            });
            _triggers.Clear();
            gameObject.SetActive(false);
        }
    }

    private Vector2 FindRandomPosition(Vector2 localPosition)
    {
        float ranX = Random.Range(-0.5f, 0.5f);
        float ranY = Random.Range(-0.5f, 0.5f);

        Vector2 position = new Vector2(ranX, ranY);
        position = position + localPosition;

        return position;
    }
}
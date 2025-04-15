using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Enemy : MonoBehaviour
{
    public float health = 10f;
    private Coroutine _flashRedCoroutine;
    private Dictionary<Renderer, Material> baseMaterials = new Dictionary<Renderer, Material>();
    public bool isBoss;
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            Destroy(gameObject);
            if (isBoss)
            {
                GameStateManager.Instance.GameOver(true);
            }
        }
        else
        {
            FlashRed();
        }
    }

    private void OnDisable()
    {
        if (isBoss)
        {
            StopCoroutine(_flashRedCoroutine);
            
            _flashRedCoroutine = null;
            
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            {
                rend.material = baseMaterials[rend];
            }
        }
    }

    public virtual void FlashRed()
    {
        if (_flashRedCoroutine == null)
        {
            _flashRedCoroutine=StartCoroutine(FlashRedCoroutine(GetComponentsInChildren<Renderer>(), 0.5f));
        }
    }

    private IEnumerator FlashRedCoroutine(Renderer[] renderers, float duration)
    {
        foreach (Renderer rend in renderers)
        {
            baseMaterials[rend] = rend.material;
            rend.material = PlayerEquipabbles.S.enemyHitMaterial;
        }
        yield return new WaitForSeconds(duration);
        foreach (Renderer rend in renderers)
        {
            rend.material = baseMaterials[rend];
        }
        baseMaterials.Clear();
        _flashRedCoroutine = null;
    }
}

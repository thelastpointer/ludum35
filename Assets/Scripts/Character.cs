﻿using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public GameObject GFX;
    public GameObject Selector;

    public GameObject[] Hearts;

    public bool IsDead = false;
    public int Health = 3;

    public int DelayedDamage = 0;

    public Ability SelectedAbility;
    public Character SelectedTarget;

    public int SkipTurn = 0;

    public void ChangeHealth(int add)
    {
        Health += add;

        if (Health > 3)
            Health = 3;

        for (int i = 0; i < Hearts.Length; ++i)
            Hearts[i].SetActive(i < Health);

        if (Health <= 0)
        {
            IsDead = true;
        }
    }

    public bool IsAttacker()
    {
        return ((SelectedAbility != null) && (
            (SelectedAbility.GetType() == typeof(Abilities.Attack)) ||
            (SelectedAbility.GetType() == typeof(Abilities.BigAttack)) ||
            (SelectedAbility.GetType() == typeof(Abilities.DelayedAttack)))
        );
    }

    public void SetState(GameObject go)
    {
        if (GFX != null)
            Destroy(GFX);

        GFX = Instantiate(go);
        GFX.transform.parent = transform;
        GFX.transform.localPosition = Vector3.zero;
        GFX.transform.localScale = Vector3.one * 0.5f;

        foreach (SpriteRenderer r in GFX.GetComponentsInChildren<SpriteRenderer>())
            r.sortingLayerName = "Character";

        Animator a = GFX.GetComponent<Animator>();
        if (a != null)
            a.SetTrigger("Shoot");
    }
}

using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public SpriteRenderer GFX;
    public GameObject Selector;

    public bool IsDead = false;
    public int Health = 3;

    public Ability SelectedAbility;
    public Character SelectedTarget;

    public bool SkipTurn = false;

    public void ChangeHealth(int add)
    {
        Health += add;

        if (Health <= 0)
        {
            IsDead = true;
        }
    }

    public bool IsAttacker()
    {
        return ((SelectedAbility != null) && (
            (SelectedAbility is Abilities.Attack) ||
            (SelectedAbility is Abilities.BigAttack) ||
            (SelectedAbility is Abilities.DelayedAttack))
        );
    }

    public void SetState(GameObject go)
    {
        //...
    }
}

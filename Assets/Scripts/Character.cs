using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public GameObject GFX;
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
    }
}

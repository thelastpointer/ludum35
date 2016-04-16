using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    public SpriteRenderer GFX;
    public GameObject Selector;

    public bool IsDead = false;
    public int Health = 3;

    public int SelectedAbility;
    public int SelectedTarget;
}

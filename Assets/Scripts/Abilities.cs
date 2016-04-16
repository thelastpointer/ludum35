
using System;
using UnityEngine;

public interface Ability
{
    Abilities.Target Target();
    void Resolve(Character user, Character target);
}

namespace Abilities
{
    public enum Target
    {
        None,
        Friend,
        Enemy
    }
    public class SkipTurn : Ability
    {
        public Target Target()
        {
            return Abilities.Target.None;
        }

        public void Resolve(Character user, Character target)
        {
        }
    }

    public class Attack : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Enemy;
        }

        public void Resolve(Character user, Character target)
        {
            if (!(target.SelectedAbility is Defend))
            {
                int chance = 50;
                if (UnityEngine.Random.Range(0, 100) <= chance)
                    target.ChangeHealth(-1);
                else
                {
                    // ...miss
                }
            }
        }
    }
    public class BigAttack : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Enemy;
        }

        public void Resolve(Character user, Character target)
        {
            if (!(target.SelectedAbility is Defend))
            {
                target.ChangeHealth(-1);
            }

            user.SkipTurn = true;
        }
    }
    public class Defend : Ability
    {
        public Target Target()
        {
            return Abilities.Target.None;
        }

        public void Resolve(Character user, Character target)
        {
        }
    }
    public class Disable : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Enemy;
        }

        public void Resolve(Character user, Character target)
        {
            target.SelectedAbility = new SkipTurn();
        }
    }
    public class SelfDestruct : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Enemy;
        }

        public void Resolve(Character user, Character target)
        {
            user.ChangeHealth(-100);

            if (target.SelectedAbility is Defend)
                target.ChangeHealth(-1);
            else
                target.ChangeHealth(-2);
        }
    }
    public class Heal : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Friend;
        }

        public void Resolve(Character user, Character target)
        {
            throw new NotImplementedException();
        }
    }

    public class DelayedAttack : Ability
    {
        public Target Target()
        {
            return Abilities.Target.Enemy;
        }

        public void Resolve(Character user, Character target)
        {
            throw new NotImplementedException();
        }
    }
}
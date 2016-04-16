using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Controls : MonoBehaviour
{
    public Team[] Teams;
    public GameObject WaitForPlayerPanel;
    public AbilitySelector[] AbilitySelectors;

    int currentTeam = 0;
    int currentCharacter = 0;
    int selectedTarget = 0;
    Ability[] abilityList;

    Team CurrentTeam { get { return Teams[currentTeam]; } }
    Team EnemyTeam { get { return currentTeam == 0 ? Teams[1] : Teams[0]; } }

    public enum State
    {
        None,
        WaitForPlayer,
        SelectAbility,
        SelectTarget,
        Resolve
    }
    State state = State.None;

    void Awake()
    {
        foreach (AbilitySelector sel in AbilitySelectors)
            sel.OnSelected.AddListener(OnSelectAbility);
    }
    
    void Start()
    {
        Setup();
        currentCharacter = 0;
        currentTeam = 0;
        SetState(State.WaitForPlayer);
    }

    void Update()
    {
        if (state == State.WaitForPlayer)
        {
            if (Input.anyKeyDown)
            {
                SetState(State.SelectAbility);
            }
        }
        else if (state == State.SelectTarget)
        {
            int oldSelected = selectedTarget;

            if (CurrentTeam.Chars[currentCharacter].SelectedAbility.Target() == Abilities.Target.Enemy)
            {
                // Down
                if (Input.GetAxisRaw("Vertical") < 0)
                {
                    ++selectedTarget;
                    if (selectedTarget >= EnemyTeam.Chars.Length)
                        selectedTarget = 0;
                }
                // Up
                else if (Input.GetAxisRaw("Vertical") > 0)
                {
                    --selectedTarget;
                    if (selectedTarget < 0)
                        selectedTarget = EnemyTeam.Chars.Length - 1;
                }

                // Selection changed
                if (oldSelected != selectedTarget)
                {
                    EnemyTeam.Chars[oldSelected].Selector.SetActive(false);
                    EnemyTeam.Chars[selectedTarget].Selector.SetActive(false);
                }

                // Target selected
                if (Input.GetButtonDown("Submit"))
                {
                    CurrentTeam.Chars[currentCharacter].SelectedTarget = EnemyTeam.Chars[selectedTarget];
                }
            }
            else if (CurrentTeam.Chars[currentCharacter].SelectedAbility.Target() == Abilities.Target.Friend)
            {
                // Down
                if (Input.GetAxisRaw("Vertical") < 0)
                {
                    ++selectedTarget;
                    if (selectedTarget >= CurrentTeam.Chars.Length)
                        selectedTarget = 0;
                }
                // Up
                else if (Input.GetAxisRaw("Vertical") > 0)
                {
                    --selectedTarget;
                    if (selectedTarget < 0)
                        selectedTarget = CurrentTeam.Chars.Length - 1;
                }

                // Selection changed
                if (oldSelected != selectedTarget)
                {
                    CurrentTeam.Chars[oldSelected].Selector.SetActive(false);
                    CurrentTeam.Chars[selectedTarget].Selector.SetActive(false);
                }

                // Target selected
                if (Input.GetButtonDown("Submit"))
                {
                    CurrentTeam.Chars[currentCharacter].SelectedTarget = CurrentTeam.Chars[selectedTarget];
                }
            }
        }
        else if (state == State.Resolve)
        {

        }
    }

    void SetState(State newState)
    {
        if (state == newState)
            return;

        switch (state)
        {
            case State.WaitForPlayer:
                WaitForPlayerPanel.SetActive(false);
                break;
            case State.SelectAbility:
                
                break;
            case State.SelectTarget:
                break;
            case State.Resolve:
                break;
            default:
                break;
        }

        switch (newState)
        {
            case State.WaitForPlayer:
                WaitForPlayerPanel.SetActive(true);
                WaitForPlayerPanel.GetComponentInChildren<Text>().text = string.Format("Player {0}, take your seat\nPress any key when ready", (currentTeam+1));
                break;
            case State.SelectAbility:

                CurrentTeam.Chars[currentCharacter].Selector.SetActive(true);
                
                // Activate ability selector
                AbilitySelectors[currentTeam].SetActivated(true);

                break;
            case State.SelectTarget:
                break;
            case State.Resolve:
                // Reset skip turns
                foreach (Team t in Teams)
                {
                    foreach (Character ch in t.Chars)
                    {
                        ch.SkipTurn = false;
                    }
                }

                StartCoroutine(ResolveTurn());
                break;
            default:
                break;
        }

        state = newState;
    }

    IEnumerator ResolveTurn()
    {
        // Note: ordering by a new guid means a randomly shuffled list

        // Defense chars
        IEnumerable<Character> defender1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility is Abilities.Defend));
        IEnumerable<Character> defender2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility is Abilities.Defend));
        IEnumerable<Character> defenderAll = defender1.Concat(defender2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in defenderAll)
        {
            //...

            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);

        // Disablers
        IEnumerable<Character> disabler1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility is Abilities.Disable));
        IEnumerable<Character> disabler2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility is Abilities.Disable));
        IEnumerable<Character> disablerAll = defender1.Concat(defender2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in disablerAll)
        {
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);

        // Healers
        IEnumerable<Character> healer1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility is Abilities.Disable));
        IEnumerable<Character> healer2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility is Abilities.Disable));
        IEnumerable<Character> healerAll = defender1.Concat(defender2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in healerAll)
        {
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);

        // Attackers -- random
        IEnumerable<Character> attacker1 = Teams[0].Chars.Where(ch => ch.IsAttacker());
        IEnumerable<Character> attacker2 = Teams[1].Chars.Where(ch => ch.IsAttacker());
        IEnumerable<Character> attackerAll = defender1.Concat(defender2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in attackerAll)
        {
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void OnSelectAbility(int idx)
    {
        Debug.LogFormat("Selected ability {0}", idx);

        AbilitySelectors[currentTeam].SetActivated(false);
        CurrentTeam.Chars[currentCharacter].SelectedAbility = abilityList[idx];
        CurrentTeam.Chars[currentCharacter].Selector.SetActive(false);

        // Select next character
        bool hasMoreCharacters = false;
        for (int i=currentCharacter+1; i<CurrentTeam.Chars.Length; ++i)
        {
            if (!CurrentTeam.Chars[i].IsDead && !CurrentTeam.Chars[i].SkipTurn)
            {
                hasMoreCharacters = true;
                currentCharacter = i;
                break;
            }
        }

        // Move on to next character
        if (hasMoreCharacters)
        {
            // (force state change I suck at this oh god)
            state = State.None;
            SetState(State.SelectAbility);
        }
        // No character, move on to second player
        else
        {
            // This was the second player, resolve round!
            if (currentTeam == 1)
            {
                SetState(State.Resolve);
            }
            else
            {
                currentCharacter = 0;
                currentTeam = 1;
                SetState(State.WaitForPlayer);
            }
        }
    }

    void Setup()
    {
        foreach (Team t in Teams)
        {
            foreach (Character ch in t.Chars)
                ch.Selector.SetActive(false);
        }

        abilityList = new Ability[7];
        abilityList[0] = new Abilities.Attack();
        abilityList[1] = new Abilities.Defend();
        abilityList[2] = new Abilities.BigAttack();
        abilityList[3] = new Abilities.Heal();
        abilityList[4] = new Abilities.Disable();
        abilityList[5] = new Abilities.SelfDestruct();
        abilityList[6] = new Abilities.DelayedAttack();
    }

    [System.Serializable]
    public class Team
    {
        public Character[] Chars;
    }
}

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
    public GameObject[] CharacterStates;
    public GameObject[] Projectiles;

    int currentTeam = 0;
    int currentCharacter = 0;
    int selectedTarget = 0;
    Ability[] abilityList;
    bool canSelect = true;

    Team CurrentTeam { get { return Teams[currentTeam]; } }
    Character CurrentCharacter { get { return CurrentTeam.Chars[currentCharacter]; } }
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
        if (Mathf.Approximately(Input.GetAxisRaw("Vertical"), 0f))
            canSelect = true;

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

            if (CurrentCharacter.SelectedAbility.Target() == Abilities.Target.Enemy)
            {
                if (canSelect)
                {
                    // Down
                    if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        canSelect = false;
                        ++selectedTarget;
                        if (selectedTarget >= EnemyTeam.Chars.Length)
                            selectedTarget = 0;
                    }
                    // Up
                    else if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        canSelect = false;
                        --selectedTarget;
                        if (selectedTarget < 0)
                            selectedTarget = EnemyTeam.Chars.Length - 1;
                    }

                    // Selection changed
                    if (oldSelected != selectedTarget)
                    {
                        EnemyTeam.Chars[oldSelected].Selector.SetActive(false);
                        EnemyTeam.Chars[selectedTarget].Selector.SetActive(true);
                    }
                }

                // Target selected
                if (Input.GetButtonDown("Submit"))
                {
                    CurrentCharacter.SelectedTarget = EnemyTeam.Chars[selectedTarget];
                    CurrentCharacter.SelectedTarget.Selector.SetActive(false);

                    // Next char
                    bool hasNextChar = false;
                    for (int i=currentCharacter+1; i<CurrentTeam.Chars.Length; ++i)
                    {
                        if (!CurrentTeam.Chars[i].IsDead && !CurrentTeam.Chars[i].SkipTurn)
                        {
                            hasNextChar = true;
                            currentCharacter = i;
                            break;
                        }
                    }

                    if (hasNextChar)
                        SetState(State.SelectAbility);
                    else
                    {
                        // Second player ended, resolve
                        if (currentTeam == 1)
                        {
                            SetState(State.Resolve);
                        }
                        // Next player
                        else
                        {
                            currentTeam = 1;
                            currentCharacter = 0;
                            SetState(State.WaitForPlayer);
                        }
                    }
                }
            }
            else if (CurrentCharacter.SelectedAbility.Target() == Abilities.Target.Friend)
            {
                // Down
                if (canSelect)
                {
                    if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        canSelect = false;
                        ++selectedTarget;
                        if (selectedTarget >= CurrentTeam.Chars.Length)
                            selectedTarget = 0;
                    }
                    // Up
                    else if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        canSelect = false;
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
                }

                // Target selected
                if (Input.GetButtonDown("Submit"))
                {
                    CurrentCharacter.SelectedTarget = EnemyTeam.Chars[selectedTarget];
                    CurrentCharacter.SelectedTarget.Selector.SetActive(false);

                    // Next char
                    bool hasNextChar = false;
                    for (int i = currentCharacter + 1; i < CurrentTeam.Chars.Length; ++i)
                    {
                        if (!CurrentTeam.Chars[i].IsDead && !CurrentTeam.Chars[i].SkipTurn)
                        {
                            hasNextChar = true;
                            currentCharacter = i;
                            break;
                        }
                    }

                    if (hasNextChar)
                        SetState(State.SelectAbility);
                    else
                    {
                        // Second player ended, resolve
                        if (currentTeam == 1)
                        {
                            SetState(State.Resolve);
                        }
                        // Next player
                        else
                        {
                            currentTeam = 1;
                            currentCharacter = 0;
                            SetState(State.WaitForPlayer);
                        }
                    }
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

                CurrentCharacter.Selector.SetActive(true);
                
                // Activate ability selector
                AbilitySelectors[currentTeam].SetActivated(true);

                break;
            case State.SelectTarget:
                AbilitySelectors[currentTeam].SetActivated(false);
                CurrentCharacter.Selector.SetActive(false);
                selectedTarget = 0;
                if (CurrentCharacter.SelectedAbility.Target() == Abilities.Target.Enemy)
                {
                    for (int i=0; i<EnemyTeam.Chars.Length; ++i)
                    {
                        if (!EnemyTeam.Chars[i].IsDead)
                        {
                            selectedTarget = i;
                            break;
                        }
                    }

                    EnemyTeam.Chars[selectedTarget].Selector.SetActive(true);
                }
                else
                {
                    for (int i = 0; i < CurrentTeam.Chars.Length; ++i)
                    {
                        if (!CurrentTeam.Chars[i].IsDead)
                        {
                            selectedTarget = i;
                            break;
                        }
                    }

                    CurrentTeam.Chars[selectedTarget].Selector.SetActive(true);
                }
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
        Debug.Log("Defenders");
        IEnumerable<Character> defender1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Defend)));
        IEnumerable<Character> defender2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Defend)));
        IEnumerable<Character> defenderAll = defender1.Concat(defender2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in defenderAll)
        {
            ch.SetState(CharacterStates[(int)CharState.Defense]);

            //...

            yield return new WaitForSeconds(0.1f);
        }
        if (defenderAll.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Disablers
        Debug.Log("Disablers");
        IEnumerable<Character> disabler1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Disable)));
        IEnumerable<Character> disabler2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Disable)));
        IEnumerable<Character> disablerAll = disabler1.Concat(disabler2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in disablerAll)
        {
            ch.SetState(CharacterStates[(int)CharState.Disable]);
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (disablerAll.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Healers
        Debug.Log("Healers");
        IEnumerable<Character> healer1 = Teams[0].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Heal)));
        IEnumerable<Character> healer2 = Teams[1].Chars.Where(ch => (ch.SelectedAbility.GetType() == typeof(Abilities.Heal)));
        IEnumerable<Character> healerAll = healer1.Concat(healer2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in healerAll)
        {
            ch.SetState(CharacterStates[(int)CharState.Heal]);
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (healerAll.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Attackers -- random
        Debug.Log("Attackers");
        IEnumerable<Character> attacker1 = Teams[0].Chars.Where(ch => ch.IsAttacker());
        IEnumerable<Character> attacker2 = Teams[1].Chars.Where(ch => ch.IsAttacker());
        IEnumerable<Character> attackerAll = attacker1.Concat(attacker2).OrderBy(ch => System.Guid.NewGuid());
        foreach (Character ch in attackerAll)
        {
            if (ch.SelectedAbility is Abilities.Attack)
            {
                ch.SetState(CharacterStates[(int)CharState.Attack]);
                StartCoroutine(AttackAnim(ch.transform, ch.SelectedTarget.transform, 0));
            }
            else if (ch.SelectedAbility is Abilities.BigAttack)
            {
                ch.SetState(CharacterStates[(int)CharState.BigAttack]);
                StartCoroutine(AttackAnim(ch.transform, ch.SelectedTarget.transform, 1));
            }
            else if (ch.SelectedAbility is Abilities.DelayedAttack)
            {
                ch.SetState(CharacterStates[(int)CharState.Delayed]);
                StartCoroutine(AttackAnim(ch.transform, ch.SelectedTarget.transform, 2));
            }
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (attackerAll.Count() > 0)
            yield return new WaitForSeconds(2f);

        Debug.Log("Resolved!");

        // Check if player died
        bool team1Dead = true;
        bool team2Dead = true;
        foreach (Character ch in Teams[0].Chars)
        {
            if (!ch.IsDead)
            {
                team1Dead = false;
                break;
            }
        }
        foreach (Character ch in Teams[1].Chars)
        {
            if (!ch.IsDead)
            {
                team2Dead = false;
                break;
            }
        }

        // Draw
        if (team1Dead && team2Dead)
        {

        }
        // Team 1 wins
        else if (team2Dead)
        {

        }
        // Team 2 wins
        else if (team1Dead)
        {

        }
        // Next round
        else
        {
            currentTeam = 0;
            currentCharacter = 0;
            SetState(State.WaitForPlayer);
        }
    }

    public void OnSelectAbility(int idx)
    {
        Debug.LogFormat("Selected ability {0}", idx);

        AbilitySelectors[currentTeam].SetActivated(false);
        CurrentCharacter.SelectedAbility = abilityList[idx];
        CurrentCharacter.Selector.SetActive(false);

        if (CurrentCharacter.SelectedAbility.Target() != Abilities.Target.None)
        {
            SetState(State.SelectTarget);
            return;
        }

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
            {
                ch.Selector.SetActive(false);
                ch.SetState(CharacterStates[0]);
            }
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

    IEnumerator AttackAnim(Transform from, Transform to, int projectile)
    {
        GameObject go = Instantiate(Projectiles[projectile]);
        go.transform.position = from.position;

        float duration = 1.5f;
        float start = Time.time;
        while ((Time.time - start) < duration)
        {
            float f = (Time.time - start) / duration;
            f = f * f;
            go.transform.position = Vector3.Lerp(from.position, to.position, f);

            yield return null;
        }

        Destroy(go);
    }

    [System.Serializable]
    public class Team
    {
        public Character[] Chars;
    }

    enum CharState
    {
        Neutral     = 0,
        Attack      = 1,
        Defense     = 2,
        BigAttack   = 3,
        Heal        = 4,
        Disable     = 5,
        SelfDestruct    = 6,
        Delayed     = 7
    }
}

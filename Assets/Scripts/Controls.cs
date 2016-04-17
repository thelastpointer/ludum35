using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Controls : MonoBehaviour
{
    public Team[] Teams;
    public GameObject WinPanel;
    public GameObject WaitForPlayerPanel;
    public GameObject SkipTurnPanel;
    public GameObject HelpPanel;
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
        Resolve,
        SkipTurn
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

        if (HelpPanel.activeSelf && Input.anyKeyDown)
            HelpPanel.SetActive(false);
        else if (!HelpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            HelpPanel.SetActive(true);

        if (state == State.WaitForPlayer)
        {
            if (Input.anyKeyDown)
            {
                SetState(State.SelectAbility);
            }
        }
        else if (state == State.SkipTurn)
        {
            if (Input.anyKeyDown)
            {
                if (currentTeam == 1)
                    SetState(State.Resolve);
                else
                {
                    currentTeam = 1;
                    StartPlayerTurn();
                }
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

                        do
                        {
                            ++selectedTarget;
                            if (selectedTarget >= EnemyTeam.Chars.Length)
                                selectedTarget = 0;
                        } while (EnemyTeam.Chars[selectedTarget].IsDead);
                    }
                    // Up
                    else if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        canSelect = false;

                        do
                        { 
                            --selectedTarget;
                            if (selectedTarget < 0)
                                selectedTarget = EnemyTeam.Chars.Length - 1;
                        } while (EnemyTeam.Chars[selectedTarget].IsDead);
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
                        if (!CurrentTeam.Chars[i].IsDead && (CurrentTeam.Chars[i].SkipTurn == 0))
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
                            StartPlayerTurn();
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

                        do
                        {
                            ++selectedTarget;
                            if (selectedTarget >= CurrentTeam.Chars.Length)
                                selectedTarget = 0;
                        } while (CurrentTeam.Chars[selectedTarget].IsDead);
                    }
                    // Up
                    else if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        canSelect = false;

                        do
                        {
                            --selectedTarget;
                            if (selectedTarget < 0)
                                selectedTarget = CurrentTeam.Chars.Length - 1;
                        } while (CurrentTeam.Chars[selectedTarget].IsDead);
                    }

                    // Selection changed
                    if (oldSelected != selectedTarget)
                    {
                        CurrentTeam.Chars[oldSelected].Selector.SetActive(false);
                        CurrentTeam.Chars[selectedTarget].Selector.SetActive(true);
                    }
                }

                // Target selected
                if (Input.GetButtonDown("Submit"))
                {
                    CurrentCharacter.SelectedTarget = CurrentTeam.Chars[selectedTarget];
                    CurrentCharacter.SelectedTarget.Selector.SetActive(false);

                    // Next char
                    bool hasNextChar = false;
                    for (int i = currentCharacter + 1; i < CurrentTeam.Chars.Length; ++i)
                    {
                        if (!CurrentTeam.Chars[i].IsDead && (CurrentTeam.Chars[i].SkipTurn == 0))
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
                            StartPlayerTurn();
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
            case State.SkipTurn:
                SkipTurnPanel.SetActive(false);
                break;
            default:
                break;
        }

        switch (newState)
        {
            case State.WaitForPlayer:
                WaitForPlayerPanel.SetActive(true);
                WaitForPlayerPanel.GetComponentInChildren<Text>().text = string.Format("PLAYER {0}, take your seat\npress any key when ready", (currentTeam+1));
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
                StartCoroutine(ResolveTurn());
                break;
            case State.SkipTurn:
                SkipTurnPanel.SetActive(true);
                break;
            default:
                break;
        }

        state = newState;
    }

    IEnumerator ResolveTurn()
    {
        // Defense chars
        Debug.Log("Defenders");
        IEnumerable<Character> chars = GetCharsWithAbility(typeof(Abilities.Defend));
        foreach (Character ch in chars)
        {
            ch.SetState(CharacterStates[(int)CharState.Defense]);
            yield return new WaitForSeconds(0.1f);
        }
        if (chars.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Disablers
        Debug.Log("Disablers");
        chars = GetCharsWithAbility(typeof(Abilities.Disable));
        foreach (Character ch in chars)
        {
            ch.SetState(CharacterStates[(int)CharState.Disable]);
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (chars.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Healers
        Debug.Log("Healers");
        chars = GetCharsWithAbility(typeof(Abilities.Heal));
        foreach (Character ch in chars)
        {
            ch.SetState(CharacterStates[(int)CharState.Heal]);
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (chars.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Self destruct
        Debug.Log("Self destructors");
        chars = GetCharsWithAbility(typeof(Abilities.SelfDestruct));
        foreach (Character ch in chars)
        {
            ch.SetState(CharacterStates[(int)CharState.SelfDestruct]);
            ch.SelectedAbility.Resolve(ch, ch.SelectedTarget);
            yield return new WaitForSeconds(0.1f);
        }
        if (chars.Count() > 0)
            yield return new WaitForSeconds(0.5f);

        // Attackers -- random
        Debug.Log("Attackers");

        chars = GetCharsWithAbility(typeof(Abilities.Attack))
            .Concat(GetCharsWithAbility(typeof(Abilities.BigAttack)))
            .Concat(GetCharsWithAbility(typeof(Abilities.DelayedAttack)));

        foreach (Character ch in chars)
        {
            if (ch.SelectedAbility is Abilities.Attack)
            {
                ch.SetState(CharacterStates[(int)CharState.Attack]);
                StartCoroutine(AttackAnim(ch, ch.SelectedTarget, 0, (c1, c2) => { c1.SelectedAbility.Resolve(c1, c2); }));
            }
            else if (ch.SelectedAbility is Abilities.BigAttack)
            {
                ch.SetState(CharacterStates[(int)CharState.BigAttack]);
                StartCoroutine(AttackAnim(ch, ch.SelectedTarget, 1, (c1, c2) => { c1.SelectedAbility.Resolve(c1, c2); }));
            }
            else if (ch.SelectedAbility is Abilities.DelayedAttack)
            {
                ch.SetState(CharacterStates[(int)CharState.Delayed]);
                StartCoroutine(AttackAnim(ch, ch.SelectedTarget, 2, (c1, c2) => { c1.SelectedAbility.Resolve(c1, c2); }));
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        if (chars.Count() > 0)
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
            WinPanel.SetActive(true);
            WinPanel.transform.FindChild("Panel/Text").GetComponent<Text>().text = string.Format("close call...\nthis is a DRAW");
        }
        // Team 1 wins
        else if (team2Dead)
        {
            WinPanel.SetActive(true);
            WinPanel.transform.FindChild("Panel/Text").GetComponent<Text>().text = string.Format("PLAYER 1 wins!\nway to go dude");
        }
        // Team 2 wins
        else if (team1Dead)
        {
            WinPanel.SetActive(true);
            WinPanel.transform.FindChild("Panel/Text").GetComponent<Text>().text = string.Format("PLAYER 2 wins!\nway to go dude");
        }
        // Next round
        else
        {
            // Reset skip turns
            foreach (Team t in Teams)
            {
                foreach (Character ch in t.Chars)
                    ch.SkipTurn = Mathf.Max(0, ch.SkipTurn - 1);
            }

            currentTeam = 0;
            StartPlayerTurn();
        }
    }

    IEnumerable<Character> GetCharsWithAbility(System.Type ability)
    {
        // Note: ordering by a new guid means a randomly shuffled list

        IEnumerable<Character> team1 = Teams[0].Chars.Where(ch => !ch.IsDead && (ch.SkipTurn == 0));
        IEnumerable<Character> team2 = Teams[1].Chars.Where(ch => !ch.IsDead && (ch.SkipTurn == 0));

        IEnumerable<Character> chars1 = team1.Where(ch => (ch.SelectedAbility.GetType() == ability));
        IEnumerable<Character> chars2 = team2.Where(ch => (ch.SelectedAbility.GetType() == ability));
        IEnumerable<Character> charsAll = chars1.Concat(chars2).OrderBy(ch => System.Guid.NewGuid());

        return charsAll;
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
            if (!CurrentTeam.Chars[i].IsDead && (CurrentTeam.Chars[i].SkipTurn == 0))
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
                currentTeam = 1;
                StartPlayerTurn();
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

    IEnumerator AttackAnim(Character from, Character to, int projectile, System.Action<Character, Character> onEnded)
    {
        GameObject go = Instantiate(Projectiles[projectile]);
        go.transform.position = from.transform.position;

        float duration = 1.5f;
        float start = Time.time;
        while ((Time.time - start) < duration)
        {
            float f = (Time.time - start) / duration;
            f = f * f;
            go.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, f);

            yield return null;
        }

        Destroy(go);

        FX.DoHitEffect(to.transform.position);

        if (onEnded != null)
            onEnded(from, to);
    }

    void StartPlayerTurn()
    {
        Debug.Log("Start turn for player " + currentTeam);

        //foreach (Character ch in EnemyTeam.Chars)
        //    ch.SkipTurn = false;

        currentCharacter = 0;
        if (SelectFirstCharacter())
            SetState(State.WaitForPlayer);
        else
            SetState(State.SkipTurn);
    }

    bool SelectFirstCharacter()
    {
        while ((CurrentTeam.Chars.Length > currentCharacter) && (CurrentTeam.Chars[currentCharacter].IsDead || (CurrentTeam.Chars[currentCharacter].SkipTurn > 0)))
            ++currentCharacter;

        if (currentCharacter >= CurrentTeam.Chars.Length)
        {
            SetState(State.SkipTurn);
            return false;
        }

        return true;
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

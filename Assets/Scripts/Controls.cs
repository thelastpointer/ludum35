using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Controls : MonoBehaviour
{
    public Team[] Teams;
    public GameObject WaitForPlayerPanel;
    public AbilitySelector[] AbilitySelectors;

    int currentTeam = 0;
    int currentCharacter = 0;
    int selectedEnemy = 0;

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
            int oldSelected = selectedEnemy;

            // Down
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                ++selectedEnemy;
                if (selectedEnemy >= EnemyTeam.Chars.Length)
                    selectedEnemy = 0;
            }
            // Up
            else if (Input.GetAxisRaw("Vertical") > 0)
            {
                --selectedEnemy;
                if (selectedEnemy < 0)
                    selectedEnemy = EnemyTeam.Chars.Length - 1;
            }

            // Selection changed
            if (oldSelected != selectedEnemy)
            {
                EnemyTeam.Chars[oldSelected].Selector.SetActive(false);
                EnemyTeam.Chars[selectedEnemy].Selector.SetActive(false);
            }

            // Target selected
            if (Input.GetButtonDown("Submit"))
            {
                //...
            }
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
                /*
                // Deselect all
                foreach (Character ch in CurrentTeam.Chars)
                    ch.Selector.SetActive(false);

                // Select first character
                for (int i=0; i<CurrentTeam.Chars.Length; ++i)
                {
                    if (!CurrentTeam.Chars[i].IsDead)
                    {
                        currentCharacter = i;
                        CurrentTeam.Chars[i].Selector.SetActive(true);
                        break;
                    }
                }
                */
                // Activate ability selector
                AbilitySelectors[currentTeam].SetActivated(true);

                break;
            case State.SelectTarget:
                break;
            case State.Resolve:
                break;
            default:
                break;
        }

        state = newState;
    }

    public void OnSelectAbility(int idx)
    {
        Debug.LogFormat("Selected ability {0}", idx);

        AbilitySelectors[currentTeam].SetActivated(false);
        CurrentTeam.Chars[currentCharacter].SelectedAbility = idx;
        CurrentTeam.Chars[currentCharacter].Selector.SetActive(false);

        // Select next character
        bool hasMoreCharacters = false;
        for (int i=currentCharacter+1; i<CurrentTeam.Chars.Length; ++i)
        {
            if (!CurrentTeam.Chars[i].IsDead)
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
    }

    [System.Serializable]
    public class Team
    {
        public Character[] Chars;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AbilitySelector : MonoBehaviour
{
    public SelectedEvent OnSelected;

    public Color SelectedColor = Color.red;
    public Color DeselectedColor = Color.clear;

    [System.Serializable]
    public class SelectedEvent : UnityEvent<int> { }

    Button[] abilities;
    bool isActive = false;
    int selected = 0;
    bool canChange = true;

    public void SetActivated(bool value)
    {
        isActive = value;
        gameObject.SetActive(value);
        selected = 0;

        foreach (Button b in abilities)
            b.GetComponent<Image>().color = DeselectedColor;
        abilities[selected].GetComponent<Image>().color = SelectedColor;
    }

    void Awake()
    {
        abilities = GetComponentsInChildren<Button>();
    }

    void Update()
    {
        if (isActive)
        {
            if (canChange)
            {
                // Right
                if (Input.GetAxisRaw("Horizontal") > 0)
                {
                    canChange = false;

                    abilities[selected].GetComponent<Image>().color = DeselectedColor;

                    ++selected;
                    if (selected >= abilities.Length)
                        selected = 0;

                    abilities[selected].GetComponent<Image>().color = SelectedColor;
                }
                // Left
                else if (Input.GetAxisRaw("Horizontal") < 0)
                {
                    canChange = false;

                    abilities[selected].GetComponent<Image>().color = DeselectedColor;

                    --selected;
                    if (selected < 0)
                        selected = abilities.Length - 1;

                    abilities[selected].GetComponent<Image>().color = SelectedColor;
                }
            }
            else
            {
                if (Mathf.Approximately(Input.GetAxisRaw("Horizontal"), 0f))
                    canChange = true;
            }

            if (Input.GetButtonDown("Submit"))
            {
                if (OnSelected != null)
                    OnSelected.Invoke(selected);
            }
        }
    }
}

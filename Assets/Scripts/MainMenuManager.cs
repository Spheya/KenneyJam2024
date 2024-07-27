using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static bool enableAi = true;

    public int selectedLevel = 0;

    public List<Button> levelButtons;
    public Button withFriendButton;
    public Button nerdButton;

    public Sprite selected;
    public Sprite notselected;

    private void Start() {
        for (int i = 0; i < levelButtons.Count; ++i) {
            int j = i;
            levelButtons[i].onClick.AddListener(() => { levelSelect(j); });
        }

        withFriendButton.onClick.AddListener(() => { playGame(false); });

        nerdButton.onClick.AddListener(() => { playGame(true); });
    }

    void playGame(bool ai) {
        AudioManager.instance.Click();
        enableAi = ai;
        SceneManager.LoadScene(1 + selectedLevel);
    }

    void levelSelect(int level) {
        levelButtons[selectedLevel].GetComponent<Image>().sprite = notselected;
        levelButtons[level].GetComponent<Image>().sprite = selected;
        selectedLevel = level;
        AudioManager.instance.Click();
    }
}

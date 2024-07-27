using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Ball> balls;
    public int turn;

    public TextMeshProUGUI redTurn;
    public TextMeshProUGUI blueTurn;
    public TextMeshProUGUI redWin;
    public TextMeshProUGUI blueWin;

    public float turnUpdateAnimationLength = 1.0f;
    public float turnUpdateFreq = 20.0f;
    public float turnUpdateExp = 10.0f;

    private bool _won = false;

    void Start() {
        instance = this;

        if(MainMenuManager.enableAi) {
            balls[0].isAi = true;
        }
    }

    void Update() {
        if (!_won) {
            bool allReady = true;
            foreach (var ball in balls)
                if (!ball.FinishedRolling)
                    allReady = false;

            if (allReady) {
                ++turn;
                turn %= balls.Count;
                foreach (var ball in balls)
                    ball.arrow.Ready(true);

                balls[turn].Ready();

                AudioManager.instance.turnswitch.Play();

                StartCoroutine(turnDisplayCoroutine(turn == 1 ? blueTurn : redTurn));
            }
        }
    }

    private IEnumerator turnDisplayCoroutine(TextMeshProUGUI text) {
        text.enabled = true;

        float t = 0.0f;
        
        while(t < 1.0f) {
            t += Time.deltaTime / 1.0f;

            float f = Mathf.Sin(turnUpdateFreq * t) * Mathf.Exp(-turnUpdateExp * t);

            text.fontSize = 100.0f + f * 40.0f;

            if(t > 0.8f) {
                text.alpha = 1.0f - ((t - 0.8f) / 0.2f);
            } else {
                text.alpha = 1.0f;
            }

            yield return null;
        }

        text.enabled = false;
    }

    public IEnumerator backToTheLobby() {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(0);
    }

    public void Win(bool red, bool aiWin) {
        _won = true;
        StartCoroutine(turnDisplayCoroutine(red ? redWin : blueWin));
        StartCoroutine(backToTheLobby());

        if (aiWin) {
            AudioManager.instance.lose.Play();
        } else {
            AudioManager.instance.win.Play();
        }
    }
}
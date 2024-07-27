using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        var ball = other.GetComponent<Ball>();
        if (ball == null)
            return;

        GameManager.instance.Win(ball == GameManager.instance.balls[0], ball.isAi);
    }
}

using System.Collections;
using UnityEditor;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject PauseMenuUI;
    public static bool Pause;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !Pause) {
            Pause = true;
            PauseState();
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            Pause = false;
            ResumeState();
        }
    }
    void PauseState() {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    void ResumeState() {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}

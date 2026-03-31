using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private UIController targetUI;
    public GameObject PauseMenuUI;
    public bool Pause;
    public bool ButtonContinuePressed;

    private void Start() {
        ButtonContinuePressed = false;
    }
    void Update()
    {
        // Appuyer sur Echap pour mettre le jeu en pause et afficher le menu pause
        if (Input.GetKeyDown(KeyCode.Escape) && !Pause) {
            PauseState();
        }
        // Rappuyer sur Echap pour reprendre le jeu et retirer le menu pause
        else if (Input.GetKeyDown(KeyCode.Escape) || ButtonContinuePressed) {
            ResumeState();
        }
    }

    // Met le jeu en pause et coupe le son
    void PauseState() {
        Pause = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Instantiate(PauseMenuUI);
    }
    // Remet le jeu en marche et réactive le son
    void ResumeState() {
        Pause = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Destroy(GameObject.FindGameObjectWithTag("PauseMenu"));
        ButtonContinuePressed = false;
    }


    // Menu pause UI
    public void Continue()
    {
        targetUI = GameObject.FindGameObjectWithTag("Player").GetComponent<UIController>();
        targetUI.ButtonContinuePressed = true;
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    // Menu Title Screen UI
    public void StartGame()
    {
        SceneManager.LoadScene("Niveau 1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

public class UIController : MonoBehaviour
{
    [SerializeField] AudioClip[] selectSound;
    [SerializeField] AudioSource myAudioSource;

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
    // Random sound effect
    void SoundSelect() {
        int monRandom = Random.Range(0, 5);
        myAudioSource.clip = selectSound[monRandom];
        myAudioSource.Play();
    }
    IEnumerator ChoixMenu(int buttonID)
    {
        SoundSelect();
        switch (buttonID)
        {
            case 0:
                targetUI = GameObject.FindGameObjectWithTag("Player").GetComponent<UIController>();
                targetUI.ButtonContinuePressed = true;
                break;
            case 1:
                yield return new WaitForSeconds(0.7f);
                SceneManager.LoadScene(0);
                break;
            case 2:
                yield return new WaitWhile(() => myAudioSource.isPlaying);
                SceneManager.LoadScene("Niveau 1");
                break;
            default:
                break;
        }
    }

    // Menu pause UI
    public void Continue()
    {
        targetUI = GameObject.FindGameObjectWithTag("Player").GetComponent<UIController>();
        targetUI.ButtonContinuePressed = true;
        //StartCoroutine(ChoixMenu(0));
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
        //StartCoroutine(ChoixMenu(1));
    }

    // Menu Title Screen UI
    public void StartGame()
    {
        StartCoroutine(ChoixMenu(2));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

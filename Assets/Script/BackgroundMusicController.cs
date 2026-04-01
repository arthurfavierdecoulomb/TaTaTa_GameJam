using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicController : MonoBehaviour
{
    [SerializeField] AudioClip[] selectMusic;
    [SerializeField] AudioSource myAudioBackgroundSource;
    int currentSceneIndex;
    int newLoopInt;
    bool alreadyPlayedOnce = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        myAudioBackgroundSource.clip = selectMusic[currentSceneIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (!myAudioBackgroundSource.isPlaying) {
            Debug.Log("test");
            if (!alreadyPlayedOnce) {
                switch (currentSceneIndex)
                {
                    case 0:
                        newLoopInt = 2;
                        break;
                    case 1:
                        newLoopInt = 3;
                        break;
                    default:
                        break;
                }
                alreadyPlayedOnce = true;
                myAudioBackgroundSource.Play();
            }
            else
            {
                myAudioBackgroundSource.clip = selectMusic[newLoopInt];
                myAudioBackgroundSource.Play();
            }
        }
    }
}

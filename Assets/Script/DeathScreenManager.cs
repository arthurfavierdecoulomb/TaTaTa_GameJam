using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance { get; private set; }

    [Header("Références UI")]
    [SerializeField] GameObject deathPanel;
    [SerializeField] TextMeshProUGUI deathTitle;
    [SerializeField] TextMeshProUGUI deathCounter;

    [Header("Timing")]
    [SerializeField] float displayDuration = 1.8f;
    [SerializeField] float fadeDuration = 0.6f;

    int deathCount = 0;

    static readonly string[] rageTitles = new string[]
    {
        "GG EZ",
        "T'es une légende... du skill négatif.",
        "Même ma grand-mère fait mieux.",
        "Touche pas à la manette.",
        "Tu l'as cherché.",
        "C'est pathétique.",
        "Réessaie dans 10 ans.",
        "Wow. Incroyable. Vraiment.",
        "Le sol t'a dominé.",
        "Tu joues avec tes pieds ?",
        "Même le tutoriel pleure pour toi.",
        "Ici repose ton skill : 0",
        "Bravo. Tu as trouvé comment perdre.",
        "Le jeu te regarde avec pitié.",
        "Retourne jouer à Candy Crush.",
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (deathPanel != null) deathPanel.SetActive(false); 
    }

    public void ShowDeathScreen(System.Action onComplete)
    {
        deathCount++;
        StartCoroutine(DeathRoutine(onComplete));
    }

    IEnumerator DeathRoutine(System.Action onComplete)
    {
        // Titre rageur aléatoire
        if (deathTitle != null)
            deathTitle.text = rageTitles[Random.Range(0, rageTitles.Length)];

        // Compteur de morts
        if (deathCounter != null)
            deathCounter.text = "tu es mort " + deathCount + " fois...";

        SetPanelAlpha(1f);
        if (deathPanel != null) deathPanel.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeOut());

        if (deathPanel != null) deathPanel.SetActive(false);

        onComplete?.Invoke();
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetPanelAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetPanelAlpha(0f);
    }

    void SetPanelAlpha(float alpha)
    {
        if (deathPanel == null) return;
        CanvasGroup cg = deathPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
        }
        else
        {
            Image img = deathPanel.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
    }
}
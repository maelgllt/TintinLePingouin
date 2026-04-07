using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QTEController : MonoBehaviour
{
    [Header("Paramètres de Difficulté")]
    public float timeToReact = 2.0f; // Temps par défaut (Easy)
    public bool isHardMode = false;

    [Header("UI Elements")]
    public GameObject qtePanel; // Le panel qui contient le texte
    public Text keyDisplay;      // Le texte qui affiche la touche (ex: "Appuyez sur Z")
    public Image timerBar;      // Une barre visuelle qui se vide

    private string currentKey;
    private bool qteActive = false;
    private List<string> keys = new List<string> { "Z", "Q", "S", "D" };

    void Start()
    {
        qtePanel.SetActive(false);
        // Ajustement automatique selon le mode
        if (isHardMode) timeToReact = 0.8f; 
    }

    // Appelée par un Trigger au niveau du virage
    public void StartQTE()
    {
        if (!qteActive)
        {
            StopAllCoroutines();
            StartCoroutine(QTERoutine());
        }
    }

    IEnumerator QTERoutine()
    {
        qteActive = true;
        currentKey = keys[Random.Range(0, keys.Count)];
        keyDisplay.text = currentKey;
        qtePanel.SetActive(true);

        float timeLeft = timeToReact;

        while (timeLeft > 0)
        {
            timerBar.fillAmount = timeLeft / timeToReact;
            
            // Vérification de l'input
            if (Input.GetKeyDown(currentKey.ToLower()))
            {
                Success();
                yield break;
            }

            // Si le joueur se trompe de touche
            if (Input.anyKeyDown && !Input.GetKeyDown(currentKey.ToLower()))
            {
                Fail();
                yield break;
            }

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Fail();
    }

    void Success()
    {
        Debug.Log("Virage réussi ! Tintin glisse parfaitement.");
        qteActive = false;
        qtePanel.SetActive(false);
        // Ici, déclenche l'animation de virage de Tintin
    }

    void Fail()
    {
        Debug.Log("Aïe ! Tintin s'est pris une table.");
        qteActive = false;
        qtePanel.SetActive(false);
        // Ici, appelle la fonction pour retirer un cœur à Tintin
    }
}
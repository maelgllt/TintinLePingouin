using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class QTEController : MonoBehaviour
{
    [Header("Difficulty Settings")]
    public float timeEasy = 2.0f;
    public float timeMedium = 1.2f;
    public float timeHard = 0.8f;
    
    private float timeToReact;

    [Header("UI Elements")]
    public GameObject qtePanel; 
    public Text keyDisplay;      
    public Image timerBar;      

    private string currentKey;
    private bool qteActive = false;
    private List<string> keys = new List<string> { "Z", "Q", "S", "D" };

    // Variables pour mémoriser Tintin et la direction
    private Transform joueurTransform;
    private Vector3 directionDuVirage;

    void Start()
    {
        switch (GameSettings.CurrentDifficulty)
        {
            case GameSettings.Difficulty.Easy: timeToReact = timeEasy; break;
            case GameSettings.Difficulty.Medium: timeToReact = timeMedium; break;
            case GameSettings.Difficulty.Hard: timeToReact = timeHard; break;
        }
        qtePanel.SetActive(false);
    }

    // Le Trigger envoie maintenant la direction et le joueur
    public void StartQTE(Vector3 directionSortie, Transform joueur)
    {
        // On mémorise les infos pour s'en servir en cas de succès !
        directionDuVirage = directionSortie;
        joueurTransform = joueur;

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
            
            if (Input.GetKeyDown(currentKey.ToLower()))
            {
                Success();
                yield break;
            }

            if (Input.anyKeyDown && !Input.GetKeyDown(currentKey.ToLower()))
            {
                if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                {
                    Fail();
                    yield break;
                }
            }

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        Fail();
    }

    void Success()
    {
        Debug.Log("Success! Le pingouin tourne bien.");
        qteActive = false;
        qtePanel.SetActive(false);

        // --- LA MAGIE DU VIRAGE EST ICI ---
        if (joueurTransform != null)
        {
            // On oriente le "Visage" de Tintin vers la nouvelle route
            // Le script TintinGlisse s'occupera automatiquement de le faire avancer dans cette nouvelle direction !
            joueurTransform.forward = directionDuVirage;
        }
    }

    void Fail() 
    {
        Debug.Log("Fail! Le pingouin va chuter.");
        qteActive = false;
        qtePanel.SetActive(false);
        // On ne le tourne pas : punition naturelle, il continue tout droit et tombe dans le vide !
    }
}
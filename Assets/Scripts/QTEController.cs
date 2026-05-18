using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class QTEController : MonoBehaviour
{
    public GameObject panelDefaite;

    [Header("Difficulty Settings")]
    public float timeEasy = 2.0f;
    public float timeMedium = 1.2f;
    public float timeHard = 0.8f;

    private float timeToReact;

    [Header("UI Elements")] 
    public GameObject qtePanel;
    public Text keyDisplay;
    public Image timerBar;

    [Header("Audio")] 
    public AudioSource lecteurAudio;
    public AudioClip sonSucces;
    public AudioClip sonEchec;
    public AudioClip sonDefaite; // <-- NOUVEAU

    private string currentKey;
    private bool qteActive = false;

    private List<string> keys = new List<string> { "Z", "Q", "S", "D" };

    private Transform joueurTransform;
    private Vector3 directionDuVirage;
    private TintinGlisse joueurGlisse;
    private Rigidbody joueurRb;
    private Vector3 positionDuVirage;   // <-- ajouter cette ligne


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

    public void StartQTE(Vector3 directionSortie, Vector3 positionVirage, Transform joueur)
    {
        directionDuVirage = directionSortie;
        joueurTransform = joueur;
        positionDuVirage = positionVirage;

        joueurGlisse = joueur.GetComponent<TintinGlisse>();
        joueurRb = joueur.GetComponent<Rigidbody>();

        if (!qteActive)
        {
            StopAllCoroutines();
            StartCoroutine(QTERoutine());
        }
    }

    private bool IsCorrectKey(string expectedKey)
    {
        var kb = Keyboard.current;
        if (kb == null) return false;

        switch (expectedKey)
        {
            case "Z": return kb.zKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
            case "Q": return kb.qKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
            case "S": return kb.sKey.wasPressedThisFrame;
            case "D": return kb.dKey.wasPressedThisFrame;
            default: return false;
        }
    }

    private bool AnyKeyPressed()
    {
        var kb = Keyboard.current;
        return kb != null && kb.anyKey.wasPressedThisFrame;
    }

    IEnumerator QTERoutine()
    {
        qteActive = true;
        currentKey = keys[Random.Range(0, keys.Count)];
        keyDisplay.text = currentKey;
        qtePanel.SetActive(true);

        if (joueurGlisse != null) joueurGlisse.enabled = false;
        if (joueurRb != null) joueurRb.linearVelocity = Vector3.zero;

        float timeLeft = timeToReact;

        while (timeLeft > 0)
        {
            timerBar.fillAmount = timeLeft / timeToReact;

            if (IsCorrectKey(currentKey))
            {
                Success();
                yield break;
            }

            if (AnyKeyPressed() && !IsCorrectKey(currentKey))
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
        Debug.Log("Success! Le pingouin tourne bien.");
        qteActive = false;
        qtePanel.SetActive(false);

        if (lecteurAudio != null && sonSucces != null)
        {
            lecteurAudio.PlayOneShot(sonSucces);
        }

        if (joueurTransform != null)
        {
            // Recentre Tintin sur la plateforme en gardant sa hauteur actuelle
            Vector3 nouvellePos = positionDuVirage;
            nouvellePos.y = joueurTransform.position.y;
            joueurTransform.position = nouvellePos;

            joueurTransform.forward = directionDuVirage;
        }

        if (joueurGlisse != null) joueurGlisse.enabled = true;
    }

    void Fail()
    {
        Debug.Log("Fail! Le pingouin va chuter.");
        qteActive = false;
        qtePanel.SetActive(false);

        if (lecteurAudio != null)
        {
            if (sonEchec != null) lecteurAudio.PlayOneShot(sonEchec); // Bruit d'erreur
            if (sonDefaite != null) lecteurAudio.PlayOneShot(sonDefaite); // Musique Game Over
        }
        
        panelDefaite.SetActive(true);
        Time.timeScale = 0f;
    }
}
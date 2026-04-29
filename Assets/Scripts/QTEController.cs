using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ← NOUVEAU
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

    public void StartQTE(Vector3 directionSortie, Transform joueur)
    {
        directionDuVirage = directionSortie;
        joueurTransform = joueur;

        if (!qteActive)
        {
            StopAllCoroutines();
            StartCoroutine(QTERoutine());
        }
    }

    // --- New Input System : check de la bonne touche (AZERTY + QWERTY) ---
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
            default:  return false;
        }
    }

    // --- New Input System : est-ce qu'une touche quelconque a été pressée ? ---
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

        if (joueurTransform != null)
        {
            joueurTransform.forward = directionDuVirage;
        }
    }

    void Fail() 
    {
        Debug.Log("Fail! Le pingouin va chuter.");
        qteActive = false;
        qtePanel.SetActive(false);
    }
}
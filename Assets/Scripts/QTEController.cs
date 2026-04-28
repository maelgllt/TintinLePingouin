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

    void Start()
    {
        // Apply selected difficulty from the Main Menu
        switch (GameSettings.CurrentDifficulty)
        {
            case GameSettings.Difficulty.Easy:
                timeToReact = timeEasy;
                break;
            case GameSettings.Difficulty.Medium:
                timeToReact = timeMedium;
                break;
            case GameSettings.Difficulty.Hard:
                timeToReact = timeHard;
                break;
        }

        StartQTE(); 
    }

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
            
            if (Input.GetKeyDown(currentKey.ToLower()))
            {
                Success();
                yield break;
            }

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
        Debug.Log("Success!");
        qteActive = false;
        qtePanel.SetActive(false);
    }

    void Fail() 
    {
        Debug.Log("Fail!");
        qteActive = false;
        qtePanel.SetActive(false);
    }
}
using UnityEngine;

public class TintinGlisse : MonoBehaviour
{
    [Header("Vitesse par difficulté")]
    public float vitesseEasy = 5f;
    public float vitesseMedium = 8f;
    public float vitesseHard = 10f;

    private float vitesse;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        switch (GameSettings.CurrentDifficulty)
        {
            case GameSettings.Difficulty.Easy:   vitesse = vitesseEasy;   break;
            case GameSettings.Difficulty.Medium: vitesse = vitesseMedium; break;
            case GameSettings.Difficulty.Hard:   vitesse = vitesseHard;   break;
        }
    }

    void FixedUpdate()
    {
        Vector3 nouvelleVitesse = transform.forward * vitesse;
        nouvelleVitesse.y = rb.linearVelocity.y;
        rb.linearVelocity = nouvelleVitesse;
    }
}
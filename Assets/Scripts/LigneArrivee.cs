using UnityEngine;

public class LigneArrivee : MonoBehaviour
{
    public GameObject panelVictoire;

    [Header("Audio")]
    public AudioSource lecteurAudio;
    public AudioClip sonVictoire;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            if (panelVictoire != null)
            {
                panelVictoire.SetActive(true);
                Time.timeScale = 0f; 
                
                if (lecteurAudio != null && sonVictoire != null)
                {
                    lecteurAudio.PlayOneShot(sonVictoire);
                }
            }
        }
    }
}
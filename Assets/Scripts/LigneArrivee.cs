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
            // NOUVEAU : On coupe la musique d'ambiance de la caméra
            AudioSource musiqueAmbiance = Camera.main.GetComponent<AudioSource>();
            if (musiqueAmbiance != null)
            {
                musiqueAmbiance.Stop();
            }

            // Affichage du panel et lancement du son de victoire
            if (panelVictoire != null)
            {
                panelVictoire.SetActive(true);
                Time.timeScale = 0f; // On fige le jeu immédiatement
                
                if (lecteurAudio != null && sonVictoire != null)
                {
                    lecteurAudio.PlayOneShot(sonVictoire);
                }
            }
        }
    }
}
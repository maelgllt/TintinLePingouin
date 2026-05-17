using UnityEngine;

public class LigneArrivee : MonoBehaviour
{
    public GameObject panelVictoire;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panelVictoire != null)
            {
                panelVictoire.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }
}
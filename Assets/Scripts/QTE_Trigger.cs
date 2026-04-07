using UnityEngine;

public class QTE_Trigger : MonoBehaviour
{
    [Tooltip("Glisse ici l'objet qui contient le script QTEController")]
    public QTEController qteController;

    private void OnTriggerEnter(Collider other)
    {
        // On vérifie si c'est bien le pingouin (le joueur) qui entre dans la zone
        // Assure-toi d'ajouter le Tag "Player" à ton personnage Tintin dans Unity !
        if (other.CompareTag("Player"))
        {
            qteController.StartQTE();
        }
    }
}
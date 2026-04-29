using UnityEngine;

public class QTE_Trigger : MonoBehaviour
{
    [Tooltip("Glisse ici l'objet qui contient le script QTEController")]
    public QTEController qteController;
    
    // NOUVEAU : On stocke la direction vers laquelle Tintin doit tourner
    public Vector3 directionDeSortie; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // On envoie le QTE, mais on lui donne aussi la direction et le Transform de Tintin !
            qteController.StartQTE(directionDeSortie, other.transform);
        }
    }
}
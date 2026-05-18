using UnityEngine;

public class QTE_Trigger : MonoBehaviour
{
    [Tooltip("Glisse ici l'objet qui contient le script QTEController")]
    public QTEController qteController;
    
    public Vector3 directionDeSortie; 
    public Vector3 positionDuVirage;   // <-- nouveau champ


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            qteController.StartQTE(directionDeSortie, positionDuVirage, other.transform);  // 3 arguments
        }
    }
}
using UnityEngine;

public class QTE_Trigger : MonoBehaviour
{
    [Tooltip("Glisse ici l'objet qui contient le script QTEController")]
    public QTEController qteController;
    
    public Vector3 directionDeSortie; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            qteController.StartQTE(directionDeSortie, other.transform);
        }
    }
}
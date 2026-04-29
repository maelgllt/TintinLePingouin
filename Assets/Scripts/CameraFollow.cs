using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cible à suivre")]
    [Tooltip("Glisse ici l'objet Tintin")]
    public Transform tintin;

    [Header("Réglages Caméra")]
    public float distance = 5f;        // La distance derrière le personnage
    public float hauteur = 3f;         // La hauteur au-dessus du personnage
    public float vitesseDeSuivi = 10f; // La vitesse de fluidité de la caméra

    // On utilise LateUpdate au lieu de Update pour la caméra, 
    // ça évite les tremblements bizarres avec la physique du Rigidbody !
    void LateUpdate()
    {
        if (tintin == null) return;

        // 1. On calcule la position idéale (derrière le dos de Tintin, et un peu en l'air)
        Vector3 positionIdeale = tintin.position - (tintin.forward * distance) + (Vector3.up * hauteur);

        // 2. On déplace la caméra vers cette position de façon très fluide grâce à Lerp
        transform.position = Vector3.Lerp(transform.position, positionIdeale, vitesseDeSuivi * Time.deltaTime);

        // 3. On fait tourner la caméra pour qu'elle regarde dans la même direction que Tintin
        Quaternion rotationIdeale = Quaternion.LookRotation(tintin.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationIdeale, vitesseDeSuivi * Time.deltaTime);
    }
}
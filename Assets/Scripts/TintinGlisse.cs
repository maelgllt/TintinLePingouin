using UnityEngine;

public class TintinGlisse : MonoBehaviour
{
    public float vitesse = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // On calcule la vitesse vers l'avant
        Vector3 nouvelleVitesse = transform.forward * vitesse;
        
        // TRÈS IMPORTANT : On garde la vitesse verticale actuelle (la gravité) !
        nouvelleVitesse.y = rb.linearVelocity.y; 
        
        // On applique cette vitesse au corps physique
        rb.linearVelocity = nouvelleVitesse;
    }
}
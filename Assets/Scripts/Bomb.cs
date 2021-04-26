using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Effects;

public class Bomb : MonoBehaviour
{

    public float delay;
    public float explosionRadius;
    public LayerMask interactionLayer;
    public GameObject explosionPrefab;
    private GameObject explosion;
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(Explode), delay);
    }

    void Explode()
    {
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, interactionLayer);

        foreach (Collider c in hitColliders)
        {
            if (c.transform.CompareTag("Player"))
            {
                c.gameObject.GetComponent<PlayerMovement>().hit();
            }
        }
        gameObject.GetComponent<AudioSource>().Play();
        explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false;
        
        Invoke(nameof(kill), 3f);
    }


    void kill()
    {
        Destroy(explosion);
        Destroy(gameObject);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}

using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPun
{
    public float speed = 1f;      // Forward speed
    public float lifetime = 5f;    // Lifetime before auto-destruction

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            
            rb.velocity = transform.forward * speed;
        }
        
        Invoke("DestroySelf", lifetime);
    }

    private void Update()
    {
        if (rb != null)
        {

            rb.velocity = transform.forward * speed;
        }
        Invoke("DestroySelf", lifetime);
    }

    void DestroySelf()
    {
        
        PhotonNetwork.Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (!other.isTrigger)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

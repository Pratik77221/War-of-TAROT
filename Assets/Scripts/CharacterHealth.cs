using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class CharacterHealth : MonoBehaviourPun
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public Slider healthSlider;
    public Vector3 healthBarOffset = new Vector3(0, 2.5f, 0);

    [Header("Damage Profile")]
    public float hookPunchDamage = 20f;
    public float heavyPunchDamage = 35f;

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBarPosition();
    }

    void Update()
    {
        UpdateHealthBarPosition();
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        if (photonView.IsMine)
        {
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            UpdateHealthBar();

            if (currentHealth <= 0) Die();
        }
    }

    void UpdateHealthBar()
    {
        healthSlider.value = currentHealth / maxHealth;
    }

    void UpdateHealthBarPosition()
    {
        healthSlider.transform.position = transform.position + healthBarOffset;
    }

    void Die()
    {
        // Add death animation/effects
        photonView.RPC("RPC_Die", RpcTarget.All);
    }

    [PunRPC]
    void RPC_Die()
    {
        gameObject.SetActive(false);
    }
}
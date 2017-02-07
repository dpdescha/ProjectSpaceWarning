using UnityEngine;
using System.Collections;

/// <summary>
/// maintains a player's health, drives health bar
/// </summary>
public class HealthManager : MonoBehaviour {

    public int maxHealth;
    public int currentHealth { get; private set; }

	// Use this for initialization
	void Start () {
        currentHealth = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
        // TODO: update healthbar
    }

    void Die()
    {
        // TODO: win condition
    }
}

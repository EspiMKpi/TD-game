using UnityEngine;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBar;
    private float currentHealth;

    private Transform target;
    private int pathIndex = 0;

    private void Start()
    {
        target = Level_Manager.main.path[pathIndex];
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.fillAmount = 1f;
        }
    }

    private void Update()
    {
        // Temporary testing for tower shooting (Using New Input System)
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(20f);
        }

        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (pathIndex >= Level_Manager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                HealthManager.main.TakeDamage(20f);
                return;
            }
            else
            {
                target = Level_Manager.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position). normalized;

        rb.linearVelocity = direction * moveSpeed;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            EnemySpawner.onEnemyDestroy.Invoke();
            Destroy(gameObject);
        }
    }
}

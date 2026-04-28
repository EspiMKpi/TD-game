using UnityEngine;

public class Bullet : MonoBehaviour
{
   [Header("References")]
   [SerializeField] private Rigidbody2D rb;

   [Header("Attribute")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private int bulletDamage = 1;

   private Transform target;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        Destroy(gameObject, bulletLifetime);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;

        rb.linearVelocity = direction * bulletSpeed; 
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        TryHitTarget(other.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitTarget(other);
    }

    private void TryHitTarget(Collider2D other)
    {
        if (!IsTargetCollider(other))
        {
            return;
        }

        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }

    private bool IsTargetCollider(Collider2D other)
    {
        if (!target || other == null)
        {
            return false;
        }

        if (other.transform == target)
        {
            return true;
        }

        if (other.attachedRigidbody != null && other.attachedRigidbody.transform == target)
        {
            return true;
        }

        return other.transform.root == target;
    }

}

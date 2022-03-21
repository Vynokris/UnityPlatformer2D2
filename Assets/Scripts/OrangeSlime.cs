using UnityEngine;

public class OrangeSlime : MonoBehaviour
{
    [SerializeField] private float     walkSpeed;
    [SerializeField] private float     detectionRadius;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask collisionLayer;

    private float    walkDir = 0;
    private Vector3  originPosition;
    private Cooldown chaseCooldown = new Cooldown(2f);

    private Rigidbody2D       rigidBody;
    private CapsuleCollider2D capsule;
    private Animator          animator;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        capsule   = GetComponent<CapsuleCollider2D>();
        animator  = GetComponent<Animator>();
        originPosition = transform.position;
        chaseCooldown.Counter = 0;
    }

    void Update()
    {
        Walk();
    }


    void Walk()
    {
        chaseCooldown.Update(Time.deltaTime);

        if (!IsWallFwd() && IsGroundFwd()) 
        {
            // Chase the player.
            if (chaseCooldown.HasEnded() &&
                Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius)
            {
                walkDir = (playerTransform.position - transform.position).normalized.x;
                rigidBody.velocity = new Vector2(walkDir * walkSpeed, rigidBody.velocity.y);
                transform.localScale = new Vector2(rigidBody.velocity.normalized.x, 1);
                animator.SetBool("Chasing", true);
            }

            // Go back to the origin point.
            else if (Vector2.Distance(originPosition, transform.position) >= 0.5f)
            {
                walkDir = (originPosition - transform.position).normalized.x;
                rigidBody.velocity = new Vector2(walkDir * walkSpeed, rigidBody.velocity.y);
                transform.localScale = new Vector2(rigidBody.velocity.normalized.x, 1);
                animator.SetBool("Chasing", false);
            }

            // Don't move.
            else
            {
                walkDir = 0;
                rigidBody.velocity = new Vector2(0, 0);
                animator.SetBool("Chasing", false);
            }
        }
    }

    /// <summary>
    /// Returns true if a wall is in front of the slime.
    /// </summary>
    bool IsWallFwd()
    {
        return Physics2D.CapsuleCast(capsule.bounds.center + new Vector3(0, 0.1f, 0), capsule.bounds.size, capsule.direction, 0, new Vector2(walkDir, 0), 0.1f, collisionLayer).collider != null;
    }

    /// <summary>
    /// Returns true if there is ground in front of the slime.
    /// </summary>
    bool IsGroundFwd()
    {
        return Physics2D.CapsuleCast(capsule.bounds.center, capsule.bounds.size, capsule.direction, 0, new Vector2(walkDir * 5, -1), 1f, collisionLayer).collider != null;
    }
    

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
            Physics2D.IgnoreCollision(capsule, other.gameObject.GetComponent<CapsuleCollider2D>());
    }

    /// <summary>
    /// Checks collisions with the player and destroys itself accordingly.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
            if (other.gameObject.GetComponent<Rigidbody2D>().velocity.y < -1f)
                Destroy(this.gameObject);
    }
}
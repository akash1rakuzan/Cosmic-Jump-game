using UnityEngine;

public class Ring : MonoBehaviour
{

    private Transform player;
    public GameObject[] childRings;
    float radius = 10f;
    float force = 700f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    private void Update()
    {
        if (transform.position.y > player.position.y + 0.5)
        {
            var ball = player.GetComponent<Ball>();
            ball.RegisterRingPassed();
            //ball.NotifyRingCleared();       // <-- add this
            DestroyRing();
        }
    }
    private System.Collections.IEnumerator DisablePhysicsAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

        }
    }

    public void DestroyRing()
    {
        GameManager.noOfPassingRings++;
        FindFirstObjectByType<AudioManager>().Play("Whoosh");

        for (int i = 0; i < childRings.Length; i++)
        {
            Rigidbody rb = childRings[i].GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            //StartCoroutine(DisablePhysicsAfterDelay(rb, 0.3f));

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider newCollider in colliders)
            {
                // Skip applying force to the ball
                if (newCollider.CompareTag("Player")) continue;

                Rigidbody colRb = newCollider.GetComponent<Rigidbody>();

                if (colRb != null)
                {
                    colRb.AddExplosionForce(force, transform.position, radius);
                }
            }

            childRings[i].GetComponent<MeshCollider>().enabled = false;
            childRings[i].transform.parent = null;
            Destroy(childRings[i].gameObject, 1f);
        }

        this.enabled = false;
        Destroy(this.gameObject, 1.2f);

    }
}

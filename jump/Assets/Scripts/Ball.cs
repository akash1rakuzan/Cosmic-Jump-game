using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Ball : MonoBehaviour
{
    public GameObject splitPrefab;
    public float bounceForce = 400f;
    public float bounceForceSmash = 250f;

    private Rigidbody rb;
    private AudioManager audioManager;

    private int consecutiveFalls = 0;
    private bool smashMode = false;

    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;

    private Color originalColor;
    public Color yellow = Color.yellow;
    public Color red = Color.red;

    private Color currentTargetColor;

    private bool tripleSmashActive = false;
    private int remainingTripleSmash = 0;

    private bool shieldActive = false;
    private float shieldTimer = 0f;
    public float shieldDuration = 5f; // seconds
    public GameObject shieldVisual;   // Optional: assign a visual effect in the inspector

    private HashSet<Ring> smashedRings = new HashSet<Ring>();

    [Header("Velocity Clamp")]
    public float maxSpeed = 8f;           // Maximum allowed speed
    [Range(0f, 1f)]
    public float clampSmoothness = 0.1f;   // 0 = instant stop, 1 = no clamp


    private bool collisionHandled = false;
    private void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        rb = GetComponent<Rigidbody>();

        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
            currentTargetColor = originalColor;
        }

        if (trailRenderer != null)
        {
            trailRenderer.startColor = originalColor;
            trailRenderer.endColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade to transparent
        }
    }

    private void Update()
    {
        if (meshRenderer != null)
        {
            Color lerped = Color.Lerp(meshRenderer.material.color, currentTargetColor, Time.deltaTime * 50f);
            meshRenderer.material.color = lerped;

            if (trailRenderer != null)
            {
                trailRenderer.startColor = lerped;
                trailRenderer.endColor = new Color(lerped.r, lerped.g, lerped.b, 0f);
            }
        }

        if (shieldActive)
        {
            shieldTimer -= Time.deltaTime;

            // Start pulsing in last 2 seconds
            if (shieldTimer <= 1.7f && shieldVisual)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.2f;
                shieldVisual.transform.localScale = new Vector3(pulse, pulse, pulse);
            }
            else if (shieldVisual)
            {
                // Keep steady size
                shieldVisual.transform.localScale = Vector3.one * 1f;
            }

            if (shieldTimer <= 0f)
            {
                shieldActive = false;
                if (shieldVisual) shieldVisual.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (collisionHandled || GameManager.gameOver || GameManager.levelWin)
            return;


        // Mark as handled to avoid double-processing
        collisionHandled = true;

        // Reset this flag after physics step so next real collision is processed
        Invoke(nameof(ResetCollisionHandled), 0.05f);

        ContactPoint contact = other.contacts[0];
        Transform otherTransform = other.transform;

        MeshRenderer mesh = otherTransform.GetComponent<MeshRenderer>();
        if (mesh == null) return;

        string materialName = mesh.material.name;

        float ballY = transform.position.y;
        float ringY = otherTransform.position.y;

        bool passedRing = ballY > ringY;

        bool destroyedRing = false;

        if (tripleSmashActive)
        {

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -8f, rb.linearVelocity.z);
        }
        else
        {
            if (passedRing)
            {
                float force = smashMode ? bounceForceSmash : bounceForce;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, force * Time.deltaTime, rb.linearVelocity.z);
            }
            else
            {
                // Slight push to avoid sticking
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -8f, rb.linearVelocity.z);
            }
        }



        audioManager.Play("Land");

        GameObject newsplit = Instantiate(splitPrefab,
            new Vector3(transform.position.x, otherTransform.position.y + 0.1f, transform.position.z),
            transform.rotation);
        newsplit.transform.localScale = Vector3.one * Random.Range(0.7f, 1.1f);
        newsplit.transform.parent = otherTransform;

        MeshRenderer splitRenderer = newsplit.GetComponent<MeshRenderer>();
        if (splitRenderer != null && meshRenderer != null)
        {
            // Clone material to avoid changing the shared one (optional but safe)
            splitRenderer.material = new Material(splitRenderer.material);

            // Copy RGB from ball, keep alpha from original split material
            Color splitColor = splitRenderer.material.color;
            Color ballColor = meshRenderer.material.color;
            splitColor.r = ballColor.r;
            splitColor.g = ballColor.g;
            splitColor.b = ballColor.b;
            // leave splitColor.a unchanged (to preserve opaque rendering)
            splitRenderer.material.color = splitColor;
        }
        // -------------------------
        //  IMMUNITY CHECK FIRST
        // -------------------------
        if ((smashMode || tripleSmashActive) && !otherTransform.CompareTag("LastRing"))
        {
            Ring ring = otherTransform.GetComponentInParent<Ring>();
            if (ring != null && !smashedRings.Contains(ring))
            {
                smashedRings.Add(ring);
                ring.DestroyRing();
                destroyedRing = true;
            }
            else if (otherTransform.CompareTag("UnSafe") || otherTransform.CompareTag("Safe"))
            {
                // Fallback: just destroy collided object
                Destroy(otherTransform.gameObject);
                destroyedRing = true;
            }

                // Update triple smash or smash mode
            if (tripleSmashActive)
            {
                    remainingTripleSmash--;
                    if (remainingTripleSmash <= 0)
                        StartCoroutine(EndTripleSmashAfterFrame());
            }
            else
            {
                    smashMode = false;
            }

            UpdateBallColor();
            consecutiveFalls = 0;
            return; // Immune, no death
        }

        // -------------------------
        //  NORMAL COLLISION LOGIC
        // -------------------------
        if (destroyedRing) return;

        if (otherTransform.CompareTag("Safe"))
        {
            consecutiveFalls = 0;
            UpdateBallColor();
        }
        else if (otherTransform.CompareTag("UnSafe"))
        {
            if (shieldActive || tripleSmashActive || smashMode) // <--- Added tripleSmashActive here
            {
                consecutiveFalls = 0; // Reset because we actually landed
                UpdateBallColor();    // Update color accordingly
                return;               // Don't die, but reset combo
            }

            GameManager.gameOver = true;
            Debug.Log($"GameOver set to true by ball.cs");
            audioManager.Play("GameOver");
        }
        else if (otherTransform.CompareTag("LastRing") && !GameManager.levelWin)
        {
            GameManager.levelWin = true;
            audioManager.Play("LevelWin");
        }
    }

    public void RegisterRingPassed()
    {
        consecutiveFalls++;

        if (consecutiveFalls >= 3 && !smashMode)
        {
            smashMode = true;
        }

        UpdateBallColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) // Or your chosen tag
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -8f, rb.linearVelocity.z);
            Destroy(other.gameObject);
            ActivateTripleSmash();
        }
        else if (other.CompareTag("Shield"))
        {
            ActivateShield();
            Destroy(other.gameObject);
        }
    }

    private void ActivateTripleSmash()
    {
        tripleSmashActive = true;
        remainingTripleSmash = 3;
        UpdateBallColor();
    }

    private void UpdateBallColor()
    {
        if (tripleSmashActive || smashMode)
        {
            currentTargetColor = red;
        }
        else if (consecutiveFalls == 1)
        {
            currentTargetColor = yellow;
        }
        else if (consecutiveFalls == 2)
        {
            currentTargetColor = Color.Lerp(yellow, red, 0.5f);
        }
        else
        {
            currentTargetColor = originalColor;
        }
    }

    private void ActivateShield()
    {
        shieldActive = true;
        shieldTimer = shieldDuration;
        if (shieldVisual) shieldVisual.SetActive(true);
    }

    private IEnumerator EndTripleSmashAfterFrame()
    {
        yield return null; // wait one frame
        tripleSmashActive = false;
        smashedRings.Clear();
    }

    private void FixedUpdate()
    {

        if (rb == null) return;
        Vector3 v = rb.linearVelocity; // or rb.linearVelocity if your project uses that API
        float speed = v.magnitude;
        if (speed > maxSpeed)
        {

            Vector3 clampedVel = v.normalized * maxSpeed;
            v = Vector3.Lerp(v, clampedVel, clampSmoothness);
            rb.linearVelocity = v;
        }
    }


    private void ResetCollisionHandled()
    {
        collisionHandled = false;
    }
}



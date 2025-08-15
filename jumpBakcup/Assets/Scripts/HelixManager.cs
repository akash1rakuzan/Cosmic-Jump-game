using UnityEngine;
using UnityEngine.UIElements;

public class HelixManager : MonoBehaviour
{
    public GameObject[] rings;

    public int noOfrings;
    public float ringDistance = 5f;

    float yPos;
    public GameObject shieldPrefab;
    public GameObject arrowPrefab;

    public Transform player; // Drag player into this in Inspector
    private float spawnThreshold = 30f; // Distance below player to trigger new ring spawn
    public float arrowoffset = 0;
    private bool levelStarted = false;
    public void StartLevel()
    {
        if (GameManager.isInfiniteMode)
        {
            SpawnInfinite();
        }
        else
        {
            noOfrings = GameManager.CurrentLevelIndex + 5;

            for (int i = 0; i < noOfrings; i++)
            {
                if (i == 0)
                    SpawnRings(0, false); // First ring: no arrow

                else
                    SpawnRings(Random.Range(1, rings.Length - 1)); // Default = true
            }

            SpawnRings(rings.Length - 1); // Only for level-based mode
        }
    }
    void SpawnRings(int index, bool spawnArrow = true)
    {
        Vector3 ringPosition = new Vector3(transform.position.x, yPos, transform.position.z);
        // Apply a random Y-axis rotation

        Quaternion rotation = (index == 0)
         ? Quaternion.identity
         : Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject newRing = Instantiate(rings[index], ringPosition, rotation);
        newRing.transform.parent = transform;
        bool isLastRing = index == rings.Length - 1;

        if (spawnArrow && !isLastRing && Random.value < 0.25f) 
        {
            int choice = Random.Range(0, 2);

            if (choice == 1)// 20% chance
            {
                float radius = 2.1f; // Distance from center of ring
                float angle = Random.Range(0f, 360f);

                // Calculate offset around the ring
                Vector3 offset = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

                // Arrow position directly relative to the current ring
                Vector3 arrowPosition = ringPosition + Vector3.up * arrowoffset + offset;

                GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.Euler(0f, angle + 90f, 0f));
                arrow.transform.parent = newRing.transform;
            }
            else
            {
                float radius = 2.1f;
                float angle = Random.Range(0f, 360f);

                Vector3 offset = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                Vector3 shieldPosition = ringPosition + Vector3.up * arrowoffset + offset;

                GameObject shield = Instantiate(shieldPrefab, shieldPosition, Quaternion.Euler(-90f, angle + 90f, 0f));
                shield.transform.parent = newRing.transform;
            }
        }
        
        yPos -= ringDistance;
    }


    private void Update()
    {
        if (!levelStarted && !GameManager.gameJustStarted)
        {
            levelStarted = true;
            StartLevel();
        }

        if (GameManager.isInfiniteMode)
        {
            if (player.position.y - yPos < spawnThreshold)
            {
                SpawnRings(Random.Range(1, rings.Length - 1));
            }
        }
    }

    public void StartInfiniteMode()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        yPos = 0;
        SpawnInfinite();

    }

    void SpawnInfinite()
    {
        // Infinite mode: only spawn normal rings, never the last ring
        SpawnRings(0, false); // First ring: no arrow
        for (int i = 1; i < 15; i++)
        {
            SpawnRings(Random.Range(1, rings.Length - 1));
        }
    }



}
using UnityEngine;



public class ArrowAutoDestroy : MonoBehaviour
{
    public float destroyDistance = 25f; // Distance from player before destroying
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}

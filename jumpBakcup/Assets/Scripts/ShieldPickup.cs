using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime); // Simple rotation
    }
}
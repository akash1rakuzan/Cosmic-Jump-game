using UnityEngine;

public class ArrowFollowRingRotation : MonoBehaviour
{
    public Transform ringToFollow;

    void Update()
    {
        if (ringToFollow != null)
        {
            transform.rotation = ringToFollow.rotation;
        }
    }
}
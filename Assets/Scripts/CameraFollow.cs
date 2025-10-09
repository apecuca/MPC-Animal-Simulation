using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        if (followTarget == null)
            return;
        
        transform.position = followTarget.position + offset;
    }
}

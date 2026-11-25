using UnityEngine;

public class S_SNowFollower : MonoBehaviour
{
    public Transform player;
    public float heightOffset = 10f;

    private float fixedY;

    void Start()
    {
        fixedY = transform.position.y;
    }
    void LateUpdate()
    {
        transform.position = new Vector3(player.position.x, fixedY, player.position.z);
    }
}

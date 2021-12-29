using UnityEngine;

public class StaticNPCBullet : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 2);
    }
}

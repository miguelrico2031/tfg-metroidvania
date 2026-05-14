using UnityEngine;

public class WaterCamera : MonoBehaviour
{
    [SerializeField] private Transform m_MainCamera;

    private void LateUpdate()
    {
        transform.position = new(m_MainCamera.position.x, transform.position.y, transform.position.z);
    }

}

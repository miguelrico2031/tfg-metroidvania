using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float m_ParallaxFactor;
    [SerializeField] private PixelPerfectCamera m_PixelPerfectCamera;

    private Vector3 m_LastCameraPosition;

    void Start()
    {
        m_LastCameraPosition = m_PixelPerfectCamera.RoundToPixel(m_PixelPerfectCamera.transform.position);
    }

    void LateUpdate()
    {
        Vector3 camPos = m_PixelPerfectCamera.RoundToPixel(m_PixelPerfectCamera.transform.position);
        Vector3 delta = camPos - m_LastCameraPosition;
        delta.z = 0f;
        transform.position += delta * m_ParallaxFactor;
        m_LastCameraPosition = camPos;
    }
}
using UnityEngine;
using TMPro;

public class TitleIdleAnimator : MonoBehaviour
{
    [Header("Idle Animation Settings")]
    public float floatSpeed = 1f;        // How fast the text moves up and down
    public float floatAmount = 10f;      // How far it moves (in pixels)
    public float pulseSpeed = 1.5f;      // Speed of the glow/pulse
    public float pulseIntensity = 0.15f; // How strong the pulse is

    private TextMeshProUGUI textComponent;
    private Vector3 startPos;
    private Color baseColor;

    private void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        startPos = transform.localPosition;
        baseColor = textComponent.color;
    }

    private void Update()
    {
        // Floating motion (gentle up and down)
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);

        // Subtle pulsing of alpha (brightness)
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1) * 0.5f; // 0–1
        Color newColor = baseColor;
        newColor.a = Mathf.Lerp(baseColor.a - pulseIntensity, baseColor.a + pulseIntensity, pulse);
        textComponent.color = newColor;
    }
}

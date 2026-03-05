using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollRectZoom : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 3.0f;

    private RectTransform contentRect;

    void Awake()
    {
        contentRect = GetComponent<ScrollRect>().content;
    }

    void Update()
    {
        // 1. Zoom com Mouse (Scroll Wheel)
        if (Mouse.current != null)
        {
            Vector2 scrollValue = Mouse.current.scroll.ReadValue();
            if (scrollValue.y != 0)
            {
                Debug.Log("Scroll Value: " + scrollValue.y);
                ApplyZoom(scrollValue.y * 0.001f * zoomSpeed * 100);
            }
        }

        // 2. Zoom Mobile (Pinch Zoom) corrigido
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;

            // No New Input System, usamos .Count com C maiúsculo
            if (touches.Count == 2)
            {
                var t0 = touches[0];
                var t1 = touches[1];

                // Verificamos se ambos estão sendo pressionados
                if (t0.press.isPressed && t1.press.isPressed)
                {
                    Vector2 pos0 = t0.position.ReadValue();
                    Vector2 pos1 = t1.position.ReadValue();

                    Vector2 delta0 = t0.delta.ReadValue();
                    Vector2 delta1 = t1.delta.ReadValue();

                    Vector2 prevPos0 = pos0 - delta0;
                    Vector2 prevPos1 = pos1 - delta1;

                    float prevMag = (prevPos0 - prevPos1).magnitude;
                    float currentMag = (pos0 - pos1).magnitude;

                    float diff = currentMag - prevMag;
                    ApplyZoom(diff * zoomSpeed);
                }
            }
        }
    }

    void ApplyZoom(float increment)
    {
        Vector3 newScale = contentRect.localScale + Vector3.one * increment;
        newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
        contentRect.localScale = newScale;

        // Garante que o Scroll Rect atualize os limites de arraste
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }
}
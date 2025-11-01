using UnityEngine;
using UnityEngine.UI;
//A

[RequireComponent(typeof(Image))]
public class PieSlice : MonoBehaviour
{
    private Image image;
    private Material material;

    void Awake()
    {
        image = GetComponent<Image>();
        material = new Material(Shader.Find("UI/Default"));
        image.material = material;
    }

    public void DrawSlice(float startAngle, float angle, float radius, Color color)
    {
        image.color = color;

        // Use a pie slice sprite or create procedurally
        // For simplicity, we'll use Unity's filled image
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = (int)Image.Origin360.Top;

        // This is a simplified approach - for proper pie slices, you'd need multiple images
        // or a custom shader. Here's a basic implementation:
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(radius * 2, radius * 2);

        // Rotate to start angle
        rt.localRotation = Quaternion.Euler(0, 0, -startAngle);

        // Set fill amount
        image.fillAmount = angle / 360f;
    }
}
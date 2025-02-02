using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private float maxWidth = 100;

    void Start() {
        var transform = this.GetComponent<RectTransform>();
        maxWidth = transform.rect.width;

        transform.sizeDelta = new Vector2(0, transform.rect.height);
    }

    public void UpdateHealthBar(float healthValue) {
        var transform = this.GetComponent<RectTransform>();
        float newWidth = healthValue / 100 * maxWidth;
        transform.sizeDelta = new Vector2(newWidth, transform.rect.height);
    }

}
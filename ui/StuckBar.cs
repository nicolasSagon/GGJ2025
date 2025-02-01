using UnityEngine;

public class StuckBar : MonoBehaviour
{
    private float maxWidth = 100;

    void Start() {
        var transform = this.GetComponent<RectTransform>();
        maxWidth = transform.rect.width;

        transform.sizeDelta = new Vector2(0, transform.rect.height);
    }

    public void UpdateStuckBar(float stuckValue) {
        var transform = this.GetComponent<RectTransform>();
        float newWidth = (stuckValue / 100) * maxWidth;
        transform.sizeDelta = new Vector2(newWidth, transform.rect.height);
    }

}
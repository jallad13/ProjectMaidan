using UnityEngine;
using UnityEngine.UI;

namespace ProjectMaidan.Units
{
    public sealed class UnitHealthBar : MonoBehaviour
    {
        private Image _fillImage;
        private Camera _camera;

        public float FillAmount => _fillImage != null ? _fillImage.fillAmount : 0f;
        public Color FillColor => _fillImage != null ? _fillImage.color : Color.clear;

        public static UnitHealthBar Create(Transform owner)
        {
            var root = new GameObject("HealthBar", typeof(RectTransform), typeof(Canvas));
            root.transform.SetParent(owner, false);
            root.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            root.transform.localScale = Vector3.one * 0.01f;

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(120f, 14f);

            var background = CreateImage("Background", root.transform, new Color(0.06f, 0.06f, 0.06f, 0.9f));
            Stretch(background.rectTransform, 0f, 1f);

            var fill = CreateImage("Fill", root.transform, Color.green);
            Stretch(fill.rectTransform, 0.04f, 0.96f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;

            var healthBar = root.AddComponent<UnitHealthBar>();
            healthBar._fillImage = fill;
            healthBar.SetHealth(1f);
            return healthBar;
        }

        public void SetHealth(float normalizedHealth)
        {
            if (_fillImage == null)
            {
                return;
            }

            float health = Mathf.Clamp01(normalizedHealth);
            _fillImage.fillAmount = health;
            _fillImage.color = health > 0.6f
                ? Color.green
                : health > 0.3f
                    ? Color.yellow
                    : Color.red;
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            var child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            var image = child.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect, float minX, float maxX)
        {
            rect.anchorMin = new Vector2(minX, 0.12f);
            rect.anchorMax = new Vector2(maxX, 0.88f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

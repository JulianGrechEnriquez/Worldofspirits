using TMPro;
using UnityEngine;
using System;

namespace WorldOfSpirits.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageNumber : MonoBehaviour
    {
        private TextMeshPro textMesh;
        private float lifetime;
        private float remainingLifetime;
        private float riseSpeed;
        private Color startColor;
        private Action<DamageNumber> release;

        public void Initialize(
            float damage, Color color, float duration, float speed, int fontSize,
            Action<DamageNumber> releaseAction)
        {
            textMesh = GetComponent<TextMeshPro>();
            textMesh.text = Mathf.CeilToInt(damage).ToString();
            textMesh.color = color;
            textMesh.fontSize = fontSize;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.sortingOrder = 100;

            startColor = color;
            lifetime = Mathf.Max(0.1f, duration);
            remainingLifetime = lifetime;
            riseSpeed = speed;
            release = releaseAction;
        }

        private void Update()
        {
            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);
            remainingLifetime -= Time.deltaTime;

            float alpha = Mathf.Clamp01(remainingLifetime / lifetime);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            if (remainingLifetime <= 0f)
            {
                if (release != null)
                {
                    Action<DamageNumber> releaseAction = release;
                    release = null;
                    releaseAction(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBurst : MonoBehaviour
{
    [SerializeField] private Renderer rend = null;

    [SerializeField] [Range(0, 2)] private float burstScale = 1;
    [SerializeField] [Range(0, 2)] private float burstTime = 1;
    private float initialScale = 0;
    private float initialAlpha = 0;
    private float burstProgress = 0;

    private void Start()
    {
        initialScale = gameObject.transform.localScale.x;
        initialAlpha = rend.material.color.a;
    }

    private void Update()
    {
        burstProgress += Time.deltaTime / burstTime;

        float newScale = Mathf.Lerp(initialScale, burstScale, burstProgress);
        float newAlpha = Mathf.Lerp(initialAlpha, 0, burstProgress);

        gameObject.transform.localScale = Vector3.one * newScale;
        rend.material.color = new Color
        (
            rend.material.color.r,
            rend.material.color.g,
            rend.material.color.b,
            newAlpha
        );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Facade;

public class MobileShadowCaster : MonoBehaviour
{
    [SerializeField] Transform meshRoot;
    [SerializeField] float distanceOffset = 0;
    [SerializeField] float shadowOffset = 0.01f;
    [SerializeField] float maxDistance = 5;
    [SerializeField] LayerMask layerMask;
    [SerializeField] new SpriteRenderer renderer;

    [SerializeField] Vector2 shadowOpacity = new Vector2(0, 200);

    private void Update()
    {
        Ray ray = new Ray();
        ray.direction = Vector3.down;
        ray.origin = meshRoot.position;
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, maxDistance + distanceOffset, layerMask))
        {
            transform.position = hit.point + Vector3.up * shadowOffset;
            transform.forward = Vector3.up;
            float distance = hit.distance - distanceOffset;
            byte alpha = (byte)Mathf.Lerp(shadowOpacity.y, shadowOpacity.x, distance / maxDistance);
            Color newColor = new Color32(0, 0, 0, alpha);
            renderer.color = newColor;
        }
        renderer.enabled = hit.collider;
    }
}
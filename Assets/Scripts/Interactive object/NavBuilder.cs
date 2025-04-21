using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Meta.XR.MRUtilityKit;

public class NavBuilder : MonoBehaviour
{
    public NavMeshSurface surface;

    public float maxJumpHeight = 2.0f;
    public float maxJumpDistance = 5.0f;

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(1.0f); // 等 SDK 生成 Mesh
        surface.BuildNavMesh();
        yield return new WaitForSeconds(0.1f); // 等建完
        AutoLinkAnchors();
    }

    void AutoLinkAnchors()
    {
        List<MRUKAnchor> groundAnchors = new();
        List<MRUKAnchor> platformAnchors = new();

        foreach (var anchor in FindObjectsOfType<MRUKAnchor>())
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
                groundAnchors.Add(anchor);
            else
                platformAnchors.Add(anchor);
        }

        foreach (var ground in groundAnchors)
        {
            foreach (var platform in platformAnchors)
            {
                Vector3 start = FindClosestEdgeTowards(ground.gameObject, platform.gameObject);
                Vector3 end = FindClosestEdgeTowards(platform.gameObject, ground.gameObject);

                float heightDiff = Mathf.Abs(end.y - start.y);
                float distXZ = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));

                if (heightDiff <= maxJumpHeight && distXZ <= maxJumpDistance)
                {
                    CreateNavLink(start, end);
                }
            }
        }
    }

    Vector3 FindClosestEdgeTowards(GameObject from, GameObject target)
    {
        Vector3 dir = (target.transform.position - from.transform.position).normalized;

        // 如果有 Collider，用它的 bounds 算邊緣點
        Collider col = from.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Bounds bounds = col.bounds;
            Vector3 edge = bounds.center + Vector3.Scale(dir, bounds.extents);
            edge.y = bounds.center.y; // 保持原高度
            return edge;
        }

        // 沒有 Collider 就 fallback
        return from.transform.position;
    }

    void CreateNavLink(Vector3 start, Vector3 end)
    {
        GameObject linkObj = new GameObject("AutoNavLink");
        linkObj.transform.position = start;

        var link = linkObj.AddComponent<NavMeshLink>();
        link.startPoint = Vector3.zero;
        link.endPoint = end - start;
        link.width = 0.5f;
        link.bidirectional = true;

        Debug.DrawLine(start, end, Color.green, 10f);
        Debug.Log($"🟢 NavLink: {start} → {end}");
    }
}

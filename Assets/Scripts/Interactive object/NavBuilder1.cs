using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Meta.XR.MRUtilityKit;

public class NavBuilder1 : MonoBehaviour
{
    public NavMeshSurface surface;

    public float maxJumpHeight = 2.0f;
    public float maxJumpDistance = 5.0f;

    public GameObject cat;
    public bool isSetMap = false;
    public SceneNavigation sn;
    void Start()
    {
        isSetMap = false; StartCoroutine(DelayedInit());
        sn = GetComponent<SceneNavigation>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            BuildMap();
        }
    }

    public void BuildMap()
    {
        isSetMap = true;
        StartCoroutine(DelayedInit());

    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(3.0f); // 等 SDK 生成 Mesh
        //surface.BuildNavMesh();
        yield return new WaitForSeconds(1.0f); // 等建完
        AutoLinkAnchors();
        yield return new WaitForSeconds(1.0f); // 等建完
        var allCats = Resources.FindObjectsOfTypeAll<CatStateManager>();
        foreach (var cat in allCats)
        {
            if (cat.CompareTag("cat"))
            {
                cat.gameObject.SetActive(true);
                cat.transform.position = new Vector3(0, 0.5f, 0);
                sn.Agents.Add(cat.GetComponent<NavMeshAgent>());

            }
        }
        sn.BuildSceneNavMesh();

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
        // 計算目標物體的方向
        Vector3 dir = (target.transform.position - from.transform.position).normalized;

        // 查找錨點的所有子物件中的 Collider
        Collider[] colliders = from.GetComponentsInChildren<Collider>();

        // 假設錨點有子物件，並且這些子物件有 Collider
        foreach (var col in colliders)
        {
            if (col != null)
            {
                // 計算該子物件的 Bounds
                Bounds bounds = col.bounds;

                // 計算平台的上邊緣位置 (y 軸為上邊界)
                Vector3 edge = bounds.center + Vector3.Scale(dir, bounds.extents);

                // 將 y 值設為平台的上邊緣
                edge.y = bounds.max.y;  // 設置為上邊界

                // 避免穿越 Collider: 檢查射線是否會與 Collider 相交
                RaycastHit hit;
                Vector3 checkStart = edge + dir * 0.1f;  // 將起點微調一點
                if (Physics.Raycast(checkStart, dir, out hit, 0.2f)) // 如果射線會碰到物體
                {
                    // 射線檢查後微調位置，將邊緣推到稍微遠離的地方
                    edge = hit.point + dir * 0.2f; // 將邊緣點推離一點，避免碰撞
                }

                return edge;
            }
        }

        // 如果沒有找到 Collider，返回錨點的 position
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class AutoNavMeshLinker : MonoBehaviour
{
    // public float maxLinkDistance = 5f;
    // public float maxHeightDifference = 2f;

    // void Start()
    // {
    //     // 找出有 "Platform" tag 的物件，或你可以用 FindObjectsOfType<Collider>() 自動抓
    //     GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
    //     List<Collider> platformColliders = new List<Collider>();
    //     Debug.Log("Found " + platforms.Length + " platforms.");
    //     foreach (var go in platforms)
    //     {
    //         Collider col = go.GetComponent<Collider>();
    //         if (col != null)
    //             platformColliders.Add(col);
    //     }

    //     for (int i = 0; i < platformColliders.Count; i++)
    //     {
    //         for (int j = i + 1; j < platformColliders.Count; j++)
    //         {
    //             Collider a = platformColliders[i];
    //             Collider b = platformColliders[j];

    //             Vector3 closestA = a.ClosestPointOnBounds(b.bounds.center);
    //             Vector3 closestB = b.ClosestPointOnBounds(a.bounds.center);
    //             // move closestA to a.center about 0.35f
    //             Vector3 offsetA = a.bounds.center - closestA;
    //             offsetA.Normalize();
    //             closestA += offsetA * 0.35f;
    //             // move closestB to b.center about 0.35f
    //             Vector3 offsetB = b.bounds.center - closestB;
    //             offsetB.Normalize();
    //             closestB += offsetB * 0.35f;
    //             Debug.Log("Closest A: " + closestA + ", Closest B: " + closestB);
    //             Vector3 offset = closestB - closestA;
    //             float horizontalDist = new Vector2(offset.x, offset.z).magnitude;
    //             float verticalDist = Mathf.Abs(offset.y);

    //             float widthA, widthB;
    //             if(offsetA.x != 0){
    //                 widthA = a.bounds.size.z;
    //             }else{
    //                 widthA = a.bounds.size.x;
    //             }
    //             if(offsetB.x != 0){
    //                 widthB = b.bounds.size.z;
    //             }else{
    //                 widthB = b.bounds.size.x;
    //             }
    //             float linkWidth = Mathf.Min(widthA, widthB) - 0.6f;
    //             if (horizontalDist <= maxLinkDistance && verticalDist <= maxHeightDifference)
    //             {
    //                 CreateLink(closestA, closestB,linkWidth);
    //             }else{
    //                 Debug.Log("Link not created between " + a.name + " and " + b.name + " due to distance constraints.");
    //                 Debug.Log("Horizontal Distance: " + horizontalDist + ", Vertical Distance: " + verticalDist);
    //             }
    //         }
    //     }
    // }

    // void CreateLink(Vector3 startPos, Vector3 endPos, float linkWidth)
    // {
    //     GameObject linkObj = new GameObject("AutoNavMeshLink");
    //     NavMeshLink link = linkObj.AddComponent<NavMeshLink>();
    //     link.startPoint = link.transform.InverseTransformPoint(startPos);
    //     link.endPoint = link.transform.InverseTransformPoint(endPos);
    //     link.width = linkWidth;
    //     link.bidirectional = true;

    //     linkObj.transform.position = (startPos + endPos) / 2f;
    // }
}
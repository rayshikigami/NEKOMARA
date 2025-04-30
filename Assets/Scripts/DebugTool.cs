using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DebugTool : MonoBehaviour
{
    // Start is called before the first frame update
    public NavMeshAgent navAgent;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (navAgent.hasPath)
        {
            Vector3 destination = navAgent.destination;
            transform.position = destination;
        }
    }
}

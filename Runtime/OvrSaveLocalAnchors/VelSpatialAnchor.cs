using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelSpatialAnchor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDestroy()
    {
        throw new NotImplementedException();
    }

    public void CreateSpatialAnchor()
    {
        StartCoroutine(CreateSpatialAnchorCo());
    }
    
    private IEnumerator CreateSpatialAnchorCo()
    {
        OVRSpatialAnchor anchor = gameObject.AddComponent<OVRSpatialAnchor>();

        // Wait for the async creation
        yield return new WaitUntil(() => anchor.Created);

        Debug.Log($"Created anchor {anchor.Uuid}");
    }
}

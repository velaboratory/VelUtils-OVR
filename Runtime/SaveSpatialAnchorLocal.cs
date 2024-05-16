using System.Collections;
using UnityEngine;
using VelUtils;
using VelUtils.Ar;

public class SaveSpatialAnchorLocal : MonoBehaviour
{
    public ArAlignment arAlignment;
    private string[] anchorGuids = new string[2];
    private OVRSpatialAnchor[] anchors = new OVRSpatialAnchor[2];

    private void Start()
    {
        // SceneManager.sceneLoaded += (_, _) => { StartCoroutine(LoadCo()); };
        arAlignment.OnManualAlignmentComplete += HandleManualAlignmentComplete;
        Load();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Load();
        }
    }

    private void HandleManualAlignmentComplete(Vector3 point1, Vector3 point2)
    {
        // Convert the points to local rig space
        Vector3 p1 = arAlignment.cameraRig.InverseTransformPoint(point1);
        Vector3 p2 = arAlignment.cameraRig.InverseTransformPoint(point2);

        PlayerPrefsJson.SetVector3("ArAlignmentSpatialAnchor1", p1);
        PlayerPrefsJson.SetVector3("ArAlignmentSpatialAnchor2", p2);
        arAlignment.Log("Saved alignment points to playerprefs. " + point1 + " " + point2);
    }
    
    private IEnumerator HandleManualAlignmentCompleteCo(Vector3 point1, Vector3 point2)
    {
        if (anchorGuids[0] != null)
        {
            anchorGuids[0] = null;
            anchors[0] = null;
        }
        yield return null;
        HandleManualAlignmentComplete(point1, point2);
    }

    public void Load()
    {
        if (arAlignment != null && FindObjectOfType<ArAlignmentOrigin>())
        {
            string localPoint1Guid = PlayerPrefsJson.GetString("ArAlignmentSpatialAnchor1", "null");
            string localPoint2Guid = PlayerPrefsJson.GetString("ArAlignmentSpatialAnchor2", "null");

            // Vector3 point1 = arAlignment.cameraRig.TransformPoint(localPoint1);
            // Vector3 point2 = arAlignment.cameraRig.TransformPoint(localPoint2);
            //
            // if (localPoint1 != Vector3.zero)
            // {
            //     arAlignment.SpawnPoint(0, point1);
            //     arAlignment.SpawnPoint(1, point2);
            //     arAlignment.Align(point1, point2, false);
            //     arAlignment.Log("Loaded alignment points from playerprefs. " + point1 + " " + point2);
            // }
        }
    }
}

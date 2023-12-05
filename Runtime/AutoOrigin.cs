using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VelNet;

public class AutoOrigin : SyncState
{
    public Transform rig;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private string uuid;
    private List<ulong> requestedShares = new List<ulong>();
    
    protected override void ReceiveState(BinaryReader binaryReader)
    {
        var temp = binaryReader.ReadString();
        if (temp != uuid)
        {
            //try to get it (it should have been shared to me)
            uuid = temp;
            StartCoroutine(findAnchor());


        }
        targetPosition = binaryReader.ReadVector3();
        targetRotation = binaryReader.ReadQuaternion();
    }

    protected override void SendState(BinaryWriter binaryWriter)
    {
        binaryWriter.Write(uuid);
        binaryWriter.Write(transform.position);
        binaryWriter.Write(transform.rotation);
    }

    IEnumerator findAnchor()
    {
        yield return StartCoroutine(eraseAnchor()); //erase any existing anchor

        OVRSpatialAnchor.LoadOptions options = new OVRSpatialAnchor.LoadOptions();
        options.StorageLocation = OVRSpace.StorageLocation.Cloud;
        options.Uuids = new System.Guid[] { System.Guid.Parse(uuid) };
        var t = OVRSpatialAnchor.LoadUnboundAnchorsAsync(options);
        yield return new WaitUntil(() => t.IsCompleted);

        var anchors = t.GetResult();
        if (anchors.Length > 0)
        {
            var t2 = anchors[0].LocalizeAsync();
            yield return new WaitUntil(() => t2.IsCompleted);
            if (t2.GetResult())
            {
                var pose = anchors[0].Pose;
                transform.position = pose.position;
                transform.rotation = pose.rotation;
                OVRSpatialAnchor anchor = gameObject.AddComponent<OVRSpatialAnchor>();
                anchors[0].BindTo(anchor);
            }
        }
    }

    IEnumerator eraseAnchor()
    {
        OVRSpatialAnchor anchor = this.GetComponent<OVRSpatialAnchor>();
        if (anchor != null)
        {
            anchor.enabled = false; //allows me to move it immediately without destroying it yet
            var t = anchor.EraseAsync();
            yield return new WaitUntil(() => t.IsCompleted);
            GameObject.Destroy(anchor);
            yield return null;
        }
        yield return null;
    }

    IEnumerator createAnchor()
    {
        requestedShares.Clear();
        yield return StartCoroutine(eraseAnchor());

        OVRSpatialAnchor anchor = gameObject.AddComponent<OVRSpatialAnchor>();
        while (!anchor.Created && !anchor.Localized)
        {
            yield return null;

        }

        var t2 = anchor.SaveAsync();
        yield return new WaitUntil(() => t2.IsCompleted);
        OVRSpatialAnchor.SaveOptions so = new OVRSpatialAnchor.SaveOptions();
        so.Storage = OVRSpace.StorageLocation.Cloud;
        var t3 = anchor.SaveAsync(so);

        yield return new WaitUntil(() => t3.IsCompleted); //save to cloud!

        if (networkObject.IsMine)
        {
            uuid = anchor.Uuid.ToString();
            PlayerPrefs.SetString("uuid", uuid);


        }

        yield return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        VelNetManager.OnJoinedRoom += Originate;
    }

    void Originate(string room)
    {
        if (networkObject.IsMine)
        {
            createAnchor();
        }
    }
    // Update is called once per frame
    void Update()
    {
        var anchor = GetComponent<OVRSpatialAnchor>();
        if (anchor != null && !networkObject.IsMine && anchor.enabled && anchor.Localized) //everyone else needs to move their rigs to compensate.  The owner doesn't
        {
            Vector3 anchorPosition = anchor.transform.position;
            Quaternion anchorRotation = anchor.transform.rotation;
            Vector3 pOffset = targetPosition - anchorPosition;
            Quaternion rOffset = targetRotation * Quaternion.Inverse(anchorRotation);


            rig.rotation = rOffset * rig.rotation;
            Vector3 rigOffset = (rOffset * pOffset);
            rig.position += rigOffset;
        }





        MetaIDSync[] playerSyncIDs = FindObjectsByType<MetaIDSync>(FindObjectsSortMode.None);
        foreach (var p in playerSyncIDs)
        {
            if (p.networkObject.IsMine || !GetComponent<NetworkObject>().IsMine)
            {
                continue;
            }
            if (!requestedShares.Contains(p.MetaID) && anchor != null && networkObject.IsMine && anchor.enabled && anchor.Localized)
            {
                OVRSpaceUser user = new OVRSpaceUser(p.MetaID);
                GetComponent<OVRSpatialAnchor>().ShareAsync(user);
                requestedShares.Add(p.MetaID);
            }
        }
    }
}

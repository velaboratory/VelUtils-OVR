using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VelUtils;
using VelUtils.Ar;

public class SaveSpatialAnchorLocal : MonoBehaviour
{
	public ArAlignment arAlignment;
	public float alignmentInterval = 1f;
	private float slowUpdateCounter;

	private void OnEnable()
	{
		arAlignment.OnManualAlignmentComplete += HandleManualAlignmentComplete;
		arAlignment.PointMoved += PointMoved;
	}

	private void OnDisable()
	{
		arAlignment.OnManualAlignmentComplete -= HandleManualAlignmentComplete;
		arAlignment.PointMoved -= PointMoved;
	}

	private void PointMoved(int index, GameObject pointObject)
	{
		OVRSpatialAnchor anchor = pointObject.GetComponent<OVRSpatialAnchor>();
		if (anchor && anchor.Created)
		{
			anchor.EraseAsync();
			Destroy(anchor);
			Debug.Log("Erasing anchor");
			PlayerPrefsJson.SetString($"arAlignmentSpatialAnchor_{SceneManager.GetActiveScene().name}_{index}", null);
		}
	}

	private void Start()
	{
		SceneManager.sceneLoaded += (_, _) => { Load(); };
	}

	// You need to make sure the anchor is ready to use before you save it.
	// Also, only save if specified
	private async void SetupAnchorAsync(int index, OVRSpatialAnchor anchor)
	{
		// Keep checking for a valid and localized anchor state
		while (!anchor.Created && !anchor.Localized)
		{
			await Task.Yield();
		}

		if (!await anchor.SaveAsync())
		{
			arAlignment.Log("Failed to save anchor");
			return;
		}

		PlayerPrefsJson.SetString($"arAlignmentSpatialAnchor_{SceneManager.GetActiveScene().name}_{index}", anchor.Uuid.ToString());
		arAlignment.Log("Saved spatial anchor");
	}

	private async void Load()
	{
		for (int i = 0; i < 2; i++)
		{
			string uuid = PlayerPrefsJson.GetString($"arAlignmentSpatialAnchor_{SceneManager.GetActiveScene().name}_{i}", null);
			if (uuid != null)
			{
				Guid[] uuids = new Guid[1];
				uuids[0] = new Guid(uuid);
				OVRSpatialAnchor.LoadOptions loadOptions = new OVRSpatialAnchor.LoadOptions
					{ Timeout = 0, StorageLocation = OVRSpace.StorageLocation.Local, Uuids = uuids };

				OVRSpatialAnchor.UnboundAnchor[] anchors = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(loadOptions);
				if (anchors == null || anchors.Length != 1)
				{
					arAlignment.Log("Failed to load unbound anchor");
					continue;
				}

				OVRSpatialAnchor.UnboundAnchor unboundAnchor = anchors[0];
				if (!(await unboundAnchor.LocalizeAsync()))
				{
					arAlignment.Log("Failed to localize anchor");
					return;
				}

				Pose pose = unboundAnchor.Pose;
				arAlignment.SpawnPoint(i, pose.position); // verify that the point exists in the world
				GameObject go = arAlignment.pointObjects[i];
				go.transform.position = pose.position; // shouldn't be necessary
				go.transform.rotation = pose.rotation;
				OVRSpatialAnchor anchor = go.GetComponent<OVRSpatialAnchor>();
				if (anchor == null)
				{
					anchor = go.AddComponent<OVRSpatialAnchor>();
				}
				else
				{
					await anchor.EraseAsync();
				}

				unboundAnchor.BindTo(anchor);
			}
			else
			{
				arAlignment.Log("Didn't load anchor " + i + " because it wasn't saved in playerprefs");
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Load();
		}

		// once every second
		if (slowUpdateCounter > alignmentInterval)
		{
			slowUpdateCounter = 0;
			arAlignment.TryAlign(false);
		}
		else
		{
			slowUpdateCounter += Time.deltaTime;
		}
	}

	private void HandleManualAlignmentComplete(Vector3 point1, Vector3 point2)
	{
		if (arAlignment.pointObjects[0] != null)
		{
			GameObject go1 = arAlignment.pointObjects[0];
			SetupAnchorAsync(0, go1.GetOrAddComponent<OVRSpatialAnchor>());
		}

		if (arAlignment.pointObjects[1] != null)
		{
			GameObject go2 = arAlignment.pointObjects[1];
			SetupAnchorAsync(1, go2.GetOrAddComponent<OVRSpatialAnchor>());
		}
	}
}
using UnityEngine;
using System.Collections;

public class BillboardPlane
	: MonoBehaviour
{
	public float BillboardBlendMax = 0.9f;
	public Transform[] Billboards;

	Transform _CameraTransform;
	Vector3[] _StoredUp;

	void Start ()
	{
		if (Camera.main != null)
		{
			// Cache the camera transform
			_CameraTransform = Camera.main.transform;

			// Store the Up vectors for all the billboards, that way we can have tilted billboards if we want!
			_StoredUp = new Vector3[Billboards.Length];
			for (int i = 0; i < Billboards.Length; ++i)
			{
				_StoredUp[i] = Billboards[i].up;
			}
		}
		else
		{
			Debug.LogError("No Main Camera in Scene");
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (_CameraTransform != null)
		{
			for (int i = 0; i < Billboards.Length; ++i)
			{
				var billboard = Billboards[i];

				// We want the flame's Y direction to align with the original up vector
				Vector3 up = _StoredUp[i];

				// The forward vector should point from the camera to the object, so that the quad faces the camera
				Vector3 properForward = (billboard.position - _CameraTransform.position).normalized;

				Vector3 cameraRight = _CameraTransform.right;
				Vector3 fakeForward = Vector3.Cross(cameraRight, up).normalized;

				// Assign that to the transform
				Quaternion properRotation = Quaternion.LookRotation(properForward, up);
				Quaternion fakeRotation = Quaternion.LookRotation(fakeForward, up);

				// Make it that when we look down on the billboard it doesn't go completely flat, that would look weird
				float blend = Mathf.Min(BillboardBlendMax, 2.0f * (Mathf.Max(0.5f, -Vector3.Dot(up, properForward)) - 0.5f));

				// Rotate the billboard
				billboard.rotation = Quaternion.Lerp(properRotation, fakeRotation, blend);
			}
		}
	}
}

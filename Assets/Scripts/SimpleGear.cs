using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGear : MonoBehaviour
{
	[SerializeField] private List<SimpleGear> meshedGears = new List<SimpleGear>();
	[SerializeField] private List<SimpleGear> stackedGears = new List<SimpleGear>();
	[SerializeField] private float turningSpeed = 1;
	//[SerializeField] private float magicNumber = 6;
	[SerializeField] private int toothCount = 20;
	[SerializeField] private bool powered;
	[SerializeField] private float offsetForAngle = 0;
	[SerializeField] private bool adjustmentMode;
	private Vector3 rotationAxis = new Vector3(0, 1, 0);


	private bool activatedLastFrame = true;


	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (powered)
		{
			TurnGear(turningSpeed, toothCount, false, transform.rotation.eulerAngles.y, transform.position);
		}
		if (adjustmentMode)
		{
			activatedLastFrame = true;
		}


	}

	public void TurnGear(float parentSpeed, int parentToothCount, bool stacked, float syncRot, Vector3 parentPos)
	{
		if (gameObject.activeSelf)
		{
			if (activatedLastFrame && !stacked)
			{
				//transform.LookAt(parentPos);
				activatedLastFrame = false;
				float parentToothWidth = 360 / parentToothCount;
				float toothWidth = 360 / toothCount;

				float newAngle = Mathf.Abs(((syncRot) % parentToothWidth / parentToothWidth)) * -toothWidth + toothWidth / 2;

				float angleBetweenGears = Vector3.Angle(Vector3.forward, transform.position - parentPos);
				//float offsetFromGearAngle = angleBetweenGears / magicNumber * toothWidth;
				//Debug.Log(angleBetweenGears + " | " + transform.eulerAngles.y);

				//Debug.Log("ParentOffset: " + ((syncRot) % parentToothWidth / parentToothWidth) + " | Ratio: " + ((float)toothCount / (float)parentToothCount) + " | SyncRot: " + syncRot);
				Debug.Log(newAngle);
				
				gameObject.transform.localRotation = Quaternion.AngleAxis(newAngle + offsetForAngle, rotationAxis);
			}
			if (stacked)
			{
				turningSpeed = parentSpeed;
			}
			else
			{
				turningSpeed = parentSpeed * ((float)parentToothCount / (float)toothCount);

			}
			foreach (var meshedGear in meshedGears)
			{
				meshedGear.TurnGear(turningSpeed * -1, toothCount, false, transform.rotation.eulerAngles.y, transform.position);
			}
			foreach (var stackedGear in stackedGears)
			{
				stackedGear.TurnGear(turningSpeed, toothCount, true, transform.rotation.eulerAngles.y, transform.position);
			}

			float currentRotation = Vector3.Scale(gameObject.transform.localRotation.eulerAngles, rotationAxis).magnitude;
			//Debug.Log(currentRotation);
			gameObject.transform.localRotation = Quaternion.AngleAxis(currentRotation + turningSpeed, rotationAxis);

		}
	}


}



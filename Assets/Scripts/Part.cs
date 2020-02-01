using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Part : MonoBehaviour
{
    [SerializeField]
    private Part[] preParts;
    [SerializeField]
    private Part[] blockingParts;
    [SerializeField]
    private Dimension dimension;
    private Dimension oldDimension;
    [SerializeField]
    private PartType type;

    public bool IsAttached { get; set; } = false;
    public bool IsAttachable { get; set; } = true;

    // Fixed information
    private Vector3 position;
    private Quaternion rotation;

    // ********* HELPER *************
    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    // ********** HELPER END ***********
    private enum Dimension
    {
        BLUEPRINT,
        PHYSICAL
    };

    private enum PartType
    {
        GEAR,
        OTHER
    };

    // Start is called before the first frame update
    void Start()
    {
        oldDimension = dimension;
        Transform transform = GetComponent<Transform>();
        position = transform.position;
        rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Check current state of object
        CheckDimension();
    }

    // Checks if dimension has changed and if so adjusts the settings
    private void CheckDimension()
    {
        if(!dimension.Equals(oldDimension))
        {
            oldDimension = dimension;
            if (dimension.Equals(Dimension.BLUEPRINT))
            {
                GetComponent<Collider>().isTrigger = true;
                GetComponent<Rigidbody>().isKinematic = true;
                // TODO: Do something with renderer??
            }else if(dimension.Equals(Dimension.PHYSICAL))
            {
                GetComponent<Collider>().isTrigger = false;
                GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    // IF BLUEPRINT:

    // Detect if this blueprint is active and the collided object is correct (has same part type as blueprint).
    // If so, set collided object to blueprint position (TODO: change that to a threshold)
    private void OnTriggerStay(Collider other)
    {
        if (IsAttachable == true && other.attachedRigidbody && dimension.Equals(Dimension.BLUEPRINT) && !Input.GetMouseButton(0))
        {
            if(other.TryGetComponent(out Part part))
            {
                if (part.type == type && part.dimension == Dimension.PHYSICAL)
                {
                    // Set collided object to blueprint position
                    other.GetComponent<Transform>().SetPositionAndRotation(position, rotation);
                    part.IsAttached = true;
                    IsAttachable = false;
                    Debug.Log("Already attached object!");
                }
            }
        }
    } 
}

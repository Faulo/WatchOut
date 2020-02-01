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
    [Range(0f, 1f)]
    [SerializeField]
    private double posThreshold = 1.0f;
    [SerializeField]
    private Renderer blueprintRenderer = default;

    public bool IsAttached = false;
    public bool IsAttachable = false;

    // Instance Information
    private Vector3 position;
    private Quaternion rotation;
    private float boundingZ;
    private float precision;
    private Part attachedPart = default;
    private Part attachedBlueprint = default;

    // ********* HELPER *************
    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown()
    {
            if (dimension == Dimension.PHYSICAL)
            {
                screenPoint = Camera.main.WorldToScreenPoint(transform.position);
                offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            }
            else
            {
                if (attachedPart != null)
                {
                    screenPoint = Camera.main.WorldToScreenPoint(attachedPart.transform.position);
                    offset = attachedPart.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                }
            }
    }

    void OnMouseDrag()
    {
            if (dimension == Dimension.PHYSICAL)
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                transform.position = curPosition;
            }
            else
            {
                if (attachedPart != null)
                {
                    Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                    Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                    attachedPart.transform.position = curPosition;
                }
            }
    }

    // ********** HELPER END ***********

    private enum Dimension
    {
        BLUEPRINT,
        PHYSICAL
    };

    private enum PartType
    {
        GEAR01,
        GEAR02,
        GEAR03,
        GEAR04,
        GEAR05,
        GEAR06,
        GEAR07,
        GEAR08,
        GEAR09,
        GEAR10,
        GEAR11,
        GEAR12,
        GEAR13,
        GEAR14,
        GEAR15,
        BALANCE_WHEEL,
        COIL,
        JEWEL,
        PALETTE,
        OTHER
    };

    // Start is called before the first frame update
    void Start()
    {
        oldDimension = dimension;
        boundingZ = GetComponent<Collider>().bounds.size.z;
        Transform transform = GetComponent<Transform>();
        position = transform.position;
        rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Check current state of object
        CheckDimension();
        CheckForAttachable();
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
    // If so and threshold is met, set collided object to blueprint position.
    private void OnTriggerStay(Collider other)
    {
        if (dimension.Equals(Dimension.PHYSICAL) && !Input.GetMouseButton(0))
        {
            if (other.TryGetComponent(out Part blueprint))
            {
                attachedBlueprint = blueprint;
            }
        }
        else if (IsAttachable && other.attachedRigidbody && dimension.Equals(Dimension.BLUEPRINT) && !Input.GetMouseButton(0))
        {
            if(other.TryGetComponent(out Part part))
            {
                if (part.type == type && part.dimension == Dimension.PHYSICAL)
                {
                    Transform collidedTransform = other.GetComponent<Transform>();
                    // Test if collided objects meets threshold
                    float distance = Vector3.Distance(collidedTransform.position, position);
                    if (distance <= posThreshold*boundingZ)
                    {
                        // Set collided object to blueprint position, save reference and disable rigidbody
                        other.GetComponent<Transform>().SetPositionAndRotation(position, rotation);
                        other.GetComponent<Rigidbody>().isKinematic = true;
                        attachedPart = part;

                        // Disable blueprint
                        blueprintRenderer.enabled = false;

                        // Set necessary variables
                        IsAttached = true;
                        IsAttachable = false;
                        precision = CalculatePrecision(distance);
                        Debug.Log("Attached part with " + precision * 100 + " % precision!");
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (dimension.Equals(Dimension.BLUEPRINT) && other.attachedRigidbody && Input.GetMouseButton(0))
        {
            if (other.TryGetComponent(out Part part))
            {
                if (part.attachedBlueprint == this) {

                    if(!CheckForBlocked())
                    {
                        // Enable physical object rigidbody and delete part reference
                        other.GetComponent<Rigidbody>().isKinematic = false;
                        part.attachedBlueprint = null;
                        attachedPart = null;

                        // Enable blueprint
                        blueprintRenderer.enabled = true;

                        // Set necessary variables
                        IsAttached = false;
                    }
                    else
                    {
                        other.GetComponent<Rigidbody>().isKinematic = false;
                        part.attachedBlueprint = null;
                        attachedPart = null;
                        part.wreakHavoc();
                    }
                }
            }
        }
    }

    private bool CheckForBlocked()
    {
        // Check if blocking parts are attached, if underlying parts are already attached. 
        foreach (Part each in blockingParts)
        {
            if (each.IsAttached)
            {
                return true;
            }
        }
        return false;
    }

    private void CheckForAttachable()
    {
        if (dimension.Equals(Dimension.BLUEPRINT))
        {
            // Check if underlying parts are attached. If there are no underlying parts, part can always be attached. 
            // If there is already an attached part, no part can be attached anymore.
            IsAttachable = true;

            if (IsAttached)
            {
                IsAttachable = false;
            }
            else
            {
                foreach (Part each in preParts)
                {
                    if (!each.IsAttached)
                    {
                        IsAttachable = false;
                        return;
                    }
                }
            }

            // Check if blocking parts are attached, if underlying parts are already attached. 
            if(CheckForBlocked())
            {
                IsAttachable = false;
                return;
            }
        }
    }

    private float CalculatePrecision(float distance)
    {
        return 1 - (distance / boundingZ);
    }

    private void wreakHavoc()
    {
        Debug.Log("Something bad is about to happen...");
        GetComponent<Rigidbody>().AddForce(new Vector3(0f,1000f,0f));
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// Unuse this if you're not using TextMeshPro for some reason
using TMPro;

// Add this directly to the UI object you want to create a tooltip for, e.g. a button. Must accept raycasts!
public class GenericTooltip : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public bool IsActive = true;
    [Tooltip("Time in seconds to wait until tooltip is shown.")]
    public float waitTime = 0.5f;

    [Multiline]
    public string tooltiptext;
    // You can use a camera instead of Screen.height and Screen.width for the max if you want.
    // Uncomment this + the definition line in start + change the max in Update.
    //public Camera cam;
    public Transform tooltipCanvasParent;
    public GameObject tooltipPrefab;
    [Tooltip("This is where the actual spawned tooltip object lives and is 'cached' between uses but you can also add an object here directly.")]
    public GameObject spawnedTooltip;
    Vector3 min, max;
    private RectTransform rect;
    private EventTrigger eventTrigger;
    [Tooltip("Y-offset from parent object; make negative for UI elements that are at the bottom of the screen!")]
    public float y_offset = 15f;
    private Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        // If this is null, we just make it the parent of the object itself, but this is a bit risky
        if (tooltipCanvasParent == null)
        {
            tooltipCanvasParent = transform.parent;
        }
        // Spawn and cache the tooltip prefab OR cache the already-existing one OR send a warning if neither exits.
        if (spawnedTooltip == null)
        {
            if (tooltipPrefab != null)
            {
                spawnedTooltip = Instantiate(tooltipPrefab, tooltipCanvasParent.transform, false);
                // Set the spawned tooltip at zero!
                spawnedTooltip.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogError("No prefab or pre-made tooltip object assigned for " + name + "!", gameObject);
            }
        }

        // If there's a text set, grab the (tmppro in this case) text object and set the text - this is very simplistic though.
        if (tooltiptext != "")
        {
            SetTooltipText(tooltiptext);
        }

        SetActive(false);
        // if (cam == null) { cam = Camera.main; };
        rect = spawnedTooltip.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Spawned tooltip for " + name + " does not have rect transform component. Tooltips must be UI objects!", gameObject);
        }

    }

    public void SetTooltipText(string text)
    { // This is a very simplistic way to do this, but in the interest of keeping it all inside one component...change to suit!
        tooltiptext = text;
        spawnedTooltip.GetComponentInChildren<TextMeshProUGUI>().text = tooltiptext;
    }

    public void SetActive(bool active) // Also add here any checks for e.g. game states and such when you don't want tooltips at all!
    {
        if (spawnedTooltip != null)
        {
            spawnedTooltip.SetActive(active);
            IsActive = active;
        }
        else
        {
            Debug.LogWarning("Tried to change active level of non-existing tooltip: check your code, yo!", gameObject);
        }
    }
    void Activate()
    {
        SetActive(true);
    }

    public void OnPointerEnter(PointerEventData data) // Hint -> don't raycast the tooltip object, or it might block this event!
    {
        //Debug.Log("Pointer entered - activate tooltip after waittime");
        CancelInvoke("Activate");
        Invoke("Activate", waitTime);
    }
    public void OnPointerExit(PointerEventData data)
    {
        //Debug.Log("Point exited, deactivate tooltip!");
        CancelInvoke("Activate");
        SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsActive)
        {
            // You can place this into a separate function for more controlled resolution changes
            min = new Vector3(0, 0, 0);
            max = new Vector3(cam.pixelWidth, cam.pixelHeight, 0);
            //max = new Vector3(Screen.width, Screen.height, 0f);
            //get the tooltip position with offset
            // This one also adds the width of the rect which doesn't work in a lot of situations so.
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - (rect.rect.height / 2 + y_offset), 0f);
            //clamp it to the screen size so it doesn't go outside
            spawnedTooltip.transform.position = new Vector3(Mathf.Clamp(position.x, min.x + rect.rect.width / 2, max.x - rect.rect.width / 2), Mathf.Clamp(position.y, min.y + rect.rect.height / 2, max.y - rect.rect.height / 2), position.z);
        }
    }
}

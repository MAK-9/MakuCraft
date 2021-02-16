using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
    private Camera playerCamera;
    public float range = 100f;

    private void Awake()
    {
        playerCamera = this.GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
            LeftClick();
    }

    void LeftClick(bool isLeft = true)
    {
        RaycastHit ray;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out ray, range))
        {
            Debug.Log(ray.transform.name);
        }
    }
}

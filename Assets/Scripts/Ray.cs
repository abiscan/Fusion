using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Ray : MonoBehaviour
{

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        Lidar_model lidarModel = this.GetComponentInParent<Lidar_model>();
        lidarModel.OnRayCollision(collision);
    }
    void OnCollisionStay(Collision collision)
    {
        Lidar_model lidarModel = this.GetComponentInParent<Lidar_model>();
        lidarModel.OnRayCollision(collision);
    }

}

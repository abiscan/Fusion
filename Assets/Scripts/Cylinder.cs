using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class Cylinder : MonoBehaviour {
    private Rigidbody rb;
    private Transform transformCylinder;
    private string[] lines = new String[10];
    private int lineIterator ;
    private int lastFrameParsed;
    public float laserLocalScaleFactor;
    public float maxRay;
    public string outputPath;
    
    // Use this for initialization
    void Start() {
        Debug.Log("[LIDAR] Start");
        rb = GetComponent<Rigidbody>();
        transformCylinder = GetComponent<Transform>();        
        lineIterator = 0;
        lastFrameParsed = 0;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[LIDAR][OnCollisionEnter][1] OnCollisionEnter: frameCount="+ Time.frameCount+" " + collision.gameObject.tag+" name: "+collision.gameObject.name);
        if(lastFrameParsed < Time.frameCount)
        {
            lastFrameParsed = Time.frameCount;
        }
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Environment"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Debug.Log("[LIDAR][OnCollisionEnter][2] OnCollisionEnter: frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " hit " + contact.otherCollider.name+" at point="+ contact.point);                
                int reflectance = 0;
                if (lineIterator > lines.Length)
                {
                    Debug.Log("[LIDAR][OnCollisionEnter][3] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + "> lines.Length=" + lines.Length);
                    break;
                }
                if (lines[lineIterator] != null)
                {
                    Debug.Log("[LIDAR][OnCollisionEnter][4] frameCount=" + Time.frameCount + " Error, lines[lineIterator] with lineIterator=" + lineIterator + " not empty");
                }else
                {
                    lines[lineIterator] = contact.otherCollider.name + " "+contact.point.x.ToString() + " " + contact.point.y.ToString() + " " + contact.point.z.ToString()+" " +reflectance;
                }
                lineIterator++;
 
            }
        }
    }

    void FixedUpdate()
    {
        Debug.Log("[LIDAR][FixedUpdate][1] frameCount=" + Time.frameCount );
        //Function called right after the Frame is finished        
        if (lineIterator > lines.Length)
        {
            Debug.Log("[LIDAR][FixedUpdate][2] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + "> lines.Length=" + lines.Length);
        }
        lineIterator = 0;
        Vector3 localScale = transformCylinder.localScale;
        float newRay = localScale.x + laserLocalScaleFactor;
        if (newRay < maxRay)
        {
            Vector3 updatedLocalScale = new Vector3(newRay, localScale.y, newRay);
            transformCylinder.localScale = updatedLocalScale;
            
        }
        writeFile(lastFrameParsed);
    }

    void writeFile (int frame) {
        string outputFile = outputPath + "frame_" + frame + ".txt";
        Debug.Log("[LIDAR][writeFile] frameCount=" + Time.frameCount + " Writing lines to output file:" + outputFile);
        //if (!File.Exists(outputFile))
        //{
        //    File.Create(outputFile);
        //}
        System.IO.File.WriteAllLines(outputFile, lines);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Lidar_model : MonoBehaviour
{
    public int numberOfVerticalRay;
    public int distance;
    public int numberOfScanIn360;
    public string outputPath;
    public int speed;

    //rivate
    private int lastFrameParsed;
    private string[] lines = new String[50];
    private int lineIterator;
    private Rigidbody rb;
    private float[][] separation;
    private Vector3[][] closestPoint;
    private GameObject laserGameObject;


    private void initialize()
    {
        //Initialization of variable
        lastFrameParsed = 0;
        rb = GetComponent<Rigidbody>();
        int i = 0;
        laserGameObject = new GameObject();
        while (laserGameObject.name != "Laser")
        {
            laserGameObject = this.transform.GetChild(i).gameObject;
            Debug.Log("[Lidar_model][initialize][1] laserGameObject found at index=" + i);
            i++;            
        }
        //laserGameObject = this.transform.GetChild(1).gameObject;
        separation = new float[numberOfScanIn360][];
        closestPoint = new Vector3[numberOfScanIn360][];
        for (i=0;i< numberOfScanIn360; i++)
        {
            separation[i] = new float[numberOfVerticalRay];
            closestPoint[i] = new Vector3[numberOfVerticalRay];
            for (int j=0; j< numberOfVerticalRay; j++)
            {
                separation[i][j] = float.PositiveInfinity;
                closestPoint[i][j] = new Vector3(float.NaN, float.NaN, float.NaN);
            }
        }
    }

    private void createRays()
    {
        // Get the coordinates of the basis
        Vector3 localPositionBasis = laserGameObject.transform.localPosition;
        Vector3 localScaleBasis = laserGameObject.transform.localScale;
        Debug.Log("[Lidar_model][createRays][1] basisGameObject coordinates: x=" + laserGameObject.transform.localPosition.x + " y=" + laserGameObject.transform.localPosition.y + " z=" + laserGameObject.transform.localPosition.z);
        int horizontalIndex = 0;
        int verticalIndex = 0;
        if (numberOfScanIn360 > 0)
        {
            //for (int angle = 0; angle <= 360; angle = angle + (360/ numberOfScanIn360))
            for (int angle = 0; angle < 360; angle += 360 / numberOfScanIn360)
            {
                //Debug.Log("[Lidar_model][Start][3.1] Create primitive Cylinder angle=" + angle);
                GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ray.transform.parent = this.transform;
                ray.name = "Ray_" + verticalIndex+"_"+ horizontalIndex;
                //Position
                {
                    //First compute offset in x and y
                    float offsetX = (float)Math.Cos((float)(-angle * Math.PI / 180)) * (localScaleBasis.x / 2);
                    float distanceX = (float)Math.Cos((float)(-angle * Math.PI / 180)) * (distance / 2);
                    //offsetX = 0;
                    float offsetZ = (float)Math.Sin((float)(-angle * Math.PI / 180)) * (localScaleBasis.z / 2);
                    float distanceZ = (float)Math.Sin((float)(-angle * Math.PI / 180)) * (distance / 2);
                    //offsetZ = 0;
                    Debug.Log("[Lidar_model][Start][3] angle=" + angle + " offsetX=" + offsetX + " distanceX=" + distanceX + " offsetZ=" + offsetZ + " distanceZ=" + distanceZ);
                    float localPositionRayX = localPositionBasis.x + offsetX + distanceX;
                    float localPositionRayY = localPositionBasis.y;
                    float localPositionRayZ = localPositionBasis.z + offsetZ + distanceZ;
                    ray.transform.position = new Vector3(localPositionRayX, localPositionRayY, localPositionRayZ);
                }
                //Rotation
                {
                    ray.transform.localRotation = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
                }
                //Scale
                {
                    ray.transform.localScale = new Vector3(distance, 1, 1);
                }

                Debug.Log("[Lidar_model][createRays][3] Create primitive Cylinder position=" + ray.transform.position + " localScale=" + ray.transform.localScale + " localRotation=" + ray.transform.localRotation);

                Destroy(ray.GetComponent<CapsuleCollider>());
                //FixedJoint joint = ray.AddComponent<FixedJoint>();
                //joint.connectedBody = rbBasis;
                Rigidbody rbRay = ray.AddComponent<Rigidbody>();
                rbRay.mass = 0;
                rbRay.useGravity = false;
                rbRay.freezeRotation = true;
                rbRay.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                ray.AddComponent<BoxCollider>();
                ray.AddComponent<Ray>();
                //rbRay.useGravity = true;
                horizontalIndex++;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("[Lidar_model][Start][1]");
        initialize();
        createRays();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        Debug.Log("[LIDAR][FixedUpdate][1] frameCount=" + Time.frameCount);
        //Function called right after the Frame is finished        
        if (lineIterator > lines.Length)
        {
            Debug.Log("[LIDAR][FixedUpdate][2] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + "> lines.Length=" + lines.Length);
        }
        lineIterator = 0;
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal * 10, 0.0f, -moveVertical * 10);
        Transform transform = GetComponent<Transform>();
        transform.position += movement;
        writeFile(lastFrameParsed);
    }

    public void OnRayCollision(Collision collision)
    {
        //Here we fill correctly separation closestPoint
        String rayName = collision.gameObject.tag;
        int verticalIndex = rayName[4];
        int horizontalIndex = rayName[6];
        Debug.Log("[LIDAR][OnCollisionEnter][1] OnCollisionEnter: frameCount=" + Time.frameCount + " " + collision.gameObject.tag + " name: " + collision.gameObject.name);
        if (lastFrameParsed < Time.frameCount)
        {
            lastFrameParsed = Time.frameCount;
        }
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Environment"))
        {
            Debug.Log("[LIDAR][OnCollisionEnter][1] OnCollisionEnter: frameCount=" + Time.frameCount + " " + collision.gameObject.tag + " name: " + collision.gameObject.name);
            float separation = float.PositiveInfinity;
            foreach (ContactPoint contact in collision.contacts)
            {
                //We compare separation of the two points in order to take the closer one
                if ( separation > contact.separation)
                {
                    Debug.Log("[Lidar_model][OnRayCollision][2] OnCollisionEnter: frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " hit " + contact.otherCollider.name + " at point=" + contact.point);
                    int reflectance = 0;
                    if (lineIterator >= lines.Length)
                    {
                        Debug.Log("[Lidar_model][OnRayCollision][3] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + "> lines.Length=" + lines.Length);
                        break;
                    }
                    if (lines[lineIterator] != null)
                    {
                        Debug.Log("[Lidar_model][OnRayCollision][4] frameCount=" + Time.frameCount + " Error, lines[lineIterator] with lineIterator=" + lineIterator + " not empty");
                    }
                    else
                    {
                        lines[lineIterator] = contact.otherCollider.name + " " + contact.point.x.ToString() + " " + contact.point.y.ToString() + " " + contact.point.z.ToString() + " " + reflectance;
                    }
                }
                separation = contact.separation;
                
            }
        }
        lineIterator++;
    }

    void writeFile(int frame)
    {
        string outputFile = outputPath + "frame_" + frame + ".txt";
        Debug.Log("[Lidar_model][writeFile] frameCount=" + Time.frameCount + " Writing lines to output file:" + outputFile);
        System.IO.File.WriteAllLines(outputFile, lines);
    }
}

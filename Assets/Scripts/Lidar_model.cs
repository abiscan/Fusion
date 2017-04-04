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
    
    private Rigidbody rb;
    private float[][] distanceTab;
    private Vector3[][] closestPointTab;
    private float[][] reflectanceTab;
    private string[][] objectNameTab;
    private float minimumDistance;
    
    private GameObject laserGameObject;


    private void initialize()
    {
        //Initialization of variable
        rb = GetComponent<Rigidbody>();
        int i = 0;
        
        laserGameObject = new GameObject();
        while (laserGameObject.name != "Laser")
        {
            laserGameObject = this.transform.GetChild(i).gameObject;
            Debug.Log("[Lidar_model][initialize][1] laserGameObject found at index=" + i);
            i++;            
        }
        minimumDistance = laserGameObject.transform.localScale.x * 2;
    }

    private void initializePointTabs()
    {
        distanceTab = new float[numberOfVerticalRay][];
        closestPointTab = new Vector3[numberOfVerticalRay][];
        objectNameTab = new string[numberOfVerticalRay][];
        reflectanceTab = new float[numberOfVerticalRay][];
        for (int i = 0; i < numberOfVerticalRay; i++)
        {
            distanceTab[i] = new float[numberOfScanIn360];
            closestPointTab[i] = new Vector3[numberOfScanIn360];
            objectNameTab[i] = new string[numberOfScanIn360];
            reflectanceTab[i] = new float[numberOfScanIn360];
            for (int j = 0; j < numberOfScanIn360; j++)
            {
                distanceTab[i][j] = float.PositiveInfinity;
                closestPointTab[i][j] = new Vector3(float.NaN, float.NaN, float.NaN);
                objectNameTab[i][j] = "";
                reflectanceTab[i][j] = float.NegativeInfinity;
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
                    ray.transform.localScale = new Vector3(distance, (float) 0.01, (float) 0.01);
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
        initializePointTabs();
        createRays();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {        
        //Function called right after the Frame is finished        
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal * 10, 0.0f, -moveVertical * 10);
        Transform transform = GetComponent<Transform>();
        transform.position += movement;
        writeFile(Time.frameCount);
        initializePointTabs();
    }

    public void OnRayCollision(Collision collision)
    {
        //Here we fill correctly distance closestPoint                
        //Debug.Log("[Lidar_model][OnCollisionEnter][1] OnCollisionEnter: frameCount=" + Time.frameCount + " " + collision.gameObject.tag + " name: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Environment"))
        {
            //Debug.Log("[Lidar_model][OnCollisionEnter][1] OnCollisionEnter: frameCount=" + Time.frameCount + " " + collision.gameObject.tag + " name: " + collision.gameObject.name);            
            foreach (ContactPoint contact in collision.contacts)
            {
                String rayName = contact.thisCollider.name;
                int verticalIndex = rayName[4] - '0';
                int horizontalIndex = rayName[6] - '0';
                if(verticalIndex > distanceTab.Length || horizontalIndex > distanceTab[0].Length)
                {
                    Debug.LogError("[Lidar_model][OnRayCollision] ERROR frameCount=" + Time.frameCount +" rayName=" + rayName + " verticalIndex=" + verticalIndex + " > distanceTab.Length=" + distanceTab.Length+ " or horizontalIndex="+ horizontalIndex+ " > distanceTab[0].Length=" + distanceTab[0].Length);
                }
                float previousDistance = distanceTab[verticalIndex][horizontalIndex];
                float distance = Vector3.Distance(contact.point, laserGameObject.transform.position);
                Debug.Log("[Lidar_model][OnRayCollision][2] frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " hit " + contact.otherCollider.name + " rayName=" + rayName + " verticalIndex=" + verticalIndex + " horizontalIndex=" + horizontalIndex+" at point=" + contact.point + " distance=" + distance +  " objectName=" + contact.otherCollider.name);
                //We compare distance of the two points in order to take the closer one

                if (  Math.Abs(distance) < Math.Abs(previousDistance) && Math.Abs(distance) > minimumDistance)
                {
                    //distance tab needs to be updated
                    Debug.Log("[Lidar_model][OnRayCollision][2] frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " Update tab previousDistance=" + previousDistance+ " distance=" + distance);
                   int reflectance = 0;

                    distanceTab[verticalIndex][horizontalIndex] = distance;
                    closestPointTab[verticalIndex][horizontalIndex] = contact.point;
                    reflectanceTab[verticalIndex][horizontalIndex] = reflectance;
                    objectNameTab[verticalIndex][horizontalIndex] = contact.otherCollider.name;                   
                }                       
            }
        }
    }

    void writeFile(int frame)
    {       
        string[] lines = new String[50];
        int lineIterator = 0;
        for (int verticalIndex = 0; verticalIndex < distanceTab.Length; verticalIndex++)
        {
            for (int horizontalIndex = 0; horizontalIndex < distanceTab[verticalIndex].Length; horizontalIndex++)
            {
                if (distanceTab[verticalIndex][horizontalIndex] < distance + 1)
                {
                    if (lineIterator >= lines.Length)
                    {
                        Debug.LogError("[Lidar_model][writeFile][3] ERROR frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + "> lines.Length=" + lines.Length);
                    }
                    else if (lines[lineIterator] != null)
                    {
                        Debug.LogError("[Lidar_model][writeFile][4] ERROR frameCount=" + Time.frameCount + " lines[lineIterator] with lineIterator=" + lineIterator + " not empty");
                    }
                    else
                    {
                        lines[lineIterator] = objectNameTab[verticalIndex][horizontalIndex] + " " + closestPointTab[verticalIndex][horizontalIndex].x.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].y.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].z.ToString() + " " + reflectanceTab[verticalIndex][horizontalIndex];
                    }
                    Debug.Log("[Lidar_model][writeFile][5] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + " lines[lineIterator]=" + lines[lineIterator]);
                    lineIterator++;
                }
            }
        }
        string outputFile = outputPath + "frame_" + frame + ".txt";
        //Debug.Log("[Lidar_model][writeFile] frameCount=" + Time.frameCount + " Writing lines to output file:" + outputFile);
        System.IO.File.WriteAllLines(outputFile, lines);        
    }
}

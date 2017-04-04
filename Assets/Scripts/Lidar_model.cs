using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Lidar_model : MonoBehaviour
{

    public int distance;
    public int numberOfVerticalRay;
    public int numberOfScanIn360;
    public string outputPath;
    public int speed;
    public int initialVerticalAngle;
    public int field_of_view;


    //rivate

    private Rigidbody rb;
    private float[][] distanceTab;
    private Vector3[][] closestPointTab;
    private float[][] reflectanceTab;
    private string[][] objectNameTab;
    private float minimumDistance;

    private GameObject laserGameObject;
    private bool useRayCast = true;
    private string[] lines;
    private int lineIterator = 0;


    // Use this for initialization
    void Start()
    {
        Debug.Log("[Lidar_model][Start][1]");
        initialize();

        if (!useRayCast)
        {
            numberOfVerticalRay = 1;
            initializePointTabs();
            createRays();
        }
    }

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
        lines = new string[numberOfVerticalRay * numberOfScanIn360];
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
        Vector3 localPositionBasis = laserGameObject.transform.localPosition;
        Vector3 localScaleBasis = laserGameObject.transform.localScale;
        // Get the coordinates of the basis            
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
                ray.name = "Ray_" + verticalIndex + "_" + horizontalIndex;
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
                    ray.transform.localScale = new Vector3(distance, (float)0.01, (float)0.01);
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

    private void createRayCast()
    {
        Vector3 positionLidar = laserGameObject.transform.position;
        int finalAngle = initialVerticalAngle + field_of_view;
        float angleIterator =  (((float) field_of_view) / ((float) numberOfVerticalRay));
        int rayCastNumber = 0;
        if (numberOfScanIn360 > 0)
        {
            //Debug.Log("[Lidar_model][createRays][1] finalAngle="+ finalAngle+ " angleIterator="+ angleIterator);
            for (float verticalAngle = initialVerticalAngle; verticalAngle < finalAngle; verticalAngle += angleIterator)
            {
                //Debug.Log("[Lidar_model][createRays][2] verticalAngle="+ verticalAngle);
                for (int horizontalAngle = 0; horizontalAngle < 360; horizontalAngle += 360 / numberOfScanIn360)
                {
                    rayCastNumber++;
                    if (rayCastNumber > lines.Length) { 
                        Debug.LogError("[Lidar_model][createRays] rayCastNumber="+ rayCastNumber+ " > lines.Length="+ lines.Length+ " verticalAngle=" + verticalAngle+ " horizontalAngle="+ horizontalAngle+ " angleIterator="+ angleIterator);
                    }
                    //Debug.Log("[Lidar_model][createRays][3] verticalAngle="+ verticalAngle+ " horizontalAngle="+ horizontalAngle);
                    float distanceX = (float)Math.Cos((float)(horizontalAngle * Math.PI / 180));
                    float distanceZ = (float)Math.Sin((float)(horizontalAngle * Math.PI / 180));
                    float distanceY= -(float)Math.Cos((float)((verticalAngle) * Math.PI / 180));
                    RaycastHit hit;
                    if (Physics.Raycast(positionLidar, new Vector3(distanceX, distanceY, distanceZ), out hit, distance))
                    {
                        //Debug.Log("[Lidar_model][createRays][1] RayCast collide rayCastNumber="+ rayCastNumber + " distance=" + hit.distance + " collider=" + hit.distance + " at point=" + hit.point + " lineIterator=" + lineIterator);
                        //Debug.DrawLine(positionLidar, hit.point);
                        int reflectance = 0;
                        //lines[lineIterator] = hit.collider.name + " " + hit.point.x.ToString() + " " + hit.point.y.ToString() + " " + hit.point.z.ToString() + " " + reflectance;
                        lines[lineIterator] = hit.point.x.ToString() + " " + hit.point.y.ToString() + " " + hit.point.z.ToString() + " " + reflectance;
                        //Debug.Log("[Lidar_model][createRays][5] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + " lines[lineIterator]=" + lines[lineIterator]);
                        lineIterator++;
                    }
                }
            }
        }
    }



    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {

        if (useRayCast)
        {
            lineIterator = 0;
            createRayCast();
        }

        //Function called right after the Frame is finished        
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal * 10, 0.0f, -moveVertical * 10);
        Transform transform = GetComponent<Transform>();
        transform.position += movement;
        writeFile(Time.frameCount);
        if (useRayCast)
        {
            initializePointTabs();
        }
    }

    public void OnRayCollision(Collision collision)
    {
        if (!useRayCast)
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
                    if (verticalIndex > distanceTab.Length || horizontalIndex > distanceTab[0].Length)
                    {
                        Debug.LogError("[Lidar_model][OnRayCollision] ERROR frameCount=" + Time.frameCount + " rayName=" + rayName + " verticalIndex=" + verticalIndex + " > distanceTab.Length=" + distanceTab.Length + " or horizontalIndex=" + horizontalIndex + " > distanceTab[0].Length=" + distanceTab[0].Length);
                    }
                    float previousDistance = distanceTab[verticalIndex][horizontalIndex];
                    float distance = Vector3.Distance(contact.point, laserGameObject.transform.position);
                    Debug.Log("[Lidar_model][OnRayCollision][2] frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " hit " + contact.otherCollider.name + " rayName=" + rayName + " verticalIndex=" + verticalIndex + " horizontalIndex=" + horizontalIndex + " at point=" + contact.point + " distance=" + distance + " objectName=" + contact.otherCollider.name);
                    //We compare distance of the two points in order to take the closer one

                    if (Math.Abs(distance) < Math.Abs(previousDistance) && Math.Abs(distance) > minimumDistance)
                    {
                        //distance tab needs to be updated
                        Debug.Log("[Lidar_model][OnRayCollision][2] frameCount=" + Time.frameCount + " " + contact.thisCollider.name + " Update tab previousDistance=" + previousDistance + " distance=" + distance);
                        int reflectance = 0;

                        distanceTab[verticalIndex][horizontalIndex] = distance;
                        closestPointTab[verticalIndex][horizontalIndex] = contact.point;
                        reflectanceTab[verticalIndex][horizontalIndex] = reflectance;
                        objectNameTab[verticalIndex][horizontalIndex] = contact.otherCollider.name;
                    }
                }
            }
        }
    }

    void writeFile(int frame)
    {
        if (!useRayCast)
        {
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
                            //lines[lineIterator] = closestPointTab[verticalIndex][horizontalIndex].x.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].y.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].z.ToString() + " " + reflectanceTab[verticalIndex][horizontalIndex] + " "+objectNameTab[verticalIndex][horizontalIndex] ;
                            lines[lineIterator] = closestPointTab[verticalIndex][horizontalIndex].x.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].y.ToString() + " " + closestPointTab[verticalIndex][horizontalIndex].z.ToString() + " " + reflectanceTab[verticalIndex][horizontalIndex];
                        }
                        Debug.Log("[Lidar_model][writeFile][5] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + " lines[lineIterator]=" + lines[lineIterator]);
                        lineIterator++;
                    }
                }
            }
        }
        string outputFile = outputPath + "frame_" + frame + ".txt";
        //Debug.Log("[Lidar_model][writeFile] frameCount=" + Time.frameCount + " Writing lines to output file:" + outputFile);
        System.IO.File.WriteAllLines(outputFile, lines);
    }
}

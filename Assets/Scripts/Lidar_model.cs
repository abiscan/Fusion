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
    public bool showRay;
    public int initialVerticalAngle;
    public int field_of_view;


    //rivate
    private GameObject laserGameObject;
    private bool useRayCast = true;
    private string[] lines;
    private int lineIterator = 0;


    // Use this for initialization
    void Start()
    {
        Debug.Log("[Lidar_model][Start][1]");
        initialize();
    }

    private void initialize()
    {
        //Initialization of variable
        int i = 0;
        laserGameObject = new GameObject();
        while (laserGameObject.name != "Laser")
        {
            laserGameObject = this.transform.GetChild(i).gameObject;
            Debug.Log("[Lidar_model][initialize][1] laserGameObject found at index=" + i);
            i++;
        }
        lines = new string[numberOfVerticalRay * numberOfScanIn360];
    }

    private void createRayCast()
    {
        if (numberOfScanIn360 > 0 && numberOfVerticalRay > 0)
        {
            Vector3 positionLidar = laserGameObject.transform.position;
            int finalAngle = initialVerticalAngle + field_of_view;
            float angleIterator = (((float)field_of_view) / ((float)numberOfVerticalRay - 1));
            int rayCastNumber = 0;

            //Debug.Log("[Lidar_model][createRays][1] finalAngle="+ finalAngle+ " angleIterator="+ angleIterator);
            for (float verticalAngle = initialVerticalAngle; verticalAngle <= finalAngle; verticalAngle += angleIterator)
            {
                //Debug.Log("[Lidar_model][createRays][2] verticalAngle="+ verticalAngle);
                for (int horizontalAngle = 0; horizontalAngle < 360; horizontalAngle += 360 / numberOfScanIn360)
                {
                    rayCastNumber++;
                    if (rayCastNumber > lines.Length)
                    {
                        Debug.LogError("[Lidar_model][createRays] rayCastNumber=" + rayCastNumber + " > lines.Length=" + lines.Length + " verticalAngle=" + verticalAngle + " horizontalAngle=" + horizontalAngle + " angleIterator=" + angleIterator);
                    }
                    //Debug.Log("[Lidar_model][createRays][3] verticalAngle=" + verticalAngle + " horizontalAngle=" + horizontalAngle + " angleIterator=" + angleIterator);
                    float distanceX = (float)Math.Cos((float)(horizontalAngle * Math.PI / 180));
                    float distanceZ = (float)Math.Sin((float)(horizontalAngle * Math.PI / 180));
                    float distanceY = -(float)Math.Cos((float)((verticalAngle) * Math.PI / 180));

                    Vector3 direction = Quaternion.Euler(horizontalAngle, verticalAngle, 1) * positionLidar;

                    RaycastHit hit;
                    //if (Physics.Raycast(positionLidar, direction, out hit, distance))
                    if (Physics.Raycast(positionLidar, new Vector3(distanceX, distanceY, distanceZ), out hit, distance))
                    {
                        if (hit.collider.tag != "Player")
                        {
                            //Debug.Log("[Lidar_model][createRays][1] RayCast collide rayCastNumber="+ rayCastNumber + " distance=" + hit.distance + " collider=" + hit.distance + " at point=" + hit.point + " lineIterator=" + lineIterator);
                            if (showRay)
                            {
                                Debug.DrawLine(positionLidar, hit.point);
                            }
                            int reflectance = 0;
                            //lines[lineIterator] = hit.collider.name + " " + hit.point.x.ToString() + " " + hit.point.y.ToString() + " " + hit.point.z.ToString() + " " + reflectance;
                            lines[lineIterator] = hit.point.x.ToString() + " " + hit.point.y.ToString() + " " + hit.point.z.ToString() + " " + reflectance;
                            //Debug.Log("[Lidar_model][createRays][5] frameCount=" + Time.frameCount + " lineIterator=" + lineIterator + " lines[lineIterator]=" + lines[lineIterator]);
                            lineIterator++;
                        }
                    }
                }
            }

            if (rayCastNumber != numberOfVerticalRay * numberOfScanIn360)
            {
                int numberExpected = numberOfVerticalRay * numberOfScanIn360;
                Debug.LogError("[Lidar_model][createRays] rayCastNumber=" + rayCastNumber + " != numberExpected=" + numberExpected);
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
    }

    void writeFile(int frame)
    {
        string outputFile = outputPath + "frame_" + frame + ".txt";
        //Debug.Log("[Lidar_model][writeFile] frameCount=" + Time.frameCount + " Writing lines to output file:" + outputFile);
        System.IO.File.WriteAllLines(outputFile, lines);
    }
}

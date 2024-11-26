using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class SonarROSPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/Sonar/image"; // To change to desired topic name
    public float publishMessageFrequency = 10f;
    private float timeElapsed;
    private SonarBeam sonarBeam;
    private RosMessageTypes.Std.HeaderMsg header;
    private uint height, width;
    private string encoding;
    private byte is_bigendian;
    private uint step;
    private byte[] data;
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        sonarBeam = GetComponent<SonarBeam>();
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            ImageMsg msg = PopulateImageMsg();
            
            ros.Publish(topicName, msg);

            timeElapsed = 0;
        }

    }

    private ImageMsg PopulateImageMsg()
    {
        ImageMsg message = new();
        message.width = (uint) sonarBeam.imageWidth;
        message.height = (uint) sonarBeam.imageHeight;
        message.data = sonarBeam.outputRaycast;
        return message;
    }
}

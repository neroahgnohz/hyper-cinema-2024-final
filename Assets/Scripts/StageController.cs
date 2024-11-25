using UnityEngine;
using System.IO.Ports;

public class StageController : MonoBehaviour
{
    private float rotationSpeed = 2.0F;
    SerialPort serialPort = new SerialPort("COM7", 9600);
    
    void Start()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open();
            serialPort.ReadTimeout = 50;
        }
    }

    void Update()
    {
        if (enableMouseInput())
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            //Debug.Log("mouseX: " + mouseX + " mouseY: " + mouseY);
            transform.Rotate(rotationSpeed * mouseY, rotationSpeed * -mouseX, 0);
        } else
        {
            getInputFromArduino();
        }
    }

    private bool enableMouseInput()
    {
        return Input.GetMouseButton(1);
    }

    private void getInputFromArduino()
    {
        if (serialPort.IsOpen)
        {
            try
            {
                serialPort.Write("x");
                string data = serialPort.ReadLine();
                string[] list = data.Trim().Split(',');
                if (list.Length > 2)
                {
                    float heading = float.Parse(list[0]);
                    float roll = float.Parse(list[1]);
                    float pitch = float.Parse(list[2]);
                    // Debug.Log(heading + "," + pitch + "," + roll);
                    Quaternion newRotation = Quaternion.Euler(-pitch, -heading, -roll); //x, y, z
                    transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * 5f);
                }
            }
            catch (System.TimeoutException) { }
        }
    }

    private void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}

using UnityEngine;
using System.IO.Ports;

public class StageController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float smoothingFactor = 0.5f;
    
    private string portName = "/dev/cu.usbmodem2101";
    private SerialPort serialPort;
    private bool isPortConnected = false;
    private float lastReadTime = 0f;
    private float lastReconnectAttempt = 0f;
    private readonly float readInterval = 0.02f;
    private readonly float reconnectInterval = 2f; // Wait 2 seconds between reconnection attempts
    private Quaternion lastValidRotation;

    void Start()
    {
        lastValidRotation = transform.rotation;
        ConnectToArduino();
    }

    private void ConnectToArduino()
    {
        // Close any existing connection
        if (serialPort != null)
        {
            if (serialPort.IsOpen)
                serialPort.Close();
            serialPort.Dispose();
            serialPort = null;
        }

        try
        {
            serialPort = new SerialPort(portName, 9600)
            {
                ReadTimeout = 50,
                WriteTimeout = 50,
                DtrEnable = true
            };

            serialPort.Open();
            isPortConnected = true;
            Debug.Log("Connected to Arduino");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Arduino connection error: {e.Message}");
            isPortConnected = false;
        }
    }

    void Update()
    {
        // Attempt reconnection if disconnected
        if (!isPortConnected && Time.time - lastReconnectAttempt >= reconnectInterval)
        {
            Debug.Log("Attempting to reconnect...");
            ConnectToArduino();
            lastReconnectAttempt = Time.time;
            return;
        }

        if (Time.time - lastReadTime < readInterval)
            return;

        if (Input.GetMouseButton(1))
        {
            HandleMouseInput();
        }
        else
        {
            GetArduinoData();
        }

        lastReadTime = Time.time;
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(rotationSpeed * mouseY, rotationSpeed * -mouseX, 0);
        lastValidRotation = transform.rotation;
    }

    private void GetArduinoData()
    {
        if (!isPortConnected || serialPort == null || !serialPort.IsOpen)
            return;

        try
        {
            serialPort.Write("x");
            string data = serialPort.ReadLine();

            if (string.IsNullOrEmpty(data))
                return;

            string[] list = data.Trim().Split(',');

            if (list.Length > 2)
            {
                float heading = float.Parse(list[0]);
                float roll = float.Parse(list[1]);
                float pitch = float.Parse(list[2]);

                Quaternion targetRotation = Quaternion.Euler(-pitch, -heading, -roll);
                transform.rotation = Quaternion.Lerp(lastValidRotation, targetRotation, smoothingFactor);
                lastValidRotation = transform.rotation;
            }
        }
        catch (System.TimeoutException)
        {
            // Use last valid rotation
            transform.rotation = lastValidRotation;
        }
        catch (System.Exception)
        {
            Debug.Log("Lost connection to Arduino. Will attempt reconnection...");
            isPortConnected = false;
        }
    }

    private void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }

    private void OnDisable()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }
}
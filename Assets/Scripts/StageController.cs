using UnityEngine;
using System.IO.Ports;
using ZigSimTools;
using Quaternion = UnityEngine.Quaternion;

public class StageController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float smoothingFactor = 0.5f;

    private Quaternion targetRotation;

    void Start()
    {
        ZigSimDataManager.Instance.StartReceiving();
        ZigSimDataManager.Instance.QuaternionCallBack += (ZigSimTools.Quaternion q) =>
        {
            var newQut = new Quaternion((float)-q.x, (float)-q.z, (float)-q.y, (float)q.w);
            var newRot = newQut * Quaternion.Euler(90f, 0, 0);
            targetRotation = newRot;
        };
    }

    void Update()
    {
        transform.localRotation = targetRotation;
    }
}
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class LampCoordReceiver : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    public Vector2 LampMidPoint;
    public float LampLineLength;

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);
            stream = client.GetStream();
        }
        catch (Exception ex)
        {
            Debug.Log("Connection error: " + ex.Message);
            return;
        }

        new Thread(() =>
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[12];
                    int dataSize = stream.Read(buffer, 0, buffer.Length);
                    if (dataSize == 0) break;

                    float _RawlampLength = BitConverter.ToSingle(buffer, 0);
                    float _RawlampMidX = BitConverter.ToSingle(buffer, 4);
                    float _RawlampMidY = BitConverter.ToSingle(buffer, 8);

                    LampMidPoint = new Vector2(_RawlampMidX, _RawlampMidY);
                    LampLineLength = _RawlampLength;

                    Debug.Log("_RawlampLength: " + _RawlampLength);
                    Debug.Log("_RawlampMid: " + _RawlampMidX + ", " + _RawlampMidY);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Receive error: " + ex.Message);
            }
        }).Start();
    }

    private void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
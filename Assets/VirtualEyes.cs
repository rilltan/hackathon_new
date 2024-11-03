// Networking Code adapted from tutorial https://github.com/ConorZAM/Python-Unity-Socket

using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Unity.VisualScripting;

public class VirtualEyes : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;

    Camera thiscam;
    public GameObject VirtualMonitor;
    MeshFilter VirtualMonitorMesh;
    Vector3 min;
    Vector3 max;

    public Vector3 lerpFactors;


    void Start()
    {
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();
        thiscam = GetComponent<Camera>();
        VirtualMonitorMesh = VirtualMonitor.GetComponent<MeshFilter>();
        Vector3[] verts = VirtualMonitorMesh.mesh.vertices;
        
        VirtualMonitor.transform.TransformPoints(verts);
        min = new Vector3(verts[0].x, verts[0].y, verts[0].z);
        max = new Vector3(verts[1].x, verts[1].y, verts[1].z);
        foreach (Vector3 vert in verts)
        {
            if (vert.x < min.x) min.x = vert.x;
            if (vert.y < min.y) min.y = vert.y;
            if (vert.z < min.z) min.z = vert.z;
            if (vert.x > max.x) max.x = vert.x;
            if (vert.y > max.y) max.y = vert.y;
            if (vert.z > max.z) max.z = vert.z;
        }
    }

    void GetData()
    {
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();
        client = server.AcceptTcpClient();
        running = true;
        while (running)
        {
            Connection();
        }
        server.Stop();
    }

    void Connection()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (dataReceived != null && dataReceived != "")
        {
            string[] data = dataReceived.Split(',');
            Vector2 right_eye = new Vector2(float.Parse(data[0]), float.Parse(data[1]));
            Vector2 left_eye = new Vector2(float.Parse(data[2]), float.Parse(data[3]));
            float face_height = float.Parse(data[4]);
            nwStream.Write(buffer, 0, bytesRead);

            Vector2 avg_clip_pos = (right_eye + left_eye) / 2f;
            float angle_x = -(avg_clip_pos.x - 0.5f) * 68f;
            float angle_y = -(avg_clip_pos.y - 0.5f) * 51f;
            float cam_x = 6 * Mathf.Sin(angle_x * Mathf.PI / 180f);
            float cam_y = 6 * Mathf.Sin(angle_y * Mathf.PI / 180f);
            float cam_z = 6 - 6.58f / (Mathf.Tan(51f * face_height / 2 * Mathf.PI / 180f)) / 10;
            position = new Vector3(cam_x, cam_y, cam_z);
        }


    }
    Vector3 position = Vector3.zero;

    void Update()
    {
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, position.x, lerpFactors.x), Mathf.Lerp(transform.position.y,position.y,lerpFactors.y), Mathf.Lerp(transform.position.z,position.z,lerpFactors.z));
        thiscam.projectionMatrix = Matrix4x4.Frustum(min.x - transform.position.x, max.x - transform.position.x, min.y - transform.position.y, max.y - transform.position.y, min.z - transform.position.z, 1000f);
        thiscam.nearClipPlane = min.z - transform.position.z;
    }
}

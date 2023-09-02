using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Skidmark_Controller : MonoBehaviour
{
    [SerializeField] Material skidmarksMaterial;

    public int maxSkids = 2048;
    public float skidWidth = 0.35f;
    public float offsetVertical = 0.02f;
    public float minDistance = 0.25f;
    private float minDistanceSquared = 0f;
    public float maxOpacity = 1.0f;



    class MarkSection
    {
        public Vector3 Pos = Vector3.zero;
        public Vector3 Normal = Vector3.zero;
        public Vector4 Tangent = Vector4.zero;
        public Vector3 Posl = Vector3.zero;
        public Vector3 Posr = Vector3.zero;
        public Color32 Colour;
        public int LastIndex;
    };



    int markIndex;
    MarkSection[] skidmarks;
    Mesh marksMesh;
    MeshRenderer mr;
    MeshFilter mf;

    Vector3[] vertices;
    Vector3[] normals;
    Vector4[] tangents;
    Color32[] colors;
    Vector2[] uvs;
    int[] triangles;

    bool meshUpdated;
    bool haveSetBounds;

    Color32 black = Color.black;



    protected void Awake()
    {
        minDistanceSquared = minDistance * minDistance;

        if (transform.position != Vector3.zero)
        {
            Debug.LogWarning("Skidmarks.cs transform must be at 0,0,0. Setting it to zero now.");
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }



    protected void Start()
    {
        skidmarks = new MarkSection[maxSkids];
        for (int i = 0; i < maxSkids; i++)
        {
            skidmarks[i] = new MarkSection();
        }

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }

        marksMesh = new Mesh();
        marksMesh.MarkDynamic();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>();
        }
        mf.sharedMesh = marksMesh;

        vertices = new Vector3[maxSkids * 4];
        normals = new Vector3[maxSkids * 4];
        tangents = new Vector4[maxSkids * 4];
        colors = new Color32[maxSkids * 4];
        uvs = new Vector2[maxSkids * 4];
        triangles = new int[maxSkids * 6];

        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.material = skidmarksMaterial;
        mr.lightProbeUsage = LightProbeUsage.Off;
    }



    protected void LateUpdate()
    {
        if (!meshUpdated) return;
        meshUpdated = false;

        marksMesh.vertices = vertices;
        marksMesh.normals = normals;
        marksMesh.tangents = tangents;
        marksMesh.triangles = triangles;
        marksMesh.colors32 = colors;
        marksMesh.uv = uvs;

        if (!haveSetBounds)
        {

            marksMesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10000, 10000, 10000));
            haveSetBounds = true;
        }

        mf.sharedMesh = marksMesh;
    }



    public int AddSkidMark(Vector3 pos, Vector3 normal, float opacity, int lastIndex)
    {
        if (opacity > 1) opacity = 1.0f;
        else if (opacity < 0) return -1;

        black.a = (byte)(opacity * 255);
        return AddSkidMark(pos, normal, black, lastIndex);
    }



    public int AddSkidMark(Vector3 pos, Vector3 normal, Color32 colour, int lastIndex)
    {
        if (colour.a == 0) return -1;

        MarkSection lastSection = null;
        Vector3 distAndDirection = Vector3.zero;
        Vector3 newPos = pos + normal * offsetVertical;
        if (lastIndex != -1)
        {
            lastSection = skidmarks[lastIndex];
            distAndDirection = newPos - lastSection.Pos;
            if (distAndDirection.sqrMagnitude < minDistanceSquared)
            {
                return lastIndex;
            }

            if (distAndDirection.sqrMagnitude > minDistanceSquared * 10)
            {
                lastIndex = -1;
                lastSection = null;
            }
        }

        colour.a = (byte)(colour.a * maxOpacity);

        MarkSection curSection = skidmarks[markIndex];

        curSection.Pos = newPos;
        curSection.Normal = normal;
        curSection.Colour = colour;
        curSection.LastIndex = lastIndex;

        if (lastSection != null)
        {
            Vector3 xDirection = Vector3.Cross(distAndDirection, normal).normalized;
            curSection.Posl = curSection.Pos + xDirection * skidWidth * 0.5f;
            curSection.Posr = curSection.Pos - xDirection * skidWidth * 0.5f;
            curSection.Tangent = new Vector4(xDirection.x, xDirection.y, xDirection.z, 1);

            if (lastSection.LastIndex == -1)
            {
                lastSection.Tangent = curSection.Tangent;
                lastSection.Posl = curSection.Pos + xDirection * skidWidth * 0.5f;
                lastSection.Posr = curSection.Pos - xDirection * skidWidth * 0.5f;
            }
        }

        UpdateSkidmarksMesh();

        int curIndex = markIndex;
        markIndex = ++markIndex % maxSkids;

        return curIndex;
    }



    void UpdateSkidmarksMesh()
    {
        MarkSection curr = skidmarks[markIndex];

        if (curr.LastIndex == -1) return;

        MarkSection last = skidmarks[curr.LastIndex];
        vertices[markIndex * 4 + 0] = last.Posl;
        vertices[markIndex * 4 + 1] = last.Posr;
        vertices[markIndex * 4 + 2] = curr.Posl;
        vertices[markIndex * 4 + 3] = curr.Posr;

        normals[markIndex * 4 + 0] = last.Normal;
        normals[markIndex * 4 + 1] = last.Normal;
        normals[markIndex * 4 + 2] = curr.Normal;
        normals[markIndex * 4 + 3] = curr.Normal;

        tangents[markIndex * 4 + 0] = last.Tangent;
        tangents[markIndex * 4 + 1] = last.Tangent;
        tangents[markIndex * 4 + 2] = curr.Tangent;
        tangents[markIndex * 4 + 3] = curr.Tangent;

        colors[markIndex * 4 + 0] = last.Colour;
        colors[markIndex * 4 + 1] = last.Colour;
        colors[markIndex * 4 + 2] = curr.Colour;
        colors[markIndex * 4 + 3] = curr.Colour;

        uvs[markIndex * 4 + 0] = new Vector2(0, 0);
        uvs[markIndex * 4 + 1] = new Vector2(1, 0);
        uvs[markIndex * 4 + 2] = new Vector2(0, 1);
        uvs[markIndex * 4 + 3] = new Vector2(1, 1);

        triangles[markIndex * 6 + 0] = markIndex * 4 + 0;
        triangles[markIndex * 6 + 2] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 1] = markIndex * 4 + 2;

        triangles[markIndex * 6 + 3] = markIndex * 4 + 2;
        triangles[markIndex * 6 + 5] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 4] = markIndex * 4 + 3;

        meshUpdated = true;
    }
}
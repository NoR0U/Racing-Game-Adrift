using UnityEngine;
using UnityEngine.Rendering;

public class Skidmarks : MonoBehaviour
{
		public Material skidmarksMaterial;
		[HideInInspector]
		public float SkidmarkWidth = 0.5f;

		private const int MaxSkidMarks = 2048;
		private const float contact_Offset = 0.02f;
		private const float MinDistance = 0.25f;
		private const float MinDistanceSquare = MinDistance * MinDistance;
		private const float MaxOpacity = 1.0f;

		class SkidMarkSection
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
		SkidMarkSection[] skidmarks;
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
			if (transform.position != Vector3.zero)
			{
				transform.position = Vector3.zero;
				transform.rotation = Quaternion.identity;
			}
		}

		protected void Start()
		{
			skidmarks = new SkidMarkSection[MaxSkidMarks];

			for (int i = 0; i < MaxSkidMarks; i++)
			{
				skidmarks[i] = new SkidMarkSection();
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

			vertices = new Vector3[MaxSkidMarks * 4];
			normals = new Vector3[MaxSkidMarks * 4];
			tangents = new Vector4[MaxSkidMarks * 4];
			colors = new Color32[MaxSkidMarks * 4];
			uvs = new Vector2[MaxSkidMarks * 4];
			triangles = new int[MaxSkidMarks * 6];

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

			SkidMarkSection lastSection = null;
			Vector3 distAndDirection = Vector3.zero;
			Vector3 newPos = pos + normal * contact_Offset;
			if (lastIndex != -1)
			{
				lastSection = skidmarks[lastIndex];
				distAndDirection = newPos - lastSection.Pos;
				if (distAndDirection.sqrMagnitude < MinDistanceSquare)
				{
					return lastIndex;
				}

				if (distAndDirection.sqrMagnitude > MinDistanceSquare * 10)
				{
					lastIndex = -1;
					lastSection = null;
				}
			}

			colour.a = (byte)(colour.a * MaxOpacity);

			SkidMarkSection curSection = skidmarks[markIndex];

			curSection.Pos = newPos;
			curSection.Normal = normal;
			curSection.Colour = colour;
			curSection.LastIndex = lastIndex;

			if (lastSection != null)
			{
				Vector3 xDirection = Vector3.Cross(distAndDirection, normal).normalized;
				curSection.Posl = curSection.Pos + xDirection * SkidmarkWidth * 0.5f;
				curSection.Posr = curSection.Pos - xDirection * SkidmarkWidth * 0.5f;
				curSection.Tangent = new Vector4(xDirection.x, xDirection.y, xDirection.z, 1);

				if (lastSection.LastIndex == -1)
				{
					lastSection.Tangent = curSection.Tangent;
					lastSection.Posl = curSection.Pos + xDirection * SkidmarkWidth * 0.5f;
					lastSection.Posr = curSection.Pos - xDirection * SkidmarkWidth * 0.5f;
				}
			}

			UpdateSkidmarksMesh();

			int curIndex = markIndex;
			markIndex = ++markIndex % MaxSkidMarks;

			return curIndex;
		}

		void UpdateSkidmarksMesh()
		{
			SkidMarkSection curr = skidmarks[markIndex];

			if (curr.LastIndex == -1) return;

			SkidMarkSection last = skidmarks[curr.LastIndex];
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

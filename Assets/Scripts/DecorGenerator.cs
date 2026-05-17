using UnityEngine;
using System.Collections.Generic;

public class DecorGenerator : MonoBehaviour
{
    [Header("Sol latéral")]
    public float sideFloorWidth = 0.5f;
    public Color sideFloorColor = new Color(0.55f, 0.35f, 0.2f);
    [Tooltip("Décalage vertical du sol latéral par rapport au chemin (négatif = en dessous)")]
    public float sideFloorYOffset = -0.05f;

    [Header("Mobilier")]
    public GameObject chairPrefab;
    public GameObject tablePrefab;
    public GameObject platePrefab;
    public GameObject cutleryPrefab;
    [Tooltip("Distance entre deux meubles le long d'un segment")]
    public float furnitureSpacing = 2.5f;
    [Tooltip("Distance entre le meuble et le bord du chemin")]
    public float furnitureOffsetFromPath = 1.2f;
    [Tooltip("Hauteur de pose de l'assiette / des couverts au-dessus de la base de la table")]
    public float tableTopHeight = 0.8f;

    private List<CombineInstance> sideFloorParts = new List<CombineInstance>();

    /// <summary>Miter uniquement sur le bord extérieur du décor (pas sur le bord collé au chemin).</summary>
    public struct CornerEndMiter
    {
        public bool isLeftBand;
        public Vector3 outerMiter;
    }

    // ---------- API publique appelée par MapGenerator ----------

    public void AddSegmentDecor(Vector3 start, Vector3 end, Vector3 right,
                                float halfW, float yStart, float yEnd,
                                CornerEndMiter? endOuterMiter = null)
    {
        AddSegmentSideFloor(start, end, right, halfW, yStart, yEnd, endOuterMiter);
        PlaceFurnitureAlongSegment(start, end, right, halfW, yStart, yEnd);
    }

    public static CornerEndMiter ComputeEndOuterMiter(Vector3 corner, Vector3 fwd, Vector3 right,
        Vector3 newFwd, Vector3 newRight, float halfW, float sideWidth)
    {
        bool leftTurn = Vector3.Cross(fwd, newFwd).y > 0f;
        float outerOff = halfW + sideWidth;
        return new CornerEndMiter
        {
            isLeftBand = leftTurn,
            outerMiter = EdgeMiter(corner, right, fwd, newRight, newFwd, outerOff, !leftTurn)
        };
    }

    public void AddCornerDecor(Vector3 platOrigin, Vector3 fwd, Vector3 right, Vector3 newFwd,
        float halfW, float platSize, float y)
    {
        AddCornerSideFloor(platOrigin, fwd, right, newFwd, halfW, platSize, y);
    }

    public void Build()
    {
        Material mat = CreateSideFloorMaterial();
        BuildSideFloorMesh(mat);
        sideFloorParts.Clear();
    }


    void AddSegmentSideFloor(Vector3 start, Vector3 end, Vector3 right,
                             float halfW, float yStart, float yEnd,
                             CornerEndMiter? endOuterMiter)
    {
        float yA = yStart + sideFloorYOffset;
        float yB = yEnd + sideFloorYOffset;

        Vector3 loEnd = end - right * (halfW + sideFloorWidth);
        if (endOuterMiter.HasValue && endOuterMiter.Value.isLeftBand)
            loEnd = endOuterMiter.Value.outerMiter;

        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                WithY(start - right * (halfW + sideFloorWidth), yA),
                WithY(start - right * halfW, yA),
                WithY(end - right * halfW, yB),
                WithY(loEnd, yB)
            ),
            transform = Matrix4x4.identity
        });

        Vector3 roEnd = end + right * (halfW + sideFloorWidth);
        if (endOuterMiter.HasValue && !endOuterMiter.Value.isLeftBand)
            roEnd = endOuterMiter.Value.outerMiter;

        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                WithY(start + right * halfW, yA),
                WithY(start + right * (halfW + sideFloorWidth), yA),
                WithY(roEnd, yB),
                WithY(end + right * halfW, yB)
            ),
            transform = Matrix4x4.identity
        });
    }


void AddCornerSideFloor(Vector3 platOrigin, Vector3 fwd, Vector3 right, Vector3 newFwd,
                        float halfW, float platSize, float y)
{
    float yFloor = y + sideFloorYOffset;

    Vector3 sw = platOrigin - right * halfW + Vector3.up * yFloor;
    Vector3 se = platOrigin + right * halfW + Vector3.up * yFloor;
    Vector3 ne = platOrigin + fwd * platSize + right * halfW + Vector3.up * yFloor;
    Vector3 nw = platOrigin + fwd * platSize - right * halfW + Vector3.up * yFloor;

    bool leftTurn = Vector3.Cross(fwd, newFwd).y > 0f;
    float turnSign = Vector3.Dot(newFwd, right);

    if (turnSign > 0)
    {
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                nw - right * sideFloorWidth,
                ne,
                ne + fwd * sideFloorWidth,
                nw - right * sideFloorWidth + fwd * sideFloorWidth
            ),
            transform = Matrix4x4.identity
        });

        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                sw - right * sideFloorWidth,
                sw,
                nw,
                nw - right * sideFloorWidth
            ),
            transform = Matrix4x4.identity
        });
    }
    else
    {
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                nw,
                ne + right * sideFloorWidth,
                ne + right * sideFloorWidth + fwd * sideFloorWidth,
                nw + fwd * sideFloorWidth
            ),
            transform = Matrix4x4.identity
        });
    }

    // Côté extérieur du virage : bord du plat collé au chemin (se-ne ou sw-nw)
    if (leftTurn)
    {
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                sw - right * sideFloorWidth,
                sw,
                nw,
                nw - right * sideFloorWidth
            ),
            transform = Matrix4x4.identity
        });
    }
    else
    {
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(se, se + right * sideFloorWidth, ne + right * sideFloorWidth, ne),
            transform = Matrix4x4.identity
        });
    }
}
    // ---------- MOBILIER ----------

    void PlaceFurnitureAlongSegment(Vector3 start, Vector3 end, Vector3 right,
                                    float halfW, float yStart, float yEnd)
    {
        if (chairPrefab == null && tablePrefab == null) return;

        float segLength = Vector3.Distance(start, end);
        int count = Mathf.FloorToInt(segLength / furnitureSpacing);
        if (count <= 0) return;

        GameObject[] cycle = { chairPrefab, tablePrefab };

        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 alongPath = Vector3.Lerp(start, end, t);
            float y = Mathf.Lerp(yStart, yEnd, t) + sideFloorYOffset;

            GameObject prefab = cycle[i % cycle.Length];
            if (prefab == null) continue;

            Vector3 leftPos  = alongPath - right * (halfW + furnitureOffsetFromPath);
            Vector3 rightPos = alongPath + right * (halfW + furnitureOffsetFromPath);

            SpawnFurniture(prefab, new Vector3(leftPos.x,  y, leftPos.z),  right);
            SpawnFurniture(prefab, new Vector3(rightPos.x, y, rightPos.z), -right);
        }
    }

    void SpawnFurniture(GameObject prefab, Vector3 pos, Vector3 facing)
    {
        Quaternion rot = Quaternion.LookRotation(facing, Vector3.up);
        GameObject obj = Instantiate(prefab, pos, rot, transform);

        if (prefab == tablePrefab)
        {
            Vector3 top = pos + Vector3.up * tableTopHeight;
            if (platePrefab != null)
                Instantiate(platePrefab, top, rot, obj.transform);
            if (cutleryPrefab != null)
                Instantiate(cutleryPrefab, top + obj.transform.right * 0.3f, rot, obj.transform);
        }
    }

    // ---------- HELPERS MESH ----------

    static Vector3 WithY(Vector3 p, float y)
    {
        p.y = y;
        return p;
    }

    static Vector3 EdgeMiter(Vector3 corner, Vector3 r1, Vector3 d1, Vector3 r2, Vector3 d2,
                             float offset, bool positiveRight)
    {
        float s = positiveRight ? 1f : -1f;
        Vector3 p1 = corner + r1 * (s * offset);
        Vector3 p2 = corner + r2 * (s * offset);
        if (TryIntersectXZ(p1, d1, p2, d2, out Vector3 hit))
            return hit;
        return p1;
    }

    static bool TryIntersectXZ(Vector3 origin1, Vector3 dir1, Vector3 origin2, Vector3 dir2, out Vector3 hit)
    {
        float dx = origin2.x - origin1.x;
        float dz = origin2.z - origin1.z;
        float det = dir1.x * dir2.z - dir1.z * dir2.x;
        if (Mathf.Abs(det) < 1e-6f)
        {
            hit = origin1;
            return false;
        }

        float t = (dx * dir2.z - dz * dir2.x) / det;
        hit = origin1 + dir1 * t;
        hit.y = origin1.y;
        return true;
    }

    Mesh CreateQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { a, b, c, d };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        mesh.RecalculateBounds();
        return mesh;
    }

    Material CreateSideFloorMaterial()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", sideFloorColor);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_Smoothness", 0.3f);
        mat.SetFloat("_Cull", 0);
        return mat;
    }

    void BuildSideFloorMesh(Material mat)
    {
        int batchSize = 100;
        for (int batch = 0; batch < sideFloorParts.Count; batch += batchSize)
        {
            int count = Mathf.Min(batchSize, sideFloorParts.Count - batch);
            var subset = new CombineInstance[count];
            for (int i = 0; i < count; i++)
                subset[i] = sideFloorParts[batch + i];

            Mesh combined = new Mesh();
            combined.CombineMeshes(subset, true, true);
            combined.RecalculateNormals();
            combined.RecalculateBounds();

            GameObject chunk = new GameObject($"SideFloorChunk_{batch / batchSize}");
            chunk.transform.parent = transform;
            chunk.AddComponent<MeshFilter>().mesh = combined;
            chunk.AddComponent<MeshRenderer>().material = mat;
            // Pas de MeshCollider : Tintin doit tomber dans le vide s'il rate un virage
        }
    }
}
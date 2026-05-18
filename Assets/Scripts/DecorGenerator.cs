using UnityEngine;
using System.Collections.Generic;

public class DecorGenerator : MonoBehaviour
{
    [Header("Sol latéral")]
    public float sideFloorWidth = 2.5f;
    public Color sideFloorColor = new Color(0.75f, 0.15f, 0.2f);
    [Tooltip("Décalage vertical du sol latéral par rapport au chemin (négatif = en dessous)")]
    public float sideFloorYOffset = -0.05f;

    [Header("Mobilier")]
    public GameObject chairPrefab;
    public GameObject tablePrefab;
    public GameObject platePrefab;
    [Tooltip("Distance entre deux meubles le long d'un segment")]
    public float furnitureSpacing = 1f;
    [Tooltip("Distance entre le meuble et le bord du chemin")]
    public float furnitureOffsetFromPath = 1.2f;
    [Tooltip("Hauteur de pose des assiettes au-dessus de la base de la table")]
    public float tableTopHeight = 0.8f;
    [Tooltip("Échelle appliquée aux assiettes (1 = taille du prefab)")]
    public float plateScale = 1.8f;          // ← nouveau, assiettes plus grosses
    [Tooltip("Écart entre les deux assiettes sur la table")]
    public float plateSpacing = 0.2f;        // ← nouveau, espacement des 2 assiettes

    [Header("Mur (bord extérieur)")]
    public GameObject wallPrefab;
    [Tooltip("Distance entre 2 instances du mur (selon la taille de ton asset)")]
    public float wallSpacing = 1f;

    [Header("Lumière (sur les plateformes de virage)")]
    [Tooltip("Optionnel - si vide, on crée une Point Light avec les valeurs ci-dessous")]
    public GameObject lightPrefab;
    [Tooltip("Hauteur de la lumière au-dessus de la plateforme")]
    public float lightHeight = 3f;
    public float lightIntensity = 2f;
    public float lightRange = 6f;
    public Color lightColor = new Color(1f, 0.85f, 0.6f);  // teinte chaude
    
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
        Vector3 nextTurnDirection)
    {
        AddSegmentSideFloor(start, end, right, halfW, yStart, yEnd, nextTurnDirection);
        PlaceFurnitureAlongSegment(start, end, right, halfW, yStart, yEnd, nextTurnDirection);
        PlaceWallsAlongSegment(start, end, right, halfW, yStart, yEnd, nextTurnDirection);  // ← ajout
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
        PlaceWallsAroundCorner(platOrigin, fwd, right, newFwd, halfW, platSize, y);  // ← ajout
        PlaceCornerLight(platOrigin + fwd * platSize * 0.5f, y);                      // ← ajout
    }

    public void Build()
    {
        Material mat = CreateSideFloorMaterial();
        BuildSideFloorMesh(mat);
        sideFloorParts.Clear();
    }


    void AddSegmentSideFloor(Vector3 start, Vector3 end, Vector3 right,
                         float halfW, float yStart, float yEnd,
                         Vector3 nextTurnDirection)
    {
        float yA = yStart + sideFloorYOffset;
        float yB = yEnd + sideFloorYOffset;

        // Quelle bande raccourcir au bout ? Celle du côté intérieur du virage à venir.
        //   tourne à droite (newFwd · right > 0) -> raccourcir la bande droite
        //   tourne à gauche (newFwd · right < 0) -> raccourcir la bande gauche
        //   pas de virage suivant -> pas de troncature
        float turnSign = Vector3.Dot(nextTurnDirection, right);
        bool shortenRight = turnSign > 0.5f;
        bool shortenLeft  = turnSign < -0.5f;

        float segLength = Vector3.Distance(start, end);
        float trunc = Mathf.Min(sideFloorWidth, segLength * 0.9f);
        Vector3 truncEnd = end - (end - start).normalized * trunc;
        float tTrunc = (segLength - trunc) / segLength;
        float yTruncEnd = Mathf.Lerp(yA, yB, tTrunc);

        // Bande gauche
        Vector3 lEnd = shortenLeft ? truncEnd : end;
        float lYEnd = shortenLeft ? yTruncEnd : yB;
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                start - right * (halfW + sideFloorWidth) + Vector3.up * yA,
                start - right * halfW                    + Vector3.up * yA,
                lEnd  - right * halfW                    + Vector3.up * lYEnd,
                lEnd  - right * (halfW + sideFloorWidth) + Vector3.up * lYEnd
            ),
            transform = Matrix4x4.identity
        });

        // Bande droite
        Vector3 rEnd = shortenRight ? truncEnd : end;
        float rYEnd = shortenRight ? yTruncEnd : yB;
        sideFloorParts.Add(new CombineInstance
        {
            mesh = CreateQuad(
                start + right * halfW                    + Vector3.up * yA,
                start + right * (halfW + sideFloorWidth) + Vector3.up * yA,
                rEnd  + right * (halfW + sideFloorWidth) + Vector3.up * rYEnd,
                rEnd  + right * halfW                    + Vector3.up * rYEnd
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
        float halfW, float yStart, float yEnd,
        Vector3 nextTurnDirection)
    {
        if (chairPrefab == null && tablePrefab == null) return;

        float segLength = Vector3.Distance(start, end);
        int count = Mathf.FloorToInt(segLength / furnitureSpacing);
        if (count <= 0) return;

        // Longueur effective de chaque côté (raccourcie côté intérieur du virage)
        float turnSign = Vector3.Dot(nextTurnDirection, right);
        float trunc = Mathf.Min(sideFloorWidth, segLength * 0.9f);
        float effectiveLeftLen  = (turnSign < -0.5f) ? segLength - trunc : segLength;
        float effectiveRightLen = (turnSign >  0.5f) ? segLength - trunc : segLength;

        GameObject[] cycle = { chairPrefab, tablePrefab, chairPrefab };

        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 alongPath = Vector3.Lerp(start, end, t);
            float y = Mathf.Lerp(yStart, yEnd, t) + sideFloorYOffset;
            float distFromStart = t * segLength;

            GameObject prefab = cycle[i % cycle.Length];
            if (prefab == null) continue;

            // Côté gauche
            if (distFromStart <= effectiveLeftLen)
            {
                Vector3 leftPos = alongPath - right * (halfW + furnitureOffsetFromPath);
                SpawnFurniture(prefab, new Vector3(leftPos.x, y, leftPos.z), right);
            }

            // Côté droit
            if (distFromStart <= effectiveRightLen)
            {
                Vector3 rightPos = alongPath + right * (halfW + furnitureOffsetFromPath);
                SpawnFurniture(prefab, new Vector3(rightPos.x, y, rightPos.z), -right);
            }
        }
    }

    void SpawnFurniture(GameObject prefab, Vector3 pos, Vector3 facing)
    {
        Quaternion rot = Quaternion.LookRotation(facing, Vector3.up);
        GameObject obj = Instantiate(prefab, pos, rot, transform);

        if (prefab == tablePrefab && platePrefab != null)
        {
            Vector3 top = pos + Vector3.up * tableTopHeight;

            Vector3 sideAxis = obj.transform.right;

            GameObject plate1 = Instantiate(platePrefab, top + sideAxis * plateSpacing,  rot, obj.transform);
            GameObject plate2 = Instantiate(platePrefab, top - sideAxis * plateSpacing,  rot, obj.transform);

            plate1.transform.localScale *= plateScale;
            plate2.transform.localScale *= plateScale;
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
    // ---------- MURS ----------

void PlaceWallsAlongSegment(Vector3 start, Vector3 end, Vector3 right,
    float halfW, float yStart, float yEnd, Vector3 nextTurnDirection)
{
    if (wallPrefab == null) return;

    float yA = yStart + sideFloorYOffset;
    float yB = yEnd   + sideFloorYOffset;

    // Mêmes troncatures que le sol latéral
    float turnSign = Vector3.Dot(nextTurnDirection, right);
    bool shortenRight = turnSign >  0.5f;
    bool shortenLeft  = turnSign < -0.5f;

    float segLength = Vector3.Distance(start, end);
    float trunc = Mathf.Min(sideFloorWidth, segLength * 0.9f);
    Vector3 segDir = (end - start).normalized;
    Vector3 truncEnd = end - segDir * trunc;
    float tTrunc = (segLength - trunc) / segLength;
    float yTruncEnd = Mathf.Lerp(yA, yB, tTrunc);

    Vector3 outer = right * (halfW + sideFloorWidth);

    // Bande gauche (mur extérieur)
    Vector3 lStart = start - outer + Vector3.up * yA;
    Vector3 lEnd   = (shortenLeft ? truncEnd : end) - outer + Vector3.up * (shortenLeft ? yTruncEnd : yB);
    SpawnWallLine(lStart, lEnd, right);   // mur face vers la droite (vers le chemin)

    // Bande droite (mur extérieur)
    Vector3 rStart = start + outer + Vector3.up * yA;
    Vector3 rEnd   = (shortenRight ? truncEnd : end) + outer + Vector3.up * (shortenRight ? yTruncEnd : yB);
    SpawnWallLine(rStart, rEnd, -right);  // mur face vers la gauche (vers le chemin)
}

void PlaceWallsAroundCorner(Vector3 platOrigin, Vector3 fwd, Vector3 right, Vector3 newFwd,
    float halfW, float platSize, float y)
{
    if (wallPrefab == null) return;

    float yFloor = y + sideFloorYOffset;
    Vector3 sw = platOrigin - right * halfW + Vector3.up * yFloor;
    Vector3 se = platOrigin + right * halfW + Vector3.up * yFloor;
    Vector3 ne = platOrigin + fwd * platSize + right * halfW + Vector3.up * yFloor;
    Vector3 nw = platOrigin + fwd * platSize - right * halfW + Vector3.up * yFloor;

    float turnSign = Vector3.Dot(newFwd, right);

    if (turnSign > 0)
    {
        // Virage à droite -> culs-de-sac à l'OUEST et au NORD
        Vector3 swOuter  = sw - right * sideFloorWidth;
        Vector3 nwOuter  = nw - right * sideFloorWidth;
        Vector3 nwOuterN = nwOuter + fwd * sideFloorWidth;
        Vector3 neN      = ne + fwd * sideFloorWidth;

        SpawnWallLine(swOuter,  nwOuter,  right);    // bord ouest, face est
        SpawnWallLine(nwOuter,  nwOuterN, fwd);      // petit segment du coin NW
        SpawnWallLine(nwOuterN, neN,      -fwd);     // bord nord, face sud
    }
    else
    {
        // Virage à gauche -> culs-de-sac à l'EST et au NORD
        Vector3 seOuter  = se + right * sideFloorWidth;
        Vector3 neOuter  = ne + right * sideFloorWidth;
        Vector3 neOuterN = neOuter + fwd * sideFloorWidth;
        Vector3 nwN      = nw + fwd * sideFloorWidth;

        SpawnWallLine(seOuter,  neOuter,  -right);
        SpawnWallLine(neOuter,  neOuterN, fwd);
        SpawnWallLine(neOuterN, nwN,      -fwd);
    }
}

void SpawnWallLine(Vector3 from, Vector3 to, Vector3 facing)
{
    if (wallPrefab == null) return;
    float length = Vector3.Distance(from, to);
    int count = Mathf.Max(1, Mathf.FloorToInt(length / wallSpacing));
    Quaternion rot = Quaternion.LookRotation(facing, Vector3.up);

    for (int i = 0; i < count; i++)
    {
        float t = (i + 0.5f) / count;
        Vector3 pos = Vector3.Lerp(from, to, t);
        Instantiate(wallPrefab, pos, rot, transform);
    }
}


    void PlaceCornerLight(Vector3 platCenter, float y)
    {
        Vector3 pos = platCenter + Vector3.up * (y + lightHeight);

        if (lightPrefab != null)
        {
            Instantiate(lightPrefab, pos, Quaternion.identity, transform);
        }
        else
        {
            GameObject lightObj = new GameObject("CornerLight");
            lightObj.transform.position = pos;
            lightObj.transform.parent = transform;

            Light light = lightObj.AddComponent<Light>();
            light.type      = LightType.Point;
            light.color     = lightColor;
            light.intensity = lightIntensity;
            light.range     = lightRange;
        }
    }
}
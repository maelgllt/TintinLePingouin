using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Chemin")]
    public int totalSegments = 10;
    public float cubeSize = 1f;
    public float pathWidth = 3f;

    [Header("Longueur des segments")]
    public int minStraight = 5;
    public int maxStraight = 15;

    [Header("Pente")]
    public float slopePerBlock = 0.15f;

    // --- NOUVEAU : Lien vers ton système ---
    [Header("Système QTE")]
    [Tooltip("Glisse ici le GameManager qui possède le script QTEController")]
    public QTEController mainQteController; 
    // ---------------------------------------

    public List<Vector3> pathPoints { get; private set; } = new List<Vector3>();
    public List<Vector3> pathDirections { get; private set; } = new List<Vector3>();

    void Start()
    {
        GeneratePath();
    }

    void GeneratePath()
    {
        Vector3[] dirs = {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0)
        };

        var meshParts = new List<CombineInstance>();
        float halfW = (pathWidth * cubeSize) / 2f;
        float platSize = pathWidth * cubeSize;

        Vector3 pos = Vector3.zero;
        int dir = 0;
        float y = 0f;

        Material pathMat = CreateMaterial();

        for (int seg = 0; seg < totalSegments; seg++)
        {
            int length = Random.Range(minStraight, maxStraight + 1);
            Vector3 fwd = dirs[dir];
            Vector3 right = new Vector3(fwd.z, 0, -fwd.x);

            // === PENTE ===
            float yEnd = y - slopePerBlock * length;

            Vector3 slopeStart = pos;
            Vector3 slopeEnd = pos + fwd * length * cubeSize;

            meshParts.Add(new CombineInstance {
                mesh = CreateQuad(
                    slopeStart - right * halfW + Vector3.up * y,
                    slopeStart + right * halfW + Vector3.up * y,
                    slopeEnd + right * halfW + Vector3.up * yEnd,
                    slopeEnd - right * halfW + Vector3.up * yEnd
                ),
                transform = Matrix4x4.identity
            });

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / length;
                pathPoints.Add(pos + fwd * i * cubeSize + Vector3.up * Mathf.Lerp(y, yEnd, t));
                pathDirections.Add(fwd);
            }

            pos = slopeEnd;
            y = yEnd;

            // === PLATEFORME PLATE (même taille que le chemin) ===
            int turn = Random.value < 0.5f ? 1 : 3;
            int newDir = (dir + turn) % 4;
            Vector3 newFwd = dirs[newDir];
            Vector3 newRight = new Vector3(newFwd.z, 0, -newFwd.x);

            Vector3 platOrigin = pos; 

            Vector3 pa = platOrigin - right * halfW + Vector3.up * y;
            Vector3 pb = platOrigin + right * halfW + Vector3.up * y;
            Vector3 pc = platOrigin + fwd * platSize + right * halfW + Vector3.up * y;
            Vector3 pd = platOrigin + fwd * platSize - right * halfW + Vector3.up * y;

            meshParts.Add(new CombineInstance {
                mesh = CreateQuad(pa, pb, pc, pd),
                transform = Matrix4x4.identity
            });

            // Point central de la plateforme
            Vector3 platCenter = platOrigin + fwd * platSize * 0.5f;
            pathPoints.Add(platCenter + Vector3.up * y);
            pathDirections.Add(newFwd);

            // --- NOUVEAU : On place ton déclencheur de QTE pile au centre du virage ! ---
            // On ne le place pas au dernier segment car il n'y a plus de virage après.
            if (seg < totalSegments - 1) 
            {
                CreateQTETrigger(platCenter + Vector3.up * y);
            }
            // ----------------------------------------------------------------------------

            Vector3 platEnd = platOrigin + fwd * platSize * 0.5f;

            if (turn == 1)
                pos = platEnd + right * halfW + newFwd * halfW;
            else
                pos = platEnd - right * halfW + newFwd * halfW;

            pos = platEnd + newFwd * halfW;
            pos = platOrigin + fwd * platSize * 0.5f + newFwd * halfW;

            dir = newDir;
        }

        BuildFinalMesh(meshParts, pathMat);
        Debug.Log($"Chemin : {totalSegments} segments, {pathPoints.Count} points");
    }

    Mesh CreateQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] { a, b, c, d };
        
        // LA MAGIE EST ICI : On a inversé l'ordre pour que le sol regarde vers le ciel (0, 2, 1 au lieu de 0, 1, 2)
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 }; 
        
        mesh.normals = new Vector3[] {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    Material CreateMaterial()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.7f, 0.85f, 0.95f));
        mat.SetFloat("_Metallic", 0.3f);
        mat.SetFloat("_Smoothness", 0.8f);
        mat.SetFloat("_Cull", 0);
        return mat;
    }

    void BuildFinalMesh(List<CombineInstance> parts, Material mat)
    {
        int batchSize = 100;
        for (int batch = 0; batch < parts.Count; batch += batchSize)
        { 
            int count = Mathf.Min(batchSize, parts.Count - batch);
            var subset = new CombineInstance[count];
            for (int i = 0; i < count; i++)
                subset[i] = parts[batch + i];

            Mesh combined = new Mesh();
            combined.CombineMeshes(subset, true, true);
            combined.RecalculateNormals();
            combined.RecalculateBounds();

            GameObject chunk = new GameObject($"PathChunk_{batch / batchSize}");
            chunk.transform.parent = transform;
            chunk.AddComponent<MeshFilter>().mesh = combined;
            chunk.AddComponent<MeshRenderer>().material = mat;
            chunk.AddComponent<MeshCollider>().sharedMesh = combined;
        }
    }

    // --- NOUVEAU : Fonction qui fabrique un trigger QTE de toutes pièces ---
    void CreateQTETrigger(Vector3 position)
    {
        if (mainQteController == null)
        {
            Debug.LogError("⚠️ Attention ! Tu as oublié de glisser le QTEController dans le MapGenerator !");
            return;
        }

        // On crée un objet vide
        GameObject triggerObj = new GameObject("QTE_Virage_Trigger");
        
        // On le place un peu au-dessus du sol pour que Tintin puisse le toucher
        triggerObj.transform.position = position + new Vector3(0, 1f, 0); 
        triggerObj.transform.parent = this.transform; // On range ça proprement

        // On lui ajoute un BoxCollider configuré en "Trigger"
        BoxCollider box = triggerObj.AddComponent<BoxCollider>();
        box.isTrigger = true;
        // On rend la zone de détection de la même taille que la largeur du chemin
        box.size = new Vector3(pathWidth, 3f, pathWidth); 

        // On lui ajoute ton script et on le connecte à ton QTEController !
        QTE_Trigger qteScript = triggerObj.AddComponent<QTE_Trigger>();
        qteScript.qteController = mainQteController;
    }
}
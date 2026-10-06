using UnityEngine;
using UnityEngine.UI;
using Game.Items;
using System.Collections.Generic;

namespace Game.GameDirector
{
    /// <summary>
    /// 战争迷雾管理器：单例，完全独立，其它脚本不需要知道它的存在。
    /// </summary>
    public class FogOfWarDirector : MonoBehaviour
    {
        public static FogOfWarDirector Instance { get; private set; }

        public RenderTexture DisplayRT => displayRT;
        public Vector3 FogCenter => fogCenter;

        private Mesh visibleMesh;
        private Mesh fogMesh;
        private MeshRenderer visibleRenderer;
        private MeshRenderer fogRenderer;
        private Transform player;
        private Camera mainCam;
        private Vector3 fogCenter;
        private float updateTimer;
        private int obstacleMask;

        [Header("迷雾Shader（从 Assets/FogOfWar 拖入）")]
        [SerializeField] private Shader visibleAreaShader;
        [SerializeField] private Shader fogOverlayShader;
        [SerializeField] private Shader blurShader;
        [SerializeField] private Shader interpShader;

        // RT方案：每帧渲染+模糊+指数平滑
        private RenderTexture fogRT;
        private RenderTexture displayRT;
        private RenderTexture blurRT;
        private Camera fogCamera;
        private Material blurMat;
        private Material interpMat;
        private const int RtResolution = 256;
        private const int BlurPasses = 3;

        private readonly List<Vector3> visibleVerts = new List<Vector3>();
        private readonly List<int> visibleTris = new List<int>();

        // 实体按Tag自动识别
        private readonly List<Renderer> entities = new List<Renderer>();
        private readonly List<Graphic> entityGuis = new List<Graphic>();
        private float scanTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            obstacleMask = LayerMask.GetMask(FogOfWarConfig.ObstacleLayerName);
            mainCam = Camera.main;

            fogRT = NewRT();
            displayRT = NewRT();
            blurRT = NewRT();

            int fogLayer = LayerMask.NameToLayer("Ignore Raycast");
            GameObject camGo = new GameObject("FogCamera");
            camGo.transform.SetParent(transform, false);
            fogCamera = camGo.AddComponent<Camera>();
            fogCamera.orthographic = true;
            fogCamera.orthographicSize = FogOfWarConfig.ViewRadius + FogOfWarConfig.RTMargin;
            fogCamera.aspect = 1f;
            fogCamera.clearFlags = CameraClearFlags.SolidColor;
            fogCamera.backgroundColor = Color.black;
            fogCamera.cullingMask = 1 << fogLayer;
            fogCamera.targetTexture = fogRT;
            fogCamera.enabled = false;
            fogCamera.nearClipPlane = 0.1f;
            fogCamera.farClipPlane = 100f;

            CreateMeshObject("VisibleArea", visibleAreaShader, isFogOverlay: false, sortOrder: 0, out visibleMesh, out visibleRenderer);
            visibleRenderer.gameObject.layer = fogLayer;

            blurMat = new Material(blurShader);
            blurMat.SetFloat("_BlurRadius", 1.5f);
            interpMat = new Material(interpShader);

            CreateMeshObject("FogOverlay", fogOverlayShader, isFogOverlay: true, sortOrder: 0, out fogMesh, out fogRenderer);
            fogRenderer.material.SetTexture("_FogTex", displayRT);
            fogRenderer.material.SetFloat("_RTSize", (FogOfWarConfig.ViewRadius + FogOfWarConfig.RTMargin) * 2);

            FindPlayer();
        }

        private RenderTexture NewRT()
        {
            RenderTexture rt = new RenderTexture(RtResolution, RtResolution, 0, RenderTextureFormat.R8);
            rt.filterMode = FilterMode.Bilinear;
            rt.wrapMode = TextureWrapMode.Clamp;
            return rt;
        }

        private void CreateMeshObject(string name, Shader shader, bool isFogOverlay, int sortOrder, out Mesh mesh, out MeshRenderer renderer)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            renderer = go.AddComponent<MeshRenderer>();
            mesh = new Mesh { name = name + "Mesh" };
            mf.mesh = mesh;

            if (shader == null)
            {
                Debug.LogError("[FogOfWar] 找不到Shader: " + name);
                return;
            }
            Material mat = new Material(shader);
            if (isFogOverlay) mat.color = FogOfWarConfig.FogColor;
            renderer.material = mat;
            renderer.sortingOrder = sortOrder + 100;
            try { renderer.sortingLayerName = FogOfWarConfig.FogSortingLayerName; }
            catch { Debug.LogWarning("[FogOfWar] Sorting Layer不存在"); }
        }

        private void FindPlayer()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        private void Update()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null)
            {
                fogCenter = mainCam.transform.position;

                if (visibleRenderer != null)
                    visibleRenderer.transform.position = new Vector3(fogCenter.x, fogCenter.y, FogOfWarConfig.FogZPosition);

                if (fogCamera != null)
                {
                    fogCamera.transform.position = new Vector3(fogCenter.x, fogCenter.y, -10);
                    fogCamera.Render();
                    if (blurMat != null)
                    {
                        for (int i = 0; i < BlurPasses; i++)
                        {
                            Graphics.Blit(fogRT, blurRT, blurMat, 0);
                            Graphics.Blit(blurRT, fogRT, blurMat, 1);
                        }
                    }
                    // 指数平滑：displayRT每帧向新形状靠15%
                    if (interpMat != null && displayRT != null)
                    {
                        interpMat.SetTexture("_MainTex", displayRT);
                        interpMat.SetTexture("_NewTex", fogRT);
                        float smoothT = 1f - Mathf.Exp(-FogOfWarConfig.FogSmoothSpeed * Time.deltaTime);
                        interpMat.SetFloat("_T", smoothT);
                        Graphics.Blit(displayRT, blurRT, interpMat, 0);
                        Graphics.Blit(blurRT, displayRT);
                    }
                }
                if (fogRenderer != null)
                    fogRenderer.material.SetVector("_PlayerPos", fogCenter);
            }

            updateTimer += Time.deltaTime;
            if (updateTimer >= FogOfWarConfig.UpdateInterval)
            {
                UpdateFog();
                updateTimer = 0f;
            }

            scanTimer += Time.deltaTime;
            if (scanTimer >= FogOfWarConfig.EntityScanInterval)
            {
                ScanEntities();
                scanTimer = 0f;
            }
        }

        private void UpdateFog()
        {
            if (player == null) { FindPlayer(); if (player == null) return; }
            if (mainCam == null) mainCam = Camera.main;

            UpdateVisibleMesh();
            UpdateFogMesh();
            UpdateEntityVisibility();
        }

        private void UpdateVisibleMesh()
        {
            visibleVerts.Clear();
            visibleTris.Clear();
            visibleVerts.Add(new Vector3(0, 0, 0));

            int count = FogOfWarConfig.RaycastCount;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * 360f;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
                RaycastHit2D hit = Physics2D.Raycast(fogCenter, dir, FogOfWarConfig.ViewRadius, obstacleMask);
                Vector2 p = hit.collider != null ? hit.point : (Vector2)fogCenter + dir * FogOfWarConfig.ViewRadius;
                p += dir * FogOfWarConfig.ObstaclePadding;
                visibleVerts.Add(new Vector3(p.x - fogCenter.x, p.y - fogCenter.y, 0));
            }

            for (int i = 1; i < count; i++)
            {
                visibleTris.Add(0); visibleTris.Add(i); visibleTris.Add(i + 1);
            }
            visibleTris.Add(0); visibleTris.Add(count); visibleTris.Add(1);

            visibleMesh.Clear();
            visibleMesh.vertices = visibleVerts.ToArray();
            visibleMesh.triangles = visibleTris.ToArray();
            visibleMesh.RecalculateBounds();
        }

        private void UpdateFogMesh()
        {
            float z = FogOfWarConfig.FogZPosition;
            Vector3 bl = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 10f));
            Vector3 tr = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10f));
            Vector3 tl = new Vector3(bl.x, tr.y, z);
            Vector3 br = new Vector3(tr.x, bl.y, z);
            bl = new Vector3(bl.x, bl.y, z);
            tr = new Vector3(tr.x, tr.y, z);

            fogMesh.Clear();
            fogMesh.vertices = new Vector3[] { bl, tl, tr, br };
            fogMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            fogMesh.RecalculateBounds();
        }

        private void ScanEntities()
        {
            entities.Clear();
            entityGuis.Clear();
            Renderer[] all = FindObjectsOfType<Renderer>();
            for (int i = 0; i < all.Length; i++)
            {
                if (ShouldControl(all[i].gameObject)) entities.Add(all[i]);
            }
            Graphic[] allGui = FindObjectsOfType<Graphic>();
            for (int i = 0; i < allGui.Length; i++)
            {
                if (ShouldControl(allGui[i].gameObject)) entityGuis.Add(allGui[i]);
            }
        }

        private bool ShouldControl(GameObject go)
        {
            Transform t = go.transform;
            while (t != null)
            {
                if (!t.CompareTag("Untagged"))
                {
                    if (t.CompareTag("Exit")) return false;
                    if (t.CompareTag("Player")) return false;
                    return true;
                }
                t = t.parent;
            }
            return false;
        }

        public bool IsPointVisible(Vector3 worldPos)
        {
            if (player == null) return false;
            Vector2 dir = worldPos - player.position;
            float dist = dir.magnitude;
            if (dist > FogOfWarConfig.ViewRadius) return false;
            if (dist < 0.001f) return true;
            RaycastHit2D hit = Physics2D.Raycast(player.position, dir.normalized, dist, obstacleMask);
            return hit.collider == null;
        }

        private void UpdateEntityVisibility()
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                Renderer r = entities[i];
                if (r == null) { entities.RemoveAt(i); continue; }
                r.enabled = IsPointVisible(r.transform.position);
            }
            for (int i = entityGuis.Count - 1; i >= 0; i--)
            {
                Graphic g = entityGuis[i];
                if (g == null) { entityGuis.RemoveAt(i); continue; }
                g.enabled = IsPointVisible(g.transform.position);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

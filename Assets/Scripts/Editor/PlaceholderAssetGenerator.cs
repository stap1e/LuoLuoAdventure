#if UNITY_EDITOR
using LuoLuoTrip.Combat;
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    public static class PlaceholderAssetGenerator
    {
        private const string PlaceholderRoot = "Assets/Art/Placeholders";
        private const string PrefabFolder = "Assets/Art/Placeholders/Prefabs";
        private const string MaterialFolder = "Assets/Art/Placeholders/Materials";

        public static readonly string PlayerCommanderPrefab = $"{PrefabFolder}/PH_PlayerCommander_Cylinder.prefab";
        public static readonly string MechaMinionPrefab = $"{PrefabFolder}/PH_MechaMinion_Cylinder.prefab";
        public static readonly string BeastMinionPrefab = $"{PrefabFolder}/PH_BeastMinion_Cylinder.prefab";
        public static readonly string CityLordPrefab = $"{PrefabFolder}/PH_CityLord_Cylinder.prefab";
        public static readonly string WarKingPrefab = $"{PrefabFolder}/PH_WarKing_Cylinder.prefab";
        public static readonly string ConvoyPrefab = $"{PrefabFolder}/PH_Convoy_Cylinder.prefab";
        public static readonly string EnergyNodePrefab = $"{PrefabFolder}/PH_EnergyNode_Cylinder.prefab";
        public static readonly string ObjectiveMarkerPrefab = $"{PrefabFolder}/PH_ObjectiveMarker_Cylinder.prefab";

        private static readonly Color PlayerColor = new Color(0.2f, 0.6f, 1f);
        private static readonly Color MechaColor = new Color(0.55f, 0.55f, 0.6f);
        private static readonly Color BeastColor = new Color(0.7f, 0.25f, 0.2f);
        private static readonly Color LeaderColor = new Color(1f, 0.85f, 0.1f);
        private static readonly Color NeutralColor = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color EnergyColor = new Color(0.2f, 1f, 0.4f);
        private static readonly Color ObjectiveColor = new Color(1f, 1f, 0.3f);

        [MenuItem("LuoLuoTrip/Setup/Generate Placeholder Assets")]
        public static void GenerateAll()
        {
            LuoLuoTripSetupMenu.EnsureFolderPublic(PlaceholderRoot);
            LuoLuoTripSetupMenu.EnsureFolderPublic(PrefabFolder);
            LuoLuoTripSetupMenu.EnsureFolderPublic(MaterialFolder);

            CreateMaterial("MAT_PH_Player", PlayerColor);
            CreateMaterial("MAT_PH_Mecha", MechaColor);
            CreateMaterial("MAT_PH_Beast", BeastColor);
            CreateMaterial("MAT_PH_Leader", LeaderColor);
            CreateMaterial("MAT_PH_Neutral", NeutralColor);
            CreateMaterial("MAT_PH_Energy", EnergyColor);
            CreateMaterial("MAT_PH_Objective", ObjectiveColor);

            CreateCharacterPrefab(PlayerCommanderPrefab, "PH_PlayerCommander_Cylinder",
                "MAT_PH_Player", new Vector3(0.5f, 1f, 0.5f), height: 1f);

            CreateCharacterPrefab(MechaMinionPrefab, "PH_MechaMinion_Cylinder",
                "MAT_PH_Mecha", new Vector3(0.4f, 0.8f, 0.4f), height: 0.8f);

            CreateCharacterPrefab(BeastMinionPrefab, "PH_BeastMinion_Cylinder",
                "MAT_PH_Beast", new Vector3(0.45f, 0.85f, 0.45f), height: 0.85f);

            CreateCharacterPrefab(CityLordPrefab, "PH_CityLord_Cylinder",
                "MAT_PH_Leader", new Vector3(0.55f, 1.3f, 0.55f), height: 1.3f);

            CreateCharacterPrefab(WarKingPrefab, "PH_WarKing_Cylinder",
                "MAT_PH_Leader", new Vector3(0.7f, 1.5f, 0.7f), height: 1.5f);

            CreateObjectivePrefab(ConvoyPrefab, "PH_Convoy_Cylinder",
                "MAT_PH_Neutral", new Vector3(0.8f, 0.6f, 1.5f),
                PrimitiveType.Cylinder, rotateVisual: true);

            CreateObjectivePrefab(EnergyNodePrefab, "PH_EnergyNode_Cylinder",
                "MAT_PH_Energy", new Vector3(0.6f, 0.4f, 0.6f),
                PrimitiveType.Cylinder, rotateVisual: false);

            CreateObjectivePrefab(ObjectiveMarkerPrefab, "PH_ObjectiveMarker_Cylinder",
                "MAT_PH_Objective", new Vector3(0.15f, 1.2f, 0.15f),
                PrimitiveType.Cylinder, rotateVisual: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LuoLuoTrip] Placeholder assets generated under " + PlaceholderRoot);
        }

        public static bool ArePrefabsGenerated()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCommanderPrefab) != null;
        }

        public static GameObject GetPrefab(string path)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void CreateMaterial(string name, Color color)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.name = name;
            mat.color = color;
            EditorUtility.SetDirty(mat);
        }

        private static void CreateCharacterPrefab(string prefabPath, string rootName, string materialName, Vector3 visualScale, float height)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return;

            var root = new GameObject(rootName);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);

            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());
            cylinder.transform.SetParent(visual.transform, false);
            cylinder.transform.localPosition = Vector3.zero;
            cylinder.transform.localRotation = Quaternion.identity;
            cylinder.transform.localScale = visualScale;

            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materialName}.mat");
            var renderer = cylinder.GetComponent<Renderer>();
            if (mat != null && renderer != null)
                renderer.material = mat;

            var collision = new GameObject("Collision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            var capsule = collision.AddComponent<CapsuleCollider>();
            capsule.height = height;
            capsule.radius = Mathf.Max(visualScale.x, visualScale.z) * 0.5f;
            capsule.center = Vector3.zero;

            var marker = new GameObject("Marker");
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, height + 0.3f, 0f);

            root.AddComponent<CharacterEntity>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            EditorUtility.SetDirty(prefab);
        }

        private static void CreateObjectivePrefab(string prefabPath, string rootName, string materialName, Vector3 visualScale, PrimitiveType shape, bool rotateVisual)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return;

            var root = new GameObject(rootName);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, visualScale.y * 0.5f, 0f);

            var prim = GameObject.CreatePrimitive(shape);
            Object.DestroyImmediate(prim.GetComponent<Collider>());
            prim.transform.SetParent(visual.transform, false);
            prim.transform.localPosition = Vector3.zero;
            if (rotateVisual)
                prim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            else
                prim.transform.localRotation = Quaternion.identity;
            prim.transform.localScale = visualScale;

            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materialName}.mat");
            var renderer = prim.GetComponent<Renderer>();
            if (mat != null && renderer != null)
                renderer.material = mat;

            var collision = new GameObject("Collision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, visualScale.y * 0.5f, 0f);
            var box = collision.AddComponent<BoxCollider>();
            box.size = visualScale;
            box.center = Vector3.zero;

            var marker = new GameObject("Marker");
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, visualScale.y + 0.3f, 0f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            EditorUtility.SetDirty(prefab);
        }
    }
}
#endif

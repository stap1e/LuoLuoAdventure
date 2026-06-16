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

        private static readonly Color PlayerAccentColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color MechaAccentColor = new Color(0.2f, 0.4f, 0.7f);
        private static readonly Color BeastAccentColor = new Color(0.4f, 0.1f, 0.05f);
        private static readonly Color WheelColor = new Color(0.15f, 0.15f, 0.18f);

        [MenuItem("LuoLuoTrip/Setup/Generate Placeholder Assets")]
        public static void GenerateAll()
        {
            GenerateInternal(forceRegenerate: false);
        }

        [MenuItem("LuoLuoTrip/Setup/Regenerate Enhanced Placeholders (Force)")]
        public static void RegenerateAllForce()
        {
            GenerateInternal(forceRegenerate: true);
        }

        private static void GenerateInternal(bool forceRegenerate)
        {
            LuoLuoTripSetupMenu.EnsureFolderPublic(PlaceholderRoot);
            LuoLuoTripSetupMenu.EnsureFolderPublic(PrefabFolder);
            LuoLuoTripSetupMenu.EnsureFolderPublic(MaterialFolder);

            CreateMaterial("MAT_PH_Player", PlayerColor);
            CreateMaterial("MAT_PH_PlayerAccent", PlayerAccentColor);
            CreateMaterial("MAT_PH_Mecha", MechaColor);
            CreateMaterial("MAT_PH_MechaAccent", MechaAccentColor);
            CreateMaterial("MAT_PH_Beast", BeastColor);
            CreateMaterial("MAT_PH_BeastAccent", BeastAccentColor);
            CreateMaterial("MAT_PH_Leader", LeaderColor);
            CreateMaterial("MAT_PH_Neutral", NeutralColor);
            CreateMaterial("MAT_PH_Energy", EnergyColor);
            CreateMaterial("MAT_PH_Objective", ObjectiveColor);
            CreateMaterial("MAT_PH_Wheel", WheelColor);

            if (forceRegenerate)
                DeleteAllPlaceholderPrefabs();

            BuildEnhancedCharacterPrefab(PlayerCommanderPrefab, "PH_PlayerCommander_Cylinder", EnhancedKind.Commander);
            BuildEnhancedCharacterPrefab(MechaMinionPrefab, "PH_MechaMinion_Cylinder", EnhancedKind.MechaMinion);
            BuildEnhancedCharacterPrefab(BeastMinionPrefab, "PH_BeastMinion_Cylinder", EnhancedKind.BeastMinion);
            BuildEnhancedCharacterPrefab(CityLordPrefab, "PH_CityLord_Cylinder", EnhancedKind.MechaCaptain);
            BuildEnhancedCharacterPrefab(WarKingPrefab, "PH_WarKing_Cylinder", EnhancedKind.BeastElite);

            BuildEnhancedObjectivePrefab(ConvoyPrefab, "PH_Convoy_Cylinder", EnhancedKind.Convoy);
            BuildEnhancedObjectivePrefab(EnergyNodePrefab, "PH_EnergyNode_Cylinder", EnhancedKind.EnergyNode);
            BuildEnhancedObjectivePrefab(ObjectiveMarkerPrefab, "PH_ObjectiveMarker_Cylinder", EnhancedKind.ObjectiveMarker);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LuoLuoTrip] Enhanced Placeholder assets generated under " + PlaceholderRoot + " (force=" + forceRegenerate + ")");
        }

        private static void DeleteAllPlaceholderPrefabs()
        {
            var paths = new[] {
                PlayerCommanderPrefab, MechaMinionPrefab, BeastMinionPrefab,
                CityLordPrefab, WarKingPrefab,
                ConvoyPrefab, EnergyNodePrefab, ObjectiveMarkerPrefab
            };
            foreach (var p in paths)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(p) != null)
                    AssetDatabase.DeleteAsset(p);
            }
        }

        private enum EnhancedKind
        {
            Commander, MechaMinion, MechaCaptain, BeastMinion, BeastElite,
            Convoy, EnergyNode, ObjectiveMarker
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

        private static void BuildEnhancedCharacterPrefab(string prefabPath, string rootName, EnhancedKind kind)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return;

            var builderKind = MapKind(kind);
            float height = PlaceholderVisualBuilder.GetHeight(builderKind);
            float radius = PlaceholderVisualBuilder.GetRadius(builderKind);

            var root = new GameObject(rootName);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;

            PlaceholderVisualBuilder.BuildCharacter(visual.transform, builderKind);

            var collision = new GameObject("Collision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            var capsule = collision.AddComponent<CapsuleCollider>();
            capsule.height = height;
            capsule.radius = radius;
            capsule.center = Vector3.zero;

            var marker = new GameObject("Marker");
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, height + 0.4f, 0f);

            root.AddComponent<CharacterEntity>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            EditorUtility.SetDirty(prefab);
        }

        private static void BuildEnhancedObjectivePrefab(string prefabPath, string rootName, EnhancedKind kind)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return;

            var builderKind = MapKind(kind);

            var root = new GameObject(rootName);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;

            Vector3 collisionSize = PlaceholderVisualBuilder.BuildObjective(visual.transform, builderKind);

            var collision = new GameObject("Collision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, collisionSize.y * 0.5f, 0f);
            var box = collision.AddComponent<BoxCollider>();
            box.size = collisionSize;
            box.center = Vector3.zero;

            var marker = new GameObject("Marker");
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, collisionSize.y + 0.4f, 0f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            EditorUtility.SetDirty(prefab);
        }

        private static PlaceholderVisualBuilder.Kind MapKind(EnhancedKind k)
        {
            switch (k)
            {
                case EnhancedKind.Commander: return PlaceholderVisualBuilder.Kind.Commander;
                case EnhancedKind.MechaMinion: return PlaceholderVisualBuilder.Kind.MechaMinion;
                case EnhancedKind.MechaCaptain: return PlaceholderVisualBuilder.Kind.MechaCaptain;
                case EnhancedKind.BeastMinion: return PlaceholderVisualBuilder.Kind.BeastMinion;
                case EnhancedKind.BeastElite: return PlaceholderVisualBuilder.Kind.BeastElite;
                case EnhancedKind.Convoy: return PlaceholderVisualBuilder.Kind.Convoy;
                case EnhancedKind.EnergyNode: return PlaceholderVisualBuilder.Kind.EnergyNode;
                case EnhancedKind.ObjectiveMarker: return PlaceholderVisualBuilder.Kind.ObjectiveMarker;
                default: return PlaceholderVisualBuilder.Kind.Commander;
            }
        }
    }
}
#endif

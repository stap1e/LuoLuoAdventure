#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LuoLuoTrip.Editor
{
    internal static class PlaceholderVisualBuilder
    {
        private const string MaterialFolder = "Assets/Art/Placeholders/Materials";

        public enum Kind
        {
            Commander, MechaMinion, MechaCaptain, BeastMinion, BeastElite,
            Convoy, EnergyNode, ObjectiveMarker
        }

        public static float GetHeight(Kind k)
        {
            switch (k)
            {
                case Kind.Commander: return 1.2f;
                case Kind.MechaMinion: return 0.9f;
                case Kind.BeastMinion: return 0.95f;
                case Kind.MechaCaptain: return 1.4f;
                case Kind.BeastElite: return 1.5f;
                default: return 1f;
            }
        }

        public static float GetRadius(Kind k)
        {
            switch (k)
            {
                case Kind.MechaCaptain: return 0.45f;
                case Kind.BeastElite: return 0.5f;
                case Kind.Commander: return 0.4f;
                default: return 0.35f;
            }
        }

        public static GameObject Add(Transform parent, PrimitiveType type, string name,
            Vector3 localPos, Vector3 scale, string materialName)
        {
            return Add(parent, type, name, localPos, scale, materialName, Quaternion.identity);
        }

        public static GameObject Add(Transform parent, PrimitiveType type, string name,
            Vector3 localPos, Vector3 scale, string materialName, Quaternion rot)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.transform.localRotation = rot;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + materialName + ".mat");
            var renderer = go.GetComponent<Renderer>();
            if (mat != null && renderer != null) renderer.sharedMaterial = mat;
            return go;
        }

        public static void BuildCharacter(Transform parent, Kind kind)
        {
            switch (kind)
            {
                case Kind.Commander: Commander(parent); break;
                case Kind.MechaMinion: Mecha(parent, false); break;
                case Kind.MechaCaptain: Mecha(parent, true); break;
                case Kind.BeastMinion: Beast(parent, false); break;
                case Kind.BeastElite: Beast(parent, true); break;
            }
        }

        public static Vector3 BuildObjective(Transform parent, Kind kind)
        {
            switch (kind)
            {
                case Kind.Convoy: return Convoy(parent);
                case Kind.EnergyNode: return EnergyNode(parent);
                case Kind.ObjectiveMarker: return ObjectiveMarker(parent);
                default: return new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        private static void Commander(Transform p)
        {
            Add(p, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.5f, 0f), new Vector3(0.55f, 0.55f, 0.55f), "MAT_PH_Player");
            Add(p, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.15f, 0f), new Vector3(0.45f, 0.45f, 0.45f), "MAT_PH_Player");
            Add(p, PrimitiveType.Cube, "Crown", new Vector3(0f, 1.45f, 0f), new Vector3(0.55f, 0.15f, 0.55f), "MAT_PH_PlayerAccent");
            Add(p, PrimitiveType.Cube, "Core", new Vector3(0f, 0.5f, 0.18f), new Vector3(0.25f, 0.25f, 0.1f), "MAT_PH_PlayerAccent");
            Add(p, PrimitiveType.Cube, "FacingFin", new Vector3(0f, 0.5f, 0.42f), new Vector3(0.1f, 0.3f, 0.4f), "MAT_PH_PlayerAccent");
        }

        private static void Mecha(Transform p, bool isCaptain)
        {
            string accent = isCaptain ? "MAT_PH_Leader" : "MAT_PH_MechaAccent";
            float yScale = isCaptain ? 1.15f : 1f;
            Add(p, PrimitiveType.Cube, "Hull", new Vector3(0f, 0.35f * yScale, 0f), new Vector3(0.7f, 0.4f, 1.1f), "MAT_PH_Mecha");
            Add(p, PrimitiveType.Cube, "Cockpit", new Vector3(0f, 0.7f * yScale, -0.05f), new Vector3(0.5f, 0.35f, 0.5f), "MAT_PH_Mecha");
            Add(p, PrimitiveType.Cube, "FrontFin", new Vector3(0f, 0.4f * yScale, 0.6f), new Vector3(0.4f, 0.15f, 0.2f), accent);
            var rot = Quaternion.Euler(0f, 0f, 90f);
            Add(p, PrimitiveType.Cylinder, "WheelFL", new Vector3(-0.4f, 0.18f, 0.35f), new Vector3(0.18f, 0.1f, 0.18f), "MAT_PH_Wheel", rot);
            Add(p, PrimitiveType.Cylinder, "WheelFR", new Vector3(0.4f, 0.18f, 0.35f), new Vector3(0.18f, 0.1f, 0.18f), "MAT_PH_Wheel", rot);
            Add(p, PrimitiveType.Cylinder, "WheelBL", new Vector3(-0.4f, 0.18f, -0.35f), new Vector3(0.18f, 0.1f, 0.18f), "MAT_PH_Wheel", rot);
            Add(p, PrimitiveType.Cylinder, "WheelBR", new Vector3(0.4f, 0.18f, -0.35f), new Vector3(0.18f, 0.1f, 0.18f), "MAT_PH_Wheel", rot);
            if (isCaptain)
            {
                Add(p, PrimitiveType.Cube, "CaptainAntenna", new Vector3(0f, 1.1f, -0.05f), new Vector3(0.08f, 0.4f, 0.08f), accent);
            }
        }

        private static void Beast(Transform p, bool isElite)
        {
            string accent = isElite ? "MAT_PH_Leader" : "MAT_PH_BeastAccent";
            float s = isElite ? 1.2f : 1f;
            Add(p, PrimitiveType.Cube, "Body", new Vector3(0f, 0.4f * s, 0f), new Vector3(0.7f * s, 0.55f * s, 1f * s), "MAT_PH_Beast");
            Add(p, PrimitiveType.Cube, "Head", new Vector3(0f, 0.55f * s, 0.55f * s), new Vector3(0.55f * s, 0.45f * s, 0.4f * s), "MAT_PH_Beast");
            Add(p, PrimitiveType.Cube, "HornLeft", new Vector3(-0.18f, 0.85f * s, 0.55f * s), new Vector3(0.1f, 0.35f * s, 0.1f), accent);
            Add(p, PrimitiveType.Cube, "HornRight", new Vector3(0.18f, 0.85f * s, 0.55f * s), new Vector3(0.1f, 0.35f * s, 0.1f), accent);
            Add(p, PrimitiveType.Cube, "ClawLeft", new Vector3(-0.45f, 0.2f, 0.5f), new Vector3(0.1f, 0.15f, 0.25f), accent);
            Add(p, PrimitiveType.Cube, "ClawRight", new Vector3(0.45f, 0.2f, 0.5f), new Vector3(0.1f, 0.15f, 0.25f), accent);
            Add(p, PrimitiveType.Cube, "LegFL", new Vector3(-0.28f, 0.1f, 0.3f), new Vector3(0.15f, 0.2f, 0.15f), "MAT_PH_Beast");
            Add(p, PrimitiveType.Cube, "LegFR", new Vector3(0.28f, 0.1f, 0.3f), new Vector3(0.15f, 0.2f, 0.15f), "MAT_PH_Beast");
            Add(p, PrimitiveType.Cube, "LegBL", new Vector3(-0.28f, 0.1f, -0.3f), new Vector3(0.15f, 0.2f, 0.15f), "MAT_PH_Beast");
            Add(p, PrimitiveType.Cube, "LegBR", new Vector3(0.28f, 0.1f, -0.3f), new Vector3(0.15f, 0.2f, 0.15f), "MAT_PH_Beast");
        }

        private static Vector3 Convoy(Transform p)
        {
            Add(p, PrimitiveType.Cube, "Cargo1", new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.6f, 1.5f), "MAT_PH_Neutral");
            Add(p, PrimitiveType.Cube, "Cargo2", new Vector3(0f, 0.95f, 0f), new Vector3(0.85f, 0.4f, 1.3f), "MAT_PH_Neutral");
            Add(p, PrimitiveType.Cube, "EnergyTank", new Vector3(0f, 1.25f, 0f), new Vector3(0.45f, 0.25f, 0.6f), "MAT_PH_Energy");
            var wheelRot = Quaternion.Euler(0f, 0f, 90f);
            Add(p, PrimitiveType.Cylinder, "WheelFL", new Vector3(-0.55f, 0.18f, 0.55f), new Vector3(0.2f, 0.12f, 0.2f), "MAT_PH_Wheel", wheelRot);
            Add(p, PrimitiveType.Cylinder, "WheelFR", new Vector3(0.55f, 0.18f, 0.55f), new Vector3(0.2f, 0.12f, 0.2f), "MAT_PH_Wheel", wheelRot);
            Add(p, PrimitiveType.Cylinder, "WheelBL", new Vector3(-0.55f, 0.18f, -0.55f), new Vector3(0.2f, 0.12f, 0.2f), "MAT_PH_Wheel", wheelRot);
            Add(p, PrimitiveType.Cylinder, "WheelBR", new Vector3(0.55f, 0.18f, -0.55f), new Vector3(0.2f, 0.12f, 0.2f), "MAT_PH_Wheel", wheelRot);
            return new Vector3(1.4f, 1.5f, 1.8f);
        }

        private static Vector3 EnergyNode(Transform p)
        {
            Add(p, PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.1f, 0f), new Vector3(0.7f, 0.1f, 0.7f), "MAT_PH_Neutral");
            Add(p, PrimitiveType.Sphere, "Core", new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 0.45f, 0.45f), "MAT_PH_Energy");
            Add(p, PrimitiveType.Cylinder, "Pillar", new Vector3(0f, 0.85f, 0f), new Vector3(0.1f, 0.4f, 0.1f), "MAT_PH_Energy");
            var ringRot = Quaternion.Euler(90f, 0f, 0f);
            Add(p, PrimitiveType.Cylinder, "Ring1", new Vector3(0f, 0.55f, 0f), new Vector3(0.85f, 0.05f, 0.85f), "MAT_PH_Energy", ringRot);
            return new Vector3(1f, 1.3f, 1f);
        }

        private static Vector3 ObjectiveMarker(Transform p)
        {
            Add(p, PrimitiveType.Cylinder, "Pillar", new Vector3(0f, 0.7f, 0f), new Vector3(0.18f, 0.7f, 0.18f), "MAT_PH_Objective");
            Add(p, PrimitiveType.Cube, "Banner", new Vector3(0f, 1.4f, 0.1f), new Vector3(0.5f, 0.35f, 0.05f), "MAT_PH_Objective");
            Add(p, PrimitiveType.Sphere, "Apex", new Vector3(0f, 1.65f, 0f), new Vector3(0.18f, 0.18f, 0.18f), "MAT_PH_Objective");
            return new Vector3(0.5f, 1.7f, 0.5f);
        }
    }
}
#endif

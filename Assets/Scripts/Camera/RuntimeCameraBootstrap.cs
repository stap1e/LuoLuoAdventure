using UnityEngine;

namespace LuoLuoTrip
{
    public class RuntimeCameraBootstrap : MonoBehaviour
    {
        private static bool _ensuredThisSession;

        public static Camera EnsureMainCamera()
        {
            var camGo = GameObject.FindWithTag("MainCamera");

            if (camGo == null)
            {
                camGo = GameObject.Find("Main Camera");
            }

            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.transform.position = new Vector3(0f, 8f, -10f);
                camGo.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                Debug.Log("[RuntimeCameraBootstrap] Created fallback Main Camera.");
            }

            camGo.tag = "MainCamera";
            camGo.SetActive(true);

            var cam = camGo.GetComponent<Camera>();
            bool repaired = false;

            if (cam == null)
            {
                cam = camGo.AddComponent<Camera>();
                repaired = true;
            }

            if (!cam.enabled)
            {
                cam.enabled = true;
                repaired = true;
            }

            if (cam.targetTexture != null)
            {
                cam.targetTexture = null;
                repaired = true;
            }

            if (cam.cullingMask == 0)
            {
                cam.cullingMask = -1;
                repaired = true;
            }

            cam.fieldOfView = 60f;
            cam.targetDisplay = 0;

            if (camGo.GetComponent<AudioListener>() == null &&
                FindObjectsOfType<AudioListener>().Length == 0)
            {
                camGo.AddComponent<AudioListener>();
                repaired = true;
            }

            if (repaired && !_ensuredThisSession)
                Debug.Log("[RuntimeCameraBootstrap] Repaired Main Camera.");

            _ensuredThisSession = true;
            return cam;
        }

        private void Awake()
        {
            EnsureMainCamera();
        }
    }
}

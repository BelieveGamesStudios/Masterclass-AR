using UnityEngine;
using UnityEngine.XR.ARFoundation;
namespace Imisi3D
  {
  [RequireComponent(typeof(ARCameraManager))]
  public class ArLightAdjuster : MonoBehaviour
  {
      [Header("Components")]
      private ARCameraManager cameraManager;
      private Light mainLight;
      private void Start()
      {
          cameraManager = GetComponent<ARCameraManager>();
          mainLight = FindFirstObjectByType<Light>();
          cameraManager.frameReceived += CameraManager_frameReceived;
      }
    
      private void CameraManager_frameReceived(ARCameraFrameEventArgs obj)
      {
          var lightInfo = obj.lightEstimation.averageBrightness;
          if(lightInfo.HasValue)
              mainLight.intensity = lightInfo.Value;
  
      }
  }
}

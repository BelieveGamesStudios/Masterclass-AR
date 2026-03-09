using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Input = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Imisi3D
{
    [RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
    public class ArPlacement : MonoBehaviour
    {
        public static ArPlacement Instance;

        [Header("Components")]
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;
        private Camera mainCam;

        [Header("Object management")]
        private Vector2 touchPosition;
        private GameObject objectToPlace;
        private GameObject selectedObject;
        private List<GameObject> placedObjects;
        private static List<ARRaycastHit> hits;
        private Transform objectParent;

        private bool isTouching = false;

        [Header("Object Transformation"),Range(0.5f,20f)]
        [SerializeField] private float smoothTime = 10;

        private void Awake()
        {
            Instance = Instance ?? this;
        }
        private void Start()
        {
            raycastManager = GetComponent<ARRaycastManager>();
            planeManager = GetComponent<ARPlaneManager>();

            mainCam = Camera.main;
            placedObjects = new List<GameObject>();
            objectParent = new GameObject("Object Container").transform;
        }
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }
        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }
        private void OnDestroy()
        {
            Instance = null;
        }
        private void Update()
        {
            GetTouch();
        }
        void GetTouch()
        {
            Input t = Input.activeTouches[0];
            touchPosition = t.screenPosition;
            switch (t.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.None:
                    break;
                case UnityEngine.InputSystem.TouchPhase.Began:
                    isTouching = true;
                    Ray ray = mainCam.ScreenPointToRay(touchPosition);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo))
                    {
                        if (placedObjects.Contains(hitInfo.collider.gameObject))
                        {
                            selectedObject = hitInfo.collider.gameObject;
                            return;
                        }
                    }

                    if (!objectToPlace)
                    {
                        GameObject newObj = Instantiate(selectedObject, TouchPose().position, TouchPose().rotation, objectParent);
                        newObj.TryGetComponent<>
                        placedObjects.Add(newObj);
                    }

                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                    if (selectedObject)
                    {
                        Vector3 smoothPos = Vector3.Lerp(selectedObject.transform.position, TouchPose().position, smoothTime * Time.deltaTime);
                        selectedObject.transform.position = smoothPos;
                    }
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                    isTouching = false;
                    break;
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    isTouching = false;
                    break;
                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    break;
                default:
                    break;
            }
        }
        public void SetObjectToPlace(GameObject newObject)
        {
            objectToPlace = newObject;
        }
        Pose TouchPose()
        {
            Ray ray = mainCam.ScreenPointToRay(touchPosition);
            if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = hits[0].pose;
                return pose;
            }
            return new Pose();
        }
    }
}
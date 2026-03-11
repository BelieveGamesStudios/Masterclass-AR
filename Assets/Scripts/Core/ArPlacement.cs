using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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
        private GameObject objectToPlace;
        private GameObject selectedObject;
        private Transform objectParent;


        private List<GameObject> placedObjects;
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
        private List<RaycastResult> raycastResult = new();

        [Header("Object Transformation")]

        [SerializeField, Range(0.5f, 20f)] private float smoothTime = 10;

        [SerializeField, Range(0.05f, 3f)] private float scaleIncrease = 1;

        [SerializeField, Range(0.05f, 1f)] private float minimumObjectSize = 1;

        [SerializeField, Range(1, 5f)] private float maximumObjectSize = 5;

        private Vector2 touchPosition;
        private float previousDistance;
        private bool scaling = false;
        private bool rotating = false;
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
            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();
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
            if (Touch.activeFingers.Count == 1)
                GetSingleTouch();
            else if (Touch.activeFingers.Count == 2)
                GetMultiTouch();
            else
            {
                previousDistance = 0;
            }
        }
        void GetSingleTouch()
        {
            Touch t = Touch.activeTouches[0];
            touchPosition = t.screenPosition;
            if (isOverUI(touchPosition)) return;
            switch (t.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    Ray ray = mainCam.ScreenPointToRay(touchPosition);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo))
                    {
                        if (placedObjects.Contains(hitInfo.collider.gameObject))
                        {
                            selectedObject = hitInfo.collider.gameObject;
                            return;
                        }
                    }

                    if (objectToPlace)
                    {
                        GameObject newObj = Instantiate(objectToPlace, TouchPose().position, TouchPose().rotation, objectParent);
                        newObj.TryGetComponent<Collider>(out Collider placedObjectCol);
                        if (!placedObjectCol)
                            newObj.AddComponent<BoxCollider>();
                        placedObjects.Add(newObj);
                        selectedObject = newObj;
                        objectToPlace = null;
                        return;
                    }

                    selectedObject = null;
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                    if (selectedObject)
                    {
                        Vector3 smoothPos = Vector3.Lerp(selectedObject.transform.position, TouchPose().position, smoothTime * Time.deltaTime);
                        selectedObject.transform.position = smoothPos;
                    }
                    break;

            }
        }
        void GetMultiTouch()
        {
            if (selectedObject == null) return;

            Touch firstTouch = Touch.activeTouches[0];
            Touch secondTouch = Touch.activeTouches[1];

            float currentDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition); //Here you can also get the magnitude instead

            float pinchDelta = currentDistance - previousDistance;
            if (pinchDelta != 0.0f)
            {
                OnPinch(pinchDelta);
            }
            previousDistance = currentDistance;

        }
        void OnPinch(float p)
        {
            Vector3 newScale = selectedObject.transform.localScale += new Vector3(
                scaleIncrease,
                scaleIncrease,
                scaleIncrease
                );
            newScale.x = Mathf.Clamp(newScale.x, minimumObjectSize, maximumObjectSize);
            newScale.y = Mathf.Clamp(newScale.y, minimumObjectSize, maximumObjectSize);
            newScale.z = Mathf.Clamp(newScale.z, minimumObjectSize, maximumObjectSize);
            selectedObject.transform.localScale = Vector3.Slerp(selectedObject.transform.localScale, newScale, smoothTime / 3 * Time.deltaTime);
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
        bool isOverUI(Vector2 screenPos)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = touchPosition;
            raycastResult = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResult);
            if (raycastResult.Count > 0)
            {
                foreach (var uiItem in raycastResult)
                {
                    Canvas c = uiItem.gameObject.GetComponentInParent<Canvas>();
                    if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        print("true");
                        return true;
                    }
                }
            }
            print("False");
            return false;
        }
    }
}
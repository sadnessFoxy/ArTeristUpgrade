using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Lets the player aim at a detected surface and stands the Tetris board on it (AR Foundation 6).
// The game runs in fixed world coordinates (one unit per cell), so instead of moving the board
// we move, turn and scale the XR origin until the board appears at the chosen real-world spot.
public class ARController : MonoBehaviour
{
    // Bottom centre of the board: cells span x 0..9 and y 0..19, centred on whole numbers.
    private static readonly Vector3 BoardPivot = new Vector3(4.5f, -0.5f, 0f);
    private const float BoardHeightUnits = 20f;
    private const float EditorPlacementDistance = 1.5f; // metres, used without AR tracking

    [Header("AR Components")]
    public XROrigin XROrigin;
    public ARRaycastManager ARRaycastManager;
    public ARPlaneManager ARPlaneManager;

    [Header("Objects")]
    public Camera ARCamera;
    public GameObject[] ARObjectPrefab;
    public GameObject PlacementIndictor;
    public GameObject ARObject;

    [Header("Placement")]
    [Tooltip("Real-world height of the board in metres.")]
    public float boardHeightMeters = 0.7f;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Vector3 placementPoint;
    private bool isPlacementValid;

    void Start()
    {
        ARObjectPrefab[0].SetActive(false);

        if (ARCamera == null)
            ARCamera = Camera.main;

        // The board stands on floors and tables; walls only clutter the view.
        ARPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
    }

    void Update()
    {
        if (ARObject != null)
            return;

        UpdatePlacementPose();

        // Editor shortcut: place the board without a tracked surface.
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            PlaceInFrontOfCamera();
    }

    public void OnClick_PlaceObject()
    {
        if (ARObject != null)
            return;

        if (isPlacementValid)
            PlaceBoard(placementPoint);
        else if (Application.isEditor)
            PlaceInFrontOfCamera(); // no AR camera on desktop
    }

    private void UpdatePlacementPose()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        isPlacementValid = ARRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        if (isPlacementValid)
        {
            placementPoint = hits[0].pose.position;
            PlacementIndictor.transform.SetPositionAndRotation(placementPoint, Quaternion.LookRotation(FlatCameraForward()));
        }
        PlacementIndictor.SetActive(isPlacementValid);
    }

    private void PlaceInFrontOfCamera()
    {
        Transform cam = ARCamera.transform;
        PlaceBoard(cam.position + FlatCameraForward() * EditorPlacementDistance + Vector3.down);
    }

    private void PlaceBoard(Vector3 worldPoint)
    {
        Transform session = XROrigin.TrackablesParent;
        Vector3 sessionPoint = session.InverseTransformPoint(worldPoint);
        Vector3 sessionForward = session.InverseTransformDirection(FlatCameraForward());
        sessionForward = new Vector3(sessionForward.x, 0f, sessionForward.z).normalized;

        AlignOrigin(sessionPoint, sessionForward, BoardHeightUnits / boardHeightMeters);

        ARObject = ARObjectPrefab[0];
        ARObject.SetActive(true);
        PlacementIndictor.SetActive(false);
        HidePlanes();
    }

    // Solves for the origin pose so that the session-space point lands on the board pivot
    // and the camera's horizontal direction maps to the board's forward (+Z) direction.
    private void AlignOrigin(Vector3 sessionPoint, Vector3 sessionForward, float scale)
    {
        Transform origin = XROrigin.Origin.transform;
        Vector3 offset = XROrigin.CameraFloorOffsetObject.transform.localPosition;

        Quaternion rotation = Quaternion.Inverse(Quaternion.LookRotation(sessionForward));
        Vector3 sessionOrigin = BoardPivot - rotation * (sessionPoint * scale);

        origin.localScale = Vector3.one * scale;
        origin.SetPositionAndRotation(sessionOrigin - rotation * (offset * scale), rotation);
    }

    // Plane overlays would cover the board, and new ones keep appearing while tracking.
    private void HidePlanes()
    {
        ARPlaneManager.enabled = false;
        foreach (ARPlane plane in ARPlaneManager.trackables)
            plane.gameObject.SetActive(false);
    }

    private Vector3 FlatCameraForward()
    {
        Vector3 forward = ARCamera.transform.forward;
        Vector3 flat = new Vector3(forward.x, 0f, forward.z);
        return flat.sqrMagnitude > 1e-6f ? flat.normalized : Vector3.forward;
    }
}

using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera thirdPersonCam;
    public Camera firstPersonCam;
    public KeyCode switchKey = KeyCode.C; // Key to switch views

    // For First Person Mouse Look
    public Transform playerBody; // Assign your main character body transform here
    public float mouseSensitivity = 100f;
    private float xRotation = 0f; // For vertical look

    private bool isFirstPersonView = false;

    void Start()
    {
        // Ensure playerBody is assigned if you want FPS mouse look
        if (playerBody == null) {
            playerBody = transform; // Default to this object's transform if not set (e.g. if script is on player)
            Debug.LogWarning("PlayerBody for CameraSwitcher not explicitly set. Defaulting to script's GameObject transform.");
        }

        // Start in third-person view by default
        ActivateThirdPersonView();
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            isFirstPersonView = !isFirstPersonView;
            if (isFirstPersonView)
            {
                ActivateFirstPersonView();
            }
            else
            {
                ActivateThirdPersonView();
            }
        }

        // First Person Mouse Look (only when in first person view)
        if (isFirstPersonView && firstPersonCam.gameObject.activeInHierarchy)
        {
            HandleFirstPersonMouseLook();
        }
    }

    void ActivateThirdPersonView()
    {
        thirdPersonCam.gameObject.SetActive(true);
        if (thirdPersonCam.GetComponent<AudioListener>() == null) thirdPersonCam.gameObject.AddComponent<AudioListener>();
        thirdPersonCam.GetComponent<AudioListener>().enabled = true;
        thirdPersonCam.GetComponent<ThirdPersonCamera>().enabled = true; // Enable the follow script

        firstPersonCam.gameObject.SetActive(false);
        if (firstPersonCam.GetComponent<AudioListener>() != null) firstPersonCam.GetComponent<AudioListener>().enabled = false;

        // Optional: Unlock cursor for third person
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ActivateFirstPersonView()
    {
        thirdPersonCam.gameObject.SetActive(false);
        if (thirdPersonCam.GetComponent<AudioListener>() != null) thirdPersonCam.GetComponent<AudioListener>().enabled = false;
        if (thirdPersonCam.GetComponent<ThirdPersonCamera>() != null) thirdPersonCam.GetComponent<ThirdPersonCamera>().enabled = false; // Disable the follow script

        firstPersonCam.gameObject.SetActive(true);
        if (firstPersonCam.GetComponent<AudioListener>() == null) firstPersonCam.gameObject.AddComponent<AudioListener>();
        firstPersonCam.GetComponent<AudioListener>().enabled = true;

        // Optional: Lock cursor for first person
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        xRotation = firstPersonCam.transform.localEulerAngles.x; // Initialize xRotation with current camera pitch
    }

    void HandleFirstPersonMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical look (Pitch) - applied to the camera directly
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping over
        firstPersonCam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal look (Yaw) - applied to the player body
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
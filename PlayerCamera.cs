using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.EventSystems;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] public Transform camFollowPos;
    [HideInInspector] public float xAxis, yAxis;
    private float mouseSense = 1f;
    public float adsFov = 25;
    [HideInInspector] public float hipFov;
    [HideInInspector] public float currentFov;
    public float fovSmoothSpeed = 10f;
    public float rigSmoothSpeed = 3f;
    private float defDis;
    float xFollowPos;
    float yFollowPos, ogYPos;
    [SerializeField] float crouchCamHeight = 1f;
    [SerializeField] float shoulderSwapSpeed = 10;

    [SerializeField] CinemachineVirtualCamera vCam;
    Cinemachine3rdPersonFollow thirdPerson;

    public bool isAim;
    public float mobileSense = 5f;
    public float aimSense = 2f;
    private float currentSense;
    public Quaternion defaultCamRotation;
    public Vector3 defaultPosition;

    public Transform aimPos;
    [SerializeField] float aimSmoothSpeed = 20;
    [SerializeField] LayerMask aimMask;
    private PlayerHealth playerHealth;

    public GameObject scopeZoom;
    public bool crouch;
    public Transform handPos;
    private int ground;
    private bool hitGround;
    private float targetWeight;
    public float rigSmooth;
    public Rig rig;
    public bool mobileInput;

    private float dampingFactor;
    private PlayerMove plMove;

    private HashSet<int> uiTouches = new HashSet<int>();

    void Start()
    {
        rigSmooth = 3f;
        ground = LayerMask.NameToLayer("Ground");
        hipFov = vCam.m_Lens.FieldOfView;
        currentFov = hipFov;
        dampingFactor = 1f;
        thirdPerson = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        defaultCamRotation = camFollowPos.localRotation;
        defaultPosition = camFollowPos.localPosition;
        playerHealth = GetComponent<PlayerHealth>();
        plMove = GetComponent<PlayerMove>();
        defDis = thirdPerson.CameraDistance;
        xFollowPos = camFollowPos.localPosition.x;
        ogYPos = camFollowPos.localPosition.y;
        yFollowPos = ogYPos;
        scopeZoom.SetActive(false);
        mobileSense = PlayerPrefs.GetFloat("Sense");
        aimSense = PlayerPrefs.GetFloat("Scope");
        currentSense = mobileSense;
    }

    private void Update()
    {
        if (!playerHealth.isAlive) return;
        CameraSet();
        SetRig();
        MoveCamera();
    }

    private void LateUpdate()
    {
        camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
    }

    void MoveCamera()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt)) xFollowPos = -xFollowPos;
        if (crouch) yFollowPos = crouchCamHeight;
        else yFollowPos = ogYPos;

        Vector3 newFollowPos = new Vector3(xFollowPos, yFollowPos, camFollowPos.localPosition.z);
        camFollowPos.localPosition = Vector3.Lerp(camFollowPos.localPosition, newFollowPos, shoulderSwapSpeed * Time.deltaTime);
    }

    void CameraSet()
    {
        if (mobileInput)
        {
            RotateScreen();
        }
        else
        {
            xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
            yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
        }

        yAxis = Mathf.Clamp(yAxis, -40f, 40f);

        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, currentFov, fovSmoothSpeed * Time.deltaTime);

        Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
        {
            if (hit.collider.gameObject.layer == ground) hitGround = true;
            else hitGround = false;
            aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
        }
        else
            aimPos.position = Vector3.Lerp(aimPos.position, ray.GetPoint(100), aimSmoothSpeed * Time.deltaTime);

    }

    private void RotateScreen()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    uiTouches.Add(touch.fingerId);
                }
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (!uiTouches.Contains(touch.fingerId))
                {
                    Vector2 touchDelta = touch.deltaPosition;

                    xAxis += touchDelta.x * currentSense * Time.deltaTime * dampingFactor * 0.5f;
                    yAxis -= touchDelta.y * currentSense * Time.deltaTime * dampingFactor * 0.5f;
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (uiTouches.Contains(touch.fingerId)) uiTouches.Remove(touch.fingerId);
            }
        }
    }

    public void ScopeZoom()
    {
        isAim = !isAim;
        scopeZoom.SetActive(isAim);

        if (isAim)
        {
            if (plMove.playerRun) plMove.RunFalse();
            SetCamera(-2);
            currentFov = adsFov;
            currentSense = aimSense;
            dampingFactor = 0.5f;
        }
        else
        {
            SetCamera(defDis);
            currentFov = hipFov;
            currentSense = mobileSense;
            dampingFactor = 1f;
        }
    }

    private void SetCamera(float distance)
    {
        thirdPerson.CameraDistance = distance;
    }

    public void SetSensitivity(float value)
    {
        mobileSense = value;
        if (!isAim) currentSense = mobileSense;
    }

    public void SetScopeSense(float value)
    {
        aimSense = value;
        if (isAim) currentSense = aimSense;
    }

    private void SetRig()
    {
        targetWeight = (Vector3.Distance(transform.position, aimPos.position) <= 3 && !hitGround) ? 0 : 1;
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, rigSmoothSpeed * Time.deltaTime);
    }
}

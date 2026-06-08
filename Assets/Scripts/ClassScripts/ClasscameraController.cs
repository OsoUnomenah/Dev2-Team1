using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] Transform player;

    float cameraRotationX;
    public float recoil;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        cameraRotationX -= mouseY;
        cameraRotationX -= recoil;

        recoil = Mathf.Lerp(recoil, 0f, Time.deltaTime * 10f);
        cameraRotationX = Mathf.Clamp(cameraRotationX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
        
        player.transform.Rotate(Vector3.up * mouseX);

    }

    public void AddRecoil(float amount)
    {
        recoil += amount;
    }
}

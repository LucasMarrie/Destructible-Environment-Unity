using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float lookSensitivity = 2f;
    bool lookActivated = true;
    float xRotation = 0;
    [SerializeField]Transform verticalAxis;
    [SerializeField]Transform horizontalAxis;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        ToggleLook();
        LookUpdate();
        MovementUpdate();
    }

    void MovementUpdate(){
        float x = Input.GetAxisRaw("Vertical");
        float z = Input.GetAxisRaw("Horizontal");

        float y = Input.GetKey(KeyCode.LeftShift) ? -1 : Input.GetKey(KeyCode.Space) ? 1 : 0;

        Vector3 directionVector = horizontalAxis.forward * x + horizontalAxis.up * y + horizontalAxis.right * z;
        transform.position += directionVector * moveSpeed * Time.deltaTime;
    }

    void ToggleLook(){
        if(Input.GetKeyDown(KeyCode.LeftAlt)){
            lookActivated = !lookActivated;
            Cursor.lockState = lookActivated ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lookActivated;
        }
    }

    void LookUpdate(){
        if(!lookActivated){
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");
        
        xRotation += mouseY * lookSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        float yRotation = horizontalAxis.localEulerAngles.y + mouseX  * lookSensitivity;
        horizontalAxis.localRotation = Quaternion.AngleAxis(yRotation, Vector3.up);
        verticalAxis.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
    }
}

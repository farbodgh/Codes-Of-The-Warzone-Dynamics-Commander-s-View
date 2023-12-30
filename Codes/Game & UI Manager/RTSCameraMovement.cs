using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    float speed = 10f;
    float zoomSpeed = 50f;
    float rotateSpeed =15f;
    float maxHeight = 400f;
    float minHeight = 0f;

    Vector2 p1;
    Vector2 p2;

    //variables that are used to rotate the camera
    Vector3 verticalMove;
    Vector3 lateralMove;
    Vector3 forwardMove;


    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 20f;
            zoomSpeed = 75; 
        }
        else
        {
            speed = 5f;
            zoomSpeed = 25;
        }

        float hsp = transform.position.y * speed * Time.deltaTime * Input.GetAxis("Horizontal");
        float vsp = transform.position.y * speed * Time.deltaTime * Input.GetAxis("Vertical");
        float scrollSp = transform.position.y * -zoomSpeed * Time.deltaTime * Input.GetAxis("Mouse ScrollWheel");

        if ((transform.position.y >= maxHeight) && (scrollSp > 0))
        {
            scrollSp = 0;
        }
        else if ((transform.position.y <= minHeight) && (scrollSp < 0))
        {
            scrollSp = 0;
        }


        if ((transform.position.y + scrollSp) > maxHeight)
        {
            scrollSp = maxHeight - transform.position.y;
        }
        else if ((transform.position.y + scrollSp) < minHeight)
        {
            scrollSp = minHeight - transform.position.y;
        }

         verticalMove = new Vector3(0, scrollSp, 0);
         lateralMove = hsp * transform.right;
         forwardMove = transform.forward;

        forwardMove.y = 0;
        forwardMove.Normalize();
        forwardMove *= vsp;

        Vector3 move = verticalMove + lateralMove + forwardMove;

        transform.position += move;

        getCameraRotation();
    }

    void getCameraRotation()
    {
        if (Input.GetMouseButtonDown(2)) 
        {
            p1 = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) 
        {
            p2 = Input.mousePosition;

            float dx = (p2 - p1).x * rotateSpeed * Time.deltaTime;
            float dy = (p2 - p1).y * rotateSpeed * Time.deltaTime;

            transform.rotation *= Quaternion.Euler(new Vector3(0, dx, 0)); 
            transform.GetChild(0).transform.rotation *= Quaternion.Euler(new Vector3(-dy, 0, 0));

            p1 = p2;
        }
    }
}

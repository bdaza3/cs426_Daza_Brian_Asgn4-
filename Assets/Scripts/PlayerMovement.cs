using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// adding namespaces
using Unity.Netcode;
// because we are using the NetworkBehaviour class
// NewtorkBehaviour class is a part of the Unity.Netcode namespace
// extension of MonoBehaviour that has functions related to multiplayer
public class PlayerMovement : NetworkBehaviour
{
    public float speed = 2.5f;
    // create a list of colors
    public List<Color> colors = new List<Color>();

    public float mouseSensitivity = 800f;
    private float rotationX = 0f;

    // getting the reference to the prefab
    [SerializeField]
    private GameObject spawnedPrefab;
    // save the instantiated prefab
    private GameObject instantiatedPrefab;

    public GameObject cannon;
    public GameObject bullet;

    Rigidbody rb;

    // reference to the camera and camera audio listener
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;


    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    // Update is called once per frame
    void Update()
    {
        // check if the player is the owner of the object
        // makes sure the script is only executed on the owners 
        // not on the other prefabs 
        if (!IsOwner) return;

        // Get movement input
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down Arrow
        Vector3 moveDirection = Vector3.zero;

        //locking the mouse cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false; // Makes it visible again
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true; // Makes it visible again
            }
            
        }

        // Get movement input (WASD)
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;    // move forward
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;    // move backward
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;      // move left
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;      // move right
        if (Input.GetKey(KeyCode.Space)) rb.AddForce(this.transform.up * 5f);
        else rb.AddForce(this.transform.up * -2.5f); // jump

        // Normalize the movement vector to avoid faster diagonal movement
        moveDirection = moveDirection.normalized;

        // Apply movement to the player's position
        transform.position += moveDirection * speed * Time.deltaTime * 15;
        rb.linearVelocity += moveDirection * speed * Time.deltaTime * 15;

        // if I is pressed spawn the object 
        // if J is pressed destroy the object
        if (Input.GetKeyDown(KeyCode.I))
        {
            //instantiate the object
            instantiatedPrefab = Instantiate(spawnedPrefab);
            // spawn it on the scene
            instantiatedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            //despawn the object
            instantiatedPrefab.GetComponent<NetworkObject>().Despawn(true);
            // destroy the object
            Destroy(instantiatedPrefab);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            // call the BulletSpawningServerRpc method
            // as client can not spawn objects
            BulletSpawningServerRpc(cannon.transform.position, cannon.transform.rotation);
        }
        // Rotate player towards mouse cursor
        RotateWithMouse();
    }

    // this method is called when the object is spawned
    // we will change the color of the objects
    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color = colors[(int)OwnerClientId];

        // check if the player is the owner of the object
        if (!IsOwner) return;
        // if the player is the owner of the object
        // enable the camera and the audio listener
        audioListener.enabled = true;
        playerCamera.enabled = true;
    }

    // need to add the [ServerRPC] attribute
    [ServerRpc]
    // method name must end with ServerRPC
    private void BulletSpawningServerRpc(Vector3 position, Quaternion rotation)
    {
        // call the BulletSpawningClientRpc method to locally create the bullet on all clients
        BulletSpawningClientRpc(position, rotation);
    }

    [ClientRpc]
    private void BulletSpawningClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject newBullet = Instantiate(bullet, position, rotation);
        newBullet.GetComponent<Rigidbody>().linearVelocity += Vector3.up * 2;
        newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.up * 1500);
        Destroy(newBullet, 5f);
    }
    //private void RotateWithMouse()
    //{
    //    float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
    //    float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

    //    // Rotate the player left and right (Y-Axis rotation)
    //    transform.Rotate(Vector3.up * mouseX);

    //    // Rotate the camera up and down (X-Axis rotation)
    //    rotationX -= mouseY;
    //    rotationX = Mathf.Clamp(rotationX, -45f, 45f); // Limit looking up/down
    //    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    //}
    private void RotateWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player left and right (Y-Axis rotation)
        Quaternion newRotation = Quaternion.Euler(0f, mouseX, 0f) * rb.rotation;
        rb.MoveRotation(newRotation);

        // Rotate the camera up and down (X-Axis rotation)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -45f, 45f); // Limit looking up/down
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

}
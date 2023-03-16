using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensivity = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool invertLook;

    public float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    private Vector3 moveDir, movement;

    public CharacterController charCon;

    private Camera cam;

    public float jumpForce = 12f, gravityMod = 2.5f;

    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;

    public GameObject bulletImpact;
    //public float timeBetweenShots = 0.1f;
    private float shotCounter;
    public float muzzleDisplayTime;
    private float muzzleCounter;

    public Gun[] guns;
    private int selectedGun;

    public GameObject playerHitImpact;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //Cursor dissapears when the game starts

        cam = Camera.main;  //Find the main camera

        SwitchGun();    //Set initial weapon

        //Configuring spawnpoint (Deprecated: Functionality is now handled in PlayerSpawner)
        /*Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;*/
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z); //Rotate character

            verticalRotStore -= mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

            if (invertLook)
                viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z); //Look up/down
            else
                viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z); //Look up/down

            //Player movement
            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            activeMoveSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed : moveSpeed;

            float yVel = movement.y;
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
            movement.y = yVel;

            if (charCon.isGrounded)
            {
                movement.y = 0;
            }

            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }

            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

            charCon.Move(movement * Time.deltaTime);

            //Shooting
            if (guns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;

                if (muzzleCounter <= 0) guns[selectedGun].muzzleFlash.SetActive(false);     //Deactivate muzzle flash
            }

            if (Input.GetMouseButtonDown(0))
                Shoot();

            if (Input.GetMouseButton(0) && guns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;

                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

            //Gun control
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                selectedGun++;

                if (selectedGun >= guns.Length) selectedGun = 0;
                SwitchGun();
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                selectedGun--;

                if (selectedGun < 0) selectedGun = guns.Length - 1;
                SwitchGun();
            }

            for (int i = 0; i < guns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    SwitchGun();
                }
            }

            //Cursor behaviour
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                    Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("Hitted " + hit.collider.gameObject.name);            

            //If hitted object is a player
            if(hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName);
            } else
            {
                GameObject bulletImpactObj = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObj, 10f);
            }
        }

        shotCounter = guns[selectedGun].timeBetweenShots;

        guns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    [PunRPC]    //Call this function and run at the same time to every copy of the player on the newtork
    public void DealDamage(string damager)
    {
        TakeDamage(damager);
    }

    public void TakeDamage(string damager)
    {
        Debug.Log(photonView.Owner.NickName + "was hitted by " + damager);
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            //Move the main camera to the players position
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }

    void SwitchGun()
    {
        //Deactivate all weapons
        foreach (Gun gun in guns)
        {
            gun.gameObject.SetActive(false);
        }
        //Activate current weapon
        guns[selectedGun].gameObject.SetActive(true);

        guns[selectedGun].muzzleFlash.SetActive(false);
    }
}

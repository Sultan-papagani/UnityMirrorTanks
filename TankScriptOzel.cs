using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankScriptOzel : NetworkBehaviour
{
    [Header("Components")]
    public Animator animator;
    public TextMesh healthBar;
    public Transform turret;

    [Header("Movement")]
    public float rotationSpeed = 100;
    public float agentSpeed = 5;

    [Header("Firing")]
    public KeyCode shootKey = KeyCode.Space;
    public GameObject projectilePrefab;
    public Transform projectileMount;

    [Header("Stats")]
    [SyncVar] public int health = 4;
    [SyncVar] public string playerName;


    Vector3 camOffset = new Vector3(0,6,-8);
    Vector3 sp = new Vector3(1,0,0);
    Vector3 refe = Vector3.zero;
    Camera playercam;
    Transform camtransform;
    CharacterController cc;
    bool alive = true;

    void Start() 
    {
        if (isLocalPlayer)
        {
            playercam = Camera.main;
            camtransform = playercam.transform;
            playerName = PlayerData.singleton.playername;
        }
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {

        if (isLocalPlayer && alive)
        {

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

            cc.Move(transform.TransformDirection(Vector3.forward * vertical) * agentSpeed * Time.deltaTime);
            
            animator.SetBool("Moving", Vector3.one != Vector3.zero);

            if (Input.GetKeyDown(shootKey))
            {
                CmdFire();
            }

            RotateTurret();
        }
    }

    private void FixedUpdate() {
        healthBar.text = playerName;
        healthBar.text += "\n" + new string('-', health);
    }

    private void LateUpdate() {
        if (isLocalPlayer){
            camtransform.position = transform.position + camOffset;
        }
    }

    // this is called on the server
    [Command]
    void CmdFire()
    {
        GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, projectileMount.rotation);
        NetworkServer.Spawn(projectile);
        RpcOnFire();
    }

    // this is called on the tank that fired for all observers
    [ClientRpc]
    void RpcOnFire()
    {
        animator.SetTrigger("Shoot");
    }


    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ProjectileOzel>() != null && alive)
        {
            --health;
            if (health <= 0)
            {
                dieee();
            }
            GetShot(health);
        }
    }

    [ClientRpc]
    public void GetShot(int h)
    {
        if (isLocalPlayer){
            TanksGameCanvas.singleton.SetHealth(h);
        }
    }

    [ClientRpc]
    public void dieee()
    {
        cc.enabled = false;
        gameObject.transform.position = new Vector3(0, 5, 0);
        alive = false;
        StartCoroutine(nameof(Die));
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(5f);
        gameObject.transform.position = NetworkManager.singleton.GetStartPosition().position;
        health = 4;
        alive = true;
        cc.enabled = true;
        GetShot(health); // canı yeniledin ya
    }


    void RotateTurret()
    {
        Ray ray = playercam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotation = new Vector3(hit.point.x, turret.transform.position.y, hit.point.z);
            turret.transform.LookAt(lookRotation);
        }
    }
}

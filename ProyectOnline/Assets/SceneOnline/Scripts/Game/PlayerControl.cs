using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerControl : Photon.MonoBehaviour
{
    public enum ShootType { ShootRay, Granade }
    public ShootType Type = ShootType.ShootRay;

    [Header("Character")]
    private CharacterController Control;
    private Vector3 MoveDir = Vector3.zero;
    public float Gravity = 20;
    public float SpeedRotation = 800;
    public float SpeedMove = 10;
    public int Life = 100;
    public Text DeathCountText;
    public bool isDead = false;
    public bool gotKilled = false;
    public int MyTeam;

    [Header("Canvas")]
    public int KillCount;
    public Image LifeBar;
    public Image PersonalLifeBar;
    public Image ChosenWeapon;
    public Text TeamText;
    public Sprite bomb;
    public Sprite ray;

    [Header("Other")]
    public GameObject Bullet;
    public GameObject Granade;
    public GameObject ShootPoint;
    public SpawnPoints[] Spawns;


    private Vector3 CurrentPosition;
    private Quaternion CurrentRotation;

    private GameObject MainCamera;
    private RaycastHit HitPlayer;

    private float ForceGranade;


    // Use this for initialization
    void Start()
    {
        MainCamera = Camera.main.gameObject;
        PersonalLifeBar = transform.Find("PersonalCanvas").GetChild(1).GetComponent<Image>();

        Control = GetComponent<CharacterController>();
        if (photonView.isMine)
        {
            MainCamera.transform.SetParent(transform.Find("PosCamera"));
            MainCamera.transform.localPosition = Vector3.zero;
            MainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);


            LifeBar = MainCamera.transform.GetChild(0).GetChild(1).GetComponent<Image>();
            DeathCountText = MainCamera.transform.GetChild(0).GetChild(2).GetComponent<Text>();
            ChosenWeapon = MainCamera.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Image>();
            TeamText = MainCamera.transform.GetChild(0).GetChild(4).GetComponent<Text>();
            Spawns = GameObject.FindObjectsOfType<SpawnPoints>();


            ShootPoint = new GameObject();
            ShootPoint.transform.name = "ShootPoint";
            ShootPoint.transform.SetParent(MainCamera.transform);
            ShootPoint.transform.localPosition = new Vector3(0.4f, 0, 1f);
            ShootPoint.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)
        {
            if (!isDead)
            {
                ChangeWeapon();
                switch (Type)
                {
                    case ShootType.ShootRay:
                        ChosenWeapon.color = Color.white;
                        ChosenWeapon.sprite = ray;
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject NewBullet = PhotonNetwork.Instantiate(Bullet.name, ShootPoint.transform.position, ShootPoint.transform.rotation, 0);
                            StartCoroutine(DestroyObjectDelay(NewBullet, 3));
                            if (Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out HitPlayer))
                            {
                                if (HitPlayer.collider.tag == "Player" && HitPlayer.collider.gameObject != gameObject)
                                {
                                    if (HitPlayer.collider.GetComponent<PlayerControl>().MyTeam != MyTeam)
                                    {
                                        print(HitPlayer.collider.GetComponent<PlayerControl>().MyTeam);
                                        print(MyTeam);
                                        //Resto Vida
                                        int TmpDamage = 20;
                                        if (HitPlayer.collider.gameObject.GetComponent<PlayerControl>().Life - TmpDamage <= 0 && HitPlayer.collider.gameObject.GetComponent<PlayerControl>().isDead == false)
                                            KillCount++;

                                        HitPlayer.collider.gameObject.GetComponent<PhotonView>().RPC("GetDamage", PhotonTargets.All, TmpDamage);
                                    }
                                }
                            }
                        }
                        break;
                    //case ShootType.ShootObj:
                    //    ChosenWeapon.color = Color.red;
                    //    if (Input.GetMouseButtonDown(0))
                    //    {
                    //        GameObject NewBullet = PhotonNetwork.Instantiate(Bullet.name, ShootPoint.transform.position, ShootPoint.transform.rotation, 0);
                    //        NewBullet.GetComponent<BulletControl>().ParentID = photonView.ownerId;

                    //        StartCoroutine(DestroyObjectDelay(NewBullet, 3));
                    //    }
                    //    break;

                    case ShootType.Granade:
                        ChosenWeapon.color = Color.white;
                        ChosenWeapon.sprite = bomb;
                        if (Input.GetMouseButton(0)) //Cargo Fuerza granada
                        {
                            ForceGranade = Mathf.MoveTowards(ForceGranade, 1500, 400 * Time.deltaTime); //Calculo fuerza de granada
                        }
                        else if (Input.GetMouseButtonUp(0)) //Disparo Granada
                        {
                            GameObject NewBullet = PhotonNetwork.Instantiate(Granade.name, ShootPoint.transform.position, ShootPoint.transform.rotation, 0);
                            NewBullet.GetComponent<Rigidbody>().AddForce(ShootPoint.transform.forward * ForceGranade);
                            GranadeControl gc = NewBullet.GetComponent<GranadeControl>();
                            gc.ParentIndex = photonView.ownerId;
                            gc.playerControl = this;
                            ForceGranade = 0;
                        }

                        break;
                } //Shooting

                #region PlayerMovement
                if (Control.isGrounded)
                {
                    MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    MoveDir *= SpeedMove;
                    MoveDir = transform.TransformDirection(MoveDir);
                }



                transform.Rotate(Vector3.up * SpeedRotation * Time.deltaTime * Input.GetAxis("Mouse X"));//PLayer Rot
                MainCamera.transform.Rotate(Vector3.right * SpeedRotation * Time.deltaTime * Input.GetAxis("Mouse Y") * -1);//CamRot
                MainCamera.transform.localEulerAngles = new Vector3(MainCamera.transform.localEulerAngles.x, 0, 0);

                #region CapCamRotation
                if (MainCamera.transform.localEulerAngles.x < 300 && MainCamera.transform.localEulerAngles.x > 180)
                {
                    MainCamera.transform.localEulerAngles = new Vector3(300, 0, 0);
                }
                if (MainCamera.transform.localEulerAngles.x < 180 && MainCamera.transform.localEulerAngles.x > 80)
                {
                    MainCamera.transform.localEulerAngles = new Vector3(80, 0, 0);
                }
                #endregion

                MoveDir.y -= Gravity * Time.deltaTime;
                Control.Move(MoveDir * Time.deltaTime);
                #endregion
            }
            LifeBar.fillAmount = (Life / 100f);
            DeathCountText.text = KillCount.ToString();
            TeamText.text = "Team: " + MyTeam.ToString();
        }
        else //OtherPlayers
        {
            transform.position = Vector3.Lerp(transform.position, CurrentPosition, 6 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, CurrentRotation, 6 * Time.deltaTime);
        }
    }

    private void ChangeWeapon()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Type += 1;
            if ((int)Type > 1)
            {
                Type = 0;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Type -= 1;
            if ((int)Type < 0)
            {
                Type += 2;
            }
        }
    }

    [PunRPC]  //Esta funcion se llama desde el servidor
    public void GetDamage(int _damage)
    {
        Life -= _damage;
        PersonalLifeBar.fillAmount = (Life / 100f);

        if (Life - _damage <= 0)
            gotKilled = true;

        if (Life <= 0)
        {
            Life = 0;
            isDead = true;
            //estoy dead (respawn)
            StartCoroutine(Respawn(2f));
        }
    }

    IEnumerator Respawn(float _t)
    {
        yield return new WaitForSeconds(_t);
        Life = 100;
        PersonalLifeBar.fillAmount = (Life / 100f);

        SpawnPoints mySpawn = Spawns[Random.Range(0, Spawns.Length)];

        transform.position = new Vector3(mySpawn.transform.position.x, mySpawn.transform.position.y, mySpawn.transform.position.z);
        isDead = false;
        gotKilled = false;

    }

    IEnumerator DestroyObjectDelay(GameObject _obj, float _time)
    {
        yield return new WaitForSeconds(_time);
        PhotonNetwork.Destroy(_obj);
    }


    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            //stream.SendNext(Animation.GetBool("Move"));
            //stream.SendNext(Animation.GetBool("Shoot"));
        }
        else
        {
            //Animator CurrentAnim = transform.GetChild(2).GetComponent<Animator>();
            CurrentPosition = (Vector3)stream.ReceiveNext();
            CurrentRotation = (Quaternion)stream.ReceiveNext();
        }
    } //Actualizar posición relativa en los demás player

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
        {
            if (gotKilled == true)
                KillCount++;

            if (other.gameObject.GetComponent<BulletControl>().ParentID != photonView.ownerId)
            {
                print("Hola");
                GetComponent<PhotonView>().RPC("GetDamage", PhotonTargets.All, 50);
            }


            PhotonNetwork.Destroy(other.gameObject);
        }
    }
}

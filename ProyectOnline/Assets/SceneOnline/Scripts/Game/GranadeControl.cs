using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadeControl : Photon.MonoBehaviour
{
    private Vector3 CurrentPosition;
    private Quaternion CurrentRotation;
    private bool FirstTake;

    private float CountDown = 3f;
    private float DistancePlayer;
    public int ParentIndex;

    public PlayerControl playerControl;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine)
        {
            CountDown -= Time.deltaTime;
            if (CountDown <= 0)
            {
                //explota
                Collider[] AllDetect = Physics.OverlapSphere(transform.position, 5); //Area de colision
                for (int i = 0; i < AllDetect.Length; i++)
                {
                    if (AllDetect[i].tag == "Player")
                    {
                        DistancePlayer = Vector3.Distance(transform.position, AllDetect[i].transform.position); //Distancia entre un objeto y otro

                        if (DistancePlayer < 1)
                            DistancePlayer = 1;

                        int TmpDamage = (int)(100 / DistancePlayer);
                        if (AllDetect[i].GetComponent<PlayerControl>().photonView.ownerId != ParentIndex)
                        {
                            if (AllDetect[i].GetComponent<PlayerControl>().MyTeam != playerControl.MyTeam)
                            {
                                if (AllDetect[i].GetComponent<PlayerControl>().Life - TmpDamage <= 0 && AllDetect[i].GetComponent<PlayerControl>().isDead == false)
                                    playerControl.KillCount++;
                            }
                        }

                        AllDetect[i].GetComponent<PhotonView>().RPC("GetDamage", PhotonTargets.All, TmpDamage);
                    }
                }
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else //OtherPlayers
        {
            transform.position = Vector3.Lerp(transform.position, CurrentPosition, 6 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, CurrentRotation, 6 * Time.deltaTime);
        }
    }


    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            CurrentPosition = (Vector3)stream.ReceiveNext();
            CurrentRotation = (Quaternion)stream.ReceiveNext();

            if (!FirstTake)
            {
                transform.position = CurrentPosition;
                transform.rotation = CurrentRotation;
                FirstTake = true;
            }
        }
    } //Actualizar posición relativa en los demás player

}

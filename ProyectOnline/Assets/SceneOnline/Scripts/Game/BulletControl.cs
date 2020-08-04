using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : Photon.MonoBehaviour
{
    private Vector3 CurrentPosition;
    private Quaternion CurrentRotation;
    private bool FirstTake;

    private float Speed = 60;

    public int ParentID;
    public int TeamID;


    void Update()
    {
        if (photonView.isMine)
        {
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
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

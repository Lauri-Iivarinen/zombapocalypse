using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentController : MonoBehaviour
{
    public GameObject beam;
    Player pl;
    public static bool beamPickedUp = false;
    public static float beamSpawnRate = 1f;

    void Start(){
        pl = GameObject.Find("Player").GetComponent<Player>();
    }

    public void PickUpBeam(){
        StartCoroutine(SpawnBeam());
        beamPickedUp = true;
        UiDisplay.PickedUpBeam();
    }

    IEnumerator SpawnBeam(){
        yield return new WaitForSeconds(5f/beamSpawnRate);
        Instantiate(beam, transform.position, transform.rotation);
        StartCoroutine(SpawnBeam());
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PokemonController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject trainer = GameObject.Find("Trainer");
        transform.rotation = Quaternion.LookRotation(transform.position - trainer.transform.position);
        GetComponent<NavMeshAgent>().destination = transform.position + transform.forward * 15;
        GetComponent<NavMeshAgent>().speed = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PokeballController : MonoBehaviour
{

    public Animator pokeballAnimator;
    public ParticleSystem pokeflashPF;
    
    private GameObject pokemon;
    private GameObject terrain;
    private int animationStage;
    private bool didOnce;
    private Transform trainer;
    private bool escaped;
    private bool checkForEscape = true;
    private LevelManager levelmanager;

    [SerializeField] private AudioClip clip_Hit;
    [SerializeField] private AudioClip clip_Collision;
    [SerializeField] private AudioClip clip_Wiggle;
    [SerializeField] private AudioClip clip_Escape;
    [SerializeField] private AudioClip clip_Success;
    private AudioSource pokeballAS1;

    private bool disableCollisionSounds;
    
    // Start is called before the first frame update
    void Start()
    {
        pokeballAnimator.speed = 0;
        trainer = GameObject.Find("Trainer").transform.Find("CameraFocus");
        levelmanager = GameObject.Find("Level Manager").GetComponent<LevelManager>();

        pokeballAS1 = GetComponents<AudioSource>()[0];
        pokeballAS1.volume = 0.25f;
        pokeballAS1.spatialBlend = 1f;
        pokeballAS1.maxDistance = 5f;
    }

    private void FixedUpdate()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        
        if (pokemon is not null)
        {
            switch (animationStage)
            {
                case 0: // Apply upward force from the pokemon we just hit
                    rigidbody.AddForce(Vector3.up * 2, ForceMode.Impulse);
                    animationStage = 1;
                    break;
                
                case 1: // Check for when the Pokeball is coming down again
                    if (rigidbody.velocity.y < 0)
                    {
                        animationStage = 2;
                    }
                    break;
                
                case 2: // Hang in thin air, rotate towards pokemon, open the pokeball, spawn a particle on the pokemon, remove the pokemon
                    rigidbody.isKinematic = true; // hang in thin air
                    Quaternion rotationTowardsPokemon = Quaternion.LookRotation(pokemon.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationTowardsPokemon, Time.fixedDeltaTime * 3); // Rotate towards
                    pokeballAnimator.speed = 4; // Speed up when opening (handled by animator)
                    if (!didOnce)
                    { 
                        Instantiate(pokeflashPF, pokemon.transform.position, quaternion.identity);
                        didOnce = true;
                    }
                    pokemon.SetActive(false);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_PokeBall_Open"))
                    {
                        animationStage = 3;   
                    }
                    break;
                
                case 3: // Close pokeball
                    pokeballAnimator.SetInteger("State", 1);
                    
                    Debug.Log("Animation: " +
                              pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_PokeBall_Close"));

                    Debug.Log("Normalized time: " +
                              pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_PokeBall_Close"))
                    {
                        animationStage = 4;
                        terrain = null;
                    }
                    break;
                
                case 4: // Rotate towards player and drop to the ground
                    transform.LookAt(trainer, Vector3.up);
                    rigidbody.isKinematic = false;
                    if (terrain is not null)
                    {
                        animationStage = 5;   
                    }
                    break;
                
                case 5: // Stop physics, Wiggle
                    rigidbody.isKinematic = true;
                    pokeballAnimator.SetInteger("State", 2);
                    pokeballAnimator.speed = 1.5f;

                    if (checkForEscape)
                    {
                        int r = Random.Range(1, 10);
                        if (r == 1)
                        {
                            escaped = true;
                            pokeballAnimator.speed = 0;
                            didOnce = false;
                            animationStage = 6;
                        }
                        StartCoroutine(WaitForCheck(1));
                        checkForEscape = false;
                    }
                    
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_PokeBall_Wiggle"))
                    {
                        pokeballAnimator.speed = 0;
                        didOnce = false;
                        animationStage = 6;
                    }
                    break;
                
                case 6: // Escape or not
                    if (escaped)
                    {
                        if (!didOnce)
                        { 
                            Instantiate(pokeflashPF, pokemon.transform.position, quaternion.identity);
                            pokeballAS1.clip = clip_Escape;
                            pokeballAS1.Play();
                            didOnce = true;
                        }
                        Destroy(gameObject);
                        pokemon.SetActive(true);
                    }
                    else
                    {
                        if (!didOnce)
                        {
                            pokeballAS1.clip = clip_Success;
                            pokeballAS1.Play();
                            didOnce = true;
                        }
                        levelmanager.removePokemon(pokemon, true);
                    }
                    disableCollisionSounds = false;
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Pokemon" && pokemon is null)
        {
            pokemon = collision.gameObject;
            pokeballAS1.clip = clip_Hit;
            pokeballAS1.Play();
            disableCollisionSounds = true;
        }
        if (collision.gameObject.name == "Terrain")
        {
            terrain = collision.gameObject;
            if (!disableCollisionSounds)
            {
                pokeballAS1.clip = clip_Collision;
                pokeballAS1.Play();
            }
        }
    }

    public void wiggleSound()
    {
        pokeballAS1.clip = clip_Wiggle;
        pokeballAS1.Play();
    }
    
    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        checkForEscape = true;
    }
}
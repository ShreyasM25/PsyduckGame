using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
// ReSharper disable All

public class MasterballController : MonoBehaviour
{
    private static readonly int State = Animator.StringToHash("State");
    
    public Animator pokeballAnimator;
    [FormerlySerializedAs("pokeflashPF")] public ParticleSystem pokeflashPf;
    
    private GameObject _pokemon;
    private GameObject _terrain;
    private int _animationStage;
    private bool _didOnce;
    private Transform _trainer;
    private bool _escaped;
    private bool _checkForEscape = true;
    private LevelManager _levelmanager;

    [FormerlySerializedAs("clip_Hit")] [SerializeField] private AudioClip clipHit;
    [FormerlySerializedAs("clip_Collision")] [SerializeField] private AudioClip clipCollision;
    [FormerlySerializedAs("clip_Wiggle")] [SerializeField] private AudioClip clipWiggle;
    [FormerlySerializedAs("clip_Escape")] [SerializeField] private AudioClip clipEscape;
    [FormerlySerializedAs("clip_Success")] [SerializeField] private AudioClip clipSuccess;
    private AudioSource _pokeballAs1;

    private bool _disableCollisionSounds;
    
    // Start is called before the first frame update
    void Start()
    {
        pokeballAnimator.speed = 0;
        _trainer = GameObject.Find("Trainer").transform.Find("CameraFocus");
        _levelmanager = GameObject.Find("Level Manager").GetComponent<LevelManager>();

        _pokeballAs1 = GetComponents<AudioSource>()[0];
        _pokeballAs1.volume = 0.25f;
        _pokeballAs1.spatialBlend = 1f;
        _pokeballAs1.maxDistance = 5f;
    }

    private void FixedUpdate()
    {
        Rigidbody component = GetComponent<Rigidbody>();
        
        if (_pokemon is not null)
        {
            switch (_animationStage)
            {
                case 0: // Apply upward force from the pokemon we just hit
                    component.AddForce(Vector3.up * 2, ForceMode.Impulse);
                    _animationStage = 1;
                    break;
                
                case 1: // Check for when the Pokeball is coming down again
                    if (component.velocity.y < 0)
                    {
                        _animationStage = 2;
                    }
                    break;
                
                case 2: // Hang in thin air, rotate towards pokemon, open the pokeball, spawn a particle on the pokemon, remove the pokemon
                    component.isKinematic = true; // hang in thin air
                    Quaternion rotationTowardsPokemon = Quaternion.LookRotation(_pokemon.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationTowardsPokemon, Time.fixedDeltaTime * 3); // Rotate towards
                    pokeballAnimator.speed = 4; // Speed up when opening (handled by animator)
                    if (!_didOnce)
                    { 
                        Instantiate(pokeflashPf, _pokemon.transform.position, quaternion.identity);
                        _didOnce = true;
                    }
                    _pokemon.SetActive(false);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Open"))
                    {
                        _animationStage = 3;   
                    }
                    break;
                
                case 3: // Close pokeball
                    pokeballAnimator.SetInteger(State, 1);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Close"))
                    {
                        _animationStage = 4;
                        _terrain = null;
                    }
                    break;
                
                case 4: // Rotate towards player and drop to the ground
                    transform.LookAt(_trainer, Vector3.up);
                    component.isKinematic = false;
                    if (_terrain is not null)
                    {
                        _animationStage = 5;   
                    }
                    break;
                
                case 5: // Stop physics, Wiggle
                    component.isKinematic = true;
                    pokeballAnimator.SetInteger(State, 2);
                    pokeballAnimator.speed = 1.5f;

                    if (_checkForEscape)
                    {
                        int r = Random.Range(1, 10);
                        if (r == 1)
                        {
                            _escaped = true;
                            pokeballAnimator.speed = 0;
                            _didOnce = false;
                            _animationStage = 6;
                        }
                        StartCoroutine(WaitForCheck(1));
                        _checkForEscape = false;
                    }
                    
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f &&
                        pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Wiggle"))
                    {
                        pokeballAnimator.speed = 0;
                        _didOnce = false;
                        _animationStage = 6;
                    }
                    break;
                
                case 6: // Escape or not
                    if (_escaped)
                    {
                        if (!_didOnce)
                        { 
                            Instantiate(pokeflashPf, _pokemon.transform.position, quaternion.identity);
                            _pokeballAs1.clip = clipEscape;
                            _pokeballAs1.Play();
                            _didOnce = true;
                        }
                        Destroy(gameObject);
                        _pokemon.SetActive(true);
                    }
                    else
                    {
                        if (!_didOnce)
                        {
                            _pokeballAs1.clip = clipSuccess;
                            _pokeballAs1.Play();
                            _didOnce = true;
                        }
                        _levelmanager.RemovePokemon(_pokemon, true);
                    }
                    _disableCollisionSounds = false;
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pokemon") && _pokemon is null)
        {
            _pokemon = collision.gameObject;
            _pokeballAs1.clip = clipHit;
            _pokeballAs1.Play();
            _disableCollisionSounds = true;
        }
        if (collision.gameObject.name == "Terrain")
        {
            _terrain = collision.gameObject;
            if (!_disableCollisionSounds)
            {
                _pokeballAs1.clip = clipCollision;
                _pokeballAs1.Play();
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    public void wiggleSound()
    {
        _pokeballAs1.clip = clipWiggle;
        _pokeballAs1.Play();
    }
    
    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _checkForEscape = true;
    }
}
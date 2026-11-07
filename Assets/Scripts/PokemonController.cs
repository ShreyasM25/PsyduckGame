using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
// ReSharper disable All

public class PokemonController : MonoBehaviour
{
    private static readonly int Saunter = Animator.StringToHash("Saunter");
    private static readonly int Flee = Animator.StringToHash("Flee");
    private static readonly int Dig = Animator.StringToHash("Dig");

    private enum State
    {
        Chill, Saunter, Flee, Dig
    }

    [SerializeField]
    private State currentState;
    private bool _transitionActive;

    [SerializeField]
    private Vector3 currentDestination;
    
    [SerializeField]
    private float runSpeed;
    private float _walkingSpeed;

    private readonly float _viewAngle = 0.25f;
    private readonly float _viewDistance = 5f;
    
    private GameObject _trainer;
    private Animator _pokemonAnimator;

    private LevelManager _levelmanager;

    [SerializeField] private AudioClip[] psySounds;
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    [SerializeField] private AudioClip[] panicSounds;
    private AudioSource _pokemonAs1;
    private AudioSource _pokemonAs2;

    private bool _shutUp;
    
    // Start is called before the first frame update
    void Start()
    {
        _levelmanager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        _trainer = GameObject.Find("Trainer");
        _walkingSpeed = GetComponent<NavMeshAgent>().speed;
        _pokemonAnimator = GetComponent<Animator>();
        SwitchToState(State.Chill);

        _pokemonAs1 = GetComponents<AudioSource>()[0];
        _pokemonAs1.volume = 1f;
        _pokemonAs1.spatialBlend = 1f;
        _pokemonAs1.maxDistance = 5f;
        
        _pokemonAs2 = GetComponents<AudioSource>()[1];
        _pokemonAs2.volume = 0.25f;
        _pokemonAs2.spatialBlend = 1f;
        _pokemonAs2.maxDistance = 5f;

        _shutUp = true;
        Invoke(nameof(ResetShutUp), Random.Range(5f, 20f));
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Chill:
                PlaySound(State.Chill);
                if (_transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    Invoke(nameof(SwitchToSaunter), Random.Range(5.0f, 6.0f));
                    UpdatePokemonAnimator(false, false, false);
                    GetComponent<NavMeshAgent>().speed = 0.0f;
                    _transitionActive = false;
                }
                if (InView(_trainer, _viewAngle, _viewDistance))
                {
                    SwitchToState(State.Flee);
                }
                break;
            
            case State.Saunter:
                PlaySound(State.Saunter);
                if (_transitionActive)
                {
                    currentDestination = ValidDestination(false);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    UpdatePokemonAnimator(true, false, false);
                    GetComponent<NavMeshAgent>().speed = _walkingSpeed;
                    _transitionActive = false;
                }
                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    SwitchToState(State.Chill);
                }
                if (InView(_trainer, _viewAngle, _viewDistance))
                {
                    SwitchToState(State.Flee);
                }
                break;

            case State.Flee:
                PlaySound(State.Flee);
                if (_transitionActive)
                {
                    CancelInvoke(nameof(SwitchToSaunter));
                    Invoke(nameof(CheckForDig), 10f);
                    currentDestination = ValidDestination(true);
                    GetComponent<NavMeshAgent>().destination = currentDestination;
                    UpdatePokemonAnimator(false, true, false);
                    GetComponent<NavMeshAgent>().speed = runSpeed;
                    _transitionActive = false;
                }
                if ((transform.position - currentDestination).magnitude < 2.5f)
                {
                    CancelInvoke(nameof(CheckForDig));
                    CheckForDig();
                }
                break;
            
            case State.Dig:
                if (_transitionActive)
                {
                    currentDestination = transform.position;
                    GetComponent<NavMeshAgent>().speed = 0.0f;
                    UpdatePokemonAnimator(false, false, true);
                    _transitionActive = false;
                }
                break;
        }
    }

    void SwitchToState(State newState)
    {
        _transitionActive = true;
        currentState = newState;
    }

    void SwitchToSaunter()
    {
        SwitchToState(State.Saunter);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SwitchToSaunter));
        CancelInvoke(nameof(CheckForDig));
        SwitchToState(State.Flee);
    }

    void CheckForDig()
    {
        SwitchToState((transform.position - _trainer.transform.position).magnitude > 25f ? State.Chill : State.Dig);
    }

    public void DigCompleted()
    {
        _levelmanager.RemovePokemon(gameObject, false);
    }

    void UpdatePokemonAnimator(bool saunter, bool flee, bool dig)
    {
        _pokemonAnimator.SetBool(Saunter, saunter);
        _pokemonAnimator.SetBool(Flee, flee);
        _pokemonAnimator.SetBool(Dig, dig);
    }
    
    void PlaySound(State curState)
    {
        if (curState == State.Chill || curState == State.Saunter)
        {
            _pokemonAs1.loop = false;
            if (!_shutUp)
            {
                if (Random.Range(1, 10) == 1)
                {
                    _pokemonAs1.clip = psySounds[Random.Range(0, psySounds.Length)];
                    _pokemonAs1.Play();
                    _shutUp = true;
                    Invoke("ResetShutUp", Random.Range(5f, 20f));
                }
            }
        }

        if (curState != State.Flee) return;
        if (!_transitionActive) return;
        _pokemonAs1.clip = panicSounds[Random.Range(0, panicSounds.Length)];
        _pokemonAs1.loop = true;
        _pokemonAs1.Play();
    }

    private void ResetShutUp()
    {
        _shutUp = false;
    }

    private Vector3 ValidDestination(bool avoidTrainer)
    {
        float[,] boundaries = { { -70f, 40f }, { 0f, 70f } };
        float x = Random.Range(boundaries[0, 0], boundaries[0, 1]);
        float z = Random.Range(boundaries[1, 0], boundaries[1, 1]);
        if (avoidTrainer)
        {
            if (_trainer.transform.position.x - boundaries[0, 0] >= boundaries[0, 1] - _trainer.transform.position.x)
            {
                x = boundaries[0, 0];
            }
            else
            {
                x = boundaries[0, 1];
            }

            z = _trainer.transform.position.z - boundaries[1, 0] >= boundaries[1, 1] - _trainer.transform.position.z ? boundaries[1, 0] : boundaries[1, 1];
        } 
        Vector3 destination = new Vector3(x, Terrain.activeTerrain.SampleHeight(new Vector3(x, 0.0f, z)), z);
        return destination;
    }

    bool InView(GameObject target, float viewingAngle, float viewingDistance)
    {
        float dotProduct = Vector3.Dot(transform.forward, Vector3.Normalize(target.transform.position - transform.position));
        float view = 1.0f - viewingAngle;
        float distance = (transform.position - target.transform.position).magnitude;
        if (dotProduct >= view && distance < viewingDistance)
        {
            return true;
        }
        return false;
    }

    public float[] GetTextureMix(Vector3 pokemonPosition, Terrain terrain)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        // Position of player in relation to terrain alphamap
        int mapPositionX = Mathf.RoundToInt((pokemonPosition.x - terrainPosition.x) / terrainData.size.x *
                                            terrainData.alphamapWidth);
        int mapPositionZ = Mathf.RoundToInt((pokemonPosition.z - terrainPosition.z) / terrainData.size.z *
                                            terrainData.alphamapHeight);

        // 3D: 1st x, 2nd z, 3rd pct of the terrain layers (grass/sound) used
        float[,,] splatMapData = terrainData.GetAlphamaps(mapPositionX, mapPositionZ, 1, 1);
        
        // Extract all the values into that cellmix converting 3D array to 1D array
        float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatMapData[0, 0, i];
        }
        return cellMix;
    }

    public string FootStepLayerName(Vector3 pokemonPosition, Terrain terrain)
    {
        float[] cellMix = GetTextureMix(pokemonPosition, terrain);
        float strongestTexture = 0;
        int maxIndex = 0;

        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestTexture)
            {
                strongestTexture = cellMix[i];
                maxIndex = i;
            }
        }
        return terrain.terrainData.terrainLayers[maxIndex].name;
    }

    public void footStep()
    {
        _pokemonAs2.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (FootStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            _pokemonAs2.clip = sandSounds[Random.Range(0, sandSounds.Length)];
        }
        _pokemonAs2.Play();
    }
}

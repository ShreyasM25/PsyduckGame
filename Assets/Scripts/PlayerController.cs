using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
// ReSharper disable All

public class PlayerController : MonoBehaviour
{
    private static readonly int IsThrowing = Animator.StringToHash("IsThrowing");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    [SerializeField]
    [Tooltip("Insert Character Controller")]
    private CharacterController controller;
    
    [SerializeField]
    [Tooltip("Insert Main Camera")]
    private Camera mainCamera;
    
    [SerializeField]
    [Tooltip("Insert Animator Controller")]
    private Animator playerAnimator;
    
    [FormerlySerializedAs("pokeBallPF")]
    [SerializeField]
    [Tooltip("Insert Pokeball Prefab")]
    private GameObject pokeBallPf;
    
    [SerializeField]
    [Tooltip("Insert Pokeball Prefab")]
    private GameObject masterBallPf;
    
    [SerializeField]
    [Tooltip("Insert Pokeball Prefab")]
    private GameObject usedPokeBallPf;
    
    [SerializeField]
    [Tooltip("Insert Pokeball Bone Transform")]
    private Transform pokeBallBone;
    
    private Vector3 _velocity;
    private bool _grounded;
    private readonly float _gravity = -9.8f;
    private readonly float _groundCastDist = 0.05f;

    public float speed = 2f;
    public float runSpeed = 5f;
    public float jumpHeight = 15f;

    // ReSharper disable once RedundantDefaultMemberInitializer
    private bool _throwing = false;
    public float throwStrength = 4f;
    private GameObject _instantiatedPokeball;
    public int masterballCount = 3;
    
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    private AudioSource _playerAs1;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _playerAs1 = GetComponents<AudioSource>()[0];
        _playerAs1.volume = 0.3f;
        _playerAs1.spatialBlend = 1f;
    }

    // Update is called once per frame
    void Update()
    { 
        // Transform acquisition
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;
        
        // Grounded
        _grounded = Physics.Raycast(playerTransform.position, Vector3.down, _groundCastDist);
        
        // DEBUG - Visualize raycast
        /*
        if (grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.red);
        }
        */
        
        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (usedPokeBallPf == pokeBallPf)
            {
                usedPokeBallPf = masterBallPf;
            }
            else
            {
                usedPokeBallPf = pokeBallPf;
            }
        }
        
        // Throw
        if (Input.GetButtonDown("Fire1") && _grounded)
        {
            _throwing = true;
            SpawnPokeballToBone();
            playerAnimator.SetBool(IsThrowing, true);
        }
        
        // Apply movement
        if (!_throwing)
        {
            // Regular movement and running
            if (Input.GetKey(KeyCode.LeftShift))
            {
                controller.Move(movement * (runSpeed * Time.deltaTime));
                playerAnimator.SetBool(IsRunning, true);
                _playerAs1.volume = 0.25f; // Louder footsteps when running
            }
            else
            {
                controller.Move(movement * (speed * Time.deltaTime));
                playerAnimator.SetBool(IsRunning, false);
                _playerAs1.volume = 0.10f;
            }
            
            // Gravity and jumping
            _velocity.y += _gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump") && _grounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight);
            }
            controller.Move(_velocity * Time.deltaTime);
            playerAnimator.SetBool(IsJumping, !_grounded);
        }

        playerAnimator.SetBool(IsWalking, movement.magnitude > 0);

        // Rotate alongside the camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
    }

    public void ThrowEnded()
    {
        _throwing = false;
        playerAnimator.SetBool(IsThrowing, false);
    }

    private void SpawnPokeballToBone()
    {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (_instantiatedPokeball is null)
        {
            if (usedPokeBallPf == masterBallPf)
            {
                if (masterballCount > 0)
                {
                    masterballCount--;
                    _instantiatedPokeball = Instantiate(usedPokeBallPf, pokeBallBone, false);
                }
            }
            else
            {
                _instantiatedPokeball = Instantiate(usedPokeBallPf, pokeBallBone, false);
            }
        }
    }

    public void ReleasePokeball()
    {
        if (_instantiatedPokeball != null)
        {
            _instantiatedPokeball.transform.parent = null;
            _instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            _instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            Transform cameraTransform = mainCamera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 0.5f, 0f);
            Vector3 throwVector = (cameraTransform.forward + throwAdjustment) * throwStrength;
            _instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
            _instantiatedPokeball = null;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private float[] GetTextureMix(Vector3 playerPosition, Terrain terrain)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        // Position of player in relation to terrain alphamap
        int mapPositionX = Mathf.RoundToInt((playerPosition.x - terrainPosition.x) / terrainData.size.x *
                                            terrainData.alphamapWidth);
        int mapPositionZ = Mathf.RoundToInt((playerPosition.z - terrainPosition.z) / terrainData.size.z *
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

    private string FootStepLayerName(Vector3 playerPosition, Terrain terrain)
    {
        float[] cellMix = GetTextureMix(playerPosition, terrain);
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

    // ReSharper disable once InconsistentNaming
    public void footStep()
    {
        _playerAs1.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (FootStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            _playerAs1.clip = sandSounds[Random.Range(0, sandSounds.Length)];
        }
        _playerAs1.Play();
    }
}

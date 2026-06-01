using UnityEngine;

public class FootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceAudio
    {
        public string surfaceTag;
        public AudioClip[] clips;
    }

    [SerializeField] private SurfaceAudio[] surfaces;
    [SerializeField] private AudioClip[] defaultClips;
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private AudioSource audioSource;
    private PlayerMovement movement;
    private float stepTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        if (!movement.IsGrounded || controller.velocity.magnitude < 0.5f)
        {
            stepTimer = 0;
            return;
        }

        float interval = movement.IsSprinting ? stepInterval * 0.6f
                       : movement.IsCrouching ? stepInterval * 1.6f
                       : stepInterval;

        stepTimer += Time.deltaTime;
        if (stepTimer >= interval)
        {
            stepTimer = 0;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        AudioClip[] clips = defaultClips;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.5f, groundMask))
        {
            foreach (var surface in surfaces)
            {
                if (hit.collider.CompareTag(surface.surfaceTag) && surface.clips.Length > 0)
                {
                    clips = surface.clips;
                    break;
                }
            }
        }

        if (clips == null || clips.Length == 0) return;

        float volume = movement.IsCrouching ? 0.25f : 0.7f;
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
}

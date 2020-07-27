using UnityEngine;

public class SSGScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 1f;

    public Camera fpsCam;

    public GameObject muzzleFlashObject;
    public float muzzleFlashTimer = 0.1f;
    private float muzzleFlashTimerStart;
    public bool muzzleFlashEnabled = false;
    
    public GameObject impactEffect;
    private Animator animator;

    private AudioSource audioSource;

    private float nextTimeToFire = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        muzzleFlashTimerStart = muzzleFlashTimer;
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            audioSource.Play();
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

        if (muzzleFlashEnabled == true)
        {
            animator.SetBool("Fire", true);
            muzzleFlashObject.SetActive(true);
            muzzleFlashTimer -= Time.deltaTime;
        }

        if (muzzleFlashTimer <= 0)
        {
            animator.SetBool("Fire", false);
            muzzleFlashObject.SetActive(false);
            muzzleFlashEnabled = false;
            muzzleFlashTimer = muzzleFlashTimerStart;
        }
    }

    void Shoot()
    {
        muzzleFlashEnabled = true;
    }
}

using UnityEngine;

public class SSGScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 3000f;
    public float fireRate = 1f;
    public int pellets = 20;
    public float horizontalSpread = 0.5f;
    public float verticalSpread = 2f;
    public float spreadThightness = 0.03f;

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
            for (int i = 0; i < pellets; i++)
            {
                Shoot();
            }
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
        RaycastHit hit;
        muzzleFlashEnabled = true;

        Vector3 spread = new Vector3(0,0,0);
        spread += fpsCam.transform.up * Random.Range(-horizontalSpread, horizontalSpread);
        spread += fpsCam.transform.right * Random.Range(-verticalSpread, verticalSpread);

        fpsCam.transform.forward += spread.normalized * Random.Range(0f, spreadThightness);

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        { 
            Target target = hit.transform.GetComponent<Target>();

            Debug.DrawLine(fpsCam.transform.position, hit.point, Color.green);

            if (target != null)
            {
                target.TakeDamage(damage);
            }

            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
        }
        else
        {
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward, Color.red, 3000f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class SSGScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 3000f;
    public float fireRate = 1f;
    public int pellets = 20;
    public float spreadAngle = 10f;

    public Camera fpsCam;

    public GameObject muzzleFlashObject;
    public float muzzleFlashTimer = 0.1f;
    private float muzzleFlashTimerStart;
    public bool muzzleFlashEnabled = false;

    public GameObject impactEffect;
    public GameObject bulletHoleImg;
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

        for (int i = 0; i < pellets; i++)
        {
            RaycastHit hit;

            Quaternion fireRotation = Quaternion.LookRotation(fpsCam.transform.forward);

            Quaternion randomRotation = Random.rotation;

            fireRotation = Quaternion.RotateTowards(fireRotation, randomRotation, Random.Range(0.0f, spreadAngle));

            if (Physics.Raycast(fpsCam.transform.position, fireRotation * Vector3.forward, out hit, range))
            {
                Target target = hit.transform.GetComponent<Target>();

                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                hit.point = new Vector3(hit.point.x + 0.001f, hit.point.y + 0.001f, hit.point.z + 0.001f);

                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                GameObject bulletHole = Instantiate(bulletHoleImg, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
                Destroy(bulletHole, 30f);
            }
        }
     }
    
}

using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroppedItem : Item
{
    [Header("Drop variables")]
    [SerializeField] private Vector3 initialVelocity;
    [SerializeField] private float gravity = 0.6f;
    [SerializeField] private Raycaster[] groundChecks;

    private Rigidbody rb;

    protected new void Awake()
    {
        this.rb = GetComponent<Rigidbody>();

        this.rb.AddForce(initialVelocity * 100, ForceMode.VelocityChange);

        base.Awake();
    }

    private void FixedUpdate()
    {
        if (!this.groundChecks.Any(g => g.Hit.transform != null))
            this.rb.AddForce(this.transform.up * this.gravity * -100);
        else
            this.rb.velocity = Vector3.zero;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.PLAYER))
        {
            var car = other.GetComponent<CarMovement>();
            if (car != null)
            {
                car.Stun(this.effectTime);
                this.StartParticles(car.transform);
            }
        }

        base.OnTriggerEnter(other);
    }
}

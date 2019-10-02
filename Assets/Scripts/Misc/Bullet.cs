using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.PlayVRoom.Managers;

namespace DivIt.PlayVRoom.Misc
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private LayerMask _hittableLayers;
        [SerializeField] private float _distance;
        [Space(10)]
        [SerializeField] private float _bulletForce = 200;

        Ray _ray;
        RaycastHit _hit;
        bool _goOnce;

        private BulletImpactManager _bulletImpactManager;

        private void Awake()
        {
            _ray = new Ray(transform.position, transform.forward);
            _hit = new RaycastHit();

            _bulletImpactManager = BulletImpactManager.Instance;
        }

        private void FixedUpdate()
        {
            if (_goOnce)
            {
                return;
            }

            bool raycasted = Physics.Raycast(_ray, out _hit, _distance, _hittableLayers);
            if (raycasted)
            {
                if (_hit.collider)
                {
                    Debug.Log("Bullet hit: " + _hit.transform.name);

                    //Create impact effect
                    _bulletImpactManager.CreateImpact(_hit.point, _hit.normal);

                    Rigidbody hitRb;
                    if (hitRb = _hit.rigidbody)
                    {
                        hitRb.AddForceAtPosition(
                            _bulletForce * transform.forward,
                            _hit.point
                            );
                    }
                }
            }

            _goOnce = true;
        }
    } 
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nitro
{
    /// <summary>
    /// A component that makes it easier to keep track of objects that have collided with an object.
    /// </summary>
    public sealed class Collidable : MonoBehaviour
    {
        private HashSet<Collider> collisions = new HashSet<Collider>();

        /// <summary>
        /// Returns a list of all the collided objects
        /// </summary>
        public IEnumerable<Collider> Collisions => collisions;

        public event Action<Collider> OnCollideStart;
        public event Action<Collider, bool> OnCollideStop;


        void OnTriggerEnter(Collider other)
        {
            if (collisions.Add(other) && enabled)
            {
                OnCollideStart?.Invoke(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (collisions.Remove(other) && enabled)
            {
                OnCollideStop?.Invoke(other, false);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collisions.Add(collision.collider) && enabled)
            {
                OnCollideStart?.Invoke(collision.collider);
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if (collisions.Remove(collision.collider) && enabled)
            {
                OnCollideStop?.Invoke(collision.collider, false);
            }
        }

        void LateUpdate()
        {
            foreach (Collider collider in collisions)
            {
                if (collider == null)
                {
                    OnCollideStop?.Invoke(collider, true);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        void OnDisable()
        {
            foreach (Collider collider in collisions)
            {
                OnCollideStop?.Invoke(collider, collider == null);
            }
            collisions.RemoveWhere(c => c == null);
        }

        void OnEnable()
        {
            foreach (Collider collider in collisions)
            {
                if (collider != null)
                {
                    OnCollideStart?.Invoke(collider);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        void OnDestroy()
        {
            if (enabled)
            {
                foreach (Collider collider in collisions)
                {
                    OnCollideStop?.Invoke(collider, collider == null);
                }
                collisions.RemoveWhere(c => c == null);
            }
        }
    }
}

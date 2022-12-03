using System.Collections.Generic;
using UnityEngine;

namespace Nitro
{
    /// <summary>
    /// A component that makes it easier to keep track of objects that have collided with an object.
    /// </summary>
    public abstract class Collidable : MonoBehaviour
    {
        private HashSet<Collider> collisions = new HashSet<Collider>();

        /// <summary>
        /// Returns a list of all the collided objects
        /// </summary>
        public IEnumerable<Collider> Collisions => collisions;

        /// <summary>
        /// Called when an object collides with this object.
        /// </summary>
        /// <param name="body">The rigidbody on the collided object</param>
        protected abstract void OnCollideStart(Collider collider);

        /// <summary>
        /// Called when an object is no longer colliding with this object.
        /// </summary>
        /// <param name="body">The rigidbody on the collided object</param>
        protected abstract void OnCollideStop(Collider collider, bool destroyed);


        protected virtual void OnTriggerEnter(Collider other)
        {
            if (collisions.Add(other) && enabled)
            {
                OnCollideStart(other);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (collisions.Remove(other) && enabled)
            {
                OnCollideStop(other, false);
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collisions.Add(collision.collider) && enabled)
            {
                OnCollideStart(collision.collider);
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (collisions.Remove(collision.collider) && enabled)
            {
                OnCollideStop(collision.collider, false);
            }
        }

        protected virtual void LateUpdate()
        {
            foreach (Collider collider in collisions)
            {
                if (collider == null)
                {
                    OnCollideStop(collider, true);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnDisable()
        {
            foreach (Collider collider in collisions)
            {
                OnCollideStop(collider, collider == null);
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnEnable()
        {
            foreach (Collider collider in collisions)
            {
                if (collider != null)
                {
                    OnCollideStart(collider);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnDestroy()
        {
            if (enabled)
            {
                foreach (Collider collider in collisions)
                {
                    OnCollideStop(collider, collider == null);
                }
                collisions.RemoveWhere(c => c == null);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitro
{
    /// <summary>
    /// A component that makes it easier to keep track of objects that have collided with an object.
    /// </summary>
    public abstract class Collidable2D : MonoBehaviour
    {
        private HashSet<Collider2D> collisions = new HashSet<Collider2D>();

        /// <summary>
        /// Returns a list of all the collided objects
        /// </summary>
        public IEnumerable<Collider2D> CollidedBodies => collisions;

        /// <summary>
        /// Called when an object collides with this object.
        /// </summary>
        /// <param name="body">The rigidbody on the collided object</param>
        protected abstract void OnCollideStart(Collider2D Collider2D);

        /// <summary>
        /// Called when an object is no longer colliding with this object.
        /// </summary>
        /// <param name="body">The rigidbody on the collided object</param>
        protected abstract void OnCollideStop(Collider2D Collider2D, bool destroyed);

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (collisions.Add(other) && enabled)
            {
                OnCollideStart(other);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (collisions.Remove(other) && enabled)
            {
                OnCollideStop(other, false);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collisions.Add(collision.collider) && enabled)
            {
                OnCollideStart(collision.collider);
            }
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            if (collisions.Remove(collision.collider) && enabled)
            {
                OnCollideStop(collision.collider, false);
            }
        }

        protected virtual void LateUpdate()
        {
            foreach (Collider2D Collider2D in collisions)
            {
                if (Collider2D == null)
                {
                    OnCollideStop(Collider2D, true);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnDisable()
        {
            foreach (Collider2D Collider2D in collisions)
            {
                OnCollideStop(Collider2D, Collider2D == null);
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnEnable()
        {
            foreach (Collider2D Collider2D in collisions)
            {
                if (Collider2D != null)
                {
                    OnCollideStart(Collider2D);
                }
            }
            collisions.RemoveWhere(c => c == null);
        }

        protected virtual void OnDestroy()
        {
            if (enabled)
            {
                foreach (Collider2D Collider2D in collisions)
                {
                    OnCollideStop(Collider2D, Collider2D == null);
                }
                collisions.RemoveWhere(c => c == null);
            }
        }
    }
}

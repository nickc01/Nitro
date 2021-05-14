using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nitro
{
	/// <summary>
	/// A special kind of variable that can easily be modified and reverted, making it easier to have stats that are changed temporarily when a powerup is collected
	/// </summary>
	/// <typeparam name="T">The value type of the nitro variable</typeparam>
	[Serializable]
	public sealed class NitroVariable<T> : INitroVariable
	{
		class RoutineRunner : MonoBehaviour { }

		struct Modifier
		{
			public class Sorter : IComparer<Modifier>
			{
				Comparer<int> numberComparer = Comparer<int>.Default;
				Comparer<float> floatComparer = Comparer<float>.Default;
				Comparer<uint> idComparer = Comparer<uint>.Default;
				public int Compare(Modifier x, Modifier y)
				{
					if (x.Priority == y.Priority)
					{
						if (x.TimeAdded == y.TimeAdded)
						{
							return idComparer.Compare(x.NitroID, y.NitroID);
						}
						else
						{
							return floatComparer.Compare(x.TimeAdded, y.TimeAdded);
						}
					}
					else
					{
						return numberComparer.Compare(x.Priority, y.Priority);
					}
				}
			}

			public uint NitroID;
			public T Value;
			public int Priority;
			public UnityEngine.Object SourceObject;
			public float TimeAdded;

			public override bool Equals(object obj)
			{
				return obj is Modifier mod && mod.NitroID == NitroID;
			}

			public override string ToString()
			{
				return Value.ToString();
			}

			public override int GetHashCode()
			{
				return NitroID.GetHashCode();
			}
		}

		private static RoutineRunner routineRunner;
		private static uint idCounter = 0;

		SortedSet<Modifier> Modifiers = new SortedSet<Modifier>(new Modifier.Sorter());

		[SerializeField]
		private T baseValue;

		[SerializeField]
		private T value;

		/// <inheritdoc/>
		object INitroVariable.BaseValue { get => BaseValue; set => BaseValue = (T)value; }
		/// <inheritdoc/>
		object INitroVariable.Value => Value;

		/// <summary>
		/// The base value of the <see cref="NitroVariable{T}"/>, without any modifiers applied
		/// </summary>
		public T BaseValue { get => baseValue; set => baseValue = value; }
		/// <summary>
		/// The current value of the <see cref="NitroVariable{T}"/>
		/// </summary>
		public T Value
		{
			get
			{
				if (Modifiers.RemoveWhere(m => m.SourceObject.Equals(null)) > 0)
				{
					value = Modifiers.Count == 0 ? BaseValue : Modifiers.Max.Value;
				}
				return value;
			}
		}

		/// <summary>
		/// Returns how many modifiers are currently being applied to the variable
		/// </summary>
		public int ModifiersApplied
		{
			get
			{
				if (Modifiers.RemoveWhere(m => m.SourceObject.Equals(null)) > 0)
				{
					value = Modifiers.Count == 0 ? BaseValue : Modifiers.Max.Value;
				}
				return Modifiers.Count;
			}
		}

		/// <inheritdoc/>
		Type INitroVariable.ValueType => typeof(T);

		/// <summary>
		/// Constructs a new nitro variable
		/// </summary>
		/// <param name="baseValue">The base value of the variable</param>
		public NitroVariable(T baseValue)
		{
			this.baseValue = baseValue;
			value = baseValue;
		}

		/// <summary>
		/// Constructs a new nitro variable
		/// </summary>
		public NitroVariable() : this(default) { }

		/// <summary>
		/// Modifies the variable with a new value
		/// </summary>
		/// <param name="value">The new value for the variable</param>
		/// <param name="sourceObject">The source object making the modification. The modification will automatically be removed when the object is destroyed</param>
		/// <param name="priority">How much priority should the modifier have over other modifiers</param>
		public void Modify(T value, GameObject sourceObject, int priority)
		{
			ModifyInternal(value, sourceObject, priority, 0f);
		}

		///<inheritdoc cref="NitroVariable{T}.Modify(T, GameObject, int)"/>
		public void Modify(T value, Component sourceObject, int priority)
		{
			ModifyInternal(value, sourceObject, priority, 0f);
		}

		///<inheritdoc cref="NitroVariable{T}.Modify(T, GameObject, int)"/>
		/// <param name="timeActive">How long should the modifier be applied for</param>
		public void Modify(T value, GameObject sourceObject, int priority, float timeActive)
		{
			ModifyInternal(value, sourceObject, priority, timeActive);
		}

		///<inheritdoc cref="NitroVariable{T}.Modify(T, GameObject, int, float)"/>
		public void Modify(T value, Component sourceObject, int priority, float timeActive)
		{
			ModifyInternal(value, sourceObject, priority, timeActive);
		}

		void ModifyInternal(T value, UnityEngine.Object sourceObject, int priority, float timeActive)
		{
			if (!Application.isPlaying)
			{
				throw new Exception("Nitro Variables can only be modified during play mode");
			}
			var nitroID = unchecked(idCounter += 1);
			if (nitroID == 0)
			{
				nitroID++;
			}

			Modifiers.Add(new Modifier
			{
				Priority = priority,
				NitroID = nitroID,
				Value = value,
				SourceObject = sourceObject,
				TimeAdded = Time.unscaledTime
			});

			this.value = Modifiers.Max.Value;

			if (timeActive > 0f)
			{
				if (routineRunner == null)
				{
					routineRunner = new GameObject("ROUTINE RUNNER").AddComponent<RoutineRunner>();
					routineRunner.gameObject.hideFlags = HideFlags.HideInHierarchy;
					UnityEngine.Object.DontDestroyOnLoad(routineRunner.gameObject);
				}
				routineRunner.StartCoroutine(Waiter());
			}

			IEnumerator Waiter()
			{
				yield return new WaitForSeconds(timeActive);
				Revert(nitroID);
			}
		}

		/// <summary>
		/// Modifies the variable with a new value
		/// </summary>
		/// <param name="value">The new value for the variable</param>
		/// <param name="priority">How much priority should the modifier have over other modifiers</param>
		/// <returns></returns>
		public uint Modify(T value, int priority)
		{
			return Modify(value, priority, 0f);
		}

		/// <inheritdoc cref="Modify(T, int)"/>
		/// <param name="timeActive">How long should the modifier be applied for</param>
		public uint Modify(T value, int priority, float timeActive)
		{
			if (!Application.isPlaying)
			{
				throw new Exception("Nitro Variables can only be modified during play mode");
			}
			var nitroID = unchecked(idCounter += 1);
			if (nitroID == 0)
			{
				nitroID++;
			}

			Modifiers.Add(new Modifier
			{
				Priority = priority,
				NitroID = nitroID,
				Value = value,
			});

			this.value = Modifiers.Max.Value;

			if (timeActive > 0f)
			{
				if (routineRunner == null)
				{
					routineRunner = new GameObject("ROUTINE RUNNER").AddComponent<RoutineRunner>();
					routineRunner.gameObject.hideFlags = HideFlags.HideInHierarchy;
					UnityEngine.Object.DontDestroyOnLoad(routineRunner.gameObject);
				}
				routineRunner.StartCoroutine(Waiter());
			}

			IEnumerator Waiter()
			{
				yield return new WaitForSeconds(timeActive);
				Revert(nitroID);
			}

			return nitroID;
		}

		/// <summary>
		/// Reverts a modifier
		/// </summary>
		/// <param name="id">The id for the modifier</param>
		public void Revert(uint id)
		{
			if (!Application.isPlaying)
			{
				throw new Exception("Nitro Variables can only be reverted during play mode");
			}
			if (Modifiers.RemoveWhere(m => m.NitroID == id) > 0)
			{
				value = Modifiers.Count == 0 ? BaseValue : Modifiers.Max.Value;
			}
		}

		/// <summary>
		/// Reverts a modifier
		/// </summary>
		/// <param name="sourceObject">The source object making the modification</param>
		public void Revert(GameObject sourceObject)
		{
			RevertInternal(sourceObject);
		}

		/// <inheritdoc cref="Revert(GameObject)"/>
		public void Revert(Component sourceObject)
		{
			RevertInternal(sourceObject);
		}

		void RevertInternal(UnityEngine.Object sourceObject)
		{
			if (!Application.isPlaying)
			{
				throw new Exception("Nitro Variables can only be reverted during play mode");
			}
			if (sourceObject == null)
			{
				return;
			}
			if (Modifiers.RemoveWhere(m => m.SourceObject.Equals(sourceObject)) > 0)
			{
				value = Modifiers.Count == 0 ? BaseValue : Modifiers.Max.Value;
			}
		}

		/// <summary>
		/// Reverts all modifications
		/// </summary>
		public void RevertAll()
		{
			if (!Application.isPlaying)
			{
				throw new Exception("Nitro Variables can only be reverted during play mode");
			}
			Modifiers.Clear();
			value = BaseValue;
		}

		/// <summary>
		/// Determines if a modifier with the specified id has been applied to this variable
		/// </summary>
		/// <param name="id">The id of the modifier</param>
		/// <returns>Returns whether the modifier with the specified id is currently applied to the variable</returns>
		public bool HasModifierApplied(uint id)
		{
			return Modifiers.Any(m => m.NitroID == id);
		}

		/// <summary>
		/// Determines if a modifier with the specified source object has been applied to this variable
		/// </summary>
		/// <param name="sourceObject"></param>
		/// <returns>Returns whether the modifier with the specified source object is currently applied to the variable</returns>
		public bool HasModifierApplied(GameObject sourceObject)
		{
			return Modifiers.Any(m => m.SourceObject == sourceObject);
		}

		/// <inheritdoc cref="HasModifierApplied(GameObject)"/>
		public bool HasModifierApplied(Component sourceObject)
		{
			return Modifiers.Any(m => m.SourceObject == sourceObject);
		}

		/// <summary>
		/// Implicitly converts from a nitro variable to the variable of type T
		/// </summary>
		/// <param name="v">The nitro variable</param>
		public static implicit operator T(NitroVariable<T> v)
		{
			return v.Value;
		}

		/// <summary>
		/// Implicitly converts from a variable of type T to a nitro variable
		/// </summary>
		/// <param name="value">The variable</param>
		public static implicit operator NitroVariable<T>(T value)
		{
			return new NitroVariable<T>(value);
		}

		public override bool Equals(object obj)
		{
			return obj is NitroVariable<T> n && n.value.Equals(value);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}
	}
}
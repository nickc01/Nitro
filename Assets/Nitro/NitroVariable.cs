using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class NitroVariable<T> : IModVariable
{
	class RoutineRunner : MonoBehaviour
	{

	}

	struct ModifierComparer : IComparer<Modifier>
	{
		Comparer<int> numberComparer;

		public int Compare(Modifier x, Modifier y)
		{
			if (numberComparer == null)
			{
				numberComparer = Comparer<int>.Default;
			}
			return numberComparer.Compare(y.Priority, x.Priority);
		}
	}

	struct Modifier
	{
		public UnityEngine.Object SourceObject;
		public T Value;
		public int Priority;
	}

	[NonSerialized]
	private SortedSet<Modifier> modDefinitions = new SortedSet<Modifier>(new ModifierComparer());

	private static RoutineRunner routineRunner;

	[NonSerialized]
	bool definitionsDirty = false;

	private T baseValue;

	[SerializeField]
	private T value;

	public NitroVariable(T baseValue)
	{
		value = baseValue;
	}

	public NitroVariable() : this(default) { }

	public void Modify(UnityEngine.Object sourceObject,T newValue, int priority)
	{
		if (sourceObject == null)
		{
			throw new Exception("The source object passed in is null");
		}
		int previousCount = modDefinitions.Count;

		modDefinitions.Add(new Modifier
		{
			Priority = priority,
			SourceObject = sourceObject,
			Value = newValue
		});

		definitionsDirty = true;

		UpdateValue(previousCount);
	}

	public void Modify(UnityEngine.Object sourceObject, T newValue, int priority, float time)
	{
		if (routineRunner == null)
		{
			routineRunner = new GameObject("ROUTINE RUNNER").AddComponent<RoutineRunner>();
			routineRunner.gameObject.hideFlags = HideFlags.HideInHierarchy;
			GameObject.DontDestroyOnLoad(routineRunner.gameObject);
		}

		Modify(sourceObject, newValue, priority);

		routineRunner.StartCoroutine(Waiter());

		IEnumerator Waiter()
		{
			yield return new WaitForSeconds(time);
			Revert(sourceObject);
		}
	}

	public void Revert(UnityEngine.Object sourceObject)
	{
		int previousCount = modDefinitions.Count;
		modDefinitions.RemoveWhere(m => m.SourceObject.GetInstanceID() == sourceObject.GetInstanceID());

		definitionsDirty = true;

		UpdateValue(previousCount);
	}


	void UpdateValue(int previousCount)
	{
		var removedAmount = modDefinitions.RemoveWhere(m => m.SourceObject.Equals(null));

		if (removedAmount > 0)
		{
			definitionsDirty = true;
		}
		if (definitionsDirty)
		{
			definitionsDirty = false;
			if (previousCount == 0 && modDefinitions.Count >= 1)
			{
				baseValue = value;
				value = modDefinitions.First().Value;
			}
			else if (previousCount >= 1 && modDefinitions.Count == 0)
			{
				value = baseValue;
			}
			else
			{
				if (modDefinitions.Count > 0)
				{
					value = modDefinitions.First().Value;
				}
				else
				{
					value = baseValue;
				}
			}
		}
	}

	public void RevertAll()
	{
		int previousCount = modDefinitions.Count;

		modDefinitions.Clear();

		definitionsDirty = true;

		UpdateValue(previousCount);
	}


	public T BaseValue
	{
		get
		{
			UpdateValue(modDefinitions.Count);
			if (modDefinitions.Count == 0)
			{
				return value;
			}
			else
			{
				return baseValue;
			}
		}
		set
		{
			UpdateValue(modDefinitions.Count);
			if (modDefinitions.Count == 0)
			{
				this.value = value;
			}
			else
			{
				baseValue = value;
			}
		}
	}

	public T Value
	{
		get
		{
			UpdateValue(modDefinitions.Count);
			return value;
		}
	}

	public static implicit operator T(NitroVariable<T> v)
	{
		return v.Value;
	}
}

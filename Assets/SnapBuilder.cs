/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SnapBuilder : MonoBehaviour
{
	[SerializeField]
	Vector3 snapIncrements = new Vector3(1f,1f,1f);
	[SerializeField]
	Vector3 snapOffsets = new Vector3(0.5f,0.5f,0.5f);

	IEnumerator routine;

	bool ctrlUsed = false;
	bool holdingCtrl = false;

	private void Start()
	{
		Debug.Log("SNAP START");


		EditorApplication.update += EditorUpdate;
	}

	private void OnEnable()
	{
		routine = Placer();
	}

	private void OnDisable()
	{
		routine = null;
	}

	static double _timeSinceStartup = -1.0;
	public static double DT
	{
		get
		{
			if (_timeSinceStartup < 0.0)
			{
				_timeSinceStartup = EditorApplication.timeSinceStartup;
				return 0.0;
			}
			else
			{
				var newTime = EditorApplication.timeSinceStartup;
				var dt = newTime - _timeSinceStartup;
				_timeSinceStartup = newTime;
				return dt;
			}
		}
	}

	private void EditorUpdate()
	{
		if (routine != null && !routine.MoveNext())
		{
			routine = null;
		}
	}

	Vector3 previousPosition = Vector3.zero;

	GameObject[] previousSelections;

	IEnumerator Placer()
	{
		while (true)
		{
			var selections = Selection.gameObjects;

			if (SelectionsAreDifferent(previousSelections,selections))
			{
				SelectionUpdate(selections);
				previousSelections = selections;
			}

			if (FindLowestBounds(selections,out var lowest))
			{
				var lowestSnapped = SnapVector(lowest);
				if (previousPosition != lowestSnapped)
				{
					OnPositionUpdate(selections, lowestSnapped, previousPosition, lowest);
					previousPosition = lowestSnapped;
				}

				var difference = lowest - previousPosition;

				for (int i = 0; i < selections.GetLength(0); i++)
				{
					selections[i].transform.position -= difference;
				}
			}

			if (Input.GetKey(KeyCode.Z))
			{
				if (!holdingCtrl)
				{
					holdingCtrl = true;
					ctrlUsed = false;
				}
			}
			else
			{
				if (holdingCtrl)
				{
					holdingCtrl = false;
					ctrlUsed = false;
				}
			}

			yield return null;
		}
	}

	bool SelectionsAreDifferent(GameObject[] a, GameObject[] b)
	{
		if (a == null || b == null)
		{
			return a != b;
		}

		if (a.GetLength(0) != b.GetLength(0))
		{
			return true;
		}

		bool different = false;

		int length = a.GetLength(0);

		for (int i = 0; i < length; i++)
		{
			if (a[i] != b[i])
			{
				different = true;
				break;
			}
		}

		return different;
	}

	Vector3 SnapVector(Vector3 source)
	{
		Vector3 lowerBound = new Vector3(Mathf.Floor(source.x), Mathf.Floor(source.y), Mathf.Floor(source.z));
		return new Vector3(Mathf.Round(source.x - lowerBound.x) + lowerBound.x + snapOffsets.x, Mathf.Round(source.y - lowerBound.y) + lowerBound.y + snapOffsets.y, Mathf.Round(source.z - lowerBound.z) + lowerBound.z + snapOffsets.z);
	}

	bool FindLowestBounds(GameObject[] objs, out Vector3 lowestBounds)
	{
		if (objs == null)
		{
			lowestBounds = default;
			return false;
		}
		GameObject lowest = null;
		Vector3 lowestVector = default;

		for (int i = 0; i < objs.GetLength(0); i++)
		{
			if (lowest == null)
			{
				lowest = objs[i];
				lowestVector = objs[i].transform.position;
			}
			else
			{
				var position = objs[i].transform.position;
				if (position.x < lowestVector.x)
				{
					lowest = objs[i];
					lowestVector.x = position.x;
				}
				if (position.y < lowestVector.y)
				{
					lowest = objs[i];
					lowestVector.y = position.y;
				}
				if (position.z < lowestVector.z)
				{
					lowest = objs[i];
					lowestVector.z = position.z;
				}
			}
		}
		if (lowest == null)
		{
			lowestBounds = default;
			return false;
			//return null;
		}
		else
		{
			lowestBounds = lowestVector;
			return true;
			//return lowestVector;
		}
	}

	void SelectionUpdate(GameObject[] selections)
	{
		if (FindLowestBounds(selections, out var lowest))
		{
			previousPosition = SnapVector(lowest);
		}
	}

	void OnPositionUpdate(GameObject[] objs, Vector3 newPosition, Vector3 oldPosition, Vector3 newPositionUnsnapped)
	{
		Debug.Log("Holding Z = " + holdingCtrl);
		if (holdingCtrl && !ctrlUsed)
		{
			Debug.Log("Using Control");
			ctrlUsed = true;
			Undo.IncrementCurrentGroup();
			foreach (var obj in objs)
			{
				var instance = GameObject.Instantiate(obj, obj.transform.position, obj.transform.rotation);
				instance.transform.position -= (newPositionUnsnapped - oldPosition);
				Undo.RegisterCreatedObjectUndo(instance,"Duplicate");
			}
		}
	}
}
*/
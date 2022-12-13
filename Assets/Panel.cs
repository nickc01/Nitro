using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    class RoutineRunner : MonoBehaviour
    {

    }

    static RoutineRunner _runner;
    static RoutineRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                var gm = new GameObject("ROUTINE_RUNNER");
                gm.hideFlags = HideFlags.HideAndDontSave;
                _runner = gm.AddComponent<RoutineRunner>();
            }
            return _runner;
        }
    }


    static Canvas _canvas;

    public static Canvas Canvas => _canvas ??= GameObject.FindObjectOfType<Canvas>();

    static AnimationCurve PanelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [SerializeField]
    float movementTime = 1f;

    [SerializeField]
    SlideDirection slideDirection = SlideDirection.Up;

    static void GetInAndOutLocations(SlideDirection direction, out Vector2 fromPos, out Vector2 toPos)
    {
        switch (direction)
        {
            case SlideDirection.Left:
                fromPos = new Vector2(Canvas.renderingDisplaySize.x * 2,0f);
                break;
            case SlideDirection.Right:
                fromPos = new Vector2(-Canvas.renderingDisplaySize.x * 2, 0f);
                break;
            case SlideDirection.Up:
                fromPos = new Vector2(0f, -Canvas.renderingDisplaySize.y * 2);
                break;
            case SlideDirection.Down:
                fromPos = new Vector2(0f, Canvas.renderingDisplaySize.y * 2);
                break;
            default:
                fromPos = Vector2.zero;
                break;
        }
        toPos = -fromPos;
    }

    public void SwitchToPanel(Panel destination)
    {
        SwitchPanels(this, destination, slideDirection, movementTime);
    }


    public void SwitchToPanel(Panel destination, SlideDirection slideDirection)
    {
        SwitchPanels(this, destination, slideDirection, movementTime);
    }


    public void SwitchToPanelWithTime(Panel destination, SlideDirection slideDirection, float time)
    {
        SwitchPanels(this, destination, slideDirection,time);
    }


    public static void SwitchPanels(Panel from, Panel to, SlideDirection slideDirection, float time)
    {
        Runner.StopAllCoroutines();
        Runner.StartCoroutine(SwitchPanelsRoutine(from, to, slideDirection,time));
    }

    static IEnumerator SwitchPanelsRoutine(Panel from, Panel to, SlideDirection slideDirection, float time)
    {
        to.gameObject.SetActive(true);
        from.gameObject.SetActive(true);

        SetButtonsState(from, false);
        SetButtonsState(to, false);

        GetInAndOutLocations(slideDirection, out var fromPos, out var toPos);

        Debug.Log("FROM POS = " + fromPos);
        Debug.Log("TO POS = " + toPos);

        Debug.Log("FROM PANEL = " + from);
        Debug.Log("TO PANEL = " + to);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            from.transform.localPosition = Vector2.Lerp(Vector2.zero,toPos, PanelCurve.Evaluate(t / time));
            to.transform.localPosition = Vector2.Lerp(fromPos,Vector2.zero, PanelCurve.Evaluate(t / time));
            yield return null;
        }

        from.transform.localPosition = toPos;
        to.transform.localPosition = Vector2.zero;

        from.gameObject.SetActive(false);

        SetButtonsState(to, true);
    }

    static void SetButtonsState(Panel panel, bool enabled)
    {
        foreach (var button in panel.GetComponentsInChildren<Button>())
        {
            button.enabled = enabled;
        }
    }
}

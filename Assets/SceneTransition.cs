using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class SceneTransition : MonoBehaviour
    {
        Graphic[] graphics;

        [field: SerializeField]
        public float TransitionTime { get; private set; } = 0.5f;

        private void Awake()
        {
            graphics = GetComponentsInChildren<Graphic>();
            GameObject.DontDestroyOnLoad(gameObject);
        }

        Coroutine mainRoutine;


        public float Show()
        {
            return Fade(1f, TransitionTime, false);
        }

        public void HideInstant()
        {
            FadeInstant(0f, false);
        }

        public void ShowInstant()
        {
            FadeInstant(1f, false);
        }

        public float Hide()
        {
            return Fade(0f, TransitionTime, false);
        }

        public float HideAndDestroy()
        {
            return Fade(0f, TransitionTime, true);
        }

        float Fade(float to, float time, bool destroy = false)
        {
            if (mainRoutine != null)
            {
                StopCoroutine(mainRoutine);
                mainRoutine = null;
            }
            mainRoutine = StartCoroutine(FadeRoutine(to, time, destroy));
            return time;
        }

        IEnumerator FadeRoutine(float toAlpha, float time, bool destroy = false)
        {
            Color[] froms = graphics.Select(g => g.color).ToArray();
            Color[] tos = graphics.Select(g => new Color(g.color.r, g.color.g, g.color.b, toAlpha)).ToArray();

            for (float t = 0; t < time; t += Time.deltaTime)
            {
                for (int i = 0; i < graphics.Length; i++)
                {
                    graphics[i].color = Color.Lerp(froms[i], tos[i],t / time);
                }
                yield return null;
            }

            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = tos[i];
            }

            if (destroy)
            {
                Destroy(gameObject);
            }
        }

        void FadeInstant(float toAlpha, bool destroy = false)
        {
            Color[] tos = graphics.Select(g => new Color(g.color.r, g.color.g, g.color.b, toAlpha)).ToArray();
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = tos[i];
            }

            if (destroy)
            {
                Destroy(gameObject);
            }
        }
    }
}

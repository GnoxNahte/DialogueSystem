using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class SpeechBubble : MonoBehaviour
    {
        // Animation State
        enum AnimType
        {
            Show,
            Close,
            SayLine,
            Others, // Might be idle, disabled, etc
        }

        [System.Serializable]
        // Coroutine animation data
        class AnimData
        {
            public IEnumerator enumerator;
            public AnimType type;

            public AnimData(IEnumerator enumerator, AnimType type)
            {
                this.enumerator = enumerator;
                this.type = type;
            }
        }

        [SerializeField] SpriteRenderer background;
        [SerializeField] TextMeshPro textBubble;

        [SerializeField] Vector2 padding = new Vector2(0.75f, 0.75f);

        [SerializeField]
        [ReadOnly]
        List<AnimData> animationQueue;

        WaitForSeconds timeBetweenCharacters;

        // Set active / inactive to this
        // Allows starting coroutines, inactive objects can't start coroutines
        GameObject childGameObj;

        public bool IsAnimating => animationQueue.Count > 0;

        private void Awake()
        {
            childGameObj = transform.GetChild(0).gameObject;
            childGameObj.SetActive(false);
        }

        private void Start()
        {
            // Converting from [word / mins] to [seconds / character] (time between each characters)
            // Average characters in a word = 4.7
            // words / min  = 4.7 * characters / 60 * seconds
            timeBetweenCharacters = new WaitForSeconds(1 / (DialogueSystem.TalkingSpeed * 4.7f / 60f));
        }

        private void OnDisable()
        {
            StopAllAnimations();
        }

        public IEnumerator Show(string initialText = "", Transform promptParent = null)
        {
            IEnumerator enumerator = ShowSpeechBubble_Routine(initialText, promptParent);
            AddAnimation(enumerator, AnimType.Show);

            return AllAnimationDoneCheck();
        }

        public IEnumerator Close()
        {
            if (animationQueue.Count == 0 && !childGameObj.activeSelf)
                return null;

            IEnumerator enumerator = CloseSpeechBubble_Routine();

            if (!AddAnimation(enumerator, AnimType.Close) &&
                (animationQueue[0].type != AnimType.Show))
            {
                animationQueue.RemoveAll((x) => x.type == AnimType.Show);
            }

            return AllAnimationDoneCheck();
        }

        public IEnumerator SayLine(string dialogue)
        {
            if (childGameObj.activeSelf)
                textBubble.text = "";
            else
                Show();


            IEnumerator enumerator = SayLine_Routine(dialogue);
            AddAnimation(enumerator, AnimType.SayLine);

            return AllAnimationDoneCheck();
        }

        // Returns if it is already animating the animType
        private bool AddAnimation(IEnumerator enumerator, AnimType type)
        {
            // If there are no animations, start the animation
            if (animationQueue.Count == 0)
            {
                animationQueue.Add(new AnimData(enumerator, type));
                StartCoroutine(animationQueue[0].enumerator);
                return false;
            }

            // Start from 1 since 0 is animating
            for (int i = 1; i < animationQueue.Count; i++)
            {
                if (animationQueue[i].type == type)
                {
                    animationQueue.RemoveAt(i);
                    animationQueue.Add(new AnimData(enumerator, type));
                    return false;
                }
            }

            animationQueue.Add(new AnimData(enumerator, type));

            return false;
        }

        // Returns when all the animations are done
        public IEnumerator AllAnimationDoneCheck()
        {
            yield return new WaitUntil(() => animationQueue.Count == 0);
        }

        public void StopAllAnimations()
        {
            StopAllCoroutines();
            animationQueue.Clear();
        }

        private IEnumerator ShowSpeechBubble_Routine(string initialText = "", Transform promptParent = null)
        {
            AnimationCurve appearScaleAnimation = DialogueSystem.AppearScaleAnimation;

            if (promptParent != null)
                transform.SetParent(promptParent, false);
            textBubble.text = initialText;
            if (!childGameObj.activeSelf)
            {
                float lerpValue = 0f;
                childGameObj.SetActive(true);
                Color backgroundColor = background.color;

                ForceUpdateBackgroundPos();

                while (lerpValue <= 1f)
                {
                    transform.localScale = appearScaleAnimation.Evaluate(lerpValue) * Vector3.one;
                    textBubble.alpha = lerpValue;
                    backgroundColor.a = lerpValue;
                    background.color = backgroundColor;

                    lerpValue += Time.deltaTime / DialogueSystem.AppearDuration;

                    yield return null;
                }

                transform.localScale = appearScaleAnimation.keys[appearScaleAnimation.length - 1].value * Vector3.one;
                textBubble.alpha = 1f;
                backgroundColor.a = 1f;
                background.color = backgroundColor;
            }
            else
            {
                yield return null;
            }

            OnDoneAnimation(AnimType.Show);
        }

        private IEnumerator CloseSpeechBubble_Routine()
        {
            // Exit if the speech bubble is not active
            if (!childGameObj.activeSelf)
            {
                OnDoneAnimation(AnimType.Close);
                yield break;
            }

            // Opposite of making the speech bubble appearing 
            // Since opposite, flip lerpValue such that it starts as 1 and end at 0
            float lerpValue = 1f;
            childGameObj.SetActive(true);
            Color backgroundColor = background.color;
            AnimationCurve appearScaleAnimation = DialogueSystem.AppearScaleAnimation;
            while (lerpValue >= 0f)
            {
                transform.localScale = appearScaleAnimation.Evaluate(lerpValue) * Vector3.one;
                textBubble.alpha = lerpValue;
                backgroundColor.a = lerpValue;
                background.color = backgroundColor;

                // TODO (GnoxNahte): Revert back to Time.deltaTime instead of Time.unscaledDeltaTime
                lerpValue -= Time.unscaledDeltaTime / DialogueSystem.AppearDuration;

                yield return null;
            }

            childGameObj.SetActive(false);

            OnDoneAnimation(AnimType.Close);
        }

        private IEnumerator SayLine_Routine(string dialogue)
        {
            if (string.IsNullOrWhiteSpace(dialogue))
            {
                Debug.LogError("Dialogue is Null / empty / White space");
                yield break;
            }

            transform.localScale = Vector3.one;
            textBubble.alpha = 1f;
            background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);

            foreach (char letter in dialogue)
            {
                textBubble.text += letter;

                //textBubble.ForceMeshUpdate();

                // NOTE: It will always update the text bounds for the text on the next frame
                Bounds textBounds = textBubble.textBounds;

                // When there is no text (textBubble.text == ""), textBounds.size will be int.Min (Negative)
                if (textBounds.size.x > 0)
                {
                    background.size = (Vector2)textBounds.size + padding * 2f;
                    background.transform.localPosition = textBounds.center;
                }
                // TODO (GnoxNahte): (0.1f * textBubble.fontSize) is temporary, replace it with something to get the height 
                else
                    background.size = (0.1f * textBubble.fontSize) * Vector2.one + 2f * padding;

                yield return timeBetweenCharacters;
            }

            Bounds finalTextBounds = textBubble.textBounds;
            background.size = (Vector2)finalTextBounds.size + padding * 2f;
            background.transform.localPosition = finalTextBounds.center;

            OnDoneAnimation(AnimType.SayLine);
        }


        // When done animating a type (Show, Close, SayLine, etc)
        // The animType param is just to just check
        private void OnDoneAnimation(AnimType type)
        {
            if (animationQueue[0].type != type)
                Debug.LogError("Wrong animation: " + type.ToString());

            animationQueue.RemoveAt(0);

            if (animationQueue.Count > 0)
                StartCoroutine(animationQueue[0].enumerator);
        }

        private void ForceUpdateBackgroundPos()
        {
            if (string.IsNullOrWhiteSpace(textBubble.text))
            {
                textBubble.text = "A";
                textBubble.ForceMeshUpdate();
                Bounds textBounds = textBubble.textBounds;
                background.size = (Vector2)textBounds.size + padding * 2f;
                background.transform.localPosition = textBounds.center;
                textBubble.text = "";
            }
            else
            {
                textBubble.ForceMeshUpdate();
                Bounds textBounds = textBubble.textBounds;
                background.size = (Vector2)textBounds.size + padding * 2f;
                background.transform.localPosition = textBounds.center;
            }
        }

        [ContextMenu("Auto Setup")]
        public void AutoSetUp()
        {
            background = GetComponentInChildren<SpriteRenderer>();
            textBubble = GetComponentInChildren<TextMeshPro>();

            ForceUpdateBackgroundPos();
        }

        private void OnDrawGizmosSelected()
        {
            // Only change when editor
            if (Application.isPlaying)
                return;

            ForceUpdateBackgroundPos();
            Bounds textBounds = textBubble.textBounds;

            Gizmos.DrawWireCube(textBounds.center + background.transform.localPosition, (Vector2)textBounds.size + padding * 2f);
            //Gizmos.DrawWireCube()
        }
    }
}

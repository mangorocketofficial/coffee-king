using System.Collections;
using CoffeeKing.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    /// <summary>
    /// MonoBehaviour host for result screen animations (score roll-up and earnings reveal).
    /// Attached to the result screen canvas at creation time.
    /// </summary>
    public sealed class ResultAnimator : MonoBehaviour
    {
        private Text scoreText;
        private Text earningsText;
        private Coroutine animCoroutine;

        public void Initialize(Text scoreText, Text earningsText)
        {
            this.scoreText = scoreText;
            this.earningsText = earningsText;
        }

        public void PlayReveal(int finalScore, int maxScore, int dailyEarnings, long totalEarnings)
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
            }

            animCoroutine = StartCoroutine(RunReveal(finalScore, maxScore, dailyEarnings, totalEarnings));
        }

        public void StopAnimation()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }
        }

        private IEnumerator RunReveal(int finalScore, int maxScore, int dailyEarnings, long totalEarnings)
        {
            // Phase 1: Score roll-up from 0 to final score over ~1.0s
            var percentage = maxScore > 0 ? (float)finalScore / maxScore : 0f;
            scoreText.text = $"Score 0/{maxScore}   0%";
            earningsText.text = $"Today  {ScoreManager.FormatWon(0)}   |   Total  {ScoreManager.FormatWon(totalEarnings - dailyEarnings)}";

            var rollDuration = 1.0f;
            var elapsed = 0f;
            while (elapsed < rollDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / rollDuration);
                // Ease out cubic for satisfying deceleration
                var eased = 1f - (1f - t) * (1f - t) * (1f - t);
                var currentScore = Mathf.RoundToInt(Mathf.Lerp(0f, finalScore, eased));
                var currentPct = Mathf.Lerp(0f, percentage * 100f, eased);
                scoreText.text = $"Score {currentScore}/{maxScore}   {currentPct:0}%";
                yield return null;
            }

            scoreText.text = $"Score {finalScore}/{maxScore}   {(percentage * 100f):0}%";

            // Phase 2: Earnings roll-up
            yield return new WaitForSecondsRealtime(0.3f);

            var earningsRollDuration = 0.8f;
            elapsed = 0f;
            while (elapsed < earningsRollDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / earningsRollDuration);
                var eased = 1f - (1f - t) * (1f - t) * (1f - t);
                var currentDailyEarnings = Mathf.RoundToInt(Mathf.Lerp(0f, dailyEarnings, eased));
                var currentTotalEarnings = (long)Mathf.Lerp(totalEarnings - dailyEarnings, totalEarnings, eased);
                earningsText.text = $"Today  {ScoreManager.FormatWon(currentDailyEarnings)}   |   Total  {ScoreManager.FormatWon(currentTotalEarnings)}";
                yield return null;
            }

            earningsText.text = $"Today  {ScoreManager.FormatWon(dailyEarnings)}   |   Total  {ScoreManager.FormatWon(totalEarnings)}";

            // Brief color flash for earnings
            var originalColor = earningsText.color;
            earningsText.color = Color.white;
            var flashElapsed = 0f;
            var flashDuration = 0.2f;
            while (flashElapsed < flashDuration)
            {
                flashElapsed += Time.unscaledDeltaTime;
                earningsText.color = Color.Lerp(Color.white, originalColor, flashElapsed / flashDuration);
                yield return null;
            }

            earningsText.color = originalColor;
            animCoroutine = null;
        }
    }
}

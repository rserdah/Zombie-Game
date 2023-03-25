using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public GameObject m_scoreUser;
    private IScoreUser scoreUser;

    public VerticalLayoutGroup grid;

    public List<Text> scoreQueue;
    public List<Text> queueOverflow;
    public List<int> scores;
    public int maxQueuedScores = 10;
    public float scoreDisplayTime = 3f;
    public float multipleScoreDisplayTime = 0.75f; //If multiple scores in queue, this delay is used before adding the score and removing it from the queue (usually a smaller delay b/c Player already saw all the scores in the queue for a while)

    public GameObject scoreTextPrefab;

    public int score;
    public int addScore;
    public int newScore;
    public int increment = 1;
    public float incrementMultiplier = 0.1f;
    public int scoreStreakCount = 5;

    public int lastStreakScoreAdded;
    public bool justAddedScoreStreak;
    public bool hadFirstScoreStreak;
    public float lastTimeAddedScore;
    public float maxInactiveScoreStreakTime = 4f;

    public bool reachedNewScore;
    public bool set;

    public IEnumerator coroutine;



    private void Start()
    {
        if(m_scoreUser)
            scoreUser = m_scoreUser.GetComponent<IScoreUser>();

        grid = GetComponentInChildren<VerticalLayoutGroup>();

        //StartCoroutine(ScrollToNewScore2());
        StartCoroutine(AddScoreLine());

        //Cache coroutine so performance is better (seems to lag when just using StartCoroutine and StopCoroutine without caching)
        coroutine = ScrollToNewScore2();
    }

    private void Update()
    {
        if(set)
        {
            StartScrollToNewScore(addScore);

            set = false;
        }
    }

    public void StartScrollToNewScore(int addScore)
    {
        newScore += addScore;
        increment = (int)((newScore - score) * incrementMultiplier);

        //If addScore and incrementMultiplier are so small that increment turns out to be zero, make it 1 so that it actually adds to the score
        if(increment == 0)
            increment = 1;

        reachedNewScore = false;

        StopCoroutine(coroutine);
        StartCoroutine(coroutine);
    }

    public void AddScore(int addedScore)
    {
        if(!hadFirstScoreStreak && scores.Count > 0 && addedScore == scores[scores.Count - 1])
        {
            lastStreakScoreAdded = addedScore;
            justAddedScoreStreak = true;
            scores[scores.Count - 1] += addedScore;
            scoreQueue[scoreQueue.Count - 1].text = "+" + scores[scores.Count - 1];


            hadFirstScoreStreak = true;
        }
        else if(scores.Count > 0 && ((justAddedScoreStreak && addedScore == lastStreakScoreAdded) || (!justAddedScoreStreak && addedScore == scores[scores.Count - 1])))
        {
            //Set lastStreakScoreAdded to addedScore (won't matter for cases where they are already equal BUT is necessary for when it is starting a new streak)
            lastStreakScoreAdded = addedScore;
            justAddedScoreStreak = true;
            lastTimeAddedScore = Time.time;
            scores[scores.Count - 1] += addedScore;
            scoreQueue[scoreQueue.Count - 1].text = "+" + scores[scores.Count - 1];
        }
        //If this is the first score in the queue, assume it will start a streak (even if that means the it turns out to not be a streak)
        else if(scores.Count == 0)
        {
            AddNewTextAndScore(GetNewScore(addedScore), addedScore);

            //test this part
            //also 
            //fix part where it is not supposed to remove the score line if it is in the middle of a streak (part in coroutine)

            lastStreakScoreAdded = addedScore;
            justAddedScoreStreak = true;
            lastTimeAddedScore = Time.time;
        }
        else
        {
            AddNewTextAndScore(GetNewScore(addedScore), addedScore);

            lastStreakScoreAdded = 0;
            justAddedScoreStreak = false;
            lastTimeAddedScore = 0;
        }
    }

    private void AddNewTextAndScore(Text text, int score)
    {
        scoreQueue.Add(text);
        scores.Add(score);
    }

    private void RemoveTextAndScore(int index)
    {
        scoreQueue.RemoveAt(index);
        scores.RemoveAt(index);

        Destroy(grid.transform.GetChild(index).gameObject);
    }

    private Text GetNewScore(int score)
    {
        GameObject g = Instantiate(scoreTextPrefab);
        Text scoreText = g.GetComponent<Text>();

        g.transform.parent = grid.transform;
        scoreText.text = "+" + score;


        return scoreText;
    }

    private IEnumerator AddScoreLine()
    {
        while(true)
        {
            if(scoreQueue.Count > 0 && !justAddedScoreStreak)
            {
                if(scoreQueue.Count == 1)
                    yield return new WaitForSeconds(scoreDisplayTime);
                else
                    yield return new WaitForSeconds(multipleScoreDisplayTime);

                //scoreUser.AddScore(scores[0]);
                StartScrollToNewScore(scores[0]);
                RemoveTextAndScore(0);
            }
            //else if(justAddedScoreStreak)
            //{
            //    yield return new WaitForSeconds(5f);

            //    if(justAddedScoreStreak)
            //        justAddedScoreStreak = false;
            //}
            else
            {
                yield return new WaitForFixedUpdate();

                if(Time.time - lastTimeAddedScore >= maxInactiveScoreStreakTime)
                {
                    justAddedScoreStreak = false;
                }
            }
        }
    }

    //private IEnumerator ScrollToNewScore()
    //{
    //    while(true)
    //    {
    //        if(score < newScore && !reachedNewScore)
    //        {
    //            yield return new WaitForSeconds(0.01f);

    //            score += increment;
    //            scoreUser.AddScore(increment);

    //            Debug.LogError("Adding increment");
    //        }
    //        else
    //        {
    //            scoreUser.AddScore(newScore - score); //If score exceeded newScore, subtract the difference from the score
    //            score += newScore - score;
    //            reachedNewScore = true;

    //            Debug.LogError("Subtracting difference");

    //            break;

    //            //yield return new WaitForFixedUpdate();
    //        }
    //    }
    //}

    private IEnumerator ScrollToNewScore2()
    {
        while(true)
        {
            if(score < newScore && !reachedNewScore)
            {
                yield return new WaitForSeconds(0.01f);

                score += increment;
                scoreUser.AddScore(increment);
            }
            else
            {
                scoreUser.AddScore(newScore - score); //If score exceeded newScore, subtract the difference from the score
                score += newScore - score;
                reachedNewScore = true;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}

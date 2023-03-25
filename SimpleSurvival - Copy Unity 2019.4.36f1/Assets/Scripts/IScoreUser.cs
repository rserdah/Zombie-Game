using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScoreUser
{
    int GetScore();

    void AddScore(int addedScore);
}

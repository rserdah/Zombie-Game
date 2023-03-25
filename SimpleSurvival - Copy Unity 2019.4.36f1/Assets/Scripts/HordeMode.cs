using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HordeMode : GameMode
{
    protected static float preWaveTimer = 5f;

    public SpawnEnemies spawner;
    private event Action onWaveEnd;
    private event Action onWaveStart;
    private bool waveActive;
    private int enemiesPerWave;
    private float enemyHealthMultiplier = 1f;
    public int wave { get; private set; }


    public HordeMode(Game game, SpawnEnemies _spawner) : base(game)
    {
        spawner = _spawner;
        //Can't win in horde mode
        winConditions = null;

        spawner.constantSpawn = false;
        spawner.spawnCount = 0;
        spawner.enemiesConstantPursuit = true;

        wave = 1;

        enemiesPerWave = 7;

        onWaveStart += () => StartWave();

        onWaveEnd += () =>
        {
            enemiesPerWave += 15;
            enemyHealthMultiplier += 0.15f;

            wave++;
        };
    }

    private void OnWaveStart()
    {
        game?.StartCoroutine(PreWaveTimer());
    }

    private IEnumerator PreWaveTimer()
    {
        yield return new WaitForSeconds(preWaveTimer);

        StartWave();
    }

    public void StartWave()
    {
        spawner.Spawn(enemiesPerWave, (GameObject g) => { Enemy e = g.GetComponent<Enemy>(); e.body.fightingSkills.health = e.body.fightingSkills.maxHealth *= enemyHealthMultiplier; });
        Transform canvas = TransformHelper.FindRecursive(UI.instance.transform, "c_hordemode");
        Image waveTallies = TransformHelper.FindRecursive(canvas, "i_wavecounter").GetComponent<Image>();
        Text waveText = TransformHelper.FindRecursive(canvas, "t_wavecounter").GetComponent<Text>();

        if(canvas && waveTallies && waveText)
        {
            bool useTallies = wave <= 10;
            Sprite tally = Resources.Load<Sprite>("UI/Sprites/tally/tally" + wave);
            waveTallies.gameObject.SetActive(useTallies);
            waveText.gameObject.SetActive(!useTallies);
            if(useTallies) waveTallies.sprite = tally;
            else waveText.text = wave.ToString();
        }
    }

    public override void GameLoop()
    {
        if(spawner.enemiesLeft <= 0)
        {
            //If no enemies and the wave is still active, then the wave just ended
            if(waveActive)
            {
                onWaveEnd?.Invoke();
                waveActive = false;
            }

            //If no enemies and the wave is not active, then the wave has not started yet
            if(!waveActive)
            {
                onWaveStart?.Invoke();
                waveActive = true;
            }
        }

        ////////////Debug.LogError($"enemies: {spawner.enemiesLeft}");
    }
}

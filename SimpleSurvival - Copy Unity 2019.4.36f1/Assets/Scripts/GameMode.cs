using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameMode
{
    protected Game game;

    [Serializable]
    public abstract class GameParameter
    {
        public abstract string name { get; set; }
    }

    [Serializable]
    public class GameParameters
    {
        [SerializeField]
        private List<GameParameter> parameters
        {
            get
            {
                List<GameParameter> list = new List<GameParameter>();
                foreach(GameString g in gameStrings)
                    list.Add(g);
                foreach(GameInt g in gameInts)
                    list.Add(g);
                foreach(GameFloat g in gameFloats)
                    list.Add(g);

                return list;
            }
        }

        [Header("Game Strings")]
        [SerializeField]
        private List<GameString> gameStrings;
        [Header("Game Ints")]
        [SerializeField]
        private List<GameInt> gameInts;
        [Header("Game Floats")]
        [SerializeField]
        private List<GameFloat> gameFloats;
        //protected List<GameParameter> parameters;

        public GameParameters()
        {

        }

        public GameParameter Get(string name)
        {
            foreach(GameParameter gp in parameters)
                if(gp.name.Equals(name))
                    return gp;

            return null;
        }

        public string GetString(string name)
        {
            GameParameter param = Get(name);

            if(param.GetType() == typeof(GameString))
                return ((GameString)param).value;

            if(param != null) Debug.LogError($"GameParameter '{name}' is of type {param.GetType()}, not GameString.");
            else Debug.LogError($"GameParameter '{name}' does not exist.");

            return "";
        }

        public int GetInt(string name)
        {
            GameParameter param = Get(name);

            if(param.GetType() == typeof(GameInt))
                return ((GameInt)param).value;

            if(param != null) Debug.LogError($"GameParameter '{name}' is of type {param.GetType()}, not GameInt.");
            else Debug.LogError($"GameParameter '{name}' does not exist.");

            return -1;
        }

        public float GetFloat(string name)
        {
            GameParameter param = Get(name);

            if(param.GetType() == typeof(GameFloat))
                return ((GameFloat)param).value;

            if(param != null) Debug.LogError($"GameParameter '{name}' is of type {param.GetType()}, not GameFloat.");
            else Debug.LogError($"GameParameter '{name}' does not exist.");

            return -1f;
        }
    }

    public interface IGameParameter<T>
    {
        T value { get; set; }
    }

    [Serializable]
    public class GameString : GameParameter, IGameParameter<string>
    {
        [SerializeField]
        private string m_name;
        public override string name { get => m_name; set { m_name = value; } }

        [SerializeField]
        private string m_value;
        public string value { get { return m_value; } set { m_value = value; } }


        public GameString(string _name, string _value)
        {
            m_name = _name;
            m_value = _value;
        }
    }

    [Serializable]
    public class GameInt : GameParameter, IGameParameter<int>
    {
        [SerializeField]
        private string m_name;
        public override string name { get => m_name; set { m_name = value; } }

        [SerializeField]
        private int m_value;
        public int value { get { return m_value; } set { m_value = value; } }


        public GameInt(string _name, int _value)
        {
            m_name = _name;
            m_value = _value;
        }
    }

    [Serializable]
    public class GameFloat : GameParameter, IGameParameter<float>
    {
        [SerializeField]
        private string m_name;
        public override string name { get => m_name; set { m_name = value; } }

        [SerializeField]
        private float m_value;
        public float value { get { return m_value; } set { m_value = value; } }


        public GameFloat(string _name, float _value)
        {
            m_name = _name;
            m_value = _value;
        }
    }

    public struct GameCondition
    {
        public delegate bool Condition();
        public Condition condition;

        public bool Evaluate()
        {
            return (bool)condition?.Invoke();
        }
    }

    [SerializeField]
    public GameParameters gameParameters = new GameParameters();

    public GameCondition[] winConditions;


    public GameMode(Game game)
    {
        this.game = game;
    }

    public virtual void GameLoop()
    {

    }
}

using UnityEngine;

public static class GameSettings
{
    public enum Difficulty { Easy, Medium, Hard }

    public static Difficulty CurrentDifficulty = Difficulty.Easy;
}
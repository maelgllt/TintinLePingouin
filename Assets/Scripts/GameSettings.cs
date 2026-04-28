using UnityEngine;

public static class GameSettings
{
    // Difficulty levels
    public enum Difficulty { Easy, Medium, Hard }

    // Global difficulty storage
    public static Difficulty CurrentDifficulty = Difficulty.Easy;
}
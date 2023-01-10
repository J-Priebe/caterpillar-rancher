using UnityEngine;

// cardinal direction expressed as a rotation
// with up being 0
public static class Constants
{
    public const float ANGLE_LEFT = 90f;
    public const float ANGLE_RIGHT = -90f;
    public const float ANGLE_DOWN = 180f;
    public const float ANGLE_UP = 0f;

    public const int tilesX = 7;
    public const int tilesY = 12;

    // caterpillar positions we pause at for tutorial stages
    public static Coords TUTORIAL_FIRST_PAUSE_HEAD_POSITION = new Coords(0, 3);
    public static Coords TUTORIAL_SECOND_PAUSE_HEAD_POSITION = new Coords(0, 6);
    public static Vector3Int TUTORIAL_FIRST_FOOD_SPAWN_POSITION = new Vector3Int(4, 7);
    public static Vector3Int TUTORIAL_SECOND_FOOD_SPAWN_POSITION = new Vector3Int(0, 3);
    public static Vector3Int TUTORIAL_SUPERFOOD_SPAWN_POSITION = new Vector3Int(5, 9);
    public static Coords TUTORIAL_WALL_AHEAD_PROMPT_POSITION = new Coords(5, 11);

    public static Vector3Int TUTORIAL_FIRST_ARROW_POSITION = new Vector3Int(0, 7);
    public static Vector3Int TUTORIAL_SECOND_ARROW_POSITION = new Vector3Int(5, 7);
    public static Vector3Int TUTORIAL_FIRST_ARROW_DIRECTION = Vector3Int.right;
    public static Vector3Int TUTORIAL_SECOND_ARROW_DIRECTION = Vector3Int.up;


    public static Vector3Int TUTORIAL_THIRD_ARROW_POSITION = new Vector3Int(5, 11);
    public static Vector3Int TUTORIAL_FOURTH_ARROW_POSITION = new Vector3Int(0, 11);
    public static Vector3Int TUTORIAL_THIRD_ARROW_DIRECTION = Vector3Int.left;
    public static Vector3Int TUTORIAL_FOURTH_ARROW_DIRECTION = Vector3Int.down;
}

public enum TutorialState
{

    // arrow placement is disabled, food spawns at set point
    Start = 0,
    MoveToFirstArrow = 1,
    // on first game, after short delay
    // game pauses and gesture prompt to place arrow at specific spot
    PlaceFirstArrow = 2,
    MoveToSecondArrow = 3,
    // then anothher short delay til pickup first arrow,
    // pause, prompt next swipe
    PlaceSecondArrow = 4,
    MoveToLeaf = 5,
    MoveToApple = 6,
    // must place two arrows in sequence
    WallAhead = 7,
    PlaceThirdArrow = 8,
    PlaceFourthArrow = 9,
    MoveToThirdArrow = 10,
    MoveToFourthArrow = 11,
    // free arrow placement enabled
    UntilFirstCrash = 12,
    // after crashing, tutorial is complete
    Complete = 13
}

public readonly struct Coords
{
    public Coords(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Coords(Vector3Int v)
    {
        X = v.x;
        Y = v.y;
    }

    public int X { get; }
    public int Y { get; }

    public override string ToString() => $"({X}, {Y})";

    public override bool Equals(object other)
    {
        return other is Coords && Equals((Coords)other);
    }

    public bool Equals(Coords other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }
}
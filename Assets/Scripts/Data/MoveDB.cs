using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    private static Dictionary<string, MoveBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();
        
        var moveList = Resources.LoadAll<MoveBase>("");
        foreach (var move in moveList)
        {
            if (!moves.TryAdd(move.Name, move))
            {
                Debug.LogError(move.Name + " is already in use");
            }
        }
    }

    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.TryGetValue(name, out MoveBase move))
        {
            Debug.LogError($"Move with name {move} not in database.");
            return null;
        }

        return move;
    }
}

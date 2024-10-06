using System;

public class Move
{
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.Name);
        PP = saveData.PP;
    }

    public MoveSaveData GetSaveData()
    {
        return new MoveSaveData
        {
            Name = Base.Name,
            PP = PP,
        };
    }
}

[Serializable]
public class MoveSaveData
{
    public string Name { get; set; }
    public int PP { get; set; }
}

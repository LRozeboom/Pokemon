using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.Name);
        HP = saveData.Hp;
        level = saveData.Level;
        Exp = saveData.Exp;

        Moves = saveData.Moves.Select(x => new Move(x)).ToList();
        
        Status = saveData.StatusId != null ? ConditionsDB.Conditions[saveData.StatusId.Value] : null;
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public PokemonBase Base => _base;

    public int Level => level;

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public Queue<string> StatusChanges { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public bool HpChanged { get; set; }

    public event Action OnStatusChanged;

    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }
    
    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData
        {
            Name = Base.Name,
            Hp = HP,
            Level = level,
            Exp = Exp,
            StatusId = Status?.Id,
            Moves = Moves.Select(x => x.GetSaveData()).ToList(),
        };
        
        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5 },
            { Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5 }
        };

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0 },
            {Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }

        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn.Base));
    }

    public int Attack => GetStat(Stat.Attack);

    public int Defense => GetStat(Stat.Defense);

    public int SpAttack => GetStat(Stat.SpAttack);

    public int SpDefense => GetStat(Stat.SpDefense);

    public int Speed => GetStat(Stat.Speed);

    public int MaxHp { get; private set; }

    /// <summary>
    /// Let's the pokemon take damage from an enemy pokemon move.
    /// </summary>
    /// <param name="move">The move that the pokemon takes damage from.</param>
    /// <param name="attacker">The attacker that performs the move.</param>
    /// <returns>Boolean value whether the pokemon has fainted.</returns>
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        // Critical hits have a multiplier of 1.5 starting from gen 5.
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 1.5f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * (attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null)
        {
            return;
        }

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null)
        {
            return;
        }

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(move => move.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[Serializable]
public class PokemonSaveData
{
    public string Name { get; set; }
    public int Hp { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public ConditionID? StatusId { get; set; }
    public List<MoveSaveData> Moves { get; set; }
}

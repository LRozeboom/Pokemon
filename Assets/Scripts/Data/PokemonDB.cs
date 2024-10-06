using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{
    private static Dictionary<string, PokemonBase> pokemons;

    public static void Init()
    {
        pokemons = new Dictionary<string, PokemonBase>();
        
        var pokemonArray = Resources.LoadAll<PokemonBase>("");

        foreach (var pokemon in pokemonArray)
        {
            if (!pokemons.TryAdd(pokemon.Name, pokemon))
            {
                Debug.LogError(pokemon.Name + " is already in DB");
            }
        }
    }
    
    public static PokemonBase GetPokemonByName(string name)
    {
        if (!pokemons.TryGetValue(name, out var pBase))
        {
            Debug.LogError(name + " is not in DB");
            return null;
        }
        
        return pBase;
    }
}

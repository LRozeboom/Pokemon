using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Remove diagonal movement
            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.black, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        foreach (var collider in colliders) 
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();

            if (triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public string Name => name;

    public Sprite Sprite => sprite;

    public Character Character => character;
    
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            Pokemons = GetComponent<PokemonParty>().Pokemons.Select(x => x.GetSaveData()).ToList()
        };
            
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        
        // Restore player position
        var position = saveData.Position;
        transform.position = new Vector3(position[0], position[1]);
        
        // Restore player party
        GetComponent<PokemonParty>().Pokemons =  saveData.Pokemons.Select(x => new Pokemon(x)).ToList();
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] Position { get; set; }
    public List<PokemonSaveData> Pokemons { get; set; }
}

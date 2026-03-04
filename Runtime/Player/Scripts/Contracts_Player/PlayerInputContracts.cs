using UnityEngine;

public class PlayerInputContracts : MonoBehaviour
{
    public struct PlayerInputState
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump; 
        public bool Interact;
        public bool Sprint;
    }

    public interface IPlayerInput
    {
        PlayerInputState Read(); 
    }
}

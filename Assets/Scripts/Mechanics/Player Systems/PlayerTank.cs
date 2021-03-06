using Interfaces;
using Mechanics.Collectibles;
using Mechanics.Tanks;
using UnityEngine;

namespace Mechanics.Player_Systems
{
    [RequireComponent(typeof(TankHealth), typeof(Inventory), typeof(TankPowerup))]
    public class PlayerTank : MonoBehaviour, IHealable, IInventory<Treasure>
    {
        private TankHealth _health;
        private Inventory _inventory;

        private void Awake()
        {
            _health = GetComponent<TankHealth>();
            _inventory = GetComponent<Inventory>();
        }

        public bool OnHeal(int amount)
        {
            return _health.IncreaseHealth(amount);
        }

        public void OnCollect(Treasure pickup)
        {
            _inventory.AddTreasure(pickup.Value);
        }
    }
}
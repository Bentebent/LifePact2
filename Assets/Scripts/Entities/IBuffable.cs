using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffable
{
    bool ReceiveDamage(int damage, Vector2 velocity, bool maxHealth = false, bool spawnBloodSpray = true);
    void AddStatusEffect(StatusEffect effect);
    void HandleStatusEffects();
}

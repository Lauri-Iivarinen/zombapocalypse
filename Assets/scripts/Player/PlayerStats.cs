using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats{
    LevelUpHandler lvlUp;
    public int level = 1;
    public float xpRequired = 50f;//150;
    public float currXp = 0f;
    public float currHealth = 100f;
    public int mobsKilled = 0;
    public ClassSpecs activeClass;
    //Upgradable stats
    public float damageMultiplier = 1f;
    public float speed = 1f; //Active factor but baseline comes from ship stats
    public float attackSpeed = 1f;
    public float maxHealth = 100f;
    public float critChance = 0.1f;
    public float critDamageMultiplier = 1.5f;
    public float healthRegen = 4f;
    public float bulletPenetration = 0f;
    public float currentPenetration = 0f;
    public float damageReduction = 1f;
    public float xpMultiplier = 1f;

    //Boost
    public float BOOSTCAP = 50f;
    public float currBoost = 50f;
    public bool usingBoost = false;
    private float boostDelay = 50;
    private float currentBoostDelay = 0;
    private float boosterRecharge = 0.1f;

    //Trackers
    public static int killCount = 0;

    public PlayerStats(){
        killCount = 0;
        damageMultiplier += PermanentStats.damage.currAmount;
        speed += PermanentStats.speed.currAmount;
        attackSpeed += PermanentStats.atkSpeed.currAmount;
        maxHealth *= PermanentStats.hp.currAmount;
        critChance += PermanentStats.critChance.currAmount;
        critDamageMultiplier *= PermanentStats.critDamage.currAmount;
        boosterRecharge += PermanentStats.boosterRate.currAmount;
        healthRegen += PermanentStats.hpRegen.currAmount;
        bulletPenetration += PermanentStats.bulletPenetration.currAmount;
        damageReduction += PermanentStats.damageReduction.currAmount;
        xpMultiplier += PermanentStats.xpGain.currAmount;
    }
    
    public void levelUp(){
        level++;
        currXp = currXp-xpRequired;
        xpRequired = (int)(xpRequired+10) * 1.1f;
        Player pl = GameObject.Find("Player").GetComponent<Player>();
        pl.LevelUpAnim();
    }

    public void checkBoost(){
        if (Input.GetKey(KeyCode.Space) && currBoost > 0 && currentBoostDelay == 0){
            if (!usingBoost){
                usingBoost = true;
                speed = speed*2;
            }
            currBoost--;
        }else if (usingBoost){
            speed = speed/2;
            usingBoost = false;
            currentBoostDelay = boostDelay;
        }else if (currBoost < BOOSTCAP){
            currBoost += boosterRecharge;
        }
        if (currentBoostDelay > 0) currentBoostDelay--;
    }
    
    public void IncreaseStat(string toolTip){
        if (toolTip.Equals("Damage Increase +5%")){
            damageMultiplier *= 1.05f;
        }else if (toolTip.Equals("Rocket Speed +5%")){
            speed *= 1.05f;
        }else if (toolTip.Equals("Attack Speed +5%")){
            attackSpeed *= 1.05f;
        }else if (toolTip.Equals("Maximum Health +150")){
            maxHealth += 150;
            currHealth += 150;
        }else if (toolTip.Equals("Critical Chance +5%")){
            critChance += 0.05f;
        }else if (toolTip.Equals("Critical Damage +10%")){
            critDamageMultiplier *= 1.1f;
        }else if(toolTip.Equals("HPS +1")){
            healthRegen += 4f;
        }else if(toolTip.Equals("Bullet penetration +20%")){
            bulletPenetration += 0.2f;
        }else if (toolTip.Equals("Damage reduction 10%")){
            damageReduction += 0.1f;
        }else if (toolTip.Equals("Laser Beam Damage +10")){
            Beam.damage += 10f;
        }else if (toolTip.Equals("Laser Beam Speed and length +10%")){
            BeamController.speed *= 1.1f;
            BeamController.beamSize *= 1.1f;
        }else if (toolTip.Equals("Laser Beam Firerate + 10%")){
            TalentController.beamSpawnRate *= 1.1f;
        }else if (toolTip.Equals("XP gain +10%")){
            xpMultiplier *= 1.1f;
        }
    }

    public void HealthPickup(float amount){
        Player pl = GameObject.Find("Player").GetComponent<Player>();
        currHealth += amount;
        if (currHealth > maxHealth){
            currHealth = maxHealth;
        }
        pl.DisplayDamage(amount, new Color(0,100,0,1f));
    }

    public void GainHealth(){
        Player pl = GameObject.Find("Player").GetComponent<Player>();
        if (currHealth < maxHealth){
            currHealth += healthRegen;
            if (currHealth > maxHealth){
                currHealth = maxHealth;
            }
        }
        pl.DisplayDamage(healthRegen, new Color(0,100,0,1f));
    }

    public int GetPenetration(){
        currentPenetration -= (int)currentPenetration;
        currentPenetration += bulletPenetration;
        return (int)currentPenetration;
    }


    public void gainXp(int amount){
        killCount++;
        currXp += amount* xpMultiplier;
        if (currXp >= xpRequired){
            levelUp();
        }
    }

}
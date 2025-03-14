using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject dmgTxt;
    [SerializeField]
    private GameObject lvlUpPrefab;
    public float movementSpeed;
    public float x;
    public float y;
    public PlayerStats stats;
    public Gun gun;
    public UiDisplay display;
    public ClassSpecs activeClass;
    private List<ClassSpecs> classes;
    public bool switchingGun = false;
    private const int SWITCHDELAY = 120;
    private const int DAMAGETICK = 25;
    private int damageInterval = 0; //Restricts taking damage in every tick
    private const int HPREGENDELAY = 200;
    private int hpRegen = 0;
    public bool dead = false;
    [SerializeField]
    private AudioSource takeDmgSound;

    [SerializeField]
    private AudioSource healSound;

    [SerializeField]
    private AudioSource currencyPickupSound;

    [SerializeField]
    private AudioSource lvlUpAudio;

    public void LevelUpAnim(){
        lvlUpAudio.Play();
        GameObject obj = Instantiate(lvlUpPrefab, transform.position, Quaternion.identity, transform);
        StartCoroutine(ToggleSkillSelection(obj));
        
    }

    public void PlayPickupSound()
    {
        currencyPickupSound.Play();
    }

    public float GetX(){
        return transform.position.x;
    }

    public float GetY(){
        return transform.position.y;
    }

    public float getHealth(){
        return stats.currHealth;
    }

    public void playHealSound(){
        healSound.Play();
    }

    string[] GetUpgradeOptions(List<string> upgradesAll,List<string> currUpgrades, int count){
        if (count == 3){
            return currUpgrades.ToArray();
        }
        int index = UnityEngine.Random.Range(0, upgradesAll.Count);
        string upgrade = upgradesAll[index];
        upgradesAll.RemoveAt(index);
        currUpgrades.Add(upgrade);
        count++;
        return GetUpgradeOptions(upgradesAll, currUpgrades, count);
        
    }

    string[] getTalentOptions(){
        string[] initialOptions = {
        "Damage Increase +10%", 
        "Rocket Speed +10%", 
        "Attack Speed +8%", 
        "Maximum Health +150", 
        "Critical Chance +5%", 
        "Critical Damage +10%", 
        "HPS +1", 
        "Bullet penetration +40%", 
        "Damage reduction 10%",
        "XP gain +15%"
        };
        List<string> options = new List<string>(initialOptions);
        if (TalentController.beamPickedUp){
            options.Add("Laser Beam Damage +10");
            options.Add("Laser Beam Speed and length +10%");
            options.Add("Laser Beam Firerate + 10%");
        }
        if (TalentController.minePickedUp){
            options.Add("Mine Damage +10%");
            options.Add("Mine Spawn Rate +10%");
            options.Add("Mine Explosion Radius +10%");
        }
        if (TalentController.multiShotPickedUp){
            options.Add("Multishot +1 bullet");
            options.Add("Multishot damage +10%");
            options.Add("Multishot firerate +10%");
        }
        return options.ToArray();
    }

    public IEnumerator ToggleSkillSelection(GameObject obj){
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
        LevelUpHandler lvlUp = GameObject.Find("Canvas").GetComponent<LevelUpHandler>();
        //Prob convert to tuples to display proper names in UI
        List<string> upgradesAll = new List<string>(getTalentOptions());
        string[] upgrades = GetUpgradeOptions(upgradesAll, new List<string>(), 0);
        //Select 1 of 3, increase stat
        lvlUp.InitiateLevelUp(upgrades);
    }

    public void DisplayDamage(float dmg, Color color){
        Quaternion rot = transform.rotation;
        rot.z = 0;
        var txt = Instantiate(dmgTxt, transform.position, rot);
        txt.GetComponent<TextMesh>().text = "" + (int)dmg;
        txt.GetComponent<TextMesh>().color = color;
    }

    public void TakeDamage(float amount){
        if (this.damageInterval <= 0 && !dead){
            takeDmgSound.Play();
            float dmg = amount/this.stats.damageReduction;
            this.stats.currHealth -= dmg;
            this.damageInterval = DAMAGETICK;
            DisplayDamage(dmg, new Color(100,0,0, 1f));
        }
    }

    void OnTriggerEnter2D(Collider2D obj)
    {
        if(obj.gameObject.name.Contains("Mob")){
            if (this.damageInterval <= 0){
                MobActions mob = obj.gameObject.GetComponent<MobActions>();
                TakeDamage(mob.GetDamage());
            }
        }
    }

    void OnTriggerStay2D(Collider2D obj){
        if(obj.gameObject.name.Contains("Mob")){
            if (this.damageInterval <= 0){
                MobActions mob = obj.gameObject.GetComponent<MobActions>();
                TakeDamage(mob.GetDamage());
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrencyController.currency = 0;
        this.activeClass = SelectClass.activeClass;
        stats = new PlayerStats();
        stats.maxHealth = activeClass.rocketHealth;
        stats.currHealth = activeClass.rocketHealth;
        stats.speed = activeClass.rocketSpeed;
        movementSpeed = 0.1f * stats.speed;
        
        this.display = GameObject.Find("Canvas").GetComponent<UiDisplay>();
        display.setWeaponName(this.activeClass.className);
        Debug.Log("Logging started");
    }

    public void saveCurrencyAndKills()
    {
        PermanentStats.killCount += PlayerStats.killCount;
        PermanentStats.currency += (int)(CurrencyController.currency * stats.currencyGain);

        int[] arr = { PermanentStats.currency, PermanentStats.killCount };
        DatabaseHandler.SaveStatTrackers(arr);
    }


    IEnumerator DestroySprite(){
        yield return new WaitForSeconds(1f);
        saveCurrencyAndKills();
        SceneManager.LoadScene("DeathScreen");
    }

    void playerDies(){
        StartCoroutine(DestroySprite());
    }

    // Update is called once per frame
    void FixedUpdate(){

        if (this.hpRegen <= 0){
            stats.GainHealth();
            this.hpRegen = HPREGENDELAY;
        }else{
            this.hpRegen--;
        }
        
        if (stats.currHealth > 0 && !switchingGun){//Dead dont walk (unles they are zombies :3)
            this.x = Input.GetAxis("Horizontal");
            this.y = Input.GetAxis("Vertical");
            float moveX = 0f;
            float moveY = 0f;
            if (Input.GetKey(KeyCode.W)) moveY += 1f;
            if (Input.GetKey(KeyCode.S)) moveY -= 1f;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;

            stats.checkBoost();

            Vector3 moveDir = new Vector3(moveX, moveY).normalized;
            //transform.Translate(this.x * movementSpeed, this.y * movementSpeed, 0);
            transform.position += moveDir * 0.1f * stats.speed;
        }else if(stats.currHealth <= 0){
            if (stats.extraLives > 0)
            {
                stats.HealthPickup(stats.maxHealth * 0.4f);
                stats.extraLives--;
                display.UseExtraLife();
                //Push mobs back?
            }
            else
            {
                playerDies();
                dead = true;
            }
            
        }

        //Prevents player from taking damage every tick
        if (damageInterval > 0) damageInterval--;
        
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public SkillsConfigObject config;
    protected Player player;
    
    // 冷却
    public float lastUsedTime = -100f;
    public bool isOnCoolDown => Time.time - lastUsedTime < config.cooldownTime;
    public void StartCoolDown() => lastUsedTime = Time.time;
    public abstract IEnumerator Execute();
    
    public virtual void Init(Player player, SkillsConfigObject config)
    {
        this.player = player;
        this.config = config;
    }

    public virtual bool CanUsed()
    {
        if (isOnCoolDown)
            return false;
        if (player.isDashing || player.isAttacking)
            return false;
        return true;
    }
}

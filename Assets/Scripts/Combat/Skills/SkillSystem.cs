using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem
{
    public Player player;
    public Dictionary<string, Skill> skillsDict = new Dictionary<string, Skill>();

    public SkillSystem(Player player)
    {
        this.player = player;
    }

    public void RegisterSkill(string name, Skill skill)
    {
        if (!skillsDict.ContainsKey("name"))
        {
            
            skill.Init(player, Resources.Load<SkillsConfigObject>($"Skills/{name}"));
            skillsDict.Add(name, skill);
        }
    }

    public bool UsedSkill(string name)
    {
        if (!skillsDict.TryGetValue(name, out Skill skill))
            return false;
        if (!skillsDict[name].CanUsed())
            return false;
        player.StartCoroutine(skill.Execute());
        return true;
    }
}

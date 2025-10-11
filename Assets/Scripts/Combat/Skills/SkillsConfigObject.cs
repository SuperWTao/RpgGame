using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillsConfig", menuName = "Skills/SkillsConfigObject")]
public class SkillsConfigObject : ScriptableObject
{
    public int skillID;
    public string skillName;
    public float duarationTime;
    public float cooldownTime;
}

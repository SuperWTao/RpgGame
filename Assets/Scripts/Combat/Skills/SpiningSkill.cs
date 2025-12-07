using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiningSkill : Skill
{
    public override IEnumerator Execute()
    {
        Debug.Log("释放技能");
        yield return null;
    }
}

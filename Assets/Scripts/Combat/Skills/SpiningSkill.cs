using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiningSkill : Skill
{
    public override IEnumerator Execute()
    {
        Debug.Log("释放技能");
        player.Anim.SetBool("Skill1", player.skill1);
        StartCoolDown();
        yield return new WaitForSeconds(config.duarationTime);
        player.Anim.SetBool("Skill1", false);
    }
}

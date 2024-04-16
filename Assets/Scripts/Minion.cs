using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Minion : MonoBehaviour
{
    private bool isTemp = true;
    public SpriteRenderer SR;
    public TMP_Text levelDisplay;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    private MinionData data;
    private ResourceGenerator generator;
    private int _myLevel = 1;

    public void Setup(MinionData minData, ResourceGenerator gen, int level, bool temp)
    {
        data = minData;
        isTemp = temp;
        SR.sprite = data.minionSprite;
        SR.color = data.testColor;
        SetLevel(level);

        if (minData.summonSound.Length > 0)
        {
            audioSource1.clip = minData.summonSound[Random.Range(0, minData.summonSound.Length)];
            audioSource1.loop = false;
            audioSource1.Play();
        }
        if (minData.summonSoundExtra.Length > 0)
        {
            audioSource2.clip = minData.summonSoundExtra[Random.Range(0, minData.summonSoundExtra.Length)];
            audioSource2.loop = false;
            audioSource2.Play();
        }
        generator = gen;
        if (isTemp == false)
            generator.AddMinion(this);
    }

    public void SetLevel(int newLevel)
    {
        _myLevel = Mathf.Max(_myLevel, newLevel);
        levelDisplay.text = _myLevel.ToString();
    }

    private void Update()
    {
        Vector3 target = Vector3.right * 3f;
        if (generator != null)
            target = generator.MinionWorkPos();
        
        transform.position = Vector3.MoveTowards(transform.position, target, data.moveSpeed * Time.deltaTime);
        if (isTemp && transform.position == target && audioSource1.isPlaying == false && audioSource2.isPlaying == false)
            AbsorbAndLevel();
    }

    private void AbsorbAndLevel()
    {
        Destroy(gameObject);
        generator.UpdateLevels(_myLevel);
    }
}

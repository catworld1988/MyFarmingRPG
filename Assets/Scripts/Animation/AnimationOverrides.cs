
using System.Collections.Generic;
using UnityEngine;

public class AnimationOverrides : MonoBehaviour
{
    [SerializeField] private GameObject character = null;
    //关联ScriptableObject 数据
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray=null;

    //创建 动画类型字典 合成属性Key字典
    private Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionaryByAnimation;
    private Dictionary<string, SO_AnimationType> animationTypeDictionaryByCompositeAttributeKey;


    private void Start()
    {
        //初始化动画片段 类型字典
        animationTypeDictionaryByAnimation = new Dictionary<AnimationClip, SO_AnimationType>();

        //遍历动画类型数组
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            //在字典中 添加片段 和动画类型
            animationTypeDictionaryByAnimation.Add(item.animationClip,item);
        }

        //初始化键值，动画类型字典
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();

        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            //so动画类型数据的键值索引
            string key = item.characterPart.ToString() + item.partVariantColour.ToString() + item.partVariantType.ToString() + item.animationName.ToString();
            //添加字典
            animationTypeDictionaryByCompositeAttributeKey.Add(key, item);
        }
    }

    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributesList)
    {
        foreach (CharacterAttribute characterAttribute in characterAttributesList)
        {
            Animator currentAnimator = null;

            /*KeyValuePair翻译过来就是键值对，也就是一个一对一的数据类型，它是值类型，可以理解为Dictionary(字典)的基本单元。它有两个属性，Key和Value。
            本质上来讲，它就是C#中很多个数据类型之一。你可以这么用。
            KeyValuePair<string, string> data1 = new KeyValuePair<string, string>("001", "John");
            Console.WriteLine(data1.Key);
            类比理解一下Size类型，有Width和Height两个属性。应该可以想明白了。*/

            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            string animatorSOAssetName = characterAttribute.characterPart.ToString();

            //Loop through all character attributes and set the animation override controller for each /循环所有角色属性，并为每个角色属性设置动画覆盖控制器
            Animator[] animatorsArray = character.GetComponentsInChildren<Animator>();

            foreach (Animator animator in animatorsArray)
            {
                //如果有部位名的动画组件 将它设置为当前
                if (animator.name== animatorSOAssetName)
                {
                    currentAnimator = animator;
                    break;
                }
            }

            //Get base current animations for animator /获取动画师的基本当前动画
            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);
            List<AnimationClip> animationsList = new List<AnimationClip>(aoc.animationClips);

            //交换动画
            foreach (AnimationClip animationClip in animationsList)
            {
                //在字典中找到动画
                SO_AnimationType so_AnimationType;
                bool foundAnimation = animationTypeDictionaryByAnimation.TryGetValue(animationClip, out so_AnimationType);

                if (foundAnimation)
                {
                    string key = characterAttribute.characterPart.ToString() + characterAttribute.partVariantColour.ToString() + characterAttribute.partVariantType.ToString() + so_AnimationType.animationName.ToString();

                    //找到交换的动画
                    SO_AnimationType swapSO_AnimationType;
                    bool foundSwapAnimation = animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if (foundSwapAnimation)
                    {
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;

                        animsKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip,swapAnimationClip));
                    }
                }
            }
            //Apply animation updates to animation override controller and then update animator with the new controller
            //将动画更新应用于动画覆盖控制器，然后使用新控制器更新Animator
            aoc.ApplyOverrides(animsKeyValuePairList);
            currentAnimator.runtimeAnimatorController = aoc;
        }
    }

}
public enum AnimationName //动画名称
{
    idleDown,
    idleUp,
    idleRight,
    idleLeft,
    walkUp,
    walkDown,
    walkRight,
    walkLeft,
    runUp,
    runDown,
    runRight,
    runLeft,
    useToolUp,
    useToolDown,
    useToolRight,
    useToolLeft,
    swingToolUp,
    swingToolDown,
    swingToolRight,
    swingToolLeft,
    liftToolUp,
    liftToolDown,
    liftToolRight,
    liftToolLeft,
    holdToolUp,
    holdToolDown,
    holdToolRight,
    holdToolLeft,
    pickDown,
    pickUp,
    pickRight,
    pickLeft,
    count
}

public enum CharacterPartAnimator
{
    body,
    arms,
    hair,
    tool,
    hat,
    count
}

public enum PartVariantColour
{
    none,
    count
}

public enum PartVariantType
{
    none,
    carry,
    hoe,
    pickaxe,
    axe,
    scythe,
    wateringCan,
    count
}

public enum GridBoolProperty //地图格 判定信息
{
    diggable,
    canDropItem,
    canPlaceFurniture,
    isPath, //寻路
    isNPCObstacle //寻路-障碍
}

public enum InventoryLocation //库存位置
{
    player,
    chest, //箱子
    count
}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin,
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    none,
    count
}
public enum ToolEffect
{
    none,
    watering
}


public enum HarvestActionEffect
{
    deciduousLeavesFalling,  //树叶落叶
    pineConesFalling,  //松果
    choppingTreeTrunk,  //树
    breakingStone,  //凿石头
    reaping, //割草
    none
}

public enum Weather
{
    dry, //干燥
    raining,
    snowing,
    none,
    count
}


public enum Direction
{
    up,
    down,
    left,
    right,
    none
}

public enum SoundName
{
    none=0,
    effectFootstepSoftGround =10,
    effectFootstepHardGround =20,
    effectAxe =30,
    effectPixkaxe =40,
    effectScythe =50,
    effectHoe =60,
    effectWateringCan =70,
    effectBasket =80,
    effectPickupSound =90,
    effectRustle =100,
    effectTreeFalling =110,
    effectPlantingSound =120,
    effectPluck =130,
    effectStoneShatter =140,
    effectWoodSplinters =150,
    ambientCountryside1 =1000,
    ambientCountryside2 =1010,
    ambientIndoors1 =1020,
    musicCalm3 =2000,
    musicCalm1 =2010,

}

public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenary,
    Furniture,
    none,
    count
}

public enum Facing
{
    none,
    front,
    back,
    right
}
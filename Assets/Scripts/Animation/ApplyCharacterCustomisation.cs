using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class colorSwap
{
    public Color fromColor;
    public Color toColor;

    public colorSwap(Color fromColor, Color toColor)
    {
        this.fromColor = fromColor;
        this.toColor = toColor;
    }
}


public class ApplyCharacterCustomisation : MonoBehaviour
{
    //输入 贴图
    [Header("Base Textures")]
    [SerializeField] private Texture2D maleFarmerBaseTexture = null;
    [SerializeField] private Texture2D femaleFarmerBaseTexture = null;
    [SerializeField] private Texture2D shirtsBaseTexture = null;
    private Texture2D farmerBaseTexture;

    //创建贴图
    [Header("OutputBase Texture To Be Used For Animation")]
    [SerializeField] private Texture2D farmerBaseCustomised = null;
    private Texture2D farmerBaseShirtsUpdated;
    private Texture2D selectedShirt;

    //选择类型
    [Header("Select Shirt Style")]
    [Range(0, 1)]
    [SerializeField] private int inputShirtStyleNo = 0;

    //性别
    [Header("Select Sex: 0=Male, 1=Female")]
    [Range(0, 1)]
    [SerializeField] private int inputSex = 0;

    private Facing[,] bodyFacingArray;
    private Vector2Int[,] bodyShirtOffsetArray;

    //尺寸 rows行 columns列
    private int bodyRows = 21;
    private int bodyColumns = 6;
    private int farmerSpriteWidth = 16;
    private int farmerSpriteHeight = 32;
    private int shirtTextureWidth = 9;
    private int shirtTextureHeight = 36;
    private int shirtSpriteWidth = 9;
    private int shirtSpriteHeight = 9;
    private int shirtStylesInSpriteWidth = 16;

    private List<colorSwap> colorSwapList;

    //目标手臂颜色用于颜色替换 Target arm colours for color replacemen color1:darkest 深黑红 color2:中红 color:浅红
     private Color32 armTargetColor1 = new Color32(77, 13, 13, 255);
     private Color32 armTargetColor2 = new Color32(138, 41, 41, 255);
     private Color32 armTargetColor3 = new Color32(172, 50, 50, 255);

    // private Color32 armTargetColor1 = new Color32(38, 54, 24, 255);
    // private Color32 armTargetColor2 = new Color32(118, 165, 74, 255);
    // private Color32 armTargetColor3 = new Color32(75, 105, 47, 255);


    private void Awake()
    {
        //初始化 换色列表
        colorSwapList = new List<colorSwap>();

        //自定义处理程序 Process Customisation
        ProcessCustomisation();
    }

    private void ProcessCustomisation()
    {
        //处理性别
        ProcessGender();

        ProcessShirt();

        ProcessArms();

        MergeCustomisations();
    }



    private void ProcessGender()
    {
        //设置性别 基础底板
        if (inputSex == 0)
        {
            farmerBaseTexture = maleFarmerBaseTexture;
        }
        else if (inputSex == 1)
        {
            farmerBaseTexture = femaleFarmerBaseTexture;
        }

        //从纹理 获取基础像素
        Color[] farmerBasePixels = farmerBaseTexture.GetPixels();

        //把基础像素 填充进自定义 并应用
        farmerBaseCustomised.SetPixels(farmerBasePixels);
        farmerBaseCustomised.Apply();
    }

    private void ProcessShirt()
    {
        //身体面对 二维数组 包含[身体列,身体行]
        bodyFacingArray = new Facing[bodyColumns, bodyRows];

        //填充身体面对二维数组
        PopulateBodyFacingArray();

        //初始化身体T恤 偏移数组
        bodyShirtOffsetArray = new Vector2Int[bodyColumns, bodyRows];

        //填充身体T恤偏移数组
        PopulateBodyShirtOffsetArray();

        //创建 选中T恤纹理
        AddShirtToTexture(inputShirtStyleNo);

        //应用T恤纹理到 底板
        ApplyShirtTextureToBase();
    }

    private void ProcessArms()
    {
        //获得手臂像素 去着色
        Color[] farmerPixelsToRecolour = farmerBaseTexture.GetPixels(0, 0, 288, farmerBaseTexture.height);

        //填充手臂颜色交换列表 加了三种颜色
        //TODO 手臂着绿色有问题
        PopulateArmColorSwapList();

        //改变手臂的颜色
        ChangePixelColors(farmerPixelsToRecolour, colorSwapList);

        //设置着色像素
        farmerBaseCustomised.SetPixels(0,0,288,farmerBaseTexture.height,farmerPixelsToRecolour);

        //应用纹理更改
        farmerBaseCustomised.Apply();
    }

    private void MergeCustomisations()
    {
        //农民 T恤像素
        Color[] farmerShirtPixels = farmerBaseShirtsUpdated.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);
        //农民 裤子像素
        Color[] farmerTrouserPixelsSelection = farmerBaseCustomised.GetPixels(288, 0, 96, farmerBaseTexture.height);
        //农民 身体像素
        Color[] farmerBodyPixels = farmerBaseCustomised.GetPixels(0, 0, bodyColumns * farmerSpriteWidth, farmerBaseTexture.height);

        //合并像素
        MergeColourArray(farmerBodyPixels, farmerTrouserPixelsSelection);
        MergeColourArray(farmerBodyPixels, farmerShirtPixels);

        //粘贴合并的像素
        farmerBaseCustomised.SetPixels(0,0,bodyColumns*farmerSpriteWidth,farmerBaseTexture.height,farmerBodyPixels);

        //应用改变
        farmerBaseCustomised.Apply();

    }




    private void PopulateArmColorSwapList()
    {
        //清空颜色交换 列表
        colorSwapList.Clear();

        //手臂重置颜色 3种想改变的目标颜色  765
        colorSwapList.Add(new colorSwap(armTargetColor1, selectedShirt.GetPixel(0, 7)));
        colorSwapList.Add(new colorSwap(armTargetColor2, selectedShirt.GetPixel(0, 6)));
        colorSwapList.Add(new colorSwap(armTargetColor3, selectedShirt.GetPixel(0, 5)));


    }

    private void ChangePixelColors(Color[] baseArray, List<colorSwap> colorSwapList)
    {
        for (int i = 0; i < baseArray.Length; i++)
        {
            //循环交换颜色列表
            if (colorSwapList.Count>0)
            {
                for (int j = 0; j < colorSwapList.Count; j++)
                {
                    //检查原始颜色是否相同
                    if (isSameColor(baseArray[i],colorSwapList[j].fromColor))
                    {
                        baseArray[i] = colorSwapList[j].toColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查是否相同颜色
    /// </summary>
    private bool isSameColor(Color color1, Color color2)
    {
        if ((color1.r==color2.r)&& (color1.g==color2.g)&& (color1.b==color2.b)&& (color1.a==color2.a))
        {
            return true;

        }
        else
        {
            return false;
        }
    }

    private void MergeColourArray(Color[] baseArray, Color[] mergeArray)
    {
        for (int i = 0; i < baseArray.Length; i++)
        {
            if (mergeArray[i].a>0)
            {
                //合并数组 有颜色
                if (mergeArray[i].a>=1)
                {
                    //Fully replace 完全代替
                    baseArray[i] = mergeArray[i];
                }
                else
                {
                    //半透明 Interpolate colors 插入颜色进行混合
                    float alpha = mergeArray[i].a;

                    baseArray[i].r += (mergeArray[i].r - baseArray[i].r) * alpha;
                    baseArray[i].g += (mergeArray[i].g - baseArray[i].g) * alpha;
                    baseArray[i].b += (mergeArray[i].b - baseArray[i].b) * alpha;
                    baseArray[i].a += mergeArray[i].a;
                }
            }
        }
    }

    private void AddShirtToTexture(int shirtStyleNo)
    {
        //初始化 选择T恤纹理
        selectedShirt = new Texture2D(shirtTextureWidth, shirtTextureHeight);
        selectedShirt.filterMode = FilterMode.Point;

        //计算衬衫像素的坐标
        int y = (shirtStyleNo / shirtStylesInSpriteWidth) * shirtTextureHeight;
        int x = (shirtStyleNo % shirtStylesInSpriteWidth) * shirtTextureWidth;
        //获取T恤像素
        Color[] shirtPixels = shirtsBaseTexture.GetPixels(x, y, shirtTextureWidth, shirtTextureHeight);
        //设置和应用
        selectedShirt.SetPixels(shirtPixels);
        selectedShirt.Apply();
    }

    private void ApplyShirtTextureToBase()
    {
        farmerBaseShirtsUpdated = new Texture2D(farmerBaseTexture.width, farmerBaseTexture.height);
        //点过滤模式
        farmerBaseShirtsUpdated.filterMode = FilterMode.Point;
        //隐藏 T恤底板 0,0,0,0
        SetTextureToTransparent(farmerBaseShirtsUpdated);

        Color[] frontShirtPixels;
        Color[] backShirtPixels;
        Color[] rightShirtPixels;

        frontShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 3, shirtSpriteWidth, shirtSpriteHeight);
        backShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 0, shirtSpriteWidth, shirtSpriteHeight);
        rightShirtPixels = selectedShirt.GetPixels(0, shirtSpriteHeight * 2, shirtSpriteWidth, shirtSpriteHeight);

        //循环应用T恤 二维数组 外层循环x 内层循环y
        for (int x = 0; x < bodyColumns; x++)
        {
            for (int y = 0; y < bodyRows; y++)
            {
                //计算像素坐标
                int pixelX = x * farmerSpriteWidth;
                int pixelY = y * farmerSpriteHeight;

                //添加偏移量 得到准确的像素坐标
                if (bodyShirtOffsetArray[x,y]!=null)
                {
                    if (bodyShirtOffsetArray[x,y].x == 99 && bodyShirtOffsetArray[x,y].y == 99) // do not populate with shirt
                        continue;

                    pixelX +=  bodyShirtOffsetArray[x, y].x;
                    pixelY +=  bodyShirtOffsetArray[x, y].y;
                }

                //切换面朝的方向 填充像素
                switch (bodyFacingArray[x,y])
                {
                    //跳过精灵图中空白区
                    case Facing.none:
                        break;

                    case Facing.front:
                        //填充 正面 的T恤像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX,pixelY,shirtSpriteWidth,shirtSpriteHeight,frontShirtPixels);
                        break;

                    case Facing.back:
                        //填充 背面 的T恤像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX,pixelY,shirtSpriteWidth,shirtSpriteHeight,backShirtPixels);
                        break;

                    case Facing.right:
                        //填充 右面 的T恤像素
                        farmerBaseShirtsUpdated.SetPixels(pixelX,pixelY,shirtSpriteWidth,shirtSpriteHeight,rightShirtPixels);
                        break;

                    default:
                        break;
                }
            }
        }
        //应用T恤纹理像素
        farmerBaseShirtsUpdated.Apply();
    }

    private void SetTextureToTransparent(Texture2D texture2D)
    {
        Color[] fill = new Color[texture2D.height * texture2D.width];
        for (int i = 0; i < fill.Length; i++)
        {
            //清除像素 0,0,0,0
            fill[i]=Color.clear;
        }
        texture2D.SetPixels(fill);
    }


    private void PopulateBodyFacingArray()
    {
        //0,0精灵图的左下角 到第10行都是没有的
        bodyFacingArray[0, 0] = Facing.none;
        bodyFacingArray[1, 0] = Facing.none;
        bodyFacingArray[2, 0] = Facing.none;
        bodyFacingArray[3, 0] = Facing.none;
        bodyFacingArray[4, 0] = Facing.none;
        bodyFacingArray[5, 0] = Facing.none;

        bodyFacingArray[0, 1] = Facing.none;
        bodyFacingArray[1, 1] = Facing.none;
        bodyFacingArray[2, 1] = Facing.none;
        bodyFacingArray[3, 1] = Facing.none;
        bodyFacingArray[4, 1] = Facing.none;
        bodyFacingArray[5, 1] = Facing.none;

        bodyFacingArray[0, 2] = Facing.none;
        bodyFacingArray[1, 2] = Facing.none;
        bodyFacingArray[2, 2] = Facing.none;
        bodyFacingArray[3, 2] = Facing.none;
        bodyFacingArray[4, 2] = Facing.none;
        bodyFacingArray[5, 2] = Facing.none;

        bodyFacingArray[0, 3] = Facing.none;
        bodyFacingArray[1, 3] = Facing.none;
        bodyFacingArray[2, 3] = Facing.none;
        bodyFacingArray[3, 3] = Facing.none;
        bodyFacingArray[4, 3] = Facing.none;
        bodyFacingArray[5, 3] = Facing.none;

        bodyFacingArray[0, 4] = Facing.none;
        bodyFacingArray[1, 4] = Facing.none;
        bodyFacingArray[2, 4] = Facing.none;
        bodyFacingArray[3, 4] = Facing.none;
        bodyFacingArray[4, 4] = Facing.none;
        bodyFacingArray[5, 4] = Facing.none;

        bodyFacingArray[0, 5] = Facing.none;
        bodyFacingArray[1, 5] = Facing.none;
        bodyFacingArray[2, 5] = Facing.none;
        bodyFacingArray[3, 5] = Facing.none;
        bodyFacingArray[4, 5] = Facing.none;
        bodyFacingArray[5, 5] = Facing.none;

        bodyFacingArray[0, 6] = Facing.none;
        bodyFacingArray[1, 6] = Facing.none;
        bodyFacingArray[2, 6] = Facing.none;
        bodyFacingArray[3, 6] = Facing.none;
        bodyFacingArray[4, 6] = Facing.none;
        bodyFacingArray[5, 6] = Facing.none;

        bodyFacingArray[0, 7] = Facing.none;
        bodyFacingArray[1, 7] = Facing.none;
        bodyFacingArray[2, 7] = Facing.none;
        bodyFacingArray[3, 7] = Facing.none;
        bodyFacingArray[4, 7] = Facing.none;
        bodyFacingArray[5, 7] = Facing.none;

        bodyFacingArray[0, 8] = Facing.none;
        bodyFacingArray[1, 8] = Facing.none;
        bodyFacingArray[2, 8] = Facing.none;
        bodyFacingArray[3, 8] = Facing.none;
        bodyFacingArray[4, 8] = Facing.none;
        bodyFacingArray[5, 8] = Facing.none;

        bodyFacingArray[0, 9] = Facing.none;
        bodyFacingArray[1, 9] = Facing.none;
        bodyFacingArray[2, 9] = Facing.none;
        bodyFacingArray[3, 9] = Facing.none;
        bodyFacingArray[4, 9] = Facing.none;
        bodyFacingArray[5, 9] = Facing.none;

        //精灵开始的地方
        bodyFacingArray[0, 10] = Facing.back;
        bodyFacingArray[1, 10] = Facing.back;
        bodyFacingArray[2, 10] = Facing.right;
        bodyFacingArray[3, 10] = Facing.right;
        bodyFacingArray[4, 10] = Facing.right;
        bodyFacingArray[5, 10] = Facing.right;

        bodyFacingArray[0, 11] = Facing.front;
        bodyFacingArray[1, 11] = Facing.front;
        bodyFacingArray[2, 11] = Facing.front;
        bodyFacingArray[3, 11] = Facing.front;
        bodyFacingArray[4, 11] = Facing.back;
        bodyFacingArray[5, 11] = Facing.back;

        bodyFacingArray[0, 12] = Facing.back;
        bodyFacingArray[1, 12] = Facing.back;
        bodyFacingArray[2, 12] = Facing.right;
        bodyFacingArray[3, 12] = Facing.right;
        bodyFacingArray[4, 12] = Facing.right;
        bodyFacingArray[5, 12] = Facing.right;

        bodyFacingArray[0, 13] = Facing.front;
        bodyFacingArray[1, 13] = Facing.front;
        bodyFacingArray[2, 13] = Facing.front;
        bodyFacingArray[3, 13] = Facing.front;
        bodyFacingArray[4, 13] = Facing.back;
        bodyFacingArray[5, 13] = Facing.back;

        bodyFacingArray[0, 14] = Facing.back;
        bodyFacingArray[1, 14] = Facing.back;
        bodyFacingArray[2, 14] = Facing.right;
        bodyFacingArray[3, 14] = Facing.right;
        bodyFacingArray[4, 14] = Facing.right;
        bodyFacingArray[5, 14] = Facing.right;

        bodyFacingArray[0, 15] = Facing.front;
        bodyFacingArray[1, 15] = Facing.front;
        bodyFacingArray[2, 15] = Facing.front;
        bodyFacingArray[3, 15] = Facing.front;
        bodyFacingArray[4, 15] = Facing.back;
        bodyFacingArray[5, 15] = Facing.back;

        bodyFacingArray[0, 16] = Facing.back;
        bodyFacingArray[1, 16] = Facing.back;
        bodyFacingArray[2, 16] = Facing.right;
        bodyFacingArray[3, 16] = Facing.right;
        bodyFacingArray[4, 16] = Facing.right;
        bodyFacingArray[5, 16] = Facing.right;

        bodyFacingArray[0, 17] = Facing.front;
        bodyFacingArray[1, 17] = Facing.front;
        bodyFacingArray[2, 17] = Facing.front;
        bodyFacingArray[3, 17] = Facing.front;
        bodyFacingArray[4, 17] = Facing.back;
        bodyFacingArray[5, 17] = Facing.back;

        bodyFacingArray[0, 18] = Facing.back;
        bodyFacingArray[1, 18] = Facing.back;
        bodyFacingArray[2, 18] = Facing.back;
        bodyFacingArray[3, 18] = Facing.right;
        bodyFacingArray[4, 18] = Facing.right;
        bodyFacingArray[5, 18] = Facing.right;

        bodyFacingArray[0, 19] = Facing.right;
        bodyFacingArray[1, 19] = Facing.right;
        bodyFacingArray[2, 19] = Facing.right;
        bodyFacingArray[3, 19] = Facing.front;
        bodyFacingArray[4, 19] = Facing.front;
        bodyFacingArray[5, 19] = Facing.front;

        bodyFacingArray[0, 20] = Facing.front;
        bodyFacingArray[1, 20] = Facing.front;
        bodyFacingArray[2, 20] = Facing.front;
        bodyFacingArray[3, 20] = Facing.back;
        bodyFacingArray[4, 20] = Facing.back;
        bodyFacingArray[5, 20] = Facing.back;

    }

    private void PopulateBodyShirtOffsetArray()
    {
        //到第10行都是没有的 所以偏移99
        bodyShirtOffsetArray[0, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 0] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 0] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 1] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 1] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 2] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 2] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 3] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 3] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 4] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 4] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 5] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 5] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 6] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 6] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 7] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 7] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 8] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 8] = new Vector2Int(99, 99);

        bodyShirtOffsetArray[0, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[1, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[2, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[3, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[4, 9] = new Vector2Int(99, 99);
        bodyShirtOffsetArray[5, 9] = new Vector2Int(99, 99);

        //开始偏移
        bodyShirtOffsetArray[0, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[1, 10] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[2, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 10] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 10] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 10] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[1, 11] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[3, 11] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 11] = new Vector2Int(4, 11);
        bodyShirtOffsetArray[5, 11] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[1, 12] = new Vector2Int(3, 9);
        bodyShirtOffsetArray[2, 12] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[3, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[4, 12] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 12] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 13] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[3, 13] = new Vector2Int(5, 9);
        bodyShirtOffsetArray[4, 13] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[5, 13] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 14] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[2, 14] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 14] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[4, 14] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 14] = new Vector2Int(4, 12);

        bodyShirtOffsetArray[0, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[1, 15] = new Vector2Int(4, 5);
        bodyShirtOffsetArray[2, 15] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 15] = new Vector2Int(4, 12);
        bodyShirtOffsetArray[4, 15] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[5, 15] = new Vector2Int(4, 5);

        bodyShirtOffsetArray[0, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[1, 16] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[2, 16] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[3, 16] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[4, 16] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 16] = new Vector2Int(4, 10);

        bodyShirtOffsetArray[0, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[1, 17] = new Vector2Int(4, 8);
        bodyShirtOffsetArray[2, 17] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 17] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 17] = new Vector2Int(4, 7);
        bodyShirtOffsetArray[5, 17] = new Vector2Int(4, 8);

        bodyShirtOffsetArray[0, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 18] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 18] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 18] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 19] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 19] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 19] = new Vector2Int(4, 9);

        bodyShirtOffsetArray[0, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[1, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[2, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[3, 20] = new Vector2Int(4, 10);
        bodyShirtOffsetArray[4, 20] = new Vector2Int(4, 9);
        bodyShirtOffsetArray[5, 20] = new Vector2Int(4, 9);
    }
}

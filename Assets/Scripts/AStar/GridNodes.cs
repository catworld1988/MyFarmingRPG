using UnityEngine;

public class GridNodes
{
   private int width;
   private int height;

   private Node[,] gridNode;

   //构造函数是一个特殊的成员函数，名字与类名相同，创建类类型对象时由编译器自动调用，保证每个数据成员都有一个合适的初始值，并且在对象的生命周期内只调用一次。
   public GridNodes(int width, int height)
   {
      this.width = width;
      this.height = height;

      gridNode = new Node[width, height];

      for (int x = 0; x < width; x++)
      {
         for (int y = 0; y < height; y++)
         {
            //转换整型
            gridNode[x, y] = new Node(new Vector2Int(x, y));
         }
      }
   }

   /// <summary>
   ///  获取网格节点
   /// </summary>
   public Node GetGridNode(int xPosition, int yPosition)
   {
      if (xPosition<width && yPosition<height)
      {
         return gridNode[xPosition, yPosition];
      }
      else
      {
         //超出地图报错
         Debug.Log("Requested grid node is out of range");
         return null;
      }
   }
}

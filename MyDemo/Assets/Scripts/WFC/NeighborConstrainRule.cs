using System.Collections.Generic;

public enum SlotType {BookShelfTop, BookShelfBottom, Blank, Chair}



public class NeighborConstrainRule
{
    public static Dictionary<SlotType, int> slotTypeWeight = new Dictionary<SlotType, int>()
    {
        {SlotType.BookShelfTop, 1},
        {SlotType.BookShelfBottom, 1},
        {SlotType.Blank, 1}
    };
    
    public static List<SlotType> GetNeightborSlots(SlotType currentSlot, int dir)
    {
        //0左边，1下边，2右边，3上边
        List<SlotType> slots = new List<SlotType>();
        
        // if (currentSlot == SlotType.BookShelfTop)
        // {
        //     if(dir == 0) slots.Add(SlotType.Blank);
        //     else if(dir == 1) slots.Add(SlotType.BookShelfBottom);
        //     else if (dir == 2) slots.Add(SlotType.Blank);
        //     else if (dir == 3) slots.Add(SlotType.Blank);
        // }
        // else if (currentSlot == SlotType.BookShelfBottom)
        // {
        //     if (dir == 0) slots.Add(SlotType.Blank);
        //     else if (dir == 1) slots.Add(SlotType.Blank);
        //     else if (dir == 2) slots.Add(SlotType.Blank);
        //     else if (dir == 3) slots.Add(SlotType.BookShelfTop);
        // }
        // else if (currentSlot == SlotType.Blank)
        // {
        //     if(dir == 0 || dir == 2)
        //     {
        //         slots.Add(SlotType.BookShelfTop);
        //         slots.Add(SlotType.BookShelfBottom);
        //         slots.Add(SlotType.Blank);
        //     }
        //     else if (dir == 1)
        //     {
        //         slots.Add(SlotType.BookShelfTop);
        //         slots.Add(SlotType.Blank);
        //     }
        //     else if (dir == 3)
        //     {
        //         slots.Add(SlotType.BookShelfBottom);
        //         slots.Add(SlotType.Blank);
        //     }
        // }
        // else if (currentSlot == SlotType.Chair)
        // {
        //     if (dir == 0)
        //     {
        //         slots.Add(SlotType.Blank);
        //     }
        //     else if (dir == 1)
        //     {
        //         slots.Add();
        //     }
        // }

        switch (currentSlot)
        {
            case SlotType.BookShelfTop:
                // 书架顶部必须下方连接书架底部，其他方向只能是空白
                if (dir == 1) slots.Add(SlotType.BookShelfBottom);
                else if (dir == 2)
                {
                    slots.Add(SlotType.BookShelfBottom);
                }
                else
                {
                    slots.Add(SlotType.BookShelfBottom);
                }
                break;

            case SlotType.BookShelfBottom:
                // 书架底部必须上方连接书架顶部，其他方向只能是空白
                // if (dir == 3) slots.Add(SlotType.BookShelfTop);
                // else
                // {
                    slots.Add(SlotType.BookShelfTop);
                    slots.Add(SlotType.Blank);
                // }
                break;

            case SlotType.Blank:
                // 空白格周围不能有书架底部
                if (dir == 0 || dir == 2) // 左右
                {
                    slots.Add(SlotType.Blank);
                    slots.Add(SlotType.BookShelfTop);
                }
                else if (dir == 1) // 下
                {
                    slots.Add(SlotType.BookShelfTop);
                    slots.Add(SlotType.Blank);
                    slots.Add(SlotType.BookShelfBottom);
                }
                else if (dir == 3) // 上
                {
                    slots.Add(SlotType.BookShelfBottom);
                    slots.Add(SlotType.Blank);
                }

                break;

            case SlotType.Chair:
                // 椅子必须放在空白格旁边
                slots.Add(SlotType.Blank);
                break;
        }

        return slots;
    }
}
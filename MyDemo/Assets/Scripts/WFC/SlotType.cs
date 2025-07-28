using System.Collections.Generic;
using UnityEngine;



public class Slot
{
    //public Vector2Int position;
    public List<SlotType> modules;
    public SlotType module;
    public bool collapsed;

    public Slot Backup()
    {
        return new Slot
        {
            modules = new List<SlotType>(modules),
            module = this.module,
            collapsed = this.collapsed
        };
    }
}


// class ModuleSet : ICollection<Module>
// {
//     [SerializeField] private long[] data;
//     private float entropy; //熵
//     private bool entropyOutdated = true;
//     private int count;
// }
//
// [SerializeField]
// class Module
// {
//     public string name;
//     public ModuleSet[] possibieNeighbor;//四个面的邻居
//     public Module[][] possibleNeighborArray;
//
//     public int index; //在ModuleSet中的索引
//
//     public float PLogP;
//
//     public Module(string name, int index, float p)
//     {
//         this.name = name;
//         this.index = index;
//         this.PLogP = p * Mathf.Log(p);
//     }
// }

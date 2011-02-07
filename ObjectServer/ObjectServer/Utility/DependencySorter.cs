using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Utility
{
    public static class DependencySorter<IdType>
    {
        public sealed class Item
        {
            public IdType Id { get; set; }
            public IList<IdType> DependsOn { get; set; }
        }

        public static Item[] TopologicalSort(IList<Item> fields)
        {
            var g = new TopologicalSorter(fields.Count);
            var indexes = new Dictionary<IdType, int>();

            //add vertices
            for (int i = 0; i < fields.Count; i++)
            {
                indexes[fields[i].Id] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].DependsOn != null)
                {
                    for (int j = 0; j < fields[i].DependsOn.Count; j++)
                    {
                        g.AddEdge(i, indexes[fields[i].DependsOn[j]]);
                    }
                }
            }

            int[] sortedIndices = g.Sort();
            var sortedList = new List<Item>(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                sortedList.Add(fields[sortedIndices[i]]);
            }

            return sortedList.ToArray();
        }

    }
}

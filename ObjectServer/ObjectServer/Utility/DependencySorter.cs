using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Utility
{
    /// <summary>
    /// 依赖排序器类
    /// </summary>
    /// <typeparam name="EleType">每个元素的类型</typeparam>
    /// <typeparam name="IdType">每个元素的唯一标识类型</typeparam>
    public static class DependencySorter<EleType, IdType>
    {
        public static EleType[] DependencySort(
            IList<EleType> fields,
            Func<EleType, IdType> getIdProc,
            Func<EleType, IList<IdType>> getDependIdProc)
        {
            var g = new TopologicalSorter(fields.Count);
            var indexes = new Dictionary<IdType, int>(fields.Count);

            //add vertices
            for (int i = 0; i < fields.Count; i++)
            {
                indexes[getIdProc(fields[i])] = g.AddVertex(i);
            }

            //add edges
            for (int i = 0; i < fields.Count; i++)
            {
                if (getDependIdProc(fields[i]) != null)
                {
                    for (int j = 0; j < getDependIdProc(fields[i]).Count; j++)
                    {
                        g.AddEdge(i, indexes[getDependIdProc(fields[i])[j]]);
                    }
                }
            }

            int[] sortedIndices = g.Sort();
            var sortedList = new List<EleType>(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                sortedList.Add(fields[sortedIndices[i]]);
            }

            return sortedList.ToArray();
        }

    }
}

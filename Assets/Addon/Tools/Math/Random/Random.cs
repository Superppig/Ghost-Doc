using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class RandomTool
    {
        private static readonly Dictionary<ERandomGrounp, System.Random> groups = new Dictionary<ERandomGrounp, System.Random>();

        static RandomTool()
        {
            foreach (ERandomGrounp key in Enum.GetValues(typeof(ERandomGrounp)))
            {
                groups.Add(key, new System.Random(DateTime.Now.Second));
            }
            spare = -1;
        }

        public static void Randomize(int seed, ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            groups[eRandomGrounp] = new System.Random(seed);
        }

        /// <summary>
        /// 返回[min,max）间的随机整数
        /// </summary>
        public static int RandomInt(int min, int max, ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
            => groups[eRandomGrounp].Next(min, max);

        /// <summary>
        /// 返回[min,max）间的随机浮点数
        /// </summary>
        public static float RandomFloat(float min, float max, ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
            => (float)(groups[eRandomGrounp].NextDouble()) * (max - min) + min;

        /// <summary>
        /// 生成随机单位二维向量（使用多个随机数）
        /// </summary>
        public static Vector2 RandomVector2(ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            double x, y, r;
            do
            {
                x = groups[eRandomGrounp].NextDouble() * 2 - 1;
                y = groups[eRandomGrounp].NextDouble() * 2 - 1;
                r = x * x + y * y;
            } while (r == 0 || r >= 1);
            return new Vector2((float)x, (float)y).normalized;
        }

        /// <summary>
        /// 生成随机单位三维向量（使用多个随机数）
        /// </summary>
        public static Vector3 RandomVector3(ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            double x, y, z, r;
            do
            {
                x = groups[eRandomGrounp].NextDouble() * 2 - 1;
                y = groups[eRandomGrounp].NextDouble() * 2 - 1;
                z = groups[eRandomGrounp].NextDouble() * 2 - 1;
                r = x * x + y * y + z * z;
            } while (r == 0 || r >= 1);
            return new Vector2((float)x, (float)y).normalized;
        }

        private static double spare;    //多余的正态分布随机数
        /// <summary>
        /// 生成符合标准正态分布的随机数(使用多个随机数)
        /// </summary>
        /// <returns></returns>
        public static double RandomNormalDistribution(ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            if (spare != -1)
            {
                double ret = spare;
                spare = -1;
                return ret;
            }
            double x, y, r;
            do
            {
                x = groups[eRandomGrounp].NextDouble() * 2 - 1;
                y = groups[eRandomGrounp].NextDouble() * 2 - 1;
                r = x * x + y * y;
            } while (r == 0 || r >= 1);
            r = Math.Sqrt(-2d * Math.Log(r) / r);
            spare = y * r;
            return x * r;
        }

        /// <summary>
        /// 生成圆内符合均匀分布且的随机点(不使用z分量)
        /// </summary>
        public static List<Vector3> RandomPointsInCircle(float radius,int n, ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            List<Vector3> points = new List<Vector3>();
            double x, y;
            for (int i = 0; i < n; i++)
            {
                do
                {
                    x = groups[eRandomGrounp].NextDouble() * 2 * radius - radius;
                    y = groups[eRandomGrounp].NextDouble() * 2 * radius - radius;
                } while (x * x + y * y > radius * radius);
                points.Add(new Vector3((float)x, (float)y, 0));
            }
            return points;
        }

        /// <summary>
        /// 生成圆内符合均匀分布且任意两点间距大于某个值的随机点(不使用z分量,最大尝试次数为点数量的10倍)
        /// </summary>
        /// <param name="des">接收结果(不会清空数组)</param>
        public static List<Vector3> RandomPointsInCircle(float radius, int n, float minDistance, ERandomGrounp eRandomGrounp = ERandomGrounp.Default)
        {
            List<Vector3> points = new List<Vector3>();
            float sqr = minDistance * minDistance;

            bool Verify(Vector3 v)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if ((points[i] - v).sqrMagnitude < sqr)
                        return false;
                }
                return true;
            }

            int count = 0;
            double x, y;
            for (int i = 0; i < n; )
            {
                do
                {
                    x = groups[eRandomGrounp].NextDouble() * 2 * radius - radius;
                    y = groups[eRandomGrounp].NextDouble() * 2 * radius - radius;
                } while (x * x + y * y > radius * radius);
                Vector3 v = new Vector3((float)x, (float)y, 0);
                if(Verify(v))
                {
                    points.Add(v);
                    i++;
                }
                count++;
                if (count > 10 * n)
                    return points;
            }
            return points;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liquid2D
{
    public static class Liquid2DUtils
    {

        public static void Swap(ref int a, ref int b)
        {
            int c = a;
            a = b;
            b = c;
        }

        public static void Swap(ref ComputeBuffer a, ref ComputeBuffer b)
        {
            ComputeBuffer c = a;
            a = b;
            b = c;
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silmoon.Security.Util
{
    public class DictionarySort : System.Collections.IComparer
    {
        public int Compare(object oLeft, object oRight)
        {
            string sLeft = oLeft as string;
            string sRight = oRight as string;
            int iLeftLength = sLeft.Length;
            int iRightLength = sRight.Length;
            int index = 0;
            while (index < iLeftLength && index < iRightLength)
            {
                if (sLeft[index] < sRight[index])
                    return -1;
                else if (sLeft[index] > sRight[index])
                    return 1;
                else
                    index++;
            }
            return iLeftLength - iRightLength;

        }
    }
}

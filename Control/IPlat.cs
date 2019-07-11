using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimoControl
{
    public  interface IPlat
    {
        bool Login();
        List<betData> getActData();

    }
}

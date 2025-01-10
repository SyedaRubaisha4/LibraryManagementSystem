using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public class SharedUtilityConnection
    {
        public static IConfiguration AppConfiguration { get; set; }
    }
}

using Box.V2.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Box.Samples.WealthManagement.Azure.Component.Test
{
    public class UserTest
    {
        private readonly ITestOutputHelper output;

        public UserTest(ITestOutputHelper output)
        {
            this.output = output;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvTracker.Tests;

public class ReturnsControllerNonActionMethod
{
    [Fact]
    public void BasicTest()
    {
        Assert.True(true);
    }
}

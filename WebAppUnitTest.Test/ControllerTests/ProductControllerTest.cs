using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppUnitTest.Models;

namespace WebAppUnitTest.Test.ControllerTests
{
    // base class to test over EF Core InMemory, SQLITE InMemory, SQL Server Express LocalDB
    public class ProductControllerTest
    {
        protected DbContextOptions<WebAppTestDbContext> contextOptions { get; private set; }

        protected void SetContext(DbContextOptions<WebAppTestDbContext> options)
        {
            contextOptions = options;
        }

    }
}

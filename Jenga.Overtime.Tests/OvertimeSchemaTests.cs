using System.Text.RegularExpressions;
using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jenga.Overtime.Tests;

public sealed class OvertimeSchemaTests
{
    [Fact]
    public void DeploymentScriptContainsEveryMappedOvertimeColumn()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=unused;Integrated Security=true;TrustServerCertificate=true").Options);
        var script = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Overtime.sql"));
        foreach (var entity in db.Model.GetEntityTypes().Where(x => x.ClrType.Namespace == typeof(OvertimeEntry).Namespace && x.ClrType.Name.StartsWith("Overtime")))
        {
            var table = entity.GetTableName()!;
            var definition = Regex.Match(script, $@"CREATE TABLE dbo\.{Regex.Escape(table)}\s*\((.*?)\r?\n\);", RegexOptions.Singleline);
            Assert.True(definition.Success, $"Missing table: {table}");
            var store = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(table, entity.GetSchema());
            foreach (var property in entity.GetProperties())
            {
                var column = property.GetColumnName(store)!;
                Assert.Matches($@"\b{Regex.Escape(column)}\s+(?:int|bit|date|datetime2|nvarchar|varbinary|rowversion)\b", definition.Groups[1].Value);
            }
        }
        Assert.Contains("Version rowversion NOT NULL", script);
        Assert.Contains("BEGIN TRANSACTION", script);
        Assert.Contains("IF OBJECT_ID", script);
    }
}

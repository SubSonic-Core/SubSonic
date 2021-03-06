﻿using FluentAssertions;
using NUnit.Framework;
using SubSonic.Extensions.Test;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SubSonic.Tests.DAL
{
    using Linq;
    using Schema;
    using System.Threading.Tasks;
    using Models = Extensions.Test.Models;

    public partial class SubSonicContextTests
    {
        protected override void SetDeleteBehaviors()
        {
            base.SetDeleteBehaviors();

            string 
                    person_delete = @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (@el_1)",
                    renter_delete = @"DELETE FROM [dbo].[Renter]
WHERE (([ID] IN (@el_1) AND [PersonID] IN (@el_2)) AND [UnitID] IN (@el_3))";

            Context.Database.Instance.AddCommandBehavior(renter_delete, cmd =>
            {
                Models.Renter renter = Renters.Single(x => 
                    x.ID == cmd.Parameters["@el_1"].GetValue<int>() &&
                    x.PersonID == cmd.Parameters["@el_2"].GetValue<int>() &&
                    x.UnitID == cmd.Parameters["@el_3"].GetValue<int>());

                Renters.Remove(renter);

                return 1;
            });

            Context.Database.Instance.AddCommandBehavior(person_delete, cmd =>
            {
                Models.Person person = People.Single(x => x.ID == cmd.Parameters["@el_1"].GetValue<int>());

                if(Renters.Any(x => x.PersonID == person.ID))
                {
                    throw Error.InvalidOperation($"referencial integrity check!");
                }

                People.Remove(person);

                return 1;
            });
        }

        const string expected_delete_udtt = @"DELETE FROM {0}
WHERE ([{1}].[ID] IN (SELECT [ID] FROM @input))";

        private static IEnumerable<IDbTestCase> DeleteTestCases()
        {
            yield return new DbTestCase<Models.Person>(false, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (@el_1)");
            yield return new DbTestCase<Models.Person>(true, @"DELETE FROM [dbo].[Person]
WHERE [ID] IN (
	SELECT [T1].[ID]
	FROM @input AS [T1])");
            yield return new DbTestCase<Models.Renter>(false, @"DELETE FROM [dbo].[Renter]
WHERE (([ID] IN (@el_1) AND [PersonID] IN (@el_2)) AND [UnitID] IN (@el_3))");
            yield return new DbTestCase<Models.Renter>(true, @"DELETE FROM [dbo].[Renter]
WHERE (([ID] IN (
	SELECT [T1].[ID]
	FROM @input AS [T1]) AND [PersonID] IN (
	SELECT [T2].[PersonID]
	FROM @input AS [T2])) AND [UnitID] IN (
	SELECT [T3].[UnitID]
	FROM @input AS [T3]))");
        }

        [Test]
        [TestCaseSource(nameof(DeleteTestCases))]
        public void ShouldBeAbleToDeleteOneOrMoreRecords(IDbTestCase dbTest)
        {
            IList<IEntityProxy>
                expected = new List<IEntityProxy>();

            foreach (IEntityProxy proxy in dbTest.FetchAll())
            {
                if (proxy != null)
                {
                    expected.Add(proxy);
                }
            }

            int
                before = dbTest.Count(),
                after = 0;

            before.Should().BeGreaterThan(0).And.Be(expected.Count());

            Context.Database.Instance.AddCommandBehavior(dbTest.Expectation, cmd =>
            {
                if (dbTest.UseDefinedTableType)
                {
                    return DeleteCmdBehaviorForUDTT(cmd, expected);
                }
                else
                {
                    return DeleteCmdBehaviorForInArray(cmd, expected);
                }
            });

            foreach(IEntityProxy proxy in expected)
            {
                if (proxy is Models.Person person)
                {
                    if (person.Renters.Any())
                    {
                        continue;
                    }

                    dbTest.Delete(proxy);
                }
                else
                {
                    dbTest.Delete(proxy);
                }
            }

            after = (before - Context.ChangeTracking
                .SelectMany(x => x.Value)
                .Count(x => x.IsDeleted));

            after.Should().BeLessOrEqualTo(before);

            if (expected.Count() > 0)
            {
                if (dbTest.UseDefinedTableType)
                {
                    using (dbTest.EntityModel.AlteredState<IDbEntityModel, DbEntityModel>(new
                    {
                        DefinedTableType = new DbUserDefinedTableTypeAttribute(dbTest.EntityModel.Name)
                    }).Apply())
                    {
                        Context.SaveChanges().Should().BeTrue();
                    }
                }
                else
                {
                    Context.SaveChanges().Should().BeTrue();
                }

                FluentActions.Invoking(() =>
                    Context.Database.Instance.RecievedCommand(dbTest.Expectation))
                    .Should().NotThrow();

                Context.Database.Instance.RecievedCommandCount(dbTest.Expectation)
                    .Should()
                    .Be(dbTest.UseDefinedTableType ? 1 : (before - after));

                dbTest.Count().Should().Be(after);
            }
        }

        private int DeleteCmdBehaviorForUDTT(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            if (cmd.Parameters["@input"].Value is DataTable data)
            {
                using(data)
                {
                    foreach(DataRow row in data.Rows)
                    {
                        if (expected.Count() > 0)
                        {
                            if (expected.ElementAt(0) is Models.Person)
                            {
                                People.Remove(People.Single(x => x.ID == (int)row[nameof(Models.Person.ID)]));
                            }
                            else if (expected.ElementAt(0) is Models.Renter)
                            {
                                Renters.Remove(Renters.Single(x =>
                                   x.ID == (int)row[nameof(Models.Renter.ID)] &&
                                   x.PersonID == (int)row[nameof(Models.Renter.PersonID)] &&
                                   x.UnitID == (int)row[nameof(Models.Renter.UnitID)]));
                            }
                        }
                    }
                }
            }

            return expected.Count();
        }

        private int DeleteCmdBehaviorForInArray(DbCommand cmd, IEnumerable<IEntityProxy> expected)
        {
            IEntityProxy proxy = expected.ElementAt(0);

            int count = 0;

            if (proxy is Models.Person)
            {
                People.Remove(People.Single(x => x.ID == cmd.Parameters["@el_1"].GetValue<int>()));

                count++;
            }
            else if (proxy is Models.Renter)
            {
                IEnumerable<Models.Renter> deleted = Renters.Where(x =>
                        x.ID == cmd.Parameters["@el_1"].GetValue<int>() &&
                        x.PersonID == cmd.Parameters["@el_2"].GetValue<int>() &&
                        x.UnitID == cmd.Parameters["@el_3"].GetValue<int>())
                    .ToArray();

                foreach (Models.Renter renter in deleted)
                {
                    Renters.Remove(renter);

                    count++;
                }
            }

            return count;
        }

        [Test]
        public async Task ShouldBeAbleToDeleteUsingObjectGraph()
        {
            foreach(var page in Context.People.ToPagedCollection(10).GetPages())
            {
                await foreach(var person in page)
                {
                    foreach(var renter in person.Renters)
                    {
                        Context.Renters.Delete(renter);
                    }

                    Context.People.Delete(person);
                }

                Context.SaveChanges().Should().BeTrue();
            }
        }
    }
}

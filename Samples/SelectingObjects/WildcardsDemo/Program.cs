using System;
using System.Collections.Generic;
using POCOGenerator;
using POCOGenerator.Objects;
using Samples.Support;

namespace WildcardsDemo
{
	internal static class Program
	{
		private static void Main()
		{
			POCORunner runner = new(GeneratorFactory.GetGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize(s =>
													 {
														 IList<string> include = s.DatabaseObjects
																				  .Tables
																				  .Include;
														 // all the tables under Sales schema
														 include.Add("Sales.*");
														 // HumanResources.Employee but not
														 // HumanResources.EmployeeDepartmentHistory
														 // or HumanResources.EmployeePayHistory
														 include.Add("Employe?");

													 });

			generator.ServerBuilt += (_, e) =>
									 {
										 foreach (Database database in e.Server.Databases)
										 {
											 foreach (Table table in database.Tables)
											 {
												 Console.WriteLine(table);
											 }
										 }
										 // do not generate classes
										 e.Stop = true;
									 };
			runner.Run();
		}
	}
}

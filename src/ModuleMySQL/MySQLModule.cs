﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using Iodine;
using MySql.Data.MySqlClient;

namespace ModuleMySQL
{

	[IodineExtensionAttribute ("mysql")]
	public class MySQLModule : IodineModule
	{
		public MySQLModule ()
			: base ("mysql")
		{
			this.SetAttribute ("openDatabase", new InternalMethodCallback (openDatabase ,this));
		}

		private IodineObject openDatabase(VirtualMachine vm, IodineObject self, IodineObject[] args)
		{
			var db = new MySqlConnection (args[0].ToString());
			db.Open ();
			return new IodineMySQLConnection (db);
		}

	}
}
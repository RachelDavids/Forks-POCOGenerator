namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		#region Connection Settings

		private sealed class ConnectionSettings
			: IConnection
		{
			private readonly object lockObject;

			internal ConnectionSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					ConnectionString = null;
					RDBMS = RDBMS.None;
				}
			}

			private string connectionString;

			public string ConnectionString
			{
				get
				{
					lock (lockObject)
					{
						return connectionString;
					}
				}

				set
				{
					lock (lockObject)
					{
						connectionString = value;
					}
				}
			}

			private RDBMS rdbms = RDBMS.None;

			public RDBMS RDBMS
			{
				get
				{
					lock (lockObject)
					{
						return rdbms;
					}
				}

				set
				{
					lock (lockObject)
					{
						rdbms = value;
					}
				}
			}
		}

		#endregion
	}
}

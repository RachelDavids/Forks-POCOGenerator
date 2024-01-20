using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class NavigationPropertiesSettings
			: INavigationProperties, INavigationPropertiesIteratorSettings
		{
			private readonly object lockObject;

			internal NavigationPropertiesSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					Enable = false;
					VirtualNavigationProperties = false;
					OverrideNavigationProperties = false;
					ManyToManyJoinTable = false;
					Comments = false;
					ListNavigationProperties = true;
					IListNavigationProperties = false;
					ICollectionNavigationProperties = false;
					IEnumerableNavigationProperties = false;
				}
			}

			private bool enable;

			public bool Enable
			{
				get
				{
					lock (lockObject)
					{
						return enable;
					}
				}

				set
				{
					lock (lockObject)
					{
						enable = value;
					}
				}
			}

			private bool virtualNavigationProperties;
			private bool overrideNavigationProperties;

			public bool VirtualNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return virtualNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (virtualNavigationProperties != value)
						{
							virtualNavigationProperties = value;
							if (virtualNavigationProperties && virtualNavigationProperties == overrideNavigationProperties)
							{
								overrideNavigationProperties = false;
							}
						}
					}
				}
			}

			public bool OverrideNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return overrideNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (overrideNavigationProperties != value)
						{
							overrideNavigationProperties = value;
							if (overrideNavigationProperties && virtualNavigationProperties == overrideNavigationProperties)
							{
								virtualNavigationProperties = false;
							}
						}
					}
				}
			}

			private bool manyToManyJoinTable;

			public bool ManyToManyJoinTable
			{
				get
				{
					lock (lockObject)
					{
						return manyToManyJoinTable;
					}
				}

				set
				{
					lock (lockObject)
					{
						manyToManyJoinTable = value;
					}
				}
			}

			private bool comments;

			public bool Comments
			{
				get
				{
					lock (lockObject)
					{
						return comments;
					}
				}

				set
				{
					lock (lockObject)
					{
						comments = value;
					}
				}
			}

			private bool listNavigationProperties = true;
			private bool ilistNavigationProperties;
			private bool icollectionNavigationProperties;
			private bool ienumerableNavigationProperties;

			public bool ListNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return listNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (listNavigationProperties != value)
						{
							if (value)
							{
								listNavigationProperties = true;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
							else
							{
								listNavigationProperties = false;
								ilistNavigationProperties = true;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
						}
					}
				}
			}

			public bool IListNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return ilistNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (ilistNavigationProperties != value)
						{
							if (value)
							{
								listNavigationProperties = false;
								ilistNavigationProperties = true;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
							else
							{
								listNavigationProperties = true;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
						}
					}
				}
			}

			public bool ICollectionNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return icollectionNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (icollectionNavigationProperties != value)
						{
							if (value)
							{
								listNavigationProperties = false;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = true;
								ienumerableNavigationProperties = false;
							}
							else
							{
								listNavigationProperties = true;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
						}
					}
				}
			}

			public bool IEnumerableNavigationProperties
			{
				get
				{
					lock (lockObject)
					{
						return ienumerableNavigationProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (ienumerableNavigationProperties != value)
						{
							if (value)
							{
								listNavigationProperties = false;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = true;
							}
							else
							{
								listNavigationProperties = true;
								ilistNavigationProperties = false;
								icollectionNavigationProperties = false;
								ienumerableNavigationProperties = false;
							}
						}
					}
				}
			}
		}
	}
}

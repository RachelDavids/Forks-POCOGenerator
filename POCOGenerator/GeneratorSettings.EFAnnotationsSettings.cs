using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		#region EF Annotations Settings

		private sealed class EFAnnotationsSettings : IEFAnnotations, IEFAnnotationsIteratorSettings
		{
			private readonly object lockObject;

			internal EFAnnotationsSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					Enable = false;
					Column = false;
					Required = false;
					RequiredWithErrorMessage = false;
					ConcurrencyCheck = false;
					StringLength = false;
					Display = false;
					Description = false;
					ComplexType = false;
					Index = false;
					ForeignKeyAndInverseProperty = false;
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

			private bool column;

			public bool Column
			{
				get
				{
					lock (lockObject)
					{
						return column;
					}
				}

				set
				{
					lock (lockObject)
					{
						column = value;
					}
				}
			}

			private bool required;
			private bool requiredWithErrorMessage;

			public bool Required
			{
				get
				{
					lock (lockObject)
					{
						return required;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (required != value)
						{
							required = value;
							if (required && required == requiredWithErrorMessage)
							{
								requiredWithErrorMessage = false;
							}
						}
					}
				}
			}

			public bool RequiredWithErrorMessage
			{
				get
				{
					lock (lockObject)
					{
						return requiredWithErrorMessage;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (requiredWithErrorMessage != value)
						{
							requiredWithErrorMessage = value;
							if (requiredWithErrorMessage && required == requiredWithErrorMessage)
							{
								required = false;
							}
						}
					}
				}
			}

			private bool concurrencyCheck;

			public bool ConcurrencyCheck
			{
				get
				{
					lock (lockObject)
					{
						return concurrencyCheck;
					}
				}

				set
				{
					lock (lockObject)
					{
						concurrencyCheck = value;
					}
				}
			}

			private bool stringLength;

			public bool StringLength
			{
				get
				{
					lock (lockObject)
					{
						return stringLength;
					}
				}

				set
				{
					lock (lockObject)
					{
						stringLength = value;
					}
				}
			}

			private bool display;

			public bool Display
			{
				get
				{
					lock (lockObject)
					{
						return display;
					}
				}

				set
				{
					lock (lockObject)
					{
						display = value;
					}
				}
			}

			private bool description;

			public bool Description
			{
				get
				{
					lock (lockObject)
					{
						return description;
					}
				}

				set
				{
					lock (lockObject)
					{
						description = value;
					}
				}
			}

			private bool complexType;

			public bool ComplexType
			{
				get
				{
					lock (lockObject)
					{
						return complexType;
					}
				}

				set
				{
					lock (lockObject)
					{
						complexType = value;
					}
				}
			}

			private bool index;

			public bool Index
			{
				get
				{
					lock (lockObject)
					{
						return index;
					}
				}

				set
				{
					lock (lockObject)
					{
						index = value;
					}
				}
			}

			private bool foreignKeyAndInverseProperty;

			public bool ForeignKeyAndInverseProperty
			{
				get
				{
					lock (lockObject)
					{
						return foreignKeyAndInverseProperty;
					}
				}

				set
				{
					lock (lockObject)
					{
						foreignKeyAndInverseProperty = value;
					}
				}
			}
		}

		#endregion
	}
}

using System.Collections;
using System.Collections.Generic;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private abstract class DbObjectsSettingsBase(object lockObject)
		{
			public void Reset()
			{
				lock (lockObject)
				{
					IncludeAll = false;
					ExcludeAll = false;
					Include.Clear();
					Exclude.Clear();
				}
			}

			private bool includeAll;

			public bool IncludeAll
			{
				get
				{
					lock (lockObject)
					{
						return includeAll;
					}
				}

				set
				{
					lock (lockObject)
					{
						includeAll = value;
					}
				}
			}

			private bool excludeAll;

			public bool ExcludeAll
			{
				get
				{
					lock (lockObject)
					{
						return excludeAll;
					}
				}

				set
				{
					lock (lockObject)
					{
						excludeAll = value;
					}
				}
			}

			private readonly SyncList<string> include = new(lockObject);

			public IList<string> Include
			{
				get
				{
					lock (lockObject)
					{
						return include;
					}
				}
			}

			private readonly SyncList<string> exclude = new(lockObject);

			public IList<string> Exclude
			{
				get
				{
					lock (lockObject)
					{
						return exclude;
					}
				}
			}

			private sealed class SyncList<T>(object lockObject)
				: IList<T>
			{
				private readonly List<T> list = [];

				public T this[int index]
				{
					get
					{
						lock (lockObject)
						{
							return list[index];
						}
					}

					set
					{
						lock (lockObject)
						{
							list[index] = value;
						}
					}
				}

				public int Count
				{
					get
					{
						lock (lockObject)
						{
							return list.Count;
						}
					}
				}

				public bool IsReadOnly => false;

				public void Add(T item)
				{
					lock (lockObject)
					{
						list.Add(item);
					}
				}

				public void Clear()
				{
					lock (lockObject)
					{
						list.Clear();
					}
				}

				public bool Contains(T item)
				{
					lock (lockObject)
					{
						return list.Contains(item);
					}
				}

				public void CopyTo(T[] array, int arrayIndex)
				{
					lock (lockObject)
					{
						list.CopyTo(array, arrayIndex);
					}
				}

				public IEnumerator<T> GetEnumerator()
				{
					lock (lockObject)
					{
						return list.GetEnumerator();
					}
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
					lock (lockObject)
					{
						return list.GetEnumerator();
					}
				}

				public int IndexOf(T item)
				{
					lock (lockObject)
					{
						return list.IndexOf(item);
					}
				}

				public void Insert(int index, T item)
				{
					lock (lockObject)
					{
						list.Insert(index, item);
					}
				}

				public bool Remove(T item)
				{
					lock (lockObject)
					{
						return list.Remove(item);
					}
				}

				public void RemoveAt(int index)
				{
					lock (lockObject)
					{
						list.RemoveAt(index);
					}
				}
			}
		}
	}
}

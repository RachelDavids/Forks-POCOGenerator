namespace POCOGenerator.POCOIterators
{
	public interface INavigationPropertiesIteratorSettings
	{
		bool Enable { get; set; }
		bool VirtualNavigationProperties { get; set; }
		bool OverrideNavigationProperties { get; set; }
		bool ManyToManyJoinTable { get; set; }
		bool Comments { get; set; }
		bool ListNavigationProperties { get; set; }
		// ReSharper disable InconsistentNaming
		bool IListNavigationProperties { get; set; }
		bool ICollectionNavigationProperties { get; set; }
		bool IEnumerableNavigationProperties { get; set; }
		// ReSharper restore InconsistentNaming
	}
}
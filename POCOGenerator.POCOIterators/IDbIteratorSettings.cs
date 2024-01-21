namespace POCOGenerator.POCOIterators
{
	public interface IDbIteratorSettings
	{
		IPOCOIteratorSettings POCOIteratorSettings { get; }
		IClassNameIteratorSettings ClassNameIteratorSettings { get; }
		INavigationPropertiesIteratorSettings NavigationPropertiesIteratorSettings { get; }
		IEFAnnotationsIteratorSettings EFAnnotationsIteratorSettings { get; }
	}
}

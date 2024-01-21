namespace POCOGenerator.POCOIterators
{
	public interface IEFAnnotationsIteratorSettings
	{
		bool Enable { get; set; }
		bool Column { get; set; }
		bool Required { get; set; }
		bool RequiredWithErrorMessage { get; set; }
		bool ConcurrencyCheck { get; set; }
		bool StringLength { get; set; }
		bool Display { get; set; }
		bool Description { get; set; }
		bool ComplexType { get; set; }
		bool Index { get; set; }
		bool ForeignKeyAndInverseProperty { get; set; }
	}
}
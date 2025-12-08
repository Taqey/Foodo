public class Tax
{
	public const decimal StandardRate = 0.14m;

	public static decimal Apply(decimal amount)
	{
		return amount * StandardRate;
	}
}

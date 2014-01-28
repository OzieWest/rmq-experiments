using System.Web;
using System.Web.Mvc;

namespace RMQ_Simple_project
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
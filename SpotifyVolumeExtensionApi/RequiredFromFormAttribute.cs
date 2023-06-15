using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace SpotifyVolumeExtensionApi;

public class RequiredFromFormAttribute : FromFormAttribute, IParameterModelConvention
{
	public void Apply(ParameterModel parameter)
	{
		var param = parameter.BindingInfo?.BinderModelName ?? parameter.ParameterName;
		var constraint = new RequiredFromFormActionConstraint(param);

		parameter.Action.Selectors.Last().ActionConstraints.Add(constraint);
	}
}

public class RequiredFromFormActionConstraint : IActionConstraint
{
	private readonly string _parameter;

	public RequiredFromFormActionConstraint(string parameter)
	{
		_parameter = parameter;
	}

	public int Order => 999;

	public bool Accept(ActionConstraintContext context) => context.RouteContext.HttpContext.Request.Form.ContainsKey(_parameter);
}

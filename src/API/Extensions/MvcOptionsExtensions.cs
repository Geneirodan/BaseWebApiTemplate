using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace API.Extensions;

public static class MvcOptionsExtensions
{

    public static void UseGeneralRoutePrefix(this MvcOptions opts, string prefix)
    {
        var attribute = new RouteAttribute(prefix);
        var convention = new RoutePrefixConvention(attribute);
        opts.Conventions.Add(convention);
    }
}

public class RoutePrefixConvention(IRouteTemplateProvider route) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix = new(route);

    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
            selector.AttributeRouteModel = selector.AttributeRouteModel is { } model
                ? AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, model) 
                : _routePrefix;
    }
}
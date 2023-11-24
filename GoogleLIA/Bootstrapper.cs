using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc4;
using GoogleLIA.Controllers;
using GoogleLIA.Services;
using GoogleLIA.Databases;

namespace GoogleLIA
{
    public static class Bootstrapper
    {
    public static IUnityContainer Initialise()
    {
        var container = BuildUnityContainer();

        DependencyResolver.SetResolver(new UnityDependencyResolver(container));

        return container;
    }

    private static IUnityContainer BuildUnityContainer()
    {
        var container = new UnityContainer();

        var dbContext = new AdsDBContext();

        // register all your components with the container here
        // it is NOT necessary to register your controllers

        // e.g. container.RegisterType<ITestService, TestService>();
        container.RegisterType<IGoogleService, GoogleService>();
        container.RegisterType<ICampaignService, CampaignService>();
        container.RegisterType<ILocationService, LocationService>();
        container.RegisterType<ISettingService, SettingService>();

        container.RegisterInstance<AdsDBContext>(dbContext);

        RegisterTypes(container);

        return container;
    }

    public static void RegisterTypes(IUnityContainer container)
    {
    
    }
    }
}
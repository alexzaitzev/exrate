using System;
using System.Collections.Generic;
using Exrate.Repos;
using Exrate.Services;
using Ninject;
using System.Web.Mvc;

namespace Exrate.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver()
        {
            _kernel = new StandardKernel();
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            _kernel.Bind<IRateService>().To<RateService>();
            _kernel.Bind<IRateRepository>().To<RateRepository>();
            _kernel.Bind<IBankService>().To<BankService>();
            _kernel.Bind<IBankRepository>().To<BankRepository>();
        }
    }
}
using Autofac;
using JetBrains.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.ConsoleApp
{
    // Ping-Pong operation simulation
    public class PingCommand : IRequest<PongResponse>
    {

    }
    //do not need to implement any interface, because its a return value
    public class PongResponse
    {
        public DateTime TimeStamp;

        public PongResponse(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
        }
    }
    [UsedImplicitly] // JetBrains.Annotation package
    public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
    {
        public async Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new PongResponse(DateTime.UtcNow))
                .ConfigureAwait(continueOnCapturedContext: false); //to avoid deadlock if this is called synchroniously from UI
        }
    }
    class Program
    {
        static async Task Main(string[] args) //to be able to await for the response Main method should be an async Task
        {
            var builder = new ContainerBuilder(); // using Autofac for Dependency Inversion
            builder.RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope(); // Making it a singleton

            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>(); //IComponentContext comes from Autofac pakage
                return t => c.Resolve(t);
            });

            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .AsImplementedInterfaces(); // registering builder(s) as implemented interfaces to Program.cs

            var container = builder.Build();

            var mediator = container.Resolve<IMediator>();

            var response = await mediator.Send(new PingCommand());
            Console.WriteLine($"Got a response at {response.TimeStamp}");

            Console.WriteLine();
            Console.WriteLine("Press any key to stop application... .. .");
            Console.ReadKey();
        }
    }
}

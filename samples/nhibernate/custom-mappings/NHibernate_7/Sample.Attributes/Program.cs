﻿using System;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Mapping.Attributes;
using NServiceBus;
using NServiceBus.Persistence;
using Environment = NHibernate.Cfg.Environment;

class Program
{

    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.CustomNhMappings.Attributes";
        var nhConfiguration = new Configuration();

        nhConfiguration.SetProperty(Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
        nhConfiguration.SetProperty(Environment.ConnectionDriver, "NHibernate.Driver.Sql2008ClientDriver");
        nhConfiguration.SetProperty(Environment.Dialect, "NHibernate.Dialect.MsSql2008Dialect");
        nhConfiguration.SetProperty(Environment.ConnectionStringName, "NServiceBus/Persistence");

        AddAttributeMappings(nhConfiguration);

        var endpointConfiguration = new EndpointConfiguration("Samples.CustomNhMappings.Attributes");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
        persistence.UseConfiguration(nhConfiguration);

        var endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        try
        {
            var startOrder = new StartOrder
            {
                OrderId = "123"
            };
            await endpoint.SendLocal(startOrder)
                .ConfigureAwait(false);
            await Task.Delay(2000)
                .ConfigureAwait(false);
            var completeOrder = new CompleteOrder
            {
                OrderId = "123"
            };
            await endpoint.SendLocal(completeOrder)
                .ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        finally
        {
            await endpoint.Stop()
                .ConfigureAwait(false);
        }
    }

    #region AttributesConfiguration

    static void AddAttributeMappings(Configuration nhConfiguration)
    {
        var hbmSerializer = new HbmSerializer
        {
            Validate = true
        };

        using (var stream = hbmSerializer.Serialize(typeof(Program).Assembly))
        {
            nhConfiguration.AddInputStream(stream);
        }
    }

    #endregion
}
using System.Data;
using Autofac;
using CodeSquirrel.RecipeApp.API.src.Controllers;
using Npgsql;

namespace CodeSquirrel.RecipeApp.API
{
    public class APIModule : Module
    {
        private readonly string _connectionString;

        public APIModule(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(connection => new NpgsqlConnection(_connectionString)).As<IDbConnection>();
            builder.RegisterType<ProductController>().InstancePerRequest();
            builder.RegisterType<NecessityController>().InstancePerRequest();
            builder.RegisterType<RecipeController>().InstancePerRequest();
            builder.RegisterType<UnitController>().InstancePerRequest();
        }
    }
}

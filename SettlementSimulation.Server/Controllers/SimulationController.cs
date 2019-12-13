using SettlementSimulation.Engine.Models.Buildings;
using SettlementSimulation.Engine.Models.Roads;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;

namespace SettlementSimulation.Server.Controllers
{
    public class SimulationController : ApiController
    {
        public IEnumerable<string> GetBuildings()
        {
            var types = Assembly.Load("SettlementSimulation.Engine")
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Building)))
                .Select(t => t.Name);

            return types;
        }

        public IEnumerable<string> GetRoads()
        {
            var types = Assembly.Load("SettlementSimulation.Engine")
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Road)))
                .Select(t => t.Name);

            return types;
        }
    }
}

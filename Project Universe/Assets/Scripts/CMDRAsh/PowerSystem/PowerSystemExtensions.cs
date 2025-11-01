using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.PowerSystem
{
    public static class PowerSystemExtensions
    {
        /// <summary>
        /// Connects this generator to a router through a waypoint path
        /// </summary>
        public static bool ConnectToRouterViaPath(this IGenerator generator, IRouter router, WaypointPath path, Template template)
        {
            var cable = PowerSystemPathManager.Instance.CreatePathConnection(generator, router, path, template);
            if (cable != null)
            {
                // Add to generator's cable list if it has one
                var genCables = generator.GetComponent<IGenerator>()?.GetType().GetField("iCableDLL",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(generator) as LinkedList<ICable>;
                genCables?.AddLast(cable);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Connects this router to a substation through a waypoint path
        /// </summary>
        public static bool ConnectToSubstationViaPath(this IRouter router, IRoutingSubstation substation, WaypointPath path, Template template)
        {
            var cable = PowerSystemPathManager.Instance.CreatePathConnection(router, substation, path, template);
            if (cable != null)
            {
                // Add to router's cable list
                var routerCables = router.GetType().GetField("iCableDLL",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(router) as LinkedList<ICable>;
                routerCables?.AddLast(cable);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all path-based connections for this component
        /// </summary>
        public static List<PathCable> GetPathConnections(this Component powerComponent)
        {
            return PowerSystemPathManager.Instance.GetComponentConnections(powerComponent);
        }

        /// <summary>
        /// Finds connected machines through the path network
        /// </summary>
        public static T FindConnectedMachine<T>(this Component powerComponent) where T : Component
        {
            return PowerSystemPathManager.Instance.FindConnectedMachine<T>(powerComponent);
        }
    }
}
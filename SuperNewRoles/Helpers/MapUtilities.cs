using System.Collections.Generic;

using Il2CppSystem;
using Object = Il2CppSystem.Object;

namespace SuperNewRoles
{
    public static class MapUtilities
    {
        public static ShipStatus CachedShipStatus = ShipStatus.Instance;

        public static void MapDestroyed()
        {
            CachedShipStatus = ShipStatus.Instance;
            _systems.Clear();
        }

        private static readonly Dictionary<SystemTypes, Object> _systems = new();
        public static Dictionary<SystemTypes, Object> Systems
        {
            get
            {
                if (_systems.Count == 0) GetSystems();
                return _systems;
            }
        }

        private static void GetSystems()
        {
            if (!CachedShipStatus) return;

            var systems = CachedShipStatus.Systems;
            if (systems.Count <= 0) return;

            foreach (var systemTypes in SystemTypeHelpers.AllTypes)
            {
                if (!systems.ContainsKey(systemTypes)) continue;
                _systems[systemTypes] = systems[systemTypes].TryCast<Object>();
            }
        }
    }
}
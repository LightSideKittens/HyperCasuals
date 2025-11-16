using FunGames.Core.Modules;
using UnityEngine;

namespace FunGames.Core.Settings
{
    public interface IFGModuleSettings
    {
        public FGModuleInfo ModuleInfo { get; set; }
        public Color Color { get;}
        public bool LogEnabled { get;}
    }
}
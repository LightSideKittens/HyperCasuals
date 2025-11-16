using System;

namespace FunGames.Core.DatabaseModels
{
    [Serializable]
    public class FGDBModuleParameter
    {
        public string id;
        public string name;
        public string value;
        public string platform;
        public string regex;
        public bool is_mandatory;
    }
}
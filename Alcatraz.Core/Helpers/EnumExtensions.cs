using System;

namespace Alcatraz.Core.Helpers
{
    public static class EnumExtensions
    {
         public static T Parse<T>(string value) where T : struct
         {
             T outEnum;
             return Enum.TryParse(value, true, out outEnum) ? outEnum : default(T);
         }
    }
}
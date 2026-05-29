using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cls.Infrastructure.Persistence.Conversions
{
    internal static class ConverterExtensions
    {
        public static ValueConverter<DateTime, long> DateTimeToTimeSpanConverter =>
             new ValueConverter<DateTime, long>(
                v => v.Ticks,
                v => new DateTime(v)
            );
    }
}

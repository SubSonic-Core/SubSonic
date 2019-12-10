﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SubSonic.Extensions.Test.Models
{
    public class RealEstateProperty
    {
        public RealEstateProperty()
        {
            Units = new HashSet<Unit>();
        }

        [Key]
        public int ID { get; set; }

        public int StatusID { get; set; }

        [ForeignKey(nameof(StatusID))]
        public virtual Status Status { get; set; }

        public bool? HasParallelPowerGeneration { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")]
        public virtual ICollection<Unit> Units { get; set; }
    }
}
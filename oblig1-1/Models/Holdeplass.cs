﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace oblig1_1.Models
{
    public class Holdeplass
    {
        [Key]
        public int HID { get; set; }
        public string Sted { get; set; }
        public string Avgangstider { get; set; }

    }
}
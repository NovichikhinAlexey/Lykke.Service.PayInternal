using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lykke.Service.PayInternal.Models.Supervising
{
    public class AddGroupForOwnerRequest
    {
        [Required]
        public string OwnerId { get; set; }
        [Required]
        public IEnumerable<string> Groups { get; set; }
    }
}

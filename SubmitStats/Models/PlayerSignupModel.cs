using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class PlayerSignupModel
    {
        public Guid Id { get; set; }
        public string QuakeId { get; set; }
        public string Name { get; set; }
        public string QuakeLoginCode { get; set; }

        public PlayerSignupModel()
        {
            Id = Guid.NewGuid();
        }
    }
}

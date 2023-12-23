using System;
using System.Collections.Generic;
using System.Text;

namespace lxEF.Server.Data.DTO
{
    public class CharacterDTO
    {
        //firstname: firstname,
        //lastname: lastname,
        //nationality: nationality,
        //birthdate: birthdate,
        //gender: gender

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
    }
}
